using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions;

namespace WhiteSparrow.Shared.DependencyInjection.Baking
{
	public class InjectionBakingProcessor
	{

		private ReaderParameters Reader;
		private CecilAssemblyResolver m_AssemblyResolver;

		public void Initialize()
		{
			m_AssemblyResolver = new CecilAssemblyResolver();
			Reader  = new Mono.Cecil.ReaderParameters { InMemory = true, AssemblyResolver = m_AssemblyResolver};
		}

		public void Process(string assemblyPath, string outputPath)
		{
			var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, Reader );

			if(!CecilAssemblyUtil.CanProcessAssembly(assembly.Name.Name))
				return;

			var module = assembly.MainModule;
			bool changes = false;
			
			foreach (var type in module.Types)
			{
				bool hasInjectedMembers =
					type.Fields.Any(f => f.Extensions().HasAttribute<InjectAttribute>(true, out _)) ||
					type.Properties.Any(p => p.Extensions().HasAttribute<InjectAttribute>(true, out _));

				if (!hasInjectedMembers)
					continue;

				BakeInjectionIntoType(type);
				changes = true;
			}

			if (changes)
			{
				// if(!Directory.Exists(outputPath))
				// 	Directory.CreateDirectory(outputPath);
				module.Write(outputPath);
			}
			module.Dispose();
		}

	
		private void BakeInjectionIntoType(TypeDefinition type)
		{
			type.Extensions().AddInterface<IInjectedType>();
			
			// Create (or replace) the _Inject method stub
			var method = type.Extensions().CreateMethod("_Inject", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual);


			foreach (var member in type.Fields)
			{
				if (!member.Extensions().HasAttribute<InjectAttribute>(true, out var attribute))
					continue;
				
				method.Extensions().CallStatic(typeof(Debug), nameof(Debug.Log), new object[]{ "Injecting field: " + member.Name });	
			}
			foreach (var member in type.Properties)
			{
				if (!member.Extensions().HasAttribute<InjectAttribute>(true, out var attribute))
					continue;
				if (member.SetMethod == null)
				{
					Debug.Log("Skipping property: " + member.Name + " because it has no setter.");
					continue;
				}
				
				method.Extensions().CallStatic(typeof(Debug), nameof(Debug.Log), new object[]{ "Injecting property: " + member.Name });	
			}
			

			method.Extensions().Return();
			
			
			
			Debug.Log("Adding method: " + method.Name + " to type: " + type.Name);
		}
		
	

	}
}