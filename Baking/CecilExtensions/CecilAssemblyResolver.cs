using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public class CecilAssemblyResolver : IAssemblyResolver
	{
		private Dictionary<string, AssemblyDefinition> m_AssemblyMapping = new Dictionary<string, AssemblyDefinition>();

		public static AssemblyDefinition Load(string assemblyPath)
		{
			CecilAssemblyResolver resolver = new CecilAssemblyResolver();
			var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, resolver.Reader );
			
			var a = resolver.Resolve(assembly.Name);
			if(a != assembly)
				assembly.Dispose();
			return a;

			return null;
		}

		public ReaderParameters Reader { get; private set; }
		public WriterParameters Writer { get; private set; }
		public CecilAssemblyResolver()
		{
			
			Reader  = new Mono.Cecil.ReaderParameters { InMemory = true, AssemblyResolver = this, ReadSymbols = false};
			Writer = new WriterParameters() { WriteSymbols = false};
		}
		
		public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, Reader);

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			
			if (m_AssemblyMapping.TryGetValue(name.FullName, out var existing))
				return existing;

			var path = GetAssemblyPath(name.Name);
			if (!File.Exists(path))
			{
				string[] assets = AssetDatabase.FindAssets(name.Name);
				foreach (var asset in assets)
				{
					path = AssetDatabase.GUIDToAssetPath(asset);
					if(path.EndsWith(".dll"))
						break;
				}
				
				FileInfo f = new FileInfo(path);
				if(!f.Exists || f.Extension != ".dll")
				{
					Debug.Log($"Assembly {name.FullName} not found");
				}
			}
			
			var assembly = AssemblyDefinition.ReadAssembly(path, parameters);
			m_AssemblyMapping[name.FullName] = assembly;
			return assembly;
		}
		
		public void Dispose()
		{
			foreach (var assembly in m_AssemblyMapping.Values)
			{
				assembly.Dispose();
			}
			m_AssemblyMapping.Clear();
		}

		internal static string GetAssemblyPath(string assemblyName)
		{
			return Path.GetFullPath(Path.Combine(GetLibraryAssembliesPath(), $"{assemblyName}.dll"));
		}

		private static string GetLibraryAssembliesPath() => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/ScriptAssemblies"));

	}
}