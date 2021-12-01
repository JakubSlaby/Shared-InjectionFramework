using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public static class Injection
	{
		private static readonly Type ObjectType = typeof(object);
		private static readonly Type AttributeType = typeof(InjectAttribute);

		private static InjectionContainer[] s_ContainersMap = new InjectionContainer[8];

		public static void Inject(this object instance)
		{
			if (instance == null || !Application.isPlaying)
				return;
			
			Type type = instance.GetType();

			while (type != null && !type.IsPrimitive && type.IsClass && type != ObjectType)
			{
				FieldInfo[] fields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				FieldInfo field = null;
				for (int i = 0; i < fields.Length; i++)
				{
					field = fields [i];
					if (Attribute.IsDefined(field, AttributeType))
					{
						InjectAttribute attribute = (InjectAttribute) Attribute.GetCustomAttribute(field, AttributeType);

						InjectionContainer container = GetContainer(attribute.Context);
						object obj = container.GetMapping(field.FieldType);
						if (obj != null)
						{
							field.SetValue(instance, obj);
						}
						else
						{
							throw new InjectionMissingMappingException(container, type, "Unable to Inject mapping.");
						}
					}
				}
				
				type = type.BaseType;
			}
		}

		public static InjectionContainer GetContainer(int context)
		{
			if (s_ContainersMap.Length <= context)
			{
				var newArray = new InjectionContainer[context + 1];
				Array.Copy(s_ContainersMap, newArray, s_ContainersMap.Length);
				s_ContainersMap = newArray;
			}

			var container = s_ContainersMap[context];
			if (container != null)
				return container;

			GameObject containerObject = new GameObject($"Injection Container ({context.ToString()})");
			container = containerObject.AddComponent<InjectionContainer>();

			s_ContainersMap[context] = container;

			return container;
		}

		public static InjectionContainer GetContainer<TContext>(TContext context) where TContext : struct, IConvertible
		{
			return GetContainer(context.ToInt32(null));
		}

		public static InjectionContainer Map<T>(object context, T instance)
		{
			if(context is IConvertible convertible)
				return Map<T>(convertible.ToInt32(null), instance);
			return null;
		}

		public static InjectionContainer Map<T>(int context, T instance)
		{
			InjectionContainer container = GetContainer(context);
			container.Map<T>(instance);
			return container;
		}

		public static T Map<T, TContext>(TContext context) where TContext : struct, IConvertible
			where T : Object
		{
			return Map<T>(context.ToInt32(null));
		}
		
		public static T Map<T>(int context)
		where T : Object
		{
			InjectionContainer container = GetContainer(context);
			return container.Map<T>();
		}

		public static T Get<T>(object context)
		{
			if(context is IConvertible convertible)
				return Get<T>(convertible.ToInt32(null));
			
			return default(T);
		}
		public static T Get<T>(int context)
		{
			InjectionContainer container = GetContainer(context);
			return container.GetMapping<T>();
		}

		public static void Clean<TContext>(TContext context) where TContext : struct, IConvertible
		{
			Clean(context.ToInt32(null));
		}
		public static void Clean(int context)
		{
			if (s_ContainersMap.Length > context && s_ContainersMap[context] != null)
			{
				GameObject.Destroy(s_ContainersMap[context].gameObject);
				s_ContainersMap[context] = null;
			}
		}

	}
}