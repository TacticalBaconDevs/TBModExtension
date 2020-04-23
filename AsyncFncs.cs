using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBModExtension
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
                    DLLAPI.execCallbackAry("fnc", 1, "Test", 2, new object[] { 1, "2", 3, true, "" }, false, "test", 1);
                    return 1;
            }

            // externe Aufrufe
            string lastApiCaller = "";
            try {
                if (DLLAPI.apiFncs.ContainsKey("assyncFncsV1"))
                {
                    foreach (MethodInfo item in DLLAPI.apiFncs["assyncFncsV1"])
                    {
                        lastApiCaller = item.ToString();
                        int returnCode = (int)item.Invoke(null, new object[] { fnc, args, taskId, execCallback, setTaskStatus });
                        if (returnCode != 0)
                            return returnCode;
                    }
                }
            }
            catch (Exception e)
            {
                DLLAPI.logError("Fehler in assyncFncsV1, lastApiCaller: "+ lastApiCaller, e);
            }

            return 0;
        }
    }
}
