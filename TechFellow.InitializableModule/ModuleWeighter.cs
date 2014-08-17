using System.Collections.Generic;
using System.Linq;

namespace TechFellow.InitializableModule
{
    internal class ModuleWeighter
    {
        public IEnumerable<ModuleDescriptor> SortModules(List<ModuleDescriptor> moduleDescriptors)
        {
            moduleDescriptors.ForEach(md => SortModule(md, moduleDescriptors));
            return moduleDescriptors.OrderByDescending(md => md.Weight);
        }

        private void SortModule(ModuleDescriptor moduleDescriptor, IEnumerable<ModuleDescriptor> moduleDescriptors)
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
}
