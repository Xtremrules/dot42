﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using ArrayType = Dot42.DexLib.ArrayType;
using ByReferenceType = Dot42.DexLib.ByReferenceType;
using FieldReference = Dot42.DexLib.FieldReference;
using MethodBody = Dot42.DexLib.Instructions.MethodBody;
using MethodDefinition = Dot42.DexLib.MethodDefinition;
using MethodReference = Dot42.DexLib.MethodReference;
using TypeReference = Dot42.DexLib.TypeReference;

namespace Dot42.CompilerLib.Target.CompilerCache
{
    public class CacheEntry
    {
        public readonly MethodBody Body;
        public readonly MethodEntry MethodEntry;
        public readonly ReadOnlyCollection<SourceCodePosition> SourceCodePositions;
        public readonly string ClassSourceFile;

        public CacheEntry(MethodBody body, MethodEntry methodEntry, IList<SourceCodePosition> sourceCodePositions, string classSourceFile)
        {
            Body = body;
            MethodEntry = methodEntry;
            ClassSourceFile = classSourceFile;
            SourceCodePositions = sourceCodePositions != null ? new ReadOnlyCollection<SourceCodePosition>(sourceCodePositions) : null;
        }
    }

    public class DexMethodBodyCompilerCache
    {
        private AssemblyModifiedDetector _modifiedDetector;

        private readonly Task _initialize;

        private DexLib.Dex _dex;
        private DexLookup _dexLookup;
        private MapFileLookup _map;

        private readonly Dictionary<Tuple<string, string>, Tuple<TypeEntry, MethodEntry>> _methodsByScopeId = new Dictionary<Tuple<string, string>, Tuple<TypeEntry, MethodEntry>>();

        private int statCacheHits;
        private int statCacheMisses;

        public bool IsEnabled { get; private set; }

        public DexMethodBodyCompilerCache()
        {
        }

        public DexMethodBodyCompilerCache(string cacheDirectory, Func<Mono.Cecil.AssemblyDefinition, string> filenameFromAssembly, string dexFilename = "classes.dex")
        {
            dexFilename = Path.Combine(cacheDirectory, dexFilename);
            var mapfile = Path.ChangeExtension(dexFilename, ".d42map");

            if (!File.Exists(dexFilename) || !File.Exists(mapfile))
                return;

            IsEnabled = true;

            _initialize = Task.Factory.StartNew(()=>Initialize(dexFilename, mapfile, filenameFromAssembly), TaskCreationOptions.LongRunning);
        }

        private void Initialize(string dexFilename, string mapfile, Func<AssemblyDefinition, string> filenameFromAssembly)
        {
            try
            {
                var readDex = Task.Factory.StartNew(() => DexLib.Dex.Read(dexFilename));
                var readMap = Task.Factory.StartNew(() => new MapFileLookup(new MapFile(mapfile)));

                Task.WaitAll(readMap, readDex);
                var dex = readDex.Result;
                var map = readMap.Result;
                
                _modifiedDetector = new AssemblyModifiedDetector(filenameFromAssembly, map);

                foreach (var type in map.TypeEntries)
                {
                    if(type.ScopeId == null)
                        continue;
                    
                    var typeScopeId = GetTypeScopeId(type);
                    
                    // redirect the generated class if neccessary.
                    var dexType = type.Id == 0 ? map.GeneratedType : type;

                    foreach (var method in type.Methods)
                    {
                        if (type.ScopeId == null)
                            continue;
                        
                        var scopeKey = Tuple.Create(typeScopeId, method.ScopeId);
                        _methodsByScopeId[scopeKey] = Tuple.Create(dexType, method);
                    }
                }

                _dexLookup = new DexLookup(dex);

                _dex = dex;
                _map = map;
            }
            catch (Exception ex)
            {
                IsEnabled = false;
                DLog.Warning(DContext.CompilerCodeGenerator, "Unable to initialize compiler cache: {0}", ex.Message);
            }            
        }

        public void PrintStatistics()
        {
            if(IsEnabled)
                DLog.Info(DContext.CompilerCodeGenerator, "Compiler cache: {0} hits and {1} misses.", statCacheHits, statCacheMisses);
        }

        public CacheEntry GetFromCache(MethodDefinition targetMethod, XMethodDefinition sourceMethod, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var ret = GetFromCacheImpl(targetMethod, sourceMethod, compiler, targetPackage);
            
            if (ret != null) Interlocked.Increment(ref statCacheHits);
            else             Interlocked.Increment(ref statCacheMisses);
            
            return ret;
        }

        public CacheEntry GetFromCacheImpl(MethodDefinition targetMethod, XMethodDefinition sourceMethod, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            if (_initialize == null)
                return null;
            _initialize.Wait();

            if (!IsEnabled)
                return null;

            if (sourceMethod.ScopeId == null || sourceMethod.ScopeId == "(none)")
            {
                return null;
            }

            if (IsUnderlyingCodeModified(sourceMethod)) 
                return null;

            Tuple<TypeEntry, MethodEntry> entry;

            string typeScopeId = sourceMethod.DeclaringType.ScopeId;
            string methodScopeId = sourceMethod.ScopeId;

            MethodDefinition cachedMethod;

            if (_methodsByScopeId.TryGetValue(Tuple.Create(typeScopeId, methodScopeId), out entry))
            {
                cachedMethod = _dexLookup.GetMethod(entry.Item1.DexName, entry.Item2.DexName, entry.Item2.DexSignature);
            }
            else
            {
                // try directly in the dexlookup, for jar imports
                cachedMethod = _dexLookup.GetMethod(typeScopeId.Replace("/", "."), targetMethod.Name, targetMethod.Prototype.ToSignature());
            }

            if (cachedMethod == null)
                return null;

            if (cachedMethod.Body == null)
            {
                // I believe there is a bug in MethodExplicitInterfaceConverter generating
                // stubs for interfaces if they derive from an imported interface.
                // Bail out for now until this is fixed.
                DLog.Debug(DContext.CompilerCodeGenerator, "Compiler cache: no method body found on cached version of {0}, even though one was expected.", sourceMethod);
                return null;
            }

            try
            {
                if (!Equals(cachedMethod.Prototype,targetMethod.Prototype))
                {
                    throw new Exception("internal error, got the wrong method.");
                }

                var body = DexMethodBodyCloner.Clone(targetMethod, cachedMethod);
                FixReferences(body, compiler, targetPackage);

                string className = entry != null ? entry.Item1.DexName : body.Owner.Owner.Fullname;
                var @class = _dexLookup.GetClass(className);

                return new CacheEntry(body, entry != null ? entry.Item2 : null, entry != null ? _map.GetSourceCodePositions(entry.Item2) : null, @class.SourceFile);
            }
            catch (CompilerCacheResolveException ex)
            {
                // This happens at the moment for methods using fields in the __generated class,
                // as well as for references to generated methods (mostly explicit interfac stubs)
                // during the IL conversion phase.
                // This also seems to happen for Framework-nested classes, maybe because these do
                // not get an entry in the map file. This should be fixed.
                // The number of these failures in my test is 890 out of ~12000. We gracefully
                // handle errors by re-compiling the method body.
                Debug.WriteLine(string.Format("Compiler cache: error while converting cached body: {0}: {1}. Not using cached body.", sourceMethod, ex.Message));
                return null;
            }
            catch (Exception ex)
            {
                DLog.Warning(DContext.CompilerCodeGenerator, "Compiler cache: exception while converting cached body: {0}: {1}. Not using cached body.", sourceMethod, ex.Message);
                Trace.WriteLine(string.Format("Compiler cache: error while converting cached body: {0}: {1}. Not using cached body.", sourceMethod, ex.Message));
                return null;
            }
        }

        private bool IsUnderlyingCodeModified(XMethodDefinition sourceMethod)
        {
            var ilMethod = sourceMethod as XModel.DotNet.XBuilder.ILMethodDefinition;
            var javaMethod = sourceMethod as XModel.Java.XBuilder.JavaMethodDefinition;
            if (ilMethod != null)
            {
                var assembly = ilMethod.OriginalMethod.DeclaringType.Module.Assembly;

                if (_modifiedDetector.IsModified(assembly))
                    return true;
            }
            else if (javaMethod != null)
            {
                var javaType = (XModel.Java.XBuilder.JavaTypeDefinition)javaMethod.DeclaringType;
                var classFile = (ClassFile) javaType.OriginalTypeDefinition;
                var loader = classFile.Loader as AssemblyClassLoader;
                var assembly = loader == null ? null : loader.GetAssembly(classFile);
                if (assembly == null || _modifiedDetector.IsModified(assembly))
                    return true;
            }
            else
            {
                // TODO: synthetic methods could be resolved from the cache as well.
                //       check if this would bring any performance benefits.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Operands refering to types, methods or fields need to be fixed, as they might have
        /// gotten another name in the target package. he same applies for catch references.
        /// </summary>
        private void FixReferences(MethodBody body, AssemblyCompiler compiler,  DexTargetPackage targetPackage)
        {
            // fix operands
            foreach (var ins in body.Instructions)
            {
                var fieldRef = ins.Operand as FieldReference;
                var methodRef = ins.Operand as MethodReference;
                var classRef = ins.Operand as ClassReference;

                if (classRef != null)
                {
                    ins.Operand = ConvertClassReference(classRef, compiler, targetPackage);
                }
                else if (fieldRef != null)
                {
                    ins.Operand = ConvertFieldReference(fieldRef, compiler, targetPackage);
                }
                else if (methodRef != null)
                {
                    ins.Operand = ConvertMethodReference(methodRef, compiler, targetPackage);
                }
            }

            // fix catch clauses
            foreach (var @catch in body.Exceptions.SelectMany(e => e.Catches))
            {
                if (@catch.Type != null)
                    @catch.Type = ConvertTypeReference(@catch.Type, compiler, targetPackage);
            }

        }
     
        private TypeReference ConvertTypeReference(TypeReference sourceRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            if (sourceRef is PrimitiveType)
            {
                return sourceRef;
            }

            if (sourceRef is ByReferenceType)
            {
                var type = (ByReferenceType) sourceRef;
                var elementType = ConvertTypeReference(type.ElementType, compiler, targetPackage);
                return new ByReferenceType(elementType);
            }

            if (sourceRef is ArrayType)
            {
                var arrayType = (ArrayType) sourceRef;
                var elementType = ConvertTypeReference(arrayType.ElementType,compiler, targetPackage);
                return new ArrayType(elementType);
            }
            
            // must be ClassReference
            return ConvertClassReference((ClassReference)sourceRef, compiler, targetPackage);
        }

        private ClassReference ConvertClassReference(ClassReference sourceRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            TypeEntry type = _map.GetTypeBySignature(sourceRef.Descriptor);

            if (IsDelegateInstance(type))
            {
                // special delegate handling.
                return GetDelegateInstanceType(type, sourceRef, compiler, targetPackage).InstanceDefinition;
            }
            else
            {
                var xTypeDef = ResolveToType(type, sourceRef, compiler);
                return xTypeDef.GetClassReference(targetPackage);
            }
        }

        private MethodReference ConvertMethodReference(MethodReference methodRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            TypeEntry typeEntry;
            
            var owner = methodRef.Owner as ClassReference;
            if (owner == null)
                return methodRef; // this must be an internal method. return as-is.
            
            MethodEntry methodEntry = _map.GetMethodByDexSignature(owner.Fullname, methodRef.Name, methodRef.Prototype.ToSignature());
            string scopeId = null;

            if (methodEntry != null)
            {
                // important to do this indirection, to correctly resolve methods in
                // the "__generated" class
                typeEntry = _map.GetTypeByMethodId(methodEntry.Id);
                scopeId = methodEntry.ScopeId;
            }
            else
            {
                typeEntry = _map.GetTypeBySignature(methodRef.Owner.Descriptor);
                
                // special delegate handling
                if (IsDelegateInstance(typeEntry))
                {
                    var delInstanceType = GetDelegateInstanceType(typeEntry, owner, compiler, targetPackage);
                    return new MethodReference(delInstanceType.InstanceDefinition, methodRef.Name, methodRef.Prototype);
                }
            }

            if(scopeId == null)
                scopeId = methodRef.Name + methodRef.Prototype.ToSignature();

            if(scopeId == "(none)")
                throw new CompilerCacheResolveException("unable to resolve method without scope: " + methodRef);

            var xTypeDef = ResolveToType(typeEntry, owner, compiler);

            var methodDef = xTypeDef.GetMethodByScopeId(scopeId);

            if (methodDef == null)
            {
                throw new CompilerCacheResolveException("unable to resolve method by it's scope id: " + methodRef + " (" + scopeId + ")");
            }

            return methodDef.GetReference(targetPackage);
        }

        /// <summary>
        /// Delegate methods are created unfortunately during the compilation phase in AstCompiler.VisitExpression.
        /// Model this behaviour here.
        /// </summary>
        private DelegateInstanceType GetDelegateInstanceType(TypeEntry typeEntry, ClassReference classRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var scopeIds = typeEntry.ScopeId.Split(new[] { ":delegate:" }, StringSplitOptions.None);

            var typeScopId = scopeIds[0];
            var xTypeDef = compiler.Module.GetTypeByScopeID(GetTypeScopeId(typeEntry.Scope, typeScopId, typeEntry.Name));
            var delegateType = compiler.GetDelegateType(xTypeDef);
            
            var calledMethodId = scopeIds[1];
            var calledTypeScopeId = calledMethodId.Split('|')[0];
            var calledMethodScope = calledMethodId.Split('|')[1];

            var calledTypeDef = compiler.Module.GetTypeByScopeID(calledTypeScopeId);
            var calledMethod = calledTypeDef.GetMethodByScopeId(calledMethodScope);

            // NOTE: we are loosing the SequencePoint (DebugInfo) here. I'm not sure if this
            //       was ever valuable anyways.
            var delInstanceType = delegateType.GetOrCreateInstance(null, targetPackage, calledMethod);
            return delInstanceType;
        }

        private object ConvertFieldReference(FieldReference fieldRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            // We could also handle access to fields in the generated class; but see the coverage comment above.
            // It would require to map fields in the MapFile as well, at least for those in the __generated 
            // class. I don't think its worth it.
            if (fieldRef.Owner.Descriptor == _map.GeneratedType.DexSignature)
                throw new CompilerCacheResolveException("unable to resolve fields in __generated: " + fieldRef);

            // I don't believe we have to protect ourselfs from field name changes. 
            // Except for obfuscation, there is no reason to rename fields. They are 
            // independent of other classes not in their class hierachy (of which we know
            // that it can not have changed)

            var classRef = ConvertClassReference(fieldRef.Owner, compiler, targetPackage);
            var typeRef = ConvertTypeReference(fieldRef.Type, compiler, targetPackage);

            return new FieldReference(classRef, fieldRef.Name, typeRef);
        }

        /// <summary>
        /// will throw if type is not found.
        /// </summary>
        private static XTypeDefinition ResolveToType(TypeEntry type, ClassReference sourceRef, AssemblyCompiler compiler)
        {
            XTypeDefinition xTypeDef = null;

            if (type != null)
            {
                string scopeId = GetTypeScopeId(type);
                xTypeDef = compiler.Module.GetTypeByScopeID(scopeId);
            }
            else
            {
                string scopeId = sourceRef.Descriptor.Substring(1,sourceRef.Descriptor.Length-2);
                xTypeDef = compiler.Module.GetTypeByScopeID(scopeId);
            }

            if (xTypeDef == null)
            {
                throw new CompilerCacheResolveException("unable to resolve " + sourceRef);
            }
            return xTypeDef;
        }

        private static string GetTypeScopeId(TypeEntry type)
        {
            return GetTypeScopeId(type.Scope, type.ScopeId, type.Name);
        }

        private static string GetTypeScopeId(string scope, string scopeId, string typeFullname)
        {
            return scope == null || scopeId == null ? typeFullname : string.Join(":", scope, scopeId);
        }

        private bool IsDelegateInstance(TypeEntry type)
        {
            return type != null && type.ScopeId != null && type.ScopeId.Contains(":delegate:");
        }

    }
}
