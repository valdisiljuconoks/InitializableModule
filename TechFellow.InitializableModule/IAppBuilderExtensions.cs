using System;
using Owin;

namespace TechFellow.InitializableModule
{
    public static class IAppBuilderExtensions
    {
        public static ModuleExecutionContext ExecuteInitializableModules(this IAppBuilder app, Func<Type, object> moduleConstructor)
        {
            var process = new ModuleExecutionProcess(app, moduleConstructor);
            return process.Execute();
        }
    }
}
