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
        protected static long taskIdCounter = 0;
        public static ExtensionCallback callback;
        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);

        #if WIN64
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
                output.Append("TBModExtension wurde initalisiert und Callback erstellt!");
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
                args = args != null ? args : new string[] {};

                if (function.Length > 0)
                {
                    // syncrone FNCs
                    int result = SyncFncs.execute(function, args);
                    if (result != 0)
                        return result;

                    switch (function)
                    {
                        case "status":
                            long taskId;

                            if (argCount >= 1 && long.TryParse(args[0], out taskId) && tasksStatus.ContainsKey(taskId))
                            {
                                output.Append(tasksStatus[taskId]);
                                return 1;
                            }
                            break;
                        default:
                            // TaskId hochzählen und als ersten Value übergeben
                            Interlocked.Increment(ref taskIdCounter);
                            if (!tasksStatus.TryAdd(taskIdCounter, "QUEUE"))
                                return -3;
                            List<String> values = new List<string>();
                            values.Add(taskIdCounter.ToString());
                            values.Add(function);
                            values.AddRange(args);

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

            return 0;
        }

        public static void execCallback(string fnc, params object[] obj)
        {
            execCallback(fnc, ArmaString.toAry(obj));
        }

        public static void execCallback(string fnc, string data)
        {
            if (callback != null)
                callback.Invoke("TBModExtension", fnc, data);
        }

        private static void processInput(object input)
        {
            try
            {
                List<string> args = input as List<String>;
                long taskId = long.Parse(args[0]);
                string funktion = args[1];
                args.RemoveRange(0, 2);

                // async FNCs
                tasksStatus[taskId] = AsyncFncs.execute(funktion, taskId, args) >= 0 ? "DONE" : "ERROR";
                execCallback("task", taskId, tasksStatus[taskId]);
            }
            catch (Exception e)
            {
                logError(e);
            }
        }

        private static void logError(Exception e)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                string text = string.Format("[{0}][ERROR] {1} -> {2}\n", DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss"), e.Message, e);
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
