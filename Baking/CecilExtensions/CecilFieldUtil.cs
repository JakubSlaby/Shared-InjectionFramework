using Mono.Cecil;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilFieldUtil
	{
		public static CecilWrapper<FieldDefinition> Extensions(this FieldDefinition type)
		{
			return new CecilWrapper<FieldDefinition>(type);
		}

		
	}
}