using System;
using System.Collections.Generic;
using System.Linq;
using Owin;
using StructureMap;
using Xunit;

namespace TechFellow.InitializableModule.Tests
{
    public class ModuleDependenciesTests
    {
        private readonly IContainer _container;

        public ModuleDependenciesTests()
        {
            this._container = ObjectFactory.Container;
        }

        [Fact]
        public void DependencyTheSameModuleTest()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new ModuleDescriptor(new Module1(), typeof(Module1), new[] { typeof(Module1) }));
        }

        [Fact]
        public void GetTypeAttributes()
        {
            var type = typeof(Module2);
            var attr = TypeHelper.GetAttributes<ModuleDependencyAttribute>(type).ToList();

            Assert.NotEmpty(attr);
            Assert.Equal(1, attr.Count());
            Assert.Equal(typeof(ModuleDependencyAttribute), attr.First().GetType());
        }

        [Fact]
        public void ModuleExecutionTests()
        {
            var modules = new List<Type> { typeof(Module2), typeof(Module1), typeof(Module5), typeof(Module3), typeof(Module4) };

            var modulesWithDep =
                modules.Select(
                               m =>
                               new ModuleDescriptor(this._container.GetInstance(m) as IInitializableModule,
                                                    m,
                                                    TypeHelper.GetAttributes<ModuleDependencyAttribute>(m).Select(d => d.DependencyModule))).ToList();

            Assert.NotNull(modulesWithDep);

            var engine = new ModuleExecutionEngine();
            var weighter = new ModuleWeighter();

            var context = engine.RunModules(weighter.SortModules(modulesWithDep), new ModuleExecutionContext());
            Assert.NotEmpty(context.Log);
        }

        [Fact]
        public void ModuleExecutionTests_ClientApi()
        {
            var process = new ModuleExecutionProcess(this._container.GetInstance);
            var context = process.Execute();

            Assert.NotNull(context.Log);
        }

        [Fact]
        public void ModuleExecutionTests_WithActivator()
        {
            var process = new ModuleExecutionProcess();
            var context = process.Execute();

            Assert.NotNull(context.Log);
        }

        [Fact]
        public void ModuleExecutionTests_WithAppBuilder()
        {
            var process = new ModuleExecutionProcess(new AppBuilder());
            process.SetModulesTypes(new[] { typeof(Module10) });
            var context = process.Execute();

            Assert.NotNull(context.Log);
        }

        [Fact]
        public void WeighterTests_2Levels()
        {
            var modules = new List<Type> { typeof(Module2), typeof(Module1), typeof(Module3) };
            var weighter = new ModuleWeighter();
            var modulesWithDep =
                modules.Select(
                               m =>
                               new ModuleDescriptor(this._container.GetInstance(m) as IInitializableModule,
                                                    m,
                                                    TypeHelper.GetAttributes<ModuleDependencyAttribute>(m).Select(d => d.DependencyModule))).ToList();

            var executableModules = weighter.SortModules(modulesWithDep).ToList();

            Assert.Equal(2, executableModules.Single(md => md.ModuleType == typeof(Module1)).Weight);
            Assert.Equal(1, executableModules.Single(md => md.ModuleType == typeof(Module2)).Weight);
        }

        [Fact]
        public void WeighterTests_4Levels()
        {
            var modules = new List<Type> { typeof(Module2), typeof(Module1), typeof(Module3), typeof(Module5), typeof(Module4) };
            var weighter = new ModuleWeighter();
            var modulesWithDep =
                modules.Select(
                               m =>
                               new ModuleDescriptor(this._container.GetInstance(m) as IInitializableModule,
                                                    m,
                                                    TypeHelper.GetAttributes<ModuleDependencyAttribute>(m).Select(d => d.DependencyModule))).ToList();

            var executableModules = weighter.SortModules(modulesWithDep).ToList();

            Assert.Equal(4, executableModules.Single(md => md.ModuleType == typeof(Module1)).Weight);
            Assert.Equal(3, executableModules.Single(md => md.ModuleType == typeof(Module2)).Weight);
            Assert.Equal(0, executableModules.Single(md => md.ModuleType == typeof(Module3)).Weight);
            Assert.Equal(1, executableModules.Single(md => md.ModuleType == typeof(Module4)).Weight);
            Assert.Equal(0, executableModules.Single(md => md.ModuleType == typeof(Module5)).Weight);
        }
    }

    public class AppBuilder : IAppBuilder
    {
        public IAppBuilder Use(object middleware, params object[] args)
        {
            throw new NotImplementedException();
        }

        public object Build(Type returnType)
        {
            throw new NotImplementedException();
        }

        public IAppBuilder New()
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Properties { get; private set; }
    }

    public class Module1 : IInitializableModule
    {
        public void Initialize()
        {
        }

        public void Release()
        {
        }
    }

    [ModuleDependency(typeof(Module1))]
    public class Module2 : IInitializableModule
    {
        public void Initialize()
        {
        }

        public void Release()
        {
        }
    }

    [ModuleDependency(typeof(Module2))]
    public class Module3 : IInitializableModule
    {
        public void Initialize()
        {
        }

        public void Release()
        {
        }
    }

    [ModuleDependency(typeof(Module2))]
    public class Module4 : IInitializableModule
    {
        public void Initialize()
        {
        }

        public void Release()
        {
        }
    }

    [ModuleDependency(typeof(Module4))]
    public class Module5 : IInitializableModule
    {
        public void Initialize()
        {
        }

        public void Release()
        {
        }
    }

    public class Module10 : IInitializableModule
    {
        public Module10(IAppBuilder app)
        {
        }

        public void Initialize()
        {
        }

        public void Release()
        {
        }
    }
}
