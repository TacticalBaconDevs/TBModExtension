using System;
using System.Text;
using TBModExtensionHost;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Network
{
    class CmdGetCache : TBModExtensionCommand
    {
        public override string getName() => "getcache";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            long inputTaskId = argument is long ? (long)argument : -1;
            if (inputTaskId <= 0)
                return callbackError(execCallback, "getCache: benötigt einen Parameter, taskId (Ganzzahl)");

            int result = MultiMessageCache.getFromCache(inputTaskId.ToString(), out string downloadString);
            output.Append(downloadString);
            return result;

            /*bool result = TBModExtensionAPI_Network.cache.TryGetValue(id, out string downloadString);
            if (result && downloadString.Length >= OUTPUT_SIZE)
            {
                string trimmed = downloadString.Substring(0, OUTPUT_SIZE);
                string rest = downloadString.Substring(OUTPUT_SIZE);
                TBModExtensionAPI_Network.cache.TryUpdate(id, rest, downloadString);
                output.Append(trimmed);
                return 1 + (int)Math.Ceiling((double)rest.Length / (double)OUTPUT_SIZE);
            }
            else
            {
                TBModExtensionAPI_Network.cache.TryRemove(id, out _);
                output.Append(downloadString);
                return result ? 1 : -1;
            }*/
        }
    }
}
