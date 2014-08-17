using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TechFellow.InitializableModule
{
    internal class ModuleExecutionEngine
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
}
