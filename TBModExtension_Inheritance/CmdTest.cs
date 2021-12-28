using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    class CmdTest : TBModExtensionCommand
    {
        public override string getName() => "test";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            output.Append("test was good");
            return 1;
        }
    }
}
