using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdAllCmds : TBModExtensionCommand
    {
        public override string getName() => "allcmds";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            foreach (Plugin plugin in HostAPI.pluginLoader.plugins.Values)
            {
                output.AppendLine(string.Format("=== {0} ===", plugin.alias));

                foreach (TBModExtensionCommand cmd in plugin.commands)
                {
                    output.AppendLine(string.Format(" -> {0} [isSync: {1}]", cmd.getName(), cmd.isSync()));
                }
            }

            return 1;
        }
    }
}
