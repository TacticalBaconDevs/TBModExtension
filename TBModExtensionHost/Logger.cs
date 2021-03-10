using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TBModExtensionHost
{

    class Logger
    {
        protected static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        public static void logError(Exception e)
        {
            logError(null, e);
        }

        public static void logError(string input)
        {
            logError(input, null);
        }

        public static void logError(string input, Exception e)
        {
            readWriteLock.EnterWriteLock();
            try
            {
                if (input == null)
                    input = getErrorMessage(e);
                else
                    input += " - " + getErrorMessage(e);

                string text = string.Format("[{0}][ERROR] {1} -> {2}\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), input, e);
                HostAPI.execCallback("error", text);
                File.AppendAllText("TBModExtention_ERRORS.log", text, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler: " + ex);
                HostAPI.execCallback("error", ex.ToString());
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }

        public static string getErrorMessage(Exception exception)
        {
            if (exception == null)
                return "";

            string stackTrace = exception.StackTrace;
            if (stackTrace != null && stackTrace.Length != 0)
                stackTrace = " - Stacktrace:\n" + stackTrace;

            string innerEx = getErrorMessage(exception.InnerException);
            return string.Format("Error: {0}{1}{2}", exception.Message, stackTrace, innerEx != "" ? "\n" + innerEx : "");
        }

    }
}
