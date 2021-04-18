using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdCallbackTest : TBModExtensionCommand
    {
        public override string getName() => "callbacktest";

        public override bool isSync() => false;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            execCallback.Invoke(CallbackModes.LOG, new object[] { 1, "Test", 2, new object[] { 1, "2", 3, true, "" }, false, "test", 1 });
            return 1;
        }
    }
}
