using System;
using WhiteSparrow.Shared.DependencyInjection.Containers;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Tests.Mock
{
    public static class MockInjectionExtensions
    {
        public static readonly ContextIdentifier MockApplicationContext = "MockApplicationContext";
        
        public static IInjectionContainer MockApplication(this ContextMap map)
        {
            return map.Impl.Get(MockApplicationContext);
        }
		
        public static readonly ContextIdentifier MockNetworkingContext = "MockNetworkingContext";
        
        public static IInjectionContainer MockNetworking(this ContextMap map)
        {
            return map.Impl.Get(MockNetworkingContext);
        }
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectApplicationAttribute : InjectAttribute
    {
        public InjectApplicationAttribute() : base(MockInjectionExtensions.MockApplicationContext)
        {
        }
    }
	
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectNetworkingAttribute : InjectAttribute
    {
        public InjectNetworkingAttribute() : base(MockInjectionExtensions.MockNetworkingContext)
        {
        }
    }
    
    
  
}