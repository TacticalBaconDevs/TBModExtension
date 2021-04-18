using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    class CmdSaveFile : TBModExtensionCommand
    {
        public override string getName() => "savefile";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            TBModExtensionAPI_Inheritance.readWriteLock.EnterWriteLock();
            try
            {
                string json = JsonConvert.SerializeObject(TBModExtensionAPI_Inheritance.saves);
                File.WriteAllText("inheritance.data", json);
            }
            finally
            {
                TBModExtensionAPI_Inheritance.readWriteLock.ExitWriteLock();
            }

            return 1;
        }
    }
}
