using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    class CmdLoadFile : TBModExtensionCommand
    {
        public override string getName() => "loadfile";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            if (!File.Exists("inheritance.data"))
                return callbackError(execCallback, "loadfile: inheritance.data existiert nicht");

            TBModExtensionAPI_Inheritance.readWriteLock.EnterWriteLock();
            try
            {
                TBModExtensionAPI_Inheritance.saves = JsonConvert.DeserializeObject<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(File.ReadAllText("inheritance.data"));
            }
            finally
            {
                TBModExtensionAPI_Inheritance.readWriteLock.ExitWriteLock();
            }

            return 1;
        }
    }
}
