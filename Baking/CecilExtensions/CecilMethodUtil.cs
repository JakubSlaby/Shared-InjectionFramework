using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
			
			TypeReference rtReference = CecilTypeUtil.GetDefaultTypeReference(returnType, typeDefinition.Module);
			MethodDefinition method = new MethodDefinition(name, attributes, rtReference);
			
			typeDefinition.Methods.Add(method);

			return method;
		}


		public static CecilWrapper<MethodDefinition> CallStatic(this CecilWrapper<MethodDefinition> methodWrapper, Type type, string method, object[] parameters)
		{
			var methodDefinition = methodWrapper.Token;
			var module = methodDefinition.Module;
			
			var methodReference = module.ImportReference(type.GetMethod(method, parameters?.Select(p => p.GetType()).ToArray() ?? Array.Empty<Type>()) );
			
			var processor = methodDefinition.Body.GetILProcessor();
			var instructions = new Instruction[parameters.Length + 1];
			
			for (int i = 0; i < parameters.Length; i++)
			{
				if(parameters[i] is string str)
					instructions[i] = processor.Create(OpCodes.Ldstr, str);
			}
			
			instructions[parameters.Length] = processor.Create(OpCodes.Call, methodReference);
			
			foreach (var instruction in instructions)
			{
				processor.Append(instruction);
			}
			
			
			return methodWrapper;
		}
		
		public static CecilWrapper<MethodDefinition> Return(this CecilWrapper<MethodDefinition> methodWrapper)
		{
			var methodDefinition = methodWrapper.Token;
			var processor = methodDefinition.Body.GetILProcessor();
			processor.Emit(OpCodes.Ret);
			return methodWrapper;
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