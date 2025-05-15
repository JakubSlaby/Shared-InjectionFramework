using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace WhiteSparrow.Shared.DependencyInjection.Baking.CecilExtensions
{
	public static class CecilMethodUtil
	{
		public static CecilWrapper<MethodDefinition> Extensions(this MethodDefinition method)
		{
			return new CecilWrapper<MethodDefinition>(method);
		}
		
		public static MethodDefinition CreateMethod(this CecilWrapper<TypeDefinition> typeWrapper, string name, MethodAttributes attributes)
		{
			return CreateMethod(typeWrapper, name, typeof(void), attributes);
		}

		public static MethodDefinition CreateMethod(this CecilWrapper<TypeDefinition> typeWrapper, string name, Type returnType, MethodAttributes attributes)
		{
			var typeDefinition = typeWrapper.Token;

			var existingMethod = typeDefinition.Methods.FirstOrDefault(m => m.Name == name);
			if (existingMethod != null)
			{
				Debug.LogWarning($"Found existing method {typeDefinition.FullName}::{name}");
				return existingMethod;
			}
			
			TypeReference rtReference = CecilTypeUtil.GetDefaultTypeReference(returnType, typeDefinition.Module);
			MethodDefinition method = new MethodDefinition(name, attributes, rtReference);
			
			typeDefinition.Methods.Add(method);

			return method;
		}


		public static CecilWrapper<MethodDefinition> This(this CecilWrapper<MethodDefinition> methodWrapper)
		{
			var methodDefinition = methodWrapper.Token;
			var module = methodDefinition.Module;

			var processor = methodDefinition.Body.GetILProcessor();
			processor.Append(processor.Create(OpCodes.Ldarg_0));
			
			
			return methodWrapper;
		}
		
		public static CecilWrapper<MethodDefinition> CallMethod(this CecilWrapper<MethodDefinition> methodWrapper, CecilReflectionHelper.MethodSearchParameters search)
		{
			return CallMethodInternal(methodWrapper, search, null);
		}
		
		public static CecilWrapper<MethodDefinition> CallMethod(this CecilWrapper<MethodDefinition> methodWrapper, CecilReflectionHelper.MethodSearchParameters search, params object[] parameters)
		{
			return CallMethodInternal(methodWrapper, search, parameters);
		}
		

		private static CecilWrapper<MethodDefinition> CallMethodInternal(this CecilWrapper<MethodDefinition> methodWrapper, CecilReflectionHelper.MethodSearchParameters search, object[] parameters)
		{
			var methodDefinition = methodWrapper.Token;
			var module = methodDefinition.Module;
			
			var methodInfo = search.Evaluate();
			// if(search.GenericArgumentTypes != null && search.GenericArgumentTypes.Length > 0)
			// 	methodInfo = methodInfo.MakeGenericMethod(search.GenericArgumentTypes);

			if (methodInfo.IsGenericMethod)
			{
				
			}
			
			var methodReference = module.ImportReference(methodInfo);
			
			
			
			var processor = methodDefinition.Body.GetILProcessor();
			int len = (parameters?.Length ?? 0) + 1;
			var instructions = new Instruction[len];
			
			for (int i = 0; parameters != null && i < parameters.Length; i++)
			{
				if(parameters[i] is string str)
					instructions[i] = processor.Create(OpCodes.Ldstr, str);
				if(parameters[i] is FieldDefinition fldf)
					instructions[i] = processor.Create(OpCodes.Ldsfld, fldf);
				if(parameters[i] is TypeReference tref)
					instructions[i] = processor.Create(OpCodes.Ldtoken, tref);

				MethodDefinition md = null;
				if (parameters[i] is PropertyDefinition prdf)
					md = prdf.GetMethod;
				

				if (parameters[i] is MethodDefinition mdf)
					md = mdf;

				if (md != null)
				{
					if(md.IsStatic)
						instructions[i] = processor.Create(OpCodes.Call, md);
					else
						instructions[i] = processor.Create(OpCodes.Callvirt, md);
				}					

			}
			
			OpCode callCode = OpCodes.Callvirt;
			if (methodInfo.IsStatic)
				callCode = OpCodes.Call;
			
			GenericInstanceMethod genericInstanceMethod = null;
			if (methodInfo.IsGenericMethod)
			{
				genericInstanceMethod = new GenericInstanceMethod(methodReference);
				for (int i = 0; i < search.GenericArgumentTypes.Length; i++)
				{
					var t = search.GenericArgumentTypes[i];
					var it = module.ImportReference(t);
					genericInstanceMethod.GenericArguments.Add(it);
				}
				instructions[^1] = processor.Create(callCode, genericInstanceMethod);
			}
			else
			{
				instructions[^1] = processor.Create(callCode, methodReference);
			}

			foreach (var instruction in instructions)
			{
				processor.Append(instruction);
			}
			
			
			return methodWrapper;
		}

		public static CecilWrapper<MethodDefinition> CallPropertyGet(this CecilWrapper<MethodDefinition> methodWrapper, CecilReflectionHelper.PropertySearchParameters search)
		{
			var methodDefinition = methodWrapper.Token;
			var module           = methodDefinition.Module;
			
			var propInfo     = search.Evaluate();
			if (propInfo == null)
				throw new ArgumentException($"Static property '{search.PropertyName}' not found on {search.Type.FullName}.");

			var getter        = propInfo.GetGetMethod(true);
			var getterRef     = module.ImportReference(getter);

			// Build IL
			var processor     = methodDefinition.Body.GetILProcessor();
			
			
			OpCode callCode = OpCodes.Callvirt;
			if (search.Flags.HasFlag(BindingFlags.Static))
				callCode = OpCodes.Call;
			// Call the getter
			processor.Append(processor.Create(callCode, getterRef));

			return methodWrapper;
		}

		public static CecilWrapper<MethodDefinition> CallPropertySet(this CecilWrapper<MethodDefinition> methodWrapper, PropertyDefinition property)
		{
			var methodDefinition = methodWrapper.Token;
			var module           = methodDefinition.Module;

			var setterRef = module.ImportReference(property.SetMethod);
			// Build IL
			var processor     = methodDefinition.Body.GetILProcessor();
			
			// Call the getter
			processor.Append(processor.Create(OpCodes.Callvirt, setterRef));
			return methodWrapper;
		}
		public static CecilWrapper<MethodDefinition> CallPropertySet(this CecilWrapper<MethodDefinition> methodWrapper, CecilReflectionHelper.PropertySearchParameters search)
		{
			var methodDefinition = methodWrapper.Token;
			var module           = methodDefinition.Module;
			
			var propInfo     = search.Evaluate();
			if (propInfo == null)
				throw new ArgumentException($"Static property '{search.PropertyName}' not found on {search.Type.FullName}.");

			var setter        = propInfo.GetSetMethod(true);
			var setterRef     = module.ImportReference(setter);

			// Build IL
			var processor     = methodDefinition.Body.GetILProcessor();
			
			OpCode callCode = OpCodes.Callvirt;
			if (search.Flags.HasFlag(BindingFlags.Static))
				callCode = OpCodes.Call;
			// Call the getter
			processor.Append(processor.Create(callCode, setterRef));

			return methodWrapper;
		}

		public static CecilWrapper<MethodDefinition> Nop(this CecilWrapper<MethodDefinition> methodWrapper)
		{
			var methodDefinition = methodWrapper.Token;
			var processor = methodDefinition.Body.GetILProcessor();
			processor.Emit(OpCodes.Nop);
			return methodWrapper;
		}
		
		public static CecilWrapper<MethodDefinition> Return(this CecilWrapper<MethodDefinition> methodWrapper)
		{
			var methodDefinition = methodWrapper.Token;
			var processor = methodDefinition.Body.GetILProcessor();
			processor.Emit(OpCodes.Ret);
			return methodWrapper;
		}

		public static string InstructionsToString(this CecilWrapper<MethodDefinition> methodWrapper)
		{
			StringBuilder sb = new StringBuilder();
			var methodDefinition = methodWrapper.Token;
			var processor = methodDefinition.Body.GetILProcessor();
			foreach (var instruction in processor.Body.Instructions)
			{
				sb.AppendLine(instruction.ToString());
			}

			return sb.ToString();
		}
	}

	public struct CecilWrapper<T> : ICecilWrapperWithCustomAttributes
		where T : MemberReference, ICustomAttributeProvider
	{
		public T Token { get; private set; }
		
		MemberReference ICecilWrapper.Token => this.Token;
		ICustomAttributeProvider ICecilWrapperWithCustomAttributes.Token => this.Token;
		
		public CecilWrapper(T target)
		{
			Token = target;
		}
	}

	public interface ICecilWrapper
	{
		MemberReference Token { get; }
		ModuleDefinition Module => Token.Module;
	}
	public interface ICecilWrapperWithCustomAttributes : ICecilWrapper
	{
		new ICustomAttributeProvider Token { get; }
	}
}