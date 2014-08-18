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
                var module = moduleDescriptor.Module;
                try
                {
                    sw.Restart();
                    module.Initialize();
                    sw.Stop();

                    context.AddExecutionInfo(moduleDescriptor, sw.ElapsedMilliseconds);
                }
                finally
                {
                    try
                    {
                        module.Release();
                    }
                    catch (Exception e)
                    {
                        context.AddError(moduleDescriptor, e.Message);
                    }
                }
            }

            return context;
        }
    }
}
