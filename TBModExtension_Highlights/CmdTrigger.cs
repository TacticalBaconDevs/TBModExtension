using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Highlights
{
    class CmdTrigger : TBModExtensionCommand
    {
        public override string getName() => "trigger";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            object[] systemTimeRaw = getArgsEntry<object[]>(argument, 0);
            long buffertime = getArgsEntry<long>(argument, 1);

            if (systemTimeRaw == null || systemTimeRaw.Length != 7 || buffertime < 1)
                return callbackError(execCallback, "trigger: benötigt zwei Parameter, systemTime (systemTime) und buffertime (Ganzzahl)");

            //systemTimeRaw = new List<object>(systemTimeRaw).GetRange(0, 6).ToArray();
            //DateTime triggerTime = DateTime.ParseExact(String.Join(",", systemTimeRaw), "yyyy,MM,dd,HH,mm,ss", CultureInfo.InvariantCulture)
            //    .AddSeconds(halfBuffertimeTemp);

            double halfBuffertimeTemp = buffertime / 2d;
            int[] systemTime = Array.ConvertAll<object, int>(systemTimeRaw, x => Convert.ToInt32(x));
            DateTime triggerTime = new DateTime(systemTime[0], systemTime[1], systemTime[2], systemTime[3], systemTime[4], systemTime[5], systemTime[6])
                .AddSeconds(halfBuffertimeTemp);

            TimeSpan waitTime = triggerTime.Subtract(DateTime.Now);
            if (waitTime.TotalMilliseconds > 0)
            {
                Task.Delay(waitTime).ContinueWith(_ =>
                {
                    UdpClient udpClient = new UdpClient();
                    udpClient.Connect("localhost", 56789);
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("trigger");
                    udpClient.Send(sendBytes, sendBytes.Length);
                    udpClient.Close();
                });

                return 1;
            }
            else
            {
                return -1;
            }
        }

    }
}
