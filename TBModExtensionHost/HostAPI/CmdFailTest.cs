using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdFailTest : TBModExtensionCommand
    {
        public override string getName() => "failtest";

        public override bool isSync() => false;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            return -1;
        }
    }
}
