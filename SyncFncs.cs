using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TBModExtension
{
    class SyncFncs
    {
        public static int execute(string fnc, object[] args, Action<string, object[]> execCallback)
        {
            // eigene Aufrufe
            switch (fnc)
            {
                case "teststring":
                    execCallback.Invoke("testString-Output", args);
                    return 1;
            }

            // externe Aufrufe
            string lastApiCaller = "";
            try
            {
                if (DLLAPI.apiFncs.ContainsKey("syncFncsV1"))
                {
                    foreach (MethodInfo item in DLLAPI.apiFncs["syncFncsV1"])
                    {
                        lastApiCaller = item.ToString();
                        int returnCode = (int)item.Invoke(null, new object[] { fnc, args, execCallback });
                        if (returnCode != 0)
                            return returnCode;
                    }
                }
            }
            catch (Exception e)
            {
                DLLAPI.logError("Fehler in syncFncsV1, lastApiCaller: " + lastApiCaller, e);
            }
            
            return 0;
        }
    }
}
