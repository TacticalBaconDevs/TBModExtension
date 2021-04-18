using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdTestString : TBModExtensionCommand
    {
        public override string getName() => "teststring";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            execCallback.Invoke(CallbackModes.LOG, argument is object[]? argument as object[] : new object[] { argument as object });
            return 1;
        }
    }
}
