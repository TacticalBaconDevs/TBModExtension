using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Network
{
    class CmdGetDownloadString : TBModExtensionCommand
    {
        public override string getName() => "getdownloadstring";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            long id = argument is long ? (long)argument : -1;
            if (id < 0)
                return callbackError(execCallback, "getDownloadString: benötigt einen Parameter, id (Ganzzahl)");

            bool result = TBModExtensionAPI_Network.cache.TryGetValue(id, out string downloadString);
            if (result && downloadString.Length >= outputSize)
            {
                string trimmed = downloadString.Substring(0, outputSize);
                string rest = downloadString.Substring(outputSize);
                TBModExtensionAPI_Network.cache.TryUpdate(id, rest, downloadString);
                output.Append(trimmed);
                return 1 + (int)Math.Ceiling((double)rest.Length / (double)outputSize);
            }
            else
            {
                TBModExtensionAPI_Network.cache.TryRemove(id, out _);
                output.Append(downloadString);
                return result ? 1 : -1;
            }
        }
    }
}
