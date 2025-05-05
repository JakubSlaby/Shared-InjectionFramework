using System;
using System.Linq;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilAssemblyUtil
	{
		private static readonly string[] SkipAssemblyPrefixes = new[]
		{
			"Unity.",
			"UnityEngine.",
			"UnityEditor.",
			"System.",
			"Microsoft.",
			"Mono.",
			"mscorlib",
			"netstandard",
			"nunit.",
			"Mono.Cecil",
			"WhiteSparrow.Injection",
		};
		
		public static bool CanProcessAssembly(string assemblyName)
		{
			return !SkipAssemblyPrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}
		
		private static readonly string[] SkipSourceAssemblyPrefixes = new[]
		{
			"Unity.",
			"UnityEngine.",
			"UnityEditor.",
			"System.",
			"Microsoft.",
			"Mono.",
			"mscorlib",
			"netstandard",
			"nunit.",
			"Mono.Cecil"
		};
		
		public static bool CanProcessSourceAssembly(string assemblyName)
		{
			return !SkipSourceAssemblyPrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		}
	}
}