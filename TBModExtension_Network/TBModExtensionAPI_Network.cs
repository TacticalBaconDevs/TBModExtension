using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Network
{
    public class TBModExtensionAPI_Network : TBModExtensionAPI
    {
        internal static ConcurrentDictionary<long, string> cache = new ConcurrentDictionary<long, string>();

        public override string init()
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.DefaultConnectionLimit = 10;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            return @"diag_log ""TBModExtension-Network wurde initalisiert!"";";
        }

        public override string getAlias()
        {
            return "network";
        }

        protected override Assembly getAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
