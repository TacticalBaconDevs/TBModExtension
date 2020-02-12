using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBModExtension_Logging
{
    public class TBModExtensionAPI_v1
    {
        public const int apiVersion = 1;
        /// <summary>
        /// Die Version der anbietenden API DLL
        /// </summary>
        public readonly static string version = "1.0.0";
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private static Dictionary<string, string> logger = new Dictionary<string, string>();


        /// <summary>
        /// Wird aufgerufen wenn syncrone Functionen ausgelöst werden. Nichts was Lange dauert oder pausiert verwenden, nur schnelle Sachen
        /// </summary>
        /// <param name="fnc">Funktion die aufgerufen wird (IMMER LOWERCASE)</param>
        /// <param name="args">Argumente aus Arma3</param>
        /// <param name="execCallback">der Callback zurück zu Arma3 -  Parameter1: fncName, Parameter2: args</param>
        /// <returns>0 - nichts passiert, 1 = erfolgreich, -1 = fehlgeschlagen</returns>
        public static int syncFncs(string fnc, object[] args, Action<string, object[]> execCallback)
        {
            switch (fnc)
            {
                case "registerlogger":
                    if (args.Length != 2 || !(args[0] is string) || !(args[1] is string))
                        return callbackError(execCallback, "registerlogger: braucht 2 Parameter - loggerName (String) und FileName (String)");
                    string loggerName = (args[0] as string).ToLower();
                    
                    if (!logger.ContainsKey(loggerName))
                    {
                        logger.Add(loggerName, args[1] as string);
                        return 1;
                    }
                    else
                    {
                        return callbackError(execCallback, "registerlogger: '" + loggerName + "' ist bereits registriert");
                    }
            }

            return 0;
        }

        /// <summary>
        /// Wid aufgerufen, wenn keine syncrone Aufgabe gefunden wird. Läuft im Threadpool, kann also mit sleep usw arbeiten
        /// </summary>
        /// <param name="fnc">Funktion die aufgerufen wird (IMMER LOWERCASE)</param>
        /// <param name="args">Argumente aus Arma3</param>
        /// <param name="taskId">die Audgabennummer</param>
        /// <param name="execCallback">der Callback zurück zu Arma3 -  Parameter1: fncName, Parameter2: args</param>
        /// <param name="setTaskStatus">setzt den Status in der Laufzeit der Ausführung - Parameter1: taskId, Parameter2: Status (UPPERCASE)</param>
        /// <returns>0 - nichts passiert (Status: DONE), 1 = erfolgreich (Status: DONE), -1 = fehlgeschlagen (Status: ERROR)</returns>
        public static int assyncFncs(string fnc, object[] args, long taskId, Action<string, object[]> execCallback, Action<long, string> setTaskStatus)
        {
            switch (fnc)
            {
                case "logger":
                    if (args.Length != 3 || !(args[0] is string) || !(args[1] is string) || !(args[2] is string))
                        return callbackError(execCallback, "logger: braucht 3 Parameter - loggerName (String), Type (String), und Nachricht (String)");

                    return log2File(args[0] as string, args[1] as string, args[2] as string) ? 1 : -1;
            }

            return 0;
        }

        private static int callbackError(Action<string, object[]> execCallback, string errorMsg)
        {
            execCallback.Invoke("error", new object[] { errorMsg });
            return -1;
        }

        private static bool log2File(string loggername, string type, string msg)
        {
            if (!logger.ContainsKey(loggername.ToLower()))
                return false;

            _readWriteLock.EnterWriteLock();
            try
            {
                string text = string.Format("[{0}][{1}] {2}\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), type, msg);
                File.AppendAllText(logger[loggername], text, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler: " + ex);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }

            return true;
        }

    }
}
