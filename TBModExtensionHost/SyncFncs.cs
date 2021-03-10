using System;
using System.Linq;
using System.Text;

namespace TBModExtensionHost
{
    class SyncFncs
    {
        public static int execute(string fnc, object[] args, Action<string, object[]> execCallback, StringBuilder output)
        {
            // eigene Aufrufe
            switch (fnc)
            {
                case "teststring":
                    execCallback.Invoke("testString-Output", args);
                    return 1;
                case "status":
                    if (args.Length != 1 && !(args[0] is double))
                    {
                        execCallback("error", new object[] { "status: benötigt einen Parameter, taskId (Ganzzahl)" });
                        return -1;
                    }
                    long taskId = Convert.ToInt64(args[0]);

                    if (HostAPI.tasksStatus.ContainsKey(taskId))
                    {
                        output.Append(HostAPI.tasksStatus[taskId]);
                        return 1;
                    }
                    else
                    {
                        output.Append("UNKNOWN_TASKID");
                        return -1;
                    }
                case "check":
                    if (args.Length != 1 && !(args[0] is string))
                    {
                        execCallback("error", new object[] { "check: benötigt einen Parameter, dllName (String)" });
                        return -1;
                    }
                    string dllName = (args[0] as string).ToLower().Replace(".dll", "");
                    bool knownDll = HostAPI.loadedAPIs.Contains(dllName) || HostAPI.loadedAPIs.Contains("TBModExtension_".ToLower() + dllName);
                    return knownDll ? 1 : -1;
            }

            if (HostAPI.aliasAPIs.Contains(fnc, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    Func<object[], int> apiFnc = HostAPI.apiFncManager.getFnc("syncFncsV1", fnc);
                    int returnCode = apiFnc.Invoke(new object[] { args, execCallback, output });
                    if (returnCode != 0)
                        return returnCode;
                }
                catch (Exception e)
                {
                    Logger.logError("Fehler in syncFncsV1, lastApiCaller: " + fnc, e);
                }
            }

            return 0;
        }
    }
}
