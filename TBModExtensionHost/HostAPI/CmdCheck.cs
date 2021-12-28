using System;
using System.Linq;
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
            string alias = argument is string ? argument as string : null;
            if (alias == null)
                return callbackError(execCallback, "check: benötigt einen Parameter, aliasName (String)");

            StringComparison comp = StringComparison.CurrentCultureIgnoreCase;
            bool knownDll = HostAPI.pluginLoader.plugins.Keys.Any(x => x.Equals(alias, comp) || x.Equals("TBModExtension_" + alias, comp));

            output.Append(knownDll ? "YES" : "NO");
            return knownDll ? 1 : -1;
        }
    }
}
