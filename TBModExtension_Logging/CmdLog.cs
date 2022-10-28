using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Logging
{
    class CmdLog : TBModExtensionCommand
    {
        public override string getName() => "log";

        public override bool isSync() => false;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string loggerName = getArgsEntry<string>(argument, 0);
            string type = getArgsEntry<string>(argument, 1);
            string msg = getArgsEntry<string>(argument, 2);

            if (loggerName == null || type == null || msg == null)
                return callbackError(execCallback, "log: braucht 3 Parameter - loggerName (String), Type (String), und Nachricht (String)");

            return TBModExtensionAPI_Logging.log2File(loggerName, type, msg) ? 1 : -1;
        }
    }
}
