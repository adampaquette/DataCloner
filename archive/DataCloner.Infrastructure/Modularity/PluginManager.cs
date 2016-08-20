using DataCloner.Infrastructure.Modularity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DataCloner.Infrastructure.Modularity
{
    public class PluginManager<T> : IPluginManager<T>
    {
        public List<T> LoadPlugins(string pluginFolderPath)
        {
            if (String.IsNullOrWhiteSpace(pluginFolderPath))
                throw new ArgumentNullException(nameof(pluginFolderPath));

            if (Directory.Exists(pluginFolderPath))
            {
                var files = Directory.GetFiles(pluginFolderPath, "*.dll");
                foreach(var file in files)
                {
                    //var asmName = AssemblyName.GetAssemblyName()
                    //var asm =Assembly.Load(asmName);
                }
            }

            return null;
        }
    }
}
