using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TBModExtensionHost.PluginAPI;
using TBModExtensionHost.Tools;

namespace TBModExtensionHost
{
    class HostAPI
    {
        public const int outputSize = 10240 - 1;
        protected static bool error = false;
        protected static long taskIdCounter = 0;
        private static bool loaded = false;
        public static ConcurrentDictionary<long, string> tasksStatus = new ConcurrentDictionary<long, string>();
        public static PluginLoader pluginLoader = new PluginLoader();
        public static string initCode = null;

        public static void init()
        {
            // Load embedded ressources
            AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) => pluginLoader.pluginAssemblyResolve(bargs.Name);
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
                function = function != null ? function.ToLower() : null;

                // TODO: reloading of assemblies, need to unload before
                bool reload = false; // "reload".Equals(function);

                if (loaded && !reload)
                {
                    output.Append("TBModExtension already loaded!");
                    execCallback(CallbackModes.CALL, initCode);
                    return;
                }

                loaded = true;
                StringBuilder sb = new StringBuilder((reload ? "TBModExtension reloaded!" : "TBModExtension is now active and callback is created!"));
                pluginLoader.embeddedDlls.TryAdd("tbmodextensionhost_x64.dll", Assembly.GetExecutingAssembly());

                // Load from Arma3.exe dir and dll location
                HashSet<string> files = new HashSet<string>();
                foreach (string item in new string[] { "TBModExtensionHost_x64.dll", "TBModExtension_*.dll" })
                {
                    files.UnionWith(Directory.GetFiles(Directory.GetCurrentDirectory(), item, SearchOption.TopDirectoryOnly));
                    files.UnionWith(Directory.GetFiles(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, item, SearchOption.TopDirectoryOnly));
                }

                // try to load files
                foreach (string filePath in files)
                {
                    string dllName = new FileInfo(filePath).Name.Replace(".dll", "").ToLower();

                    // load API defined stuff
                    string pluginInitCode = pluginLoader.loadPlugin(dllName, filePath, ref sb);
                    if (pluginInitCode != null)
                        initCode += "\n" + pluginInitCode;
                }

                execCallback(CallbackModes.CALL, initCode);
                output.Append(sb.ToString());
                output.Length = outputSize;
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

        public static int handleRvExtensionArgs(StringBuilder output, int outputSize, string alias, string[] args, int argCount)
        {
            try
            {
                // ciritical error block everything
                if (error)
                    return errorReturn(ErrorCode.CRITICAL_BLOCKING, output);

                // input management
                alias = alias != null ? alias.ToLower() : "";
                args = args != null ? args : Array.Empty<string>();
                object[] convertsArgs = args.Select(x => ArmaToolbox.convert2C(x)).ToArray();

                // value checking
                if (alias.Length <= 0)
                    return errorReturn(ErrorCode.EMPTY_FNC, output);
                if (argCount <= 0)
                    return errorReturn(ErrorCode.EMPTY_ARGS, output);
                if (argCount > 2)
                    return errorReturn(ErrorCode.TOO_MUCH_ARGS, output);
                if (!(convertsArgs[0] is string))
                    return errorReturn(ErrorCode.WRONG_CMD_TYPE, output);
                if (!pluginLoader.aliasAPIs.Contains(alias))
                    return errorReturn(ErrorCode.UNKNOWN_ALIAS, output);

                // read cmd name and mode
                string cmdName = convertsArgs[0] as string;
                bool isSync = !cmdName.StartsWith("#");
                if (!isSync)
                    cmdName = cmdName.Substring(1);
                //convertsArgs = convertsArgs.Skip(1).ToArray();

                // read argument
                object functionArgs = argCount == 2 ? convertsArgs[1] : null;

                // read cmd
                TBModExtensionCommand cmd = pluginLoader.getCmd(alias, /*isSync,*/ cmdName);
                if (cmd == null)
                    return errorReturn(ErrorCode.UNKNOWN_CMD, output);
                if (cmd.isSync() != isSync)
                    return errorReturn(ErrorCode.WRONG_SYNC_STATE, output);

                if (isSync)
                {
                    try
                    {
                        int result = cmd.execute(functionArgs, execCallback, output, -1);
                        if (output.Length == 0)
                            output.Append(result > 0 ? "DONE" : "ERROR");
                        return result;
                    }
                    catch (Exception e)
                    {
                        Logger.logError(string.Format("Fehler in syncFncs, lastApiCaller: {0}", cmd.getName()), e);
                        return errorReturn(ErrorCode.EXCEPTION_SYNC, output);
                    }
                }
                else
                {
                    // create new taskId and set status
                    Interlocked.Increment(ref taskIdCounter);
                    if (!tasksStatus.TryAdd(taskIdCounter, "QUEUE"))
                        return errorReturn(ErrorCode.EXCEPTION, output);

                    List<object> values = new List<object>();
                    values.Add(taskIdCounter);
                    values.Add(functionArgs);
                    values.Add(cmd);

                    output.Append(taskIdCounter.ToString());
                    return ThreadPool.QueueUserWorkItem(new WaitCallback(processInput), values) ? Convert.ToInt32(taskIdCounter) : errorReturn(ErrorCode.EXCEPTION, output);
                }
            }
            catch (Exception e)
            {
                Logger.logError(e);
                return errorReturn(ErrorCode.EXCEPTION, output);
            }
        }

        private static void processInput(object input)
        {
            try
            {
                List<object> args = input as List<object>;
                long taskId = Convert.ToInt64(args[0]);
                object argument = args[1];
                TBModExtensionCommand cmd = args[2] as TBModExtensionCommand;

                tasksStatus[taskId] = cmd.execute(argument, execCallback, null, taskId) >= 0 ? "DONE" : "ERROR";
                execCallback(CallbackModes.TASK, taskId, cmd.getName(), tasksStatus[taskId]);
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

        public static void execCallback(CallbackModes mode, params object[] objArray)
        {
            try
            {
                string data = null;
                if (objArray != null & objArray.Length == 1 && (objArray[0] is string || objArray[0] is String))
                {
                    data = objArray[0] as string;
                }
                else
                {
                    data = ArmaToolbox.convert2Arma(objArray);
                }

                if (ArmaAPI.callback != null)
                {
                    // Multipart messages are identified by an underscore (_) and a random identifier
                    string multipartId = null;

                    // debug test
                    while (data.Length >= outputSize)
                    {
                        if (multipartId == null)
                            multipartId = "_" + Convert.ToString(new Random().Next(999999));

                        string part = data.Substring(0, outputSize);
                        data = data.Substring(outputSize);

                        ArmaAPI.callback.Invoke("TBModExtension", "MULTIPART".ToLower() + multipartId, part);
                    }

                    ArmaAPI.callback.Invoke("TBModExtension", Enum.GetName(typeof(CallbackModes), mode).ToLower() + multipartId, data);
                }
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

        private static int errorReturn(ErrorCode errorCodes, StringBuilder sb)
        {
            sb.Append(string.Format("ERROR: {0}", errorCodes));
            int errorInt = (int)errorCodes;

            // critical error, blocking input
            if (!error && errorInt <= (int)ErrorCode.CRITICAL_BLOCKING)
                error = true;

            return errorInt;
        }

    }
}
