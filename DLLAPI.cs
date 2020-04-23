using RGiesecke.DllExport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBModExtension
{
    class DLLAPI
    {
        protected static bool error = false;
        protected static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public static ConcurrentDictionary<long, string> tasksStatus = new ConcurrentDictionary<long, string>();
        public static ConcurrentDictionary<string, List<MethodInfo>> apiFncs = new ConcurrentDictionary<string, List<MethodInfo>>();
        protected static long taskIdCounter = 0;
        public static ExtensionCallback callback;
        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        private static bool loaded = false;
        private static List<string> loadedAPIs = new List<string>();

#       if WIN64
         [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
        #else
            [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
        #endif
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            output.Append("TBModExtension v"+ Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        #if WIN64
            [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
        #else
            [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
        #endif
        public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            try
            {
                if (loaded)
                {
                    output.Append("TBModExtension wurde bereits geladen!");
                    return;
                }

                output.Append("TBModExtension wurde initalisiert und Callback erstellt!");
                loaded = true;

                List<String> files = new List<string>();
                files.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), "TBModExtension_*.dll", SearchOption.TopDirectoryOnly));
                files.AddRange(Directory.GetFiles(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "TBModExtension_*.dll", SearchOption.TopDirectoryOnly));

                foreach (string filePath in files)
                {
                    string dllName = new FileInfo(filePath).Name.Replace(".dll", "");
                    if (dllName == "TBModExtension_x64" || loadedAPIs.Contains(dllName.ToLower()))
                        continue;
                    
                    try
                    {
                        Assembly DLL = Assembly.LoadFile(filePath);
                        bool apiFound = false;

                        foreach (Type apiClass in DLL.GetTypes())
                        {
                            if (apiClass.Name.StartsWith("TBModExtensionAPI"))
                            {
                                apiFound = true;
                                int apiVersion = (int) apiClass.GetField("apiVersion").GetValue(null);
                                string dllVersion = (string) apiClass.GetField("version").GetValue(null);
                                
                                switch (apiVersion)
                                {
                                    case 1:
                                        if (!apiFncs.ContainsKey("syncFncsV1"))
                                            apiFncs["syncFncsV1"] = new List<MethodInfo>();
                                        if (!apiFncs.ContainsKey("assyncFncsV1"))
                                            apiFncs["assyncFncsV1"] = new List<MethodInfo>();

                                        apiFncs["syncFncsV1"].Add(apiClass.GetMethod("syncFncs"));
                                        apiFncs["assyncFncsV1"].Add(apiClass.GetMethod("assyncFncs"));

                                        execCallback("call", (string)apiClass.GetMethod("init").Invoke(null, null));
                                        break;
                                    default:
                                        throw new Exception("APIVersion wird nicht unterstützt");
                                }

                                loadedAPIs.Add(dllName.ToLower());
                                output.Append(String.Format(" - Laden der Extension '{0}' erfolgreich [Version: {1}, APIVersion: {2}, Ort: {3}]", dllName, dllVersion, apiVersion, filePath));
                            }
                        }

                        if (!apiFound)
                            throw new Exception("TBModExtensionAPI Klasse nicht gefunden!");
                    }
                    catch (Exception)
                    {
                        output.Append(String.Format(" - Laden der Extension '{0}' fehlgeschlagen", dllName));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                logError(e);
            }
        }

        #if WIN64
            [DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
        #else
            [DllExport("_RVExtensionRegisterCallback@4", CallingConvention = CallingConvention.Winapi)]
        #endif
        public static void RVExtensionRegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)] ExtensionCallback func)
        {
            callback = func;
        }

        #if WIN64
            [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
        #else
            [DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
        #endif
        public static int RvExtensionArgs(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
        {
            try
            {
                if (error)
                    return -10;

                function = function != null ? function.ToLower() : "";
                args = args != null ? args : Array.Empty<string>();
                //Console.WriteLine("Input: "+ String.Join(",", args));
                object[] cArgs = args.Select(x => ArmaString.convert2C(x)).ToArray();

                if (function.Length > 0)
                {
                    // syncrone FNCs
                    int result = SyncFncs.execute(function, cArgs, execCallbackAry);
                    if (result != 0)
                    {
                        output.Append(result > 0 ? "DONE" : "ERROR");
                        return result;
                    }
                    
                    // wichtige reservierte CMDs
                    switch (function)
                    {
                        case "status":
                            if (argCount != 1 && !(cArgs[0] is double))
                            {
                                execCallback("error", "status: benötigt einen Parameter, taskId (Ganzzahl)");
                                output.Append("ERROR");
                                return -1;
                            }
                            long taskId = Convert.ToInt64(cArgs[0]);

                            if (tasksStatus.ContainsKey(taskId))
                            {
                                output.Append(tasksStatus[taskId]);
                                return 1;
                            }
                            else
                            {
                                output.Append("UNKNOWN_TASKID");
                                return -1;
                            }
                        case "check":
                            if (argCount != 1 && !(cArgs[0] is string))
                            {
                                execCallback("error", "check: benötigt einen Parameter, dllName (String)");
                                output.Append("ERROR");
                                return -1;
                            }
                            string dllName = (cArgs[0] as string).ToLower().Replace(".dll", "");
                            bool knownDll = loadedAPIs.Contains(dllName) || loadedAPIs.Contains("TBModExtension_".ToLower() + dllName);
                            output.Append(knownDll ? "DONE" : "ERROR");
                            return knownDll ? 1 : -1;
                        default:
                            // TaskId hochzählen und als ersten Value übergeben
                            Interlocked.Increment(ref taskIdCounter);
                            if (!tasksStatus.TryAdd(taskIdCounter, "QUEUE"))
                                return -3;
                            List<object> values = new List<object>();
                            values.Add(taskIdCounter);
                            values.Add(function);
                            values.AddRange(cArgs);

                            output.Append(taskIdCounter.ToString());
                            return ThreadPool.QueueUserWorkItem(new WaitCallback(processInput), values) ? 1 : 0;
                    }
                }
                else
                {
                    return -2;
                }
            }
            catch (Exception e)
            {
                logError(e);
                return -1;
            }
        }

        public static string getPath()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return path.Substring(0, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).LastIndexOf('\\'));
        }

        public static void execCallbackAry(string fnc, params object[] obj)
        {
            if (obj != null & obj.Length == 1 && (obj[0] is string || obj[0] is String))
            {
                execCallback(fnc, obj[0] as string);
            }
            else
            {
                execCallback(fnc, ArmaString.toArray(obj));
            }
        }

        public static void execCallback(string fnc, string data)
        {
            try
            {
                if (callback != null)
                    callback.Invoke("TBModExtension", fnc, data);
            }
            catch (Exception e)
            {
                logError(e);
            }
        }

        private static void setTaskStatus(long taskId, string status)
        {
            try
            {
                if (!tasksStatus.ContainsKey(taskId))
                tasksStatus[taskIdCounter] = status;
            }
            catch (Exception e)
            {
                logError(e);
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
                logError(e);
            }
        }

        public static void logError(Exception e)
        {
            logError(null, e);
        }

        public static void logError(string input, Exception e)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                string text = string.Format("[{0}][ERROR] {1} -> {2}\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), input == null ? e.Message : null, e);
                execCallback("error", text);
                File.AppendAllText("TBModExtention_ERRORS.log", text, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler: "+ ex);
                execCallback("error", ex.ToString());
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}
