using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Logging
{
    class CmdRegister : TBModExtensionCommand
    {
        public override string getName() => "register";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string loggerName = getArgsEntry<string>(argument, 0);
            string filename = getArgsEntry<string>(argument, 1);

            if (loggerName == null || filename == null)
                return callbackError(execCallback, "register: braucht 2 Parameter - loggerName (String) und FileName (String)");
            loggerName = loggerName.ToLower();

            if (!TBModExtensionAPI_Logging.logger.ContainsKey(loggerName))
            {
                TBModExtensionAPI_Logging.logger.Add(loggerName, filename);
                return 1;
            }
            else
            {
                return callbackError(execCallback, string.Format("register: '{0}' ist bereits registriert", loggerName));
            }
        }
    }
}
