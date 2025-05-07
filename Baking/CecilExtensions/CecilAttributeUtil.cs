using System;
using Mono.Cecil;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilAttributeUtil
	{
		public static bool HasAttribute<T>(this ICecilWrapperWithCustomAttributes wrapper, bool includeSubclasses, out CustomAttribute attribute)
		{
			return HasAttribute(wrapper, typeof(T), includeSubclasses, out attribute);
		}
		
		public static bool HasAttribute(this ICecilWrapperWithCustomAttributes wrapper, Type attributeType, bool includeSubclasses, out CustomAttribute attribute)
		{
			if (wrapper.Token.CustomAttributes == null)
			{
				attribute = null;
				return false;
			}
			
			TypeDefinition attrType = wrapper.Module.ImportReference(attributeType).Resolve();
			
			foreach (var attr in wrapper.Token.CustomAttributes)
			{
				if (!CecilAssemblyUtil.CanProcessSourceAssembly(attr.AttributeType.Scope.Name))
					continue;
				if (!includeSubclasses)
				{
					if (attr.AttributeType.FullName == attrType.FullName)
					{
						attribute = attr;
						return true;
					}

					continue;
				}

				if (CecilTypeUtil.IsTypeOrSubtype(attr.AttributeType, attrType))
				{
					attribute = attr;
					return true;
				}
			}

			attribute = null;
			return false;
		}
		
	}
}