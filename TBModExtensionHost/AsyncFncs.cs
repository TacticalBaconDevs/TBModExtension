using System;
using System.Linq;
using System.Threading;

namespace TBModExtensionHost
{
    class AsyncFncs
    {
        public static int execute(string fnc, long taskId, object[] args, Action<string, object[]> execCallback, Action<long, string> setTaskStatus)
        {
            // eigene Aufrufe
            switch (fnc)
            {
                case "waittest":
                    Thread.Sleep(10000);
                    return 1;
                case "failtest":
                    return -1;
                case "callbacktest":
                    HostAPI.execCallbackAry("fnc", 1, "Test", 2, new object[] { 1, "2", 3, true, "" }, false, "test", 1);
                    return 1;
            }

            // externe Aufrufe
            if (HostAPI.aliasAPIs.Contains(fnc, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    Func<object[], int> apiFnc = HostAPI.apiFncManager.getFnc("assyncFncsV1", fnc);
                    int returnCode = apiFnc.Invoke(new object[] { args, taskId, execCallback, setTaskStatus });
                    if (returnCode != 0)
                        return returnCode;
                }
                catch (Exception e)
                {
                    Logger.logError("Fehler in assyncFncsV1, lastApiCaller: " + fnc, e);
                }
            }

            return 0;
        }
    }
}
