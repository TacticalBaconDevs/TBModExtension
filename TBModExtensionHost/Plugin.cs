using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost
{
    internal class Plugin
    {
        private Assembly dll;
        internal string alias;
        internal string initCode;
        internal Version version;
        internal Dictionary<string, Assembly> embeddedDlls = new Dictionary<string, Assembly>();
        internal List<TBModExtensionCommand> commands = new List<TBModExtensionCommand>();

        internal Plugin(Assembly dll)
        {
            this.dll = dll;
        }

        internal bool fetchFromApi()
        {
            Type[] dllTypes = dll.GetTypes();

            Type typeAPI = dllTypes.FirstOrDefault(type => typeof(TBModExtensionAPI).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);
            TBModExtensionAPI extensionAPI = typeAPI != null ? Activator.CreateInstance(typeAPI) as TBModExtensionAPI : null;
            if (extensionAPI != null)
            {
                // load common values
                alias = extensionAPI.getAlias().ToLower();
                initCode = extensionAPI.initAPI();
                version = extensionAPI.getVersion();

                // loading plugin dependencies
                foreach (KeyValuePair<string, Assembly> dependencies in extensionAPI.loadPluginDependencies())
                    embeddedDlls.Add(dependencies.Key.ToLower(), dependencies.Value);

                // loading commands
                foreach (Type typeCmd in dllTypes.Where(type => typeof(TBModExtensionCommand).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).ToList())
                {
                    TBModExtensionCommand command = typeCmd != null ? Activator.CreateInstance(typeCmd) as TBModExtensionCommand : null;
                    if (command != null)
                        commands.Add(command);
                }

                return true;
            }
            else
            {
                // DLL has no api class
                return false;
            }
        }
    }
}