using System;
using System.Collections.Concurrent;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    class CmdAddEntry : TBModExtensionCommand
    {
        public override string getName() => "addentry";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string savename = getArgsEntry<string>(argument, 0);
            string config = getArgsEntry<string>(argument, 1);
            string parent = getArgsEntry<string>(argument, 2);

            if (savename == null || config == null || parent == null)
                return callbackError(execCallback, "addEntry: braucht 3 Parameter - savename (String), config (String) und parent (String)");

            ConcurrentDictionary<string, string> values = TBModExtensionAPI_Inheritance.saves.GetOrAdd(savename.ToLower(), new ConcurrentDictionary<string, string>());

            if (!values.ContainsKey(config.ToLower()))
            {
                values.TryAdd(config.ToLower(), parent);
            }
            else
            {
                return callbackError(execCallback, string.Format("addEntry-{0}: value '{1}' hat bereits den Wert '{2}' und soll mit '{3}' überschrieben werden", savename, config, values[config.ToLower()], parent));
            }

            return 1;
        }
    }
}
