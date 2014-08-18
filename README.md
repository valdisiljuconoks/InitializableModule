InitializableModule
===================

Let's switch to ordinary Asp.Net Mvc application initialization code:

```csharp
[assembly: OwinStartupAttribute(typeof(WebApplication1.Startup))]
namespace WebApplication1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
```

In order to add new steps for initialization you may need to add new method invocation under or above `ConfigureAuth(app)` method:

```csharp
public void Configuration(IAppBuilder app)
{
    ConfigureAuth(app);
    // OtherSetupMethod();
}
```

Then you may need to create new `Startup` class partial fragment that is located under `App_Start` folder just to follow used conventions in default project template.

## Initialization Modules
So I took inspiration EPiServer gave and created a small initialization module library that does exactly what it says - you are able to create a initialization modules which will be called during application start-up or at any your preferred time during app life cycle.

So by using [InitializationModule library](https://github.com/valdisiljuconoks/InitializableModule) you are able to rewrite user authentication setup in default Asp.Net Mvc template with following code.

```csharp
[assembly: OwinStartupAttribute(typeof(WebApplication1.Startup))]
namespace WebApplication1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var process = new ModuleExecutionProcess(app);
            var context = process.Execute();
        }
    }
}
```

## Dependencies between modules
Initialization modules provides built-in support for specifying cross-module dependencies. Let's say we do have following modules:

```csharp
public class Module1 : IInitializableModule
{
    public void Initialize()
    {
    }
}

[ModuleDependency(typeof(Module1))]
public class Module2 : IInitializableModule
{
    public void Initialize()
    {
    }
}

[ModuleDependency(typeof(Module2))]
public class Module3 : IInitializableModule
{
    public void Initialize()
    {
    }
}

[ModuleDependency(typeof(Module2))]
public class Module4 : IInitializableModule
{
    public void Initialize()
    {
    }
}

[ModuleDependency(typeof(Module4))]
public class Module5 : IInitializableModule
{
    public void Initialize()
    {
    }
}
```

Using `ModuleDependency` attribute you can specify that one module is dependent on others' module execution result, e.g., `Module3` will not be executed before `Module2`, `Module5` before `Module4`, etc.

Graphically this looks like this:

![](https://ilke7g.bn1304.livefilestore.com/y2pjXgkGnj4m5T_Ug9KXIVa0G6icdwVkFDh90demJJxDABI0meHp4h2ILjIJg6oXkRHYHvfK4fbEFd-1MbPe7ZAhhJBzZk-GpQHqC6J3_ZurVQ/depend.PNG)

Engine takes care of proper execution order for the dependent modules. However - it's defined which module will be executed first - module `Module3` or module `Module4`. If you need to define a specific execution order for the modules on "the same level" - probably they are not on the same level - somebody has dependency on other.

## Module resolution
Initialization modules sometimes may depend not only on other modules but they may require something from outer world as it's an ordinary case in enterprise applications - some dependencies injected.

It's possible to provide module resolution function to [InitializationModule library](https://github.com/valdisiljuconoks/InitializableModule) in order to specify how modules are resolved in [RRR](http://blog.ploeh.dk/2010/09/29/TheRegisterResolveReleasepattern/) cycle.

Let's say in this particular case we are using StructureMap DI container (concrete implementation of container does not play a role in this case as framework is container implementation agnostic - power of `System.Func`).

```csharp
var process = new ModuleExecutionProcess(ObjectFactory.Container.GetInstance);
var context = process.Execute();
```

In this case every discovered module will be resolved using specified function (`Func<Type, object>`).
