using System;
using System.Text;
using WhiteSparrow.Shared.DependencyInjection.Containers;

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

		
		private static readonly StringBuilder s_HelperStringBuilder = new StringBuilder();

		private static string BuildMessage(InjectionContainer container, Type type, string message)
		{
			if(s_HelperStringBuilder.Length > 0)
				s_HelperStringBuilder.Clear();

			if (!string.IsNullOrEmpty(message))
			{
				s_HelperStringBuilder.Insert(0, message);
				s_HelperStringBuilder.Append(' ');
			}
			s_HelperStringBuilder.Append($"Container '{container.name}' already has an instance mapped for Type={type.FullName}.");

			string output = s_HelperStringBuilder.ToString();
			s_HelperStringBuilder.Clear();
			return output;
		}
	}

	public class InjectionMissingMappingException : Exception
	{
			
		public InjectionMissingMappingException(IInjectionContainer container, Type type) : this(container, type, null, null)
		{
			
		}
		
		public InjectionMissingMappingException(IInjectionContainer container, Type type, string message) : base(BuildMessage(container, type, null, message))
		{
			
		}
		
		public InjectionMissingMappingException(IInjectionContainer container, Type type, Type target, string message) : base(BuildMessage(container, type, target, message))
		{
			
		}
	
		
		private static StringBuilder s_HelperStringBuilder = new StringBuilder();
		
		private static string BuildMessage(IInjectionContainer container, Type type, Type target, string message)
		{
			if(s_HelperStringBuilder.Length > 0)
				s_HelperStringBuilder.Clear();

			if (!string.IsNullOrEmpty(message))
			{
				s_HelperStringBuilder.Insert(0, message);
				s_HelperStringBuilder.Append(' ');
			}

			s_HelperStringBuilder.Append($"Container '{container.name}' does not have a mapping for Type={type.FullName}.");
			if(target != null)
				s_HelperStringBuilder.Append($" Attempting to inject to type {target.FullName}.");
			
			return s_HelperStringBuilder.ToString();
		}
	}

	public class BindingAlreadyHasInstanceException : Exception
	{
		public BindingAlreadyHasInstanceException(IInstanceBinding instanceBinding, string message) : base(message)
		{
			
		}
	}
	public class BindingDisposingUnmanagedInstanceException : Exception
	{
		public BindingDisposingUnmanagedInstanceException(IInstanceBinding instanceBinding, string message) : base(message)
		{
			
		}
	}
}