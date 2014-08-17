using System;
using System.Linq;

namespace TechFellow.InitializableModule
{
    public class ModuleExecutionProcess
    {
        private readonly Func<Type, object> _moduleConstructor;

        public ModuleExecutionProcess(Func<Type, object> moduleConstructor)
        {
            this._moduleConstructor = moduleConstructor;
        }


        public ModuleExecutionContext Execute()
        {
            var modules = TypeHelper.GetTypesImplementingInterface<IInitializableModule>();
            var modulesWithDep =
                modules.Select(
                    m =>
                        new ModuleDescriptor(this._moduleConstructor(m) as IInitializableModule,
                            m,
                            TypeHelper.GetAttributes<ModuleDependencyAttribute>(m).Select(d => d.DependencyModule)));

            var engine = new ModuleExecutionEngine();
            var weighter = new ModuleWeighter();

            return engine.RunModules(weighter.SortModules(modulesWithDep.ToList()), new ModuleExecutionContext());
        }
    }
}
