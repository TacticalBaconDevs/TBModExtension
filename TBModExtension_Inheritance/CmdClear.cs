using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    class CmdClear : TBModExtensionCommand
    {
        public override string getName() => "clear";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            TBModExtensionAPI_Inheritance.saves.Clear();
            return 1;
        }
    }
}
