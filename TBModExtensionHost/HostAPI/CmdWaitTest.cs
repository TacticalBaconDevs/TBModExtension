using System;
using System.Text;
using System.Threading;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdWaitTest : TBModExtensionCommand
    {
        public override string getName() => "waittest";

        public override bool isSync() => false;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            Thread.Sleep(3000);
            return 1;
        }
    }
}
