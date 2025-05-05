using Mono.Cecil;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilPropertyUtil
	{
		public static CecilWrapper<PropertyDefinition> Extensions(this PropertyDefinition type)
		{
			return new CecilWrapper<PropertyDefinition>(type);
		}
	}
}