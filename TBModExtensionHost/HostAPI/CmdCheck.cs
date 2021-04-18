using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdCheck : TBModExtensionCommand
    {
        public override string getName() => "check";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string inputName = argument is string ? argument as string : null;
            if (inputName == null)
                return callbackError(execCallback, "check: benötigt einen Parameter, dllName (String)");

            string dllName = inputName.ToLower().Replace(".dll", "");
            bool knownDll = HostAPI.pluginLoader.aliasAPIs.Contains(dllName) || HostAPI.pluginLoader.loadedAPIs.Contains(dllName) || HostAPI.pluginLoader.loadedAPIs.Contains("TBModExtension_".ToLower() + dllName);

            output.Append(knownDll ? "YES" : "NO");
            return knownDll ? 1 : -1;
        }
    }
}
