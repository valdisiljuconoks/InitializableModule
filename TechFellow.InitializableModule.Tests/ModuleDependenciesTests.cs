using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
        public void ModuleDependnecyTests()
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

            Assert.Equal(3, executableModules.Single(md => md.ModuleType == typeof(Module1)).Weight);
            Assert.Equal(2, executableModules.Single(md => md.ModuleType == typeof(Module2)).Weight);
            Assert.Equal(0, executableModules.Single(md => md.ModuleType == typeof(Module3)).Weight);
            Assert.Equal(1, executableModules.Single(md => md.ModuleType == typeof(Module4)).Weight);
            Assert.Equal(0, executableModules.Single(md => md.ModuleType == typeof(Module5)).Weight);
        }
    }

    public class ModuleExecutionContext
    {
        private readonly StringBuilder _log = new StringBuilder();

        public string Log
        {
            get
            {
                return this._log.ToString();
            }
        }

        public void AddExecutionInfo(ModuleDescriptor moduleDescriptor, long elapsedMilliseconds)
        {
            this._log.AppendLine(string.Format("{0} executed in {1}ms", moduleDescriptor.ModuleType.Name, elapsedMilliseconds));
        }
    }

    public class ModuleWeighter
    {
        public IEnumerable<ModuleDescriptor> SortModules(List<ModuleDescriptor> moduleDescriptors)
        {
            moduleDescriptors.ForEach(md => SortModule(md, moduleDescriptors));
            return moduleDescriptors.OrderByDescending(md => md.Weight);
        }

        private void SortModule(ModuleDescriptor moduleDescriptor, List<ModuleDescriptor> moduleDescriptors)
        {
            if (!moduleDescriptor.Dependencies.Any())
            {
                return;
            }

            foreach (var parentModule in
                moduleDescriptor.Dependencies.Select(dependency => moduleDescriptors.SingleOrDefault(m => m.ModuleType == dependency))
                                .Where(parentModule => parentModule != null))
            {
                parentModule.MarkAsParent();

                // update all parents of this parent as well
                SortModule(parentModule, moduleDescriptors);
            }
        }
    }

    public class ModuleExecutionEngine
    {
        public ModuleExecutionContext RunModules(IEnumerable<ModuleDescriptor> modulesWithDep, ModuleExecutionContext context)
        {
            if (modulesWithDep == null)
            {
                throw new ArgumentNullException("modulesWithDep");
            }

            // start with independent modules (those don't have any dependencies)
            var sw = new Stopwatch();
            foreach (var moduleDescriptor in modulesWithDep)
            {
                sw.Restart();
                moduleDescriptor.Module.Initialize();
                sw.Stop();

                context.AddExecutionInfo(moduleDescriptor, sw.ElapsedMilliseconds);
            }

            return context;
        }
    }

    public class ModuleDescriptor
    {
        public ModuleDescriptor(IInitializableModule module, Type moduleType, IEnumerable<Type> dependencies)
        {
            this.Weight = 0;
            this.Module = module;
            this.ModuleType = moduleType;
            this.Dependencies = dependencies.ToList();

            if (this.Dependencies.Contains(moduleType))
            {
                throw new InvalidOperationException("Dependency cannot be module itself");
            }
        }

        public IInitializableModule Module { get; private set; }

        public Type ModuleType { get; private set; }

        public IEnumerable<Type> Dependencies { get; private set; }
        public int Weight { get; private set; }

        public void MarkAsParent()
        {
            this.Weight++;
        }
    }

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
}
