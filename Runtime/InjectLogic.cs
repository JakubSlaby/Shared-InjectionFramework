using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Containers;
using Attribute = System.Attribute;
using Type = System.Type;

namespace WhiteSparrow.Shared.DependencyInjection
{
	internal static class InjectLogic
	{
		private static readonly Type AttributeType = typeof(InjectAttribute);
		private static Dictionary<Type, InjectionTypeMapping> s_InjectionTypeMapping = new Dictionary<Type, InjectionTypeMapping>();

		public static void Inject(object instance)
		{
			if (instance == null)
				return;

			Type type = instance.GetType();

			if (!s_InjectionTypeMapping.TryGetValue(type, out var mapping))
			{
				mapping = MapType(type);
				s_InjectionTypeMapping[type] = mapping;
			}

			if (!mapping.HasInjectionTargets)
				return;

			var fields = mapping.Fields;
			foreach (var fieldMappingRecord in fields)
			{
				IInjectionContainer container = Injection.Context.Impl.Get(fieldMappingRecord.Attribute.Context);
				object content = container.Get(fieldMappingRecord.FieldInfo.FieldType);
				if (content != null)
				{
					fieldMappingRecord.FieldInfo.SetValue(instance, content);
				}
				else
				{
					throw new InjectionMissingMappingException(container, fieldMappingRecord.FieldInfo.FieldType, type, "Unable to Inject mapping.");
				}
			}

			var properties = mapping.Properties;
			foreach (var propertyMappingRecord in properties)
			{
				IInjectionContainer container = Injection.Context.Impl.Get(propertyMappingRecord.Attribute.Context);
				object content = container.Get(propertyMappingRecord.PropertyInfo.PropertyType);
				if (content != null)
				{
					propertyMappingRecord.PropertyInfo.SetValue(instance, content);
				}
				else
				{
					throw new InjectionMissingMappingException(container, propertyMappingRecord.PropertyInfo.PropertyType, type, "Unable to Inject mapping.");
				}
			}
		}

		private static List<FieldMappingRecord> s_HelperFieldInfoList = new List<FieldMappingRecord>();
		private static List<PropertyMappingRecord> s_HelperPropertyInfoList = new List<PropertyMappingRecord>();
		private static InjectionTypeMapping MapType(Type type)
		{
			while (type != null)
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                			
                foreach (var field in fields)
                {
                	if (Attribute.IsDefined(field, AttributeType))
                	{
                		s_HelperFieldInfoList.Add(new FieldMappingRecord()
                		{
                			FieldInfo = field,
                			Attribute = (InjectAttribute)Attribute.GetCustomAttribute(field, AttributeType)
                		});
                	}
                }
    
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var property in properties)
                {
                	if (Attribute.IsDefined(property, AttributeType))
                	{
                		s_HelperPropertyInfoList.Add(new PropertyMappingRecord()
                		{
                			PropertyInfo = property,
                			Attribute = (InjectAttribute)Attribute.GetCustomAttribute(property, AttributeType)
                		});
                	}
                }
				
				type = type.BaseType;
			}

			var output = new InjectionTypeMapping()
			{
				Type = type, 
				Fields = s_HelperFieldInfoList.ToArray(), 
				Properties = s_HelperPropertyInfoList.ToArray()
			};
			s_HelperFieldInfoList.Clear();
			s_HelperPropertyInfoList.Clear();
			return output;
		}

		public class InjectionTypeMapping
		{
			public Type Type;
			public FieldMappingRecord[] Fields;
			public PropertyMappingRecord[] Properties;

			public bool HasInjectionTargets => Fields is { Length: > 0 } || Properties is { Length: > 0 };
		}
		
		public class FieldMappingRecord
		{
			public InjectAttribute Attribute;
			public FieldInfo FieldInfo;
		}
			
		public class PropertyMappingRecord
		{
			public InjectAttribute Attribute;
			public PropertyInfo PropertyInfo;
		}
	}
}
