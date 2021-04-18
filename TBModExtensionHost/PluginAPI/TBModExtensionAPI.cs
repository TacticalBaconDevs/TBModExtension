using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TBModExtensionHost.PluginAPI
{
    /// <summary>
    /// Entrypoint for a plugin of the TBModExtensionHost
    /// </summary>
    public abstract class TBModExtensionAPI
    {
        /// <summary>
        /// The alias which connects commands to this plugin
        /// </summary>
        /// <returns>simple string</returns>
        public abstract string getAlias();

        /// <summary>
        /// SQF code (unscheduled) executed if plugin is loaded
        /// </summary>
        /// <returns>SQF Code</returns>
        public abstract string init();

        /// <summary>
        /// The assembly for sqf file searching in embedded resources
        /// </summary>
        /// <returns>your Assembly</returns>
        protected abstract Assembly getAssembly();

        /// <summary>
        /// This method is called if this plugin is loaded.<br/>
        /// - it calls first the init() method<br/>
        /// - after this it search for *.sqf files in the embedded files
        /// </summary>
        public string initAPI()
        {
            StringBuilder result = new StringBuilder(init());

            string sqfFiles = loadSQFFiles();
            if (sqfFiles != null && sqfFiles.Length > 0)
            {
                result.AppendLine();
                result.Append(loadSQFFiles());
            }

            return result.ToString();
        }

        /// <summary>
        /// Loading *.sqf files in the embedded files
        /// </summary>
        /// <returns>content of *.sqf files</returns>
        private string loadSQFFiles()
        {
            StringBuilder result = new StringBuilder();
            Assembly assembly = getAssembly();

            foreach (string fileName in assembly.GetManifestResourceNames().Where(resourceName => resourceName.EndsWith(".sqf", StringComparison.CurrentCultureIgnoreCase)))
            {
                using (Stream stream = assembly.GetManifestResourceStream(fileName))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string filenameTrimmed = fileName.Substring(fileName.IndexOf(".Resources.") + ".Resources.".Length);
                    result.AppendLine(string.Format("diag_log 'File {0} loaded!';", filenameTrimmed));
                    result.AppendLine(reader.ReadToEnd());
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// This method loads all embedded dlls
        /// </summary>
        /// <param name="embeddedDlls">the embeddedDlls-Dictionary of the host</param>
        public ConcurrentDictionary<string, Assembly> loadPluginDependencies()
        {
            ConcurrentDictionary<string, Assembly> result = new ConcurrentDictionary<string, Assembly>();

            // Plugin Depenency loading
            Assembly pluginAssembly = getAssembly();
            foreach (string resourceDllName in pluginAssembly.GetManifestResourceNames().Where(rn => rn.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)))
            {
                using (Stream stream = pluginAssembly.GetManifestResourceStream(resourceDllName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);

                    string key = resourceDllName.Substring(resourceDllName.IndexOf(".Resources.") + ".Resources.".Length);
                    result.TryAdd(key.ToLower(), Assembly.Load(assemblyData));
                }
            }

            return result;
        }

    }
}
