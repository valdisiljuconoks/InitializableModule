using System;

namespace TechFellow.InitializableModule
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ModuleDependencyAttribute : Attribute
    {
        public ModuleDependencyAttribute(Type dependencyModule)
        {
            DependencyModule = dependencyModule;
        }

        public Type DependencyModule { get; private set; }
    }
}
