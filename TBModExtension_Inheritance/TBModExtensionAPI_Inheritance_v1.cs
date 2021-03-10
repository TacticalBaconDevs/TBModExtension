using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TBModExtensionPlugin;

namespace TBModExtension_Inheritance
{
    public class TBModExtensionAPI_Inheritance_v1 : TBModExtensionAPI_v1
    {
        public static new string pluginVersion = "2.0.0";
        private static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> saves = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public override string init()
        {
            return @"diag_log ""TBModExtension-extension-inheritance geladen! '[_config] call TBExt_addConfig' und '[_config] call TBExt_checkConfig' wurde initalisiert!"";";
        }

        public override string getCallAlias()
        {
            return "inheritance";
        }

        public override int syncFncs(object[] args, Action<string, object[]> execCallback, StringBuilder output)
        {
            if (args.Length <= 0 || !(args[0] is string))
                return 0;

            string fnc = (args[0] as string).ToLower();
            args = args.Skip(1).ToArray();

            if (args.Length == 1 && args[0] is object[])
                args = args[0] as object[];

            switch (fnc)
            {
                case "savefile":
                    readWriteLock.EnterWriteLock();
                    try
                    {
                        string json = JsonConvert.SerializeObject(saves);
                        File.WriteAllText("inheritance.data", json);
                    }
                    finally
                    {
                        readWriteLock.ExitWriteLock();
                    }

                    return 1;
                case "loadfile":
                    if (!File.Exists("inheritance.data"))
                        return callbackError(execCallback, "loadfile: inheritance.data existiert nicht");

                    readWriteLock.EnterWriteLock();
                    try
                    {
                        saves = JsonConvert.DeserializeObject<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(File.ReadAllText("inheritance.data"));
                    }
                    finally
                    {
                        readWriteLock.ExitWriteLock();
                    }

                    return 1;
                case "clear":
                    saves.Clear();
                    return 1;
                case "addentry":
                    if (args.Length != 3 || !(args[0] is string) || !(args[1] is string) || !(args[2] is string))
                        return callbackError(execCallback, "addEntry: braucht 3 Parameter - savename (String), config (String) und parent (String)");

                    string savename = args[0] as string;
                    string config = args[1] as string;
                    string parent = args[2] as string;

                    ConcurrentDictionary<string, string> values = saves.GetOrAdd(savename.ToLower(), new ConcurrentDictionary<string, string>());

                    if (!values.ContainsKey(config.ToLower()))
                    {
                        values.TryAdd(config.ToLower(), parent);
                    }
                    else
                    {
                        return callbackError(execCallback, string.Format("addEntry-{0}: value '{1}' hat bereits den Wert '{2}' und soll mit '{3}' überschrieben werden", savename, config, values[config.ToLower()], parent));
                    }

                    return 1;
                case "checkentry":
                    if (args.Length != 3 || !(args[0] is string) || !(args[1] is string) || !(args[2] is string))
                        return callbackError(execCallback, "checkEntry: braucht 3 Parameter - savename (String), config (String) und parent (String)");

                    savename = args[0] as string;
                    config = args[1] as string;
                    parent = args[2] as string;

                    values = saves.GetOrAdd(savename.ToLower(), new ConcurrentDictionary<string, string>());

                    if (!values.ContainsKey(config.ToLower()))
                    {
                        return 1;
                    }
                    else
                    {
                        if ((values[config.ToLower()]).ToLower() == parent.ToLower())
                        {
                            return 1;
                        }
                        else
                        {
                            callbackError(execCallback, string.Format("checkEntry-{0}: value '{1}' hat original den Wert '{2}' hat beim Check aber '{3}'", savename, args[1] as string, values[config.ToLower()], parent));
                            return -1;
                        }
                    }
            }

            return 0;
        }

        public override int assyncFncs(object[] args, long taskId, Action<string, object[]> execCallback, Action<long, string> setTaskStatus)
        {
            return 0;
        }

        public override Assembly getAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
