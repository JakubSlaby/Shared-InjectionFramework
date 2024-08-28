# InjectionFramework
Simple Injection framework allowing to map Type based instances to custom contexts.

> [!NOTE]
> This is a very simple implementation of DI for the minimal required functionality, you can call this approach a "glorified singleton dictionary" if you want.

> [!WARNING]
> This system currently uses Reflection to process automatic injection through attributes. This will be optimised for AOT code generation in the future.

## Defining a Context

The context mapping works based of creating a few things
- Context identifier const string
- Extension method to easily access your context
- Custom attribute to easily point to your context for injection

```csharp
public static class CustomInjection
{
    public static readonly ContextIdentifier GlobalContext = "GlobalContext";
    
    public static IInjectionContainer Global(this ContextMap map)
    {
        return map.Impl.Get(GlobalContext);
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class InjectGlobalAttribute : InjectAttribute
{
    public InjectGlobalAttribute() : base(CustomInjection.GlobalContext)
    {
    }
}
```

## Using the context

### Context Access 

To access our newly created context you can use `Injection.Context` property which we just extended to retrieve an instance of the context `IInjectionContainer`.

> [!TIP]
> `IInjectionContainer` is responsible for mapping all the instances you want to be able to inject.

### Context Initialise

Before we want to start using a context we need to initialise it.

```csharp
Injection.Context.Global().Create();
```

This process has been made deliberate due to the fact you can destroy contexts and it's easier to catch issues when these operations not executed automatically.

### Mapping Instances

You can map any instance to any desired Type, you can also use Interfaces, abstract and base types.
```csharp
var context = Injection.Context.Global();

context.Map<NetworkManager>(GameObject.FindFirstObjectByType<NetworkManager>());
context.Map<WorldDatabase>(new WorldDatabase());
context.Map<ICharacterSettings>(new DefaultCharacterSettings());
```


### Injection

Default usage is through the injection attributes and a `this.Inject()` extension call.

```csharp
namespace Scripts_World
{
    public class GameLogic : MonoBehaviour
    {
        [InjectGlobal] 
        private NetworkManager m_NetworkManager;
        
        [InjectGlobal] 
        private WorldDatabase m_WorldDatabase;

        [InjectGlobal] 
        private ICharacterSettings m_CharacterSettings;
        
        private void Awake()
        {
            this.Inject();
        }

        private void Update()
        {
            // game logic
        }
    }
}
```
After calling `this.Inject()` the Fields and Properties marked with `[InjectGlobal]` attribute will be populated with instances mapped to their respective types.

### Injecting from a specific container
```csharp

namespace Scripts_World
{
    public class GameLogic
    {
        [Inject]
        private NetworkManager m_NetworkManager;

        [Inject]
        private WorldDatabase m_WorldDatabase;

        [Inject]
        private ICharacterSettings m_CharacterSettings;
    }
    
    public class Factory
    {
        public GameLogic Make()
        {
            GameLogic gl = new GameLogic();
            // option 1
            Injection.Context.Global().InjectInto(gl);
            // option 2
            gl.Inject(map => map.GLobal());
            return gl;
        }
    }
}
```
There are two options to inject from a specific container (and it's parents)

Option 1: If you have the Container reference you can just call `IInjectionContainer.InjectInto(target)` providing your target.
Option 2: Use the extension method overload which provides you the ContextMap from which you can fetch the desired target context.

#### Direct access
To access a mapping directly you can fetch it from the context `IInjectionContainer`.

> [!TIP]
> This method is recommended for accessing mapped instances from static/utility methods when caching the output is not recommended or when using `this.Inject` which utilises Reflection would not be desired.

```csharp
NetworkManager nm = Injection.Context.Global().Get<NetworkManager>();
```

### Context Destruction

You might be in a situation you'd want to destroy the instance of the context, either as it becomes obsolete or you're reloading a given module - to do that just call.

```csharp
Injection.Context.Global().Destroy();
```

You can also destroy all contexts by calling
```csharp
Injection.Context.Impl.DestroyAll();
```

### Unit Testing
You can use the Injection Framework in both Editor and Play mode unit tests.

> [!WARNING]
> The system automatically destroys all contexts when changing play mode state but you will have to handle cleanup between tests yourself.