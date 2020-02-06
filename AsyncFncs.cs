using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBModExtension
{
    class AsyncFncs
    {
        public static int execute(string fnc, long taskId, List<string> args)
        {
            switch (fnc)
            {
                case "waittest":
                    Thread.Sleep(10000);
                    return 1;
                case "failtest":
                    DLLAPI.tasksStatus[taskId] = "ERROR";
                    return 1;
                case "callbacktest":
                    DLLAPI.tasksStatus[taskId] = "CALLBACK";
                    DLLAPI.execCallback("fnc", 1, "Test", 2, new object[] { 1, "2", 3, true, "" }, false, "test", 1);
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
