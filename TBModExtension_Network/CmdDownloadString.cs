using System;
using System.Net;
using System.Text;
using TBModExtensionHost;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Network
{
    class CmdDownloadString : TBModExtensionCommand
    {
        public override string getName() => "downloadstring";

        public override bool isSync() => false;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string url = argument is string ? argument as string : null;
            if (url == null)
                return callbackError(execCallback, "downloadString: benötigt einen Parameter, url (String)");

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string downloadString = client.DownloadString(url);

                    MultiMessageCache.addToCache(taskId.ToString(), downloadString);
                    //TBModExtensionAPI_Network.cache.TryAdd(taskId, downloadString);

                    return 1;
                }
            }
            catch (Exception e)
            {
                return callbackError(execCallback, e.Message);
            }
        }
    }
}
