using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilReflectionHelper
	{
		public struct MethodSearchParameters
		{
			public Type Type { get; set; }
			public string MethodName { get; set; }
			public Type[] GenericArgumentTypes { get; set; }
			public int GenericArgumentCount => GenericArgumentTypes?.Length ?? 0;
			
			public bool IsGenericMethod => GenericArgumentTypes?.Any() ?? false;
			public Type[] ParameterTypes { get; set; }
			public int ParameterCount => ParameterTypes?.Length ?? 0;
			public BindingFlags Flags { get; set; }
			
			public bool IsStatic
			{
				get => Flags.HasFlag(BindingFlags.Static);
			}
		}

		public static MethodSearchParameters FindMethod(this Type type, string methodName)
		{
			return new MethodSearchParameters()
			{
				Type = type,
				MethodName = methodName,
			};
		}

		public static MethodSearchParameters FindMethod<T>(string methodName)
		{
			return FindMethod(typeof(T), methodName);
		}

		public static MethodSearchParameters WithGenericArguments(this MethodSearchParameters method,
			params Type[] genericArguments)
		{
			method.GenericArgumentTypes = genericArguments;
			return method;
		}

		public static MethodSearchParameters WithArguments(this MethodSearchParameters method, params Type[] arguments)
		{
			method.ParameterTypes = arguments;
			return method;
		}

		public static MethodSearchParameters AsStatic(this MethodSearchParameters method)
		{
			method.Flags |= BindingFlags.Static;
			return method;
		}
		
		[ThreadStatic]
		private static List<Type> s_HelperEvaluateArgumentJoin;
		public static MethodInfo Evaluate(this MethodSearchParameters methodSearch)
		{
		


			return methodSearch.Type.GetMethods().SingleOrDefault(m => PredicateMethod(m, methodSearch));
			
			
				// return methodSearch.Type.GetMethod(methodSearch.MethodName, genericArgumentCount, arguments);
			
			// return methodSearch.Type.GetMethod(methodSearch.MethodName, arguments);
		}

		private static bool PredicateMethod(MethodInfo methodInfo, MethodSearchParameters methodSearch)
		{
			if (methodSearch.MethodName != methodInfo.Name)
				return false;

			if (methodSearch.IsStatic != methodInfo.IsStatic)
				return false;

			if (methodSearch.IsGenericMethod != methodInfo.IsGenericMethod)
				return false;

			var parameters = methodInfo.GetParameters();
			if (parameters.Length != methodSearch.ParameterCount)
				return false;
			
			var genericArguments = methodInfo.GetGenericArguments();
			if (genericArguments.Length != methodSearch.GenericArgumentCount)
				return false;

			return true;
		}


		public struct PropertySearchParameters
		{
			public Type Type { get; set; }
			public string PropertyName { get; set; }
			public Type FieldType { get; set; }
			public BindingFlags Flags { get; set; }
		}

		public static PropertySearchParameters FindProperty<T>(string propertyName)
		{
			return FindProperty(typeof(T), propertyName);
		}

		public static PropertySearchParameters FindProperty(this Type type, string propertyName)
		{
			return new PropertySearchParameters()
			{
				Type = type,
				PropertyName = propertyName,
				Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
			};
		}

		public static PropertySearchParameters WithType(this PropertySearchParameters property, Type returnType)
		{
			property.FieldType = returnType;
			return property;
		}

		public static PropertySearchParameters WithBindingFlags(this PropertySearchParameters property, BindingFlags flags)
		{
			property.Flags = flags;
			return property;
		}

		public static PropertySearchParameters AsStatic(this PropertySearchParameters property)
		{
			property.Flags |= BindingFlags.Static;
			return property;
		}

		public static PropertyInfo Evaluate(this PropertySearchParameters property)
		{
			if(property.FieldType != null)
				return property.Type.GetProperty(property.PropertyName, property.FieldType);
			return property.Type.GetProperty(property.PropertyName, property.Flags);
		}
		
	}
}