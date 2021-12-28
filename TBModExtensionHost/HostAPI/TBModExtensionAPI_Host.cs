using System.Reflection;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost
{
    public class TBModExtensionAPI_Host : TBModExtensionAPI
    {
        public override string getAlias()
        {
            return "host";
        }

        public override string init()
        {
            return @"diag_log 'TBModExtensionHost wurde initalisiert!';";
        }

        protected override Assembly getAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
