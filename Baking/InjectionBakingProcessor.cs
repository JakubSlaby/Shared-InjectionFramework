using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions;
using WhiteSparrow.Shared.DependencyInjection.Containers;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Baking
{
	public class InjectionBakingProcessor
	{

		private CecilAssemblyResolver m_AssemblyResolver;

		private Type m_TypeInjectAttribute;

		public void Initialize()
		{
			m_AssemblyResolver = new CecilAssemblyResolver();
			
			m_TypeInjectAttribute = typeof(InjectAttribute);
		}

		public void Process(string assemblyPath, string outputPath)
		{
			var assembly = CecilAssemblyResolver.Load(assemblyPath );

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
				module.Write(outputPath, m_AssemblyResolver.Writer);
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
				
				MemberReference resolvedContextField = ExtractInjectionContext(attr);
				
				if(resolvedContextField != null)
					method.Extensions().CallMethod(typeof(Debug).FindMethod(nameof(Debug.Log)).AsStatic().WithArguments(typeof(string)), resolvedContextField);
			}
			foreach (var member in type.Properties)
			{
				if (!member.Extensions().HasAttribute<InjectAttribute>(true, out var attr))
					continue;
				if (member.SetMethod == null)
				{
					Debug.Log("Skipping property: " + member.Name + " because it has no setter.");
					continue;
				}
				
				MemberReference resolvedContextField = ExtractInjectionContext(attr);
				TypeDefinition memberType = member.PropertyType.Resolve();
				

				if(resolvedContextField != null)
					method.Extensions().CallMethod(typeof(Debug).FindMethod(nameof(Debug.Log)).AsStatic().WithArguments(typeof(string)), resolvedContextField);

				method.Extensions()
					.This()
					.CallPropertyGet(typeof(Injection).FindProperty(nameof(Injection.Context)).AsStatic())
					.CallPropertyGet(typeof(ContextMap).FindProperty(nameof(ContextMap.Impl)))
					.CallMethod(typeof(IContextMap).FindMethod(nameof(IContextMap.Get)).WithArguments(typeof(ContextIdentifier)), resolvedContextField)
					.CallMethod(typeof(IInjectionContainer).FindMethod(nameof(IInjectionContainer.Get)).WithGenericArguments(memberType.Extensions().ResolveType()))
					.CallPropertySet(member)
					.Nop()
					;
			}
			
			method.Extensions().Return();

			
			
			Debug.Log("Adding method: " + method.Name + " to type: " + type.Name + "\n" + method.Extensions().InstructionsToString());
		}

		
		private MemberReference ExtractInjectionContext(CustomAttribute customAttribute)
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
					continue;
				if (methodDefinition.Name != ".ctor")
					return null;

				break;
			}

			if (instructions[n - 1].Operand is MemberReference fieldDefinition)
				return fieldDefinition;
			
			return null;
		}

	}
}