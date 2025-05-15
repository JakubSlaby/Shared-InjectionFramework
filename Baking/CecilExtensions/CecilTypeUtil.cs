using System;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilTypeUtil
	{
		public static CecilWrapper<TypeDefinition> Extensions(this TypeDefinition type)
		{
			return new CecilWrapper<TypeDefinition>(type);
		}

		public static Type ResolveType(this CecilWrapper<TypeDefinition> type)
		{
			var module = type.Token.Module;
			var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == module.Assembly.FullName);
			if (assembly == null)
			{
				Debug.LogError($"Unable to resolve assembly {module.Assembly.FullName}");
				return null;
			}

			var systemType = assembly.GetType(type.Token.FullName);
			if(systemType == null)
				Debug.LogError($"Unable to resolve type {type.Token.FullName}");
			return systemType;
		}

		public static CecilWrapper<TypeDefinition> AddInterface<T>(this CecilWrapper<TypeDefinition> typeWrapper)
		{
			return AddInterface(typeWrapper, typeof(T));
		}
		

		public static CecilWrapper<TypeDefinition> AddInterface(this CecilWrapper<TypeDefinition> typeWrapper, Type interfaceType)
		{
			if(!interfaceType.IsInterface)
				throw new ArgumentException($"Type {interfaceType} is not an interface.");

			var type = typeWrapper.Token;
			var module = typeWrapper.Token.Module;
			// Import and add IInjectedType interface
			var injectedIf = module.ImportReference(interfaceType);
			if (!type.Interfaces.Any(i => i.InterfaceType.FullName == injectedIf.FullName))
			{
				type.Interfaces.Add(new InterfaceImplementation(injectedIf));
				Debug.Log($"Adding interface: {injectedIf.Name} to type: {type.Name}");
			}
			
			return typeWrapper;
		}

		
		public static bool IsTypeOrSubtype(TypeReference type, TypeReference targetType)
		{
			if (!CecilAssemblyUtil.CanProcessSourceAssembly(type.Scope.Name))
				return false;
			
			try
			{
				var def = type.Resolve();
				while (def != null)
				{
					if (def.FullName == targetType.FullName)
						return true;
					
					if (def.BaseType != null && !CecilAssemblyUtil.CanProcessSourceAssembly(def.BaseType.Scope.Name))
						return false;
					def = def.BaseType?.Resolve();
				}

			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			return false;
		}
		
		
		public static TypeReference GetDefaultTypeReference(Type type, ModuleDefinition module)
		{
			switch (type)
			{
				case Type t when t == typeof(object):
					return module.TypeSystem.Object;
				case Type t when t == typeof(void):
					return module.TypeSystem.Void;
				case Type t when t == typeof(Boolean):
					return module.TypeSystem.Boolean;
				case Type t when t == typeof(SByte):
					return module.TypeSystem.SByte;
				case Type t when t == typeof(Byte):
					return module.TypeSystem.Byte;
				case Type t when t == typeof(Char):
					return module.TypeSystem.Char;
				case Type t when t == typeof(Int16):
					return module.TypeSystem.Int16;
				case Type t when t == typeof(UInt16):
					return module.TypeSystem.UInt16;
				case Type t when t == typeof(Int32):
					return module.TypeSystem.Int32;
				case Type t when t == typeof(UInt32):
					return module.TypeSystem.UInt32;
				case Type t when t == typeof(Int64):
					return module.TypeSystem.Int64;
				case Type t when t == typeof(UInt64):
					return module.TypeSystem.UInt64;
				case Type t when t == typeof(Single):
					return module.TypeSystem.Single;
				case Type t when t == typeof(Double):
					return module.TypeSystem.Double;
				case Type t when t == typeof(IntPtr):
					return module.TypeSystem.IntPtr;
				case Type t when t == typeof(UIntPtr):
					return module.TypeSystem.UIntPtr;
				case Type t when t == typeof(String):
					return module.TypeSystem.String;
			}
			throw new ArgumentException($"Type {type} is not supported.");
		}
	}
}