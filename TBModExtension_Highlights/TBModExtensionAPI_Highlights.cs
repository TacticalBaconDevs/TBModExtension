using System;
using System.Linq;
using System.Reflection;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Highlights
{
    public class TBModExtensionAPI_Highlights : TBModExtensionAPI
    {
        public override string init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
            {
                string dllName = new AssemblyName(bargs.Name).Name + ".dll";
                var assem = Assembly.GetExecutingAssembly();
                string resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName, StringComparison.CurrentCultureIgnoreCase));

                if (resourceName == null)
                    return null; // Not found, maybe another handler will find it

                // load own embedded dll
                using (var stream = assem.GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);
                }
            };

            return @"diag_log ""TBModExtension-Highlights wurde initialisiert!"";";
        }

        public override string getAlias()
        {
            return "highlights";
        }

        protected override Assembly getAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

    }
}
