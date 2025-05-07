using System;
using System.Collections.Generic;

namespace WhiteSparrow.Shared.DependencyInjection.Context
{
	public class ContextIdentifier : IComparable<ContextIdentifier>, IEquatable<ContextIdentifier>
	{
		public readonly string Name;
		protected ContextIdentifier(string name)
		{
			this.Name = name;
		}
		
		public static implicit operator ContextIdentifier(string input)
		{
			return Get(input);
		}
		
		public static implicit operator ContextIdentifier(int intContext)
		{
			return Get(intContext);
		}

		public static implicit operator ContextIdentifier(Enum enumContext)
		{
			return Get(enumContext);
		}

		public static ContextIdentifier FromObject(object objectContext)
		{
			return Get(objectContext);
		}

#region Static Registry

		private static Dictionary<object, ContextIdentifier> s_TargetToContext = new Dictionary<object, ContextIdentifier>();

		private static ContextIdentifier Get(object target)
		{
			if (target is ContextIdentifier contextIdentifier)
				return contextIdentifier;
			
			if (s_TargetToContext.TryGetValue(target, out var context))
				return context;

			context = new ContextIdentifier(target.ToString());
			s_TargetToContext[target] = context;
			return context;
		}

#endregion

		public int CompareTo(ContextIdentifier other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (other is null) return 1;
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}


		public bool Equals(ContextIdentifier other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			return Name == other.Name;
		}

		public override bool Equals(object obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ContextIdentifier)obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return $"({this.GetType().Name}) {Name}";
		}
	}

	public class AbstractInjectionContext
	{
		
	}
}