using System.Text;

namespace TechFellow.InitializableModule
{
    public class ModuleExecutionContext
    {
        private readonly StringBuilder _log = new StringBuilder();

        public string Log
        {
            get { return this._log.ToString(); }
        }

        public void AddExecutionInfo(ModuleDescriptor moduleDescriptor, long elapsedMilliseconds)
        {
            this._log.AppendLine(string.Format("{0} executed in {1}ms", moduleDescriptor.ModuleType.Name, elapsedMilliseconds));
        }
    }
}
