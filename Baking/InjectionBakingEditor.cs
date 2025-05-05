using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions;

namespace WhiteSparrow.Shared.DependencyInjection.Baking
{
	public static class InjectionBakingEditor
	{
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
		}




		private static InjectionBakingProcessor s_StaticProcessor;
		private static void OnAssemblyCompilationFinished(string path, CompilerMessage[] messages)
		{
			if (s_StaticProcessor == null)
			{
				s_StaticProcessor = new InjectionBakingProcessor();
				s_StaticProcessor.Initialize();
			}
			
			s_StaticProcessor.Process(path, path);
		}

	}
}