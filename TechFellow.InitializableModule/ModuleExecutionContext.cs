using System.Text;

namespace TechFellow.InitializableModule
{
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

        public void AddError(ModuleDescriptor moduleDescriptor, string message)
        {
            this._log.AppendLine(string.Format("Error in module {0}: " + message, moduleDescriptor.ModuleType.Name));
        }

        public void AddExecutionInfo(ModuleDescriptor moduleDescriptor, long elapsedMilliseconds)
        {
            this._log.AppendLine(string.Format("{0} executed in {1}ms", moduleDescriptor.ModuleType.Name, elapsedMilliseconds));
        }
    }
}
