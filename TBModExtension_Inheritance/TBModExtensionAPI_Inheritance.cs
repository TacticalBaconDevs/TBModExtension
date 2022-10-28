using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using TBModExtensionHost.PluginAPI;

namespace TBModExtension_Inheritance
{
    public class TBModExtensionAPI_Inheritance : TBModExtensionAPI
    {
        internal static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();
        internal static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> saves = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public override string init()
        {
            return @"diag_log ""TBModExtension-Inheritance geladen! '[_config] call TBExt_addConfig' und '[_config] call TBExt_checkConfig' wurde initialisiert!"";";
        }

        public override string getAlias()
        {
            return "inheritance";
        }

        protected override Assembly getAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
