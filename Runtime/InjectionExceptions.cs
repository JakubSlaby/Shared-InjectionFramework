using System;
using System.Text;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public class InjectionMappingAlreadyExistsException : Exception
	{
		public InjectionMappingAlreadyExistsException(InjectionContainer container, Type type) : this(container, type, null)
		{
			
		}

		
		public InjectionMappingAlreadyExistsException(InjectionContainer container, Type type, string message) : base (BuildMessage(container, type, message))
		{
			
		}

		
		private static StringBuilder sb = new StringBuilder();

		private static string BuildMessage(InjectionContainer container, Type type, string message)
		{
			if(sb.Length > 0)
				sb.Clear();

			if (!string.IsNullOrEmpty(message))
			{
				sb.Insert(0, message);
				sb.Append(' ');
			}
			sb.Append($"Container '{container.name}' already has an instance mapped for Type={type.FullName}.");
			
			return sb.ToString();
		}
	}

	public class InjectionMissingMappingException : Exception
	{
			
		public InjectionMissingMappingException(InjectionContainer container, Type type) : this(container, type, null)
		{
			
		}
		
		public InjectionMissingMappingException(InjectionContainer container, Type type, string message) : base(BuildMessage(container, type, message))
		{
			
		}
	
		
		private static StringBuilder sb = new StringBuilder();
		
		private static string BuildMessage(InjectionContainer container, Type type, string message)
		{
			if(sb.Length > 0)
				sb.Clear();

			if (!string.IsNullOrEmpty(message))
			{
				sb.Insert(0, message);
				sb.Append(' ');
			}

			sb.Append($"Container '{container.name}' does not have a mapping for Type={type.FullName}.");
			
			return sb.ToString();
		}
	}
}