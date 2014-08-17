using System;
using System.Collections.Generic;
using System.Linq;

namespace TechFellow.InitializableModule
{
    public class ModuleDescriptor
    {
        public ModuleDescriptor(IInitializableModule module, Type moduleType, IEnumerable<Type> dependencies)
        {
            Weight = 0;
            Module = module;
            ModuleType = moduleType;
            Dependencies = dependencies.ToList();

            if (Dependencies.Contains(moduleType))
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
            Weight++;
        }
    }
}
