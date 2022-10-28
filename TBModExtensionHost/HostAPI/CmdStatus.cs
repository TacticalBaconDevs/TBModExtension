using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdStatus : TBModExtensionCommand
    {
        public override string getName() => "status";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            long inputTaskId = argument is long ? (long)argument : -1;

            if (inputTaskId < 0)
                return callbackError(execCallback, "status: benötigt einen Parameter, taskId (Ganzzahl)");

            if (HostAPI.tasksStatus.ContainsKey(inputTaskId))
            {
                output.Append(HostAPI.tasksStatus[inputTaskId]);
                return 1;
            }
            else
            {
                output.Append("UNKNOWN_TASKID");
                return -1;
            }
        }
    }
}
