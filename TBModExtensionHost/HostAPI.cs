using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TBModExtensionHost.Settings;
using TBModExtensionPlugin;

namespace TBModExtensionHost
{
    class HostAPI
    {
        protected static bool error = false;
        protected static long taskIdCounter = 0;
        private static bool loaded = false;
        public static ApiFncManager apiFncManager = new ApiFncManager();
        public static ConcurrentDictionary<long, string> tasksStatus = new ConcurrentDictionary<long, string>();
        public static BlockingCollection<string> loadedAPIs = new BlockingCollection<string>();
        public static BlockingCollection<string> aliasAPIs = new BlockingCollection<string>();
        private static ConcurrentDictionary<string, Assembly> embeddedDlls = new ConcurrentDictionary<string, Assembly>();
        public static SettingsManager settingsMgr;

        public static void init()
        {
            // Load embedded ressources

            AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
            {
                string dllName = (new AssemblyName(bargs.Name).Name + ".dll").ToLower();

                if (embeddedDlls.ContainsKey(dllName))
                    return embeddedDlls[dllName];

                return null;
            };

            settingsMgr = new SettingsManager();
            Settings.Settings settings = settingsMgr.settings;
        }

        /// <summary>
        /// Loading the api-plugins
        /// </summary>
        /// <param name="output"></param>
        /// <param name="outputSize"></param>
        /// <param name="function"></param>
        public static void handleRvExtension(StringBuilder output, int outputSize, string function)
        {
            try
            {
                if (loaded)
                {
                    output.Append("TBModExtension already loaded!");
                    return;
                }

                StringBuilder sb = new StringBuilder("TBModExtension is now active and Callback is created!");
                loaded = true;

                // Load from Arma3.exe dir and dll location
                HashSet<string> files = new HashSet<string>();
                files.UnionWith(Directory.GetFiles(Directory.GetCurrentDirectory(), "TBModExtension_*.dll", SearchOption.TopDirectoryOnly));
                files.UnionWith(Directory.GetFiles(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "TBModExtension_*.dll", SearchOption.TopDirectoryOnly));

                // try to load files
                foreach (string filePath in files)
                {
                    string dllName = new FileInfo(filePath).Name.Replace(".dll", "");
                    string dllNameLower = dllName.ToLower();

                    // dont load host dll
                    if (dllName == "TBModExtensionHost_x64")
                        continue;

                    // TODO: load newer API version
                    if (loadedAPIs.Contains(dllNameLower))
                    {
                        sb.AppendLine(string.Format(" - Cant load plugin '{0}' is already loaded [path: {1}]", dllName, filePath));
                        continue;
                    }

                    // load API defined stuff
                    try
                    {
                        Assembly DLL = Assembly.LoadFile(filePath);
                        bool apiFound = false;

                        foreach (Type apiClass in DLL.GetTypes())
                        {
                            if (apiClass.Name.StartsWith("TBModExtensionAPI"))
                            {
                                apiFound = true;

                                int apiVersion = (int)apiClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                    .First(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name == nameof(TBModExtensionAPI_v1.API_VERSION)).GetRawConstantValue();
                                string dllVersion = (string)apiClass.GetField(nameof(TBModExtensionAPI_v1.pluginVersion)).GetValue(null);

                                switch (apiVersion)
                                {
                                    case (int)PLUGIN_APIS.TBModExtensionAPI_v1:
                                        string alias = TBModExtensionAPI_v1.invoke<string>(apiClass, nameof(TBModExtensionAPI_v1.getCallAlias));
                                        aliasAPIs.Add(alias);

                                        apiFncManager.addFnc("syncFncsV1", alias, (args) => { return TBModExtensionAPI_v1.invoke<int>(apiClass, nameof(TBModExtensionAPI_v1.syncFncs), args); });
                                        apiFncManager.addFnc("assyncFncsV1", alias, (args) => { return TBModExtensionAPI_v1.invoke<int>(apiClass, nameof(TBModExtensionAPI_v1.assyncFncs), args); });

                                        Assembly pluginAssembly = TBModExtensionAPI_v1.invoke<Assembly>(apiClass, nameof(TBModExtensionAPI_v1.getAssembly));
                                        foreach (string resourceDllName in pluginAssembly.GetManifestResourceNames().Where(rn => rn.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)))
                                        {
                                            using (Stream stream = pluginAssembly.GetManifestResourceStream(resourceDllName))
                                            {
                                                byte[] assemblyData = new byte[stream.Length];
                                                stream.Read(assemblyData, 0, assemblyData.Length);

                                                string key = resourceDllName.Substring(resourceDllName.IndexOf(".Resources.") + ".Resources.".Length);
                                                embeddedDlls.TryAdd(key.ToLower(), Assembly.Load(assemblyData));
                                            }
                                        }

                                        string sqfCode = TBModExtensionAPI_v1.invoke<string>(apiClass, nameof(TBModExtensionAPI_v1.initAPI));
                                        execCallback("call", sqfCode);
                                        break;
                                    default:
                                        throw new Exception(string.Format("APIVersion '{0}' is not supported", apiVersion));
                                }

                                loadedAPIs.Add(dllNameLower);
                                sb.AppendLine(string.Format("- Plugin '{0}' successful loaded [Version: {1}, APIVersion: {2}, location: {3}]", dllName, dllVersion, apiVersion, filePath));
                            }
                        }

                        if (!apiFound)
                            throw new Exception(string.Format("TBModExtensionAPI class couldn't be found in plugin {0} [location: {1}]", dllName, filePath));
                    }
                    catch (Exception e)
                    {
                        string msg = string.Format("- Extension '{0}' cant be loaded -> {1}", dllName, e.Message);
                        sb.AppendLine(msg);
                        Logger.logError(msg, e);
                    }
                }

                output.Append(sb.ToString());
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

        public static int handleRvExtensionArgs(StringBuilder output, int outputSize, string function, string[] args, int argCount)
        {
            try
            {
                if (error)
                    return -10;

                function = function != null ? function.ToLower() : "";
                args = args != null ? args : Array.Empty<string>();

                object[] cArgs = args.Select(x => ArmaToolbox.convert2C(x)).ToArray();

                if (function.Length > 0)
                {
                    // syncrone FNCs
                    int result = SyncFncs.execute(function, cArgs, execCallbackAry, output);
                    if (result != 0)
                    {
                        if (output.Length == 0)
                            output.Append(result > 0 ? "DONE" : "ERROR");
                        return result;
                    }
                    // TODO: ouput Zustand ungewiss hier

                    // TaskId hochzählen und als ersten Value übergeben
                    Interlocked.Increment(ref taskIdCounter);
                    if (!tasksStatus.TryAdd(taskIdCounter, "QUEUE"))
                        return -3;
                    List<object> values = new List<object>();
                    values.Add(taskIdCounter);
                    values.Add(function);
                    values.AddRange(cArgs);

                    output.Append(taskIdCounter.ToString());
                    return ThreadPool.QueueUserWorkItem(new WaitCallback(processInput), values) ? 1 : -1;
                }
                else
                {
                    return -2;
                }
            }
            catch (Exception e)
            {
                Logger.logError(e);
                return -1;
            }
        }

        public static void execCallbackAry(string fnc, params object[] obj)
        {
            if (obj != null & obj.Length == 1 && (obj[0] is string || obj[0] is String))
            {
                execCallback(fnc, obj[0] as string);
            }
            else
            {
                execCallback(fnc, ArmaToolbox.convert2Arma(obj) as string);
            }
        }

        public static void execCallback(string fnc, string data)
        {
            try
            {
                if (ArmaAPI.callback != null)
                    ArmaAPI.callback.Invoke("TBModExtension", fnc, data);
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

        private static void setTaskStatus(long taskId, string status)
        {
            try
            {
                //if (!tasksStatus.ContainsKey(taskId))
                tasksStatus[taskIdCounter] = status.ToUpper();
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

        private static void processInput(object input)
        {
            try
            {
                List<object> args = input as List<object>;
                long taskId = Convert.ToInt64(args[0]);
                string funktion = args[1] as string;
                args.RemoveRange(0, 2);

                // async FNCs
                tasksStatus[taskId] = AsyncFncs.execute(funktion, taskId, args.ToArray(), execCallbackAry, setTaskStatus) >= 0 ? "DONE" : "ERROR";
                execCallbackAry("task", taskId, funktion, tasksStatus[taskId]);
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

    }
}
