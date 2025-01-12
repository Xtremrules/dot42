﻿using System;
using System.Linq;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Is the given type of the given kind?
        /// </summary>
        public static bool Is(this XTypeReference type, XTypeReferenceKind kind)
        {
            return (type != null) && (type.GetWithoutModifiers().Kind == kind);
        }

        /// <summary>
        /// Is the given type of the given kind?
        /// </summary>
        public static bool Is(this XTypeReference type, XTypeReferenceKind kind1, XTypeReferenceKind kind2)
        {
            if (type == null) return false;
            type = type.GetWithoutModifiers();
            return (type.Kind == kind1) || (type.Kind == kind2);
        }

        /// <summary>
        /// Is the given type of the given kind?
        /// </summary>
        public static bool Is(this XTypeReference type, params XTypeReferenceKind[] kinds)
        {
            return (type != null) && (Array.IndexOf(kinds, type.GetWithoutModifiers().Kind) >= 0);
        }

        /// <summary>
        /// Is the given type a bool?
        /// </summary>
        public static bool IsBoolean(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Bool.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a byte?
        /// </summary>
        public static bool IsByte(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Byte.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a sbyte?
        /// </summary>
        public static bool IsSByte(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.SByte.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a char?
        /// </summary>
        public static bool IsChar(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Char.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a short?
        /// </summary>
        public static bool IsInt16(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Short.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a ushort?
        /// </summary>
        public static bool IsUInt16(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.UShort.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a int?
        /// </summary>
        public static bool IsInt32(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Int.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a uint?
        /// </summary>
        public static bool IsUInt32(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.UInt.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a long?
        /// </summary>
        public static bool IsInt64(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Long.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a ulong?
        /// </summary>
        public static bool IsUInt64(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.ULong.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a IntPtr?
        /// </summary>
        public static bool IsIntPtr(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.IntPtr.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a float?
        /// </summary>
        public static bool IsFloat(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Float.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type a double?
        /// </summary>
        public static bool IsDouble(this XTypeReference type)
        {
            return (type != null) && type.Module.TypeSystem.Double.IsSame(type.GetWithoutModifiers());
        }

        /// <summary>
        /// Is the given type void?
        /// </summary>
        public static bool IsVoid(this XTypeReference type)
        {
            if (type == null) return false;
            type = type.GetWithoutModifiers();
            return (type.FullName == "System.Void") || type.Module.TypeSystem.Void.IsSame(type);
        }

        /// <summary>
        /// Is the given type a wide primitive (long, ulong, double)?
        /// </summary>
        public static bool IsWide(this XTypeReference type)
        {
            if (type == null) return false;
            var ts = type.Module.TypeSystem;
            
            type = type.GetWithoutModifiers();
            
            return ts.Long.IsSame(type) || ts.ULong.IsSame(type) || ts.Double.IsSame(type);
        }

        /// <summary>
        /// Is the given type System.Type?
        /// </summary>
        public static bool IsSystemType(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Type");
        }

        /// <summary>
        /// Is the given type System.Enum?
        /// </summary>
        public static bool IsSystemEnum(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Enum");
        }

        /// <summary>
        /// Is the given type Dot42.Internal.Enum?
        /// </summary>
        public static bool IsInternalEnum(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == InternalConstants.Dot42InternalNamespace + ".Enum");
        }

        /// <summary>
        /// Is the given type a reference to System.Object?
        /// </summary>
        public static bool IsSystemObject(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Object");
        }

        /// <summary>
        /// Is the given type a reference to System.String?
        /// </summary>
        public static bool IsSystemString(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.String");
        }

        /// <summary>
        /// Is the given type a reference to System.Decimal?
        /// </summary>
        public static bool IsSystemDecimal(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Decimal");
        }

        /// <summary>
        /// Is the given type a reference to System.Nullable`1?
        /// </summary>
        public static bool IsSystemNullable(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Nullable`1");
        }

        /// <summary>
        /// Is the given type a reference to System.Array?
        /// </summary>
        public static bool IsSystemArray(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Array");
        }

        /// <summary>
        /// Is the given type a reference to System.IFormattable?
        /// </summary>
        public static bool IsSystemIFormattable(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.IFormattable");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.ICollection?
        /// </summary>
        public static bool IsSystemCollectionsICollection(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Collections.ICollection");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.Generic.ICollection`1?
        /// </summary>
        public static bool IsSystemCollectionsICollectionT(this XTypeReference type)
        {
            if (type == null) return false;
            type = type.GetWithoutModifiers();
            if(!type.IsGenericInstance) return false;
            return type.FullName == "System.Collections.Generic.ICollection`1";
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.IEnumerable?
        /// </summary>
        public static bool IsSystemCollectionsIEnumerable(this XTypeReference type)
        {

            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Collections.IEnumerable");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.Generic.IEnumerable`1?
        /// </summary>
        public static bool IsSystemCollectionsIEnumerableT(this XTypeReference type)
        {
            if (type == null || !type.IsGenericInstance) return false;
            return type.GetWithoutModifiers().FullName == "System.Collections.Generic.IEnumerable`1";
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.IList?
        /// </summary>
        public static bool IsSystemCollectionsIList(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Collections.IList");
        }

        /// <summary>
        /// Is the given type a reference to System.Collections.Generic.IList`1?
        /// </summary>
        public static bool IsSystemCollectionsIListT(this XTypeReference type)
        {
            return (type != null) && (type.GetWithoutModifiers().FullName == "System.Collections.Generic.IList`1");
        }

        /// <summary>
        /// Does the given type extend from System.MulticastDelegate?
        /// </summary>
        public static bool IsDelegate(this XTypeDefinition type)
        {
            while (true)
            {
                var baseType = type.BaseType;
                if (baseType == null)
                    break;
                if (!baseType.TryResolve(out type))
                    break;
                if (type.FullName == typeof(System.MulticastDelegate).FullName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is the given type System.Collections.ICollection or a derived interface?
        /// </summary>
        public static bool ExtendsICollection(this XTypeReference type)
        {
            type = type.GetWithoutModifiers();

            if (type.FullName == "System.Collections.ICollection")
            {
                return true;
            }

            XTypeDefinition typeDef;
            if (!type.TryResolve(out typeDef) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Any(ExtendsICollection);
        }

        /// <summary>
        /// Is the given type IEnumerable or a derived interface?
        /// </summary>
        public static bool ExtendsIEnumerable(this XTypeReference type)
        {
            type = type.GetWithoutModifiers();
            if (type.FullName == "System.Collections.IEnumerable")
            {
                return true;
            }

            XTypeDefinition typeDef;
            if (!type.TryResolve(out typeDef) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Any(ExtendsIEnumerable);
        }

        /// <summary>
        /// Is the given type System.Collections.IList, System.Collections.Generic.IList(T) or a derived interface?
        /// </summary>
        public static bool ExtendsIList(this XTypeReference type)
        {
            type = type.GetWithoutModifiers();

            var fullName = type.FullName;
            if ((fullName == "System.Collections.IList") ||
                (fullName == "System.Collections.Generic.IList`1"))
            {
                return true;
            }

            XTypeDefinition typeDef;
            if (!type.TryResolve(out typeDef) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Any(ExtendsIList);
        }

        /// <summary>
        /// Is the given type an array of a generic parameter?
        /// </summary>
        public static bool IsGenericParameterArray(this XTypeReference type)
        {
            if (!type.IsArray)
                return false;
            return type.ElementType.IsGenericParameter;
        }

        /// <summary>
        /// Is the given type an array of a primitive elements?
        /// </summary>
        public static bool IsPrimitiveArray(this XTypeReference type)
        {
            if (!type.IsArray)
                return false;
            return type.ElementType.IsPrimitive;
        }

        /// <summary>
        /// Is the given type a type definition or a normal type reference?
        /// </summary>
        public static bool IsDefinitionOrReferenceOrPrimitive(this XTypeReference type)
        {
            type = type.GetWithoutModifiers();
            if (type.IsDefinition || type.IsPrimitive)
                return true;
            return (type.Kind == XTypeReferenceKind.TypeReference);
        }

        /// <summary>
        /// Is the given type System.Nullable&lt;T&gt;?
        /// </summary>
        public static bool IsNullableT(this XTypeReference type)
        {
            type = type.GetWithoutModifiers();
            return (type.FullName == "System.Nullable`1");
        }

        /// <summary>
        /// Is the given type an enum?
        /// </summary>
        public static bool IsEnum(this XTypeReference type)
        {
            type = type.GetWithoutModifiers();
            XTypeDefinition typeDef;
            return type.IsEnum(out typeDef);
        }

        /// <summary>
        /// Is the given type an enum?
        /// </summary>
        public static bool IsEnum(this XTypeReference type, out XTypeDefinition typeDef)
        {
            typeDef = null;
            if (type == null)
                return false;
            type = type.GetWithoutModifiers();
            return type.TryResolve(out typeDef) && typeDef.IsEnum;
        }

        /// <summary>
        /// Is the given type a struct?
        /// </summary>
        public static bool IsStruct(this XTypeReference type)
        {
            XTypeDefinition typeDef;
            type = type.GetWithoutModifiers();
            return type.IsStruct(out typeDef);
        }

        /// <summary>
        /// Is the given type a struct?
        /// </summary>
        public static bool IsStruct(this XTypeReference type, out XTypeDefinition typeDef)
        {
            typeDef = null;
            if (type == null)
                return false;
            type = type.GetWithoutModifiers();
            return type.TryResolve(out typeDef) && typeDef.IsStruct;
        }

        public static bool AllowConstraintAsTypeReference(this XGenericParameter gp)
        {
            return false;

            // TODO: to allow type contraints to replace System.Object seems to be quite
            //       a good idea, especially when one wants to decompile or convert to
            //       java. I couldn't get it to work in some corner cases (see compiler tests)
            //       so I'm leaving it disabled for now.
            //var constraints = gp.Constraints;
            //// use the first constraint as type, if is is a class or if there is only one.
            //if (constraints.Length == 0)
            //    return false;
            //// primitive types are no value types.
            //if (constraints[0].IsValueType || constraints[0].FullName == "System.ValueType")
            //    return false;
            

            //// don't prefer one interface over others.
            //if (constraints.Length > 1 && constraints[0].Resolve().IsInterface)
            //    return false;

            //return true;
        }

        /// <summary>
        /// Is the given type a base class of the given child?
        /// </summary>
        public static bool IsBaseOf(this XTypeDefinition type, XTypeDefinition child)
        {
            while (child != null)
            {
                if (child.BaseType == null)
                    return false;
                XTypeDefinition baseType;
                if (!child.BaseType.TryResolve(out baseType))
                    return false;
                if (baseType.IsSame(type))
                    return true;
                child = baseType;
            }
            return false;
        }
    }
}
