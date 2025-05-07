using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Baking
{
	public class InjectionBakingProcessor
	{

		private ReaderParameters Reader;
		private CecilAssemblyResolver m_AssemblyResolver;

		private Type m_TypeInjectAttribute;

		public void Initialize()
		{
			m_AssemblyResolver = new CecilAssemblyResolver();
			Reader  = new Mono.Cecil.ReaderParameters { InMemory = true, AssemblyResolver = m_AssemblyResolver};
			
			m_TypeInjectAttribute = typeof(InjectAttribute);
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
				if (!member.Extensions().HasAttribute<InjectAttribute>(true, out var attr))
					continue;
				
				FieldDefinition resolvedContextField = ExtractInjectionContext(attr);
				
				if(resolvedContextField != null)
					method.Extensions().CallStatic(typeof(Debug), nameof(Debug.Log), new object[]{ resolvedContextField });	
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
				
				method.Extensions().CallStatic(typeof(Debug), nameof(Debug.Log), new object[]{ "Injecting property: " + member.Name + " context: " + attribute.Fields });	
			}
			

			method.Extensions().Return();
			
			
			
			Debug.Log("Adding method: " + method.Name + " to type: " + type.Name);
		}

		
		private FieldDefinition ExtractInjectionContext(CustomAttribute customAttribute)
		{
			var type = customAttribute.AttributeType.Resolve();
			if (type.FullName == m_TypeInjectAttribute.FullName)
				return null;

			var baseType = type.BaseType.Resolve();
			if (baseType.FullName != m_TypeInjectAttribute.FullName)
				return null;
			
			MethodDefinition def = customAttribute.Constructor.Resolve();
			var instructions = def.Body.Instructions;

			if (instructions[0].OpCode.Code != Code.Ldarg_0)
				return null;
			
			int n = 0;
			for (n = 1; n < instructions.Count; n++)
			{
				var instruction = instructions[n];
				if (instruction.OpCode.Code != Code.Call)
					continue;

				if (instruction.Operand is MethodReference methodReference == false)
					return null;

				var methodDefinition = methodReference.Resolve();

				if (methodDefinition.DeclaringType.FullName != m_TypeInjectAttribute.FullName)
					return null;
				if (methodDefinition.Name != ".ctor")
					return null;

				break;
			}

			if (instructions[n - 1].Operand is FieldDefinition fieldDefinition)
				return fieldDefinition;
			
			return null;
		}

	}
}