using System;
using System.Collections.Concurrent;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    class CmdCheckEntry : TBModExtensionCommand
    {
        public override string getName() => "checkentry";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string savename = getArgsEntry<string>(argument, 0);
            string config = getArgsEntry<string>(argument, 1);
            string parent = getArgsEntry<string>(argument, 2);

            if (savename == null || config == null || parent == null)
                return callbackError(execCallback, "checkEntry: braucht 3 Parameter - savename (String), config (String) und parent (String)");

            savename = savename.ToLower();
            config = config.ToLower();
            parent = parent.ToLower();
            ConcurrentDictionary<string, string> values = TBModExtensionAPI_Inheritance.saves.GetOrAdd(savename, new ConcurrentDictionary<string, string>());

            if (!values.ContainsKey(config))
            {
                return 1;
            }
            else
            {
                if (values[config].ToLower() == parent)
                {
                    return 1;
                }
                else
                {
                    string msg = string.Format("checkEntry-{0}: value '{1}' hat original den Wert '{2}' hat beim Check aber '{3}'", savename, config, values[config], parent);
                    output.Append(msg);
                    return callbackError(execCallback, msg);
                }
            }
        }
    }
}
