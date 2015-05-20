﻿extern alias ilspy;
using System;
using System.Windows.Automation.Peers;
using Dot42.CompilerLib;
using Dot42.CompilerLib.XModel;
using ilspy::Mono.Cecil;
using ICSharpCode.ILSpy;


namespace Dot42.Compiler.ILSpy
{
    public abstract class CompiledLanguage : Language
    {
        private static CachedCompiler compiler = new CachedCompiler();

        public AssemblyCompiler AssemblyCompiler { get { return compiler.AssemblyCompiler; } }

        protected CompiledMethod GetCompiledMethod(MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            compiler.CompileIfRequired(assembly);
            var xMethod = GetXMethodDefinitionAfterCompilerSetup( method);
            var cmethod = AssemblyCompiler.GetCompiledMethod(xMethod);

            return cmethod;
        }

        protected XMethodDefinition GetXMethodDefinition(MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            compiler.CompileIfRequired(assembly, true);

            return GetXMethodDefinitionAfterCompilerSetup(method);        }

        protected XTypeDefinition GetXTypeDefinition(TypeDefinition type)
        {
            var assembly = type.Module.Assembly;

            compiler.CompileIfRequired(assembly, true);

            var xFullName = GetXFullName(type);

            XTypeDefinition tdef;
            if (!AssemblyCompiler.Module.TryGetType(xFullName, out tdef))
            {
                throw new Exception("type not found: " + xFullName);
            }
            return tdef;
        }

        private XMethodDefinition GetXMethodDefinitionAfterCompilerSetup(MethodDefinition method)
        {
            XTypeDefinition tdef;
            var xFullName = GetXFullName(method.DeclaringType);

            if (!AssemblyCompiler.Module.TryGetType(xFullName, out tdef))
            {
                throw new Exception("type not found: " + xFullName);
            }

            int methodIdx = method.DeclaringType.Methods.IndexOf(method);
            return tdef.GetMethodByScopeId(methodIdx.ToString());
        }        protected static string GetXFullName(TypeDefinition type)
        {
            return GetScopePrefix(type) + GetNamespace(type) + "." + type.Name;        }

        private static string GetNamespace(TypeDefinition type)
        {
            if (!type.IsNested)
                return type.Namespace;

            return GetNamespace(type.DeclaringType) + "." + type.DeclaringType.Name;
        }


        private static string GetScopePrefix(TypeDefinition type)
        {
            var scope = type.Scope.Name;
            if (scope.ToLowerInvariant().EndsWith(".dll"))
                scope = scope.Substring(0, scope.Length - 4);

            if (scope.ToLowerInvariant() == "dot42")
                return "";

            return scope.Replace(".", "_") + ((type.Namespace.Length == 0 && !type.IsNested) ? "" : ".");        }
            

    }
}
