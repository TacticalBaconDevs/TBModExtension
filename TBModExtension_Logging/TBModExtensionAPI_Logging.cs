using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using TBModExtensionHost.PluginAPI;
using TBModExtensionHost.Tools;

namespace TBModExtension_Logging
{
    public class TBModExtensionAPI_Logging : TBModExtensionAPI
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        internal static Dictionary<string, string> logger = new Dictionary<string, string>();

        public override string init()
        {
            return @"diag_log ""TBModExtension-Logging wurde initialisiert!"";";
        }

        public override string getAlias()
        {
            return "logging";
        }

        protected override Assembly getAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        internal static bool log2File(string loggername, string type, string msg)
        {
            if (!logger.ContainsKey(loggername.ToLower()))
                return false;

            _readWriteLock.EnterWriteLock();
            try
            {
                string text = string.Format("[{0}][{1}] {2}\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), type, msg);
                File.AppendAllText(logger[loggername], text, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }

            return true;
        }
    }
}
