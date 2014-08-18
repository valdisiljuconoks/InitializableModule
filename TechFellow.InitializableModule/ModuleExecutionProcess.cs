using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Owin;

namespace TechFellow.InitializableModule
{
    public class ModuleExecutionProcess
    {
        private readonly IAppBuilder _app;
        private readonly Func<Type, object> _moduleConstructor;
        private IEnumerable<Type> _modules;

        public ModuleExecutionProcess() : this(null, null)
        {
        }

        public ModuleExecutionProcess(IAppBuilder app) : this(app, null)
        {
        }

        public ModuleExecutionProcess(Func<Type, object> moduleConstructor) : this(null, moduleConstructor)
        {
        }

        public ModuleExecutionProcess(IAppBuilder app, Func<Type, object> moduleConstructor)
        {
            this._moduleConstructor = moduleConstructor;
            this._app = app;
        }

        public ModuleExecutionContext Execute()
        {
            if (this._modules == null)
            {
                this._modules = TypeHelper.GetTypesImplementingInterface<IInitializableModule>();
            }

            var modulesWithDep = this._modules.Select(
                                                      m =>
                                                      new ModuleDescriptor(CreateModule(m),
                                                                           m,
                                                                           TypeHelper.GetAttributes<ModuleDependencyAttribute>(m)
                                                                                     .Select(d => d.DependencyModule)));

            var engine = new ModuleExecutionEngine();
            var weighter = new ModuleWeighter();

            return engine.RunModules(weighter.SortModules(modulesWithDep.ToList()), new ModuleExecutionContext());
        }

        internal void SetModulesTypes(IEnumerable<Type> moduleTypes)
        {
            this._modules = moduleTypes;
        }

        private IInitializableModule CreateModule(Type moduleType)
        {
            var ctors = moduleType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            var instance = ctors.Any(c => c.GetParameters().SingleOrDefault(pi => pi.ParameterType == typeof(IAppBuilder)) != null)
                               ? Activator.CreateInstance(moduleType, this._app)
                               : (this._moduleConstructor != null ? this._moduleConstructor(moduleType) : Activator.CreateInstance(moduleType));

            return instance as IInitializableModule;
        }
    }
}
