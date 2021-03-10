using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TBModExtensionPlugin
{
    /// <summary>
    /// TBModExtensionAPI Version1<br/>
    /// Your class must be named "TBModExtensionAPI" or "TBModExtensionAPI_v*"<br/>
    /// <br/>
    /// Explanation: execCallback<br/>
    /// parameter1: function name, special reserved function names are:<br/>
    ///                 - log / error - output at diag_log/systemChat | parameter2: anything appended to the message<br/>
    ///                 - call / spawn - gets executed | parameter2: code as string (example: "player setPos [0,0,0]")<br/>
    /// parameter2: your arguments<br/>
    ///                 - at the callback EH it gets the _data array with your arguments<br/>
    ///                 - EXCEPTION: if it is only a string then _data is also only a string
    /// </summary>
    public abstract class TBModExtensionAPI_v1
    {
        /// <summary>
        /// Version of this Interface
        /// </summary>
        public const int API_VERSION = 1;

        /// <summary>
        /// Version of your plugin DLL
        /// </summary>
        public static string pluginVersion = "1.0.0";

        /// <summary>
        /// The alias which connects fnc to this API plugin
        /// </summary>
        /// <returns></returns>
        public abstract string getCallAlias();

        /// <summary>
        /// SQF code (unscheduled) executed if plugin is loaded
        /// </summary>
        /// <returns>SQF Code</returns>
        public abstract string init();

        /// <summary>
        /// This method is executed if a synchronous function is called<br/>
        /// synchronous function: input and ouput at the same time<br/>
        /// !!! Only fast and short functions should be used !!!
        /// </summary>
        /// <param name="args">arguments from Arma3</param>
        /// <param name="execCallback">response callback back to Arma - more in the interface description</param>
        /// <param name="output">direct result</param>
        /// <returns>0 - nothing happend, 1 = successful, -1 = failed</returns>
        public abstract int syncFncs(object[] args, Action<string, object[]> execCallback, StringBuilder output);

        /// <summary>
        /// This method is executed if an asynchronous function is called<br/>
        /// asynchronous function: output with an additional synchronous function per taskid<br/>
        /// Running in a Threadpool, sleeps and breaks are possible
        /// </summary>
        /// <param name="args">arguments from Arma3</param>
        /// <param name="taskId">taskid</param>
        /// <param name="execCallback">response callback back to Arma - more in the interface description</param>
        /// <param name="setTaskStatus">set an additional status between QUEUE and DONE/ERROR - parameter1: taskId, parameter2: status (UPPERCASE)</param>
        /// <returns>0 - nothing happend (Status: DONE), 1 = successful (Status: DONE), -1 = failed (Status: ERROR)</returns>
        public abstract int assyncFncs(object[] args, long taskId, Action<string, object[]> execCallback, Action<long, string> setTaskStatus);

        /// <summary>
        /// the assembly for sqf file searching in embedded resources
        /// </summary>
        /// <returns></returns>
        public abstract Assembly getAssembly();

        /// <summary>
        /// is called if plugin is loaded<br/>
        /// first it call the init() function<br/>
        /// after this it search for *.sqf files in the embedded files
        /// </summary>
        public string initAPI()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(init());
            result.AppendLine(loadSQFFiles());

            return result.ToString();
        }

        /// <summary>
        /// loading *.sqf files in the embedded files
        /// </summary>
        /// <returns>content of *.sqf files</returns>
        private string loadSQFFiles()
        {
            StringBuilder result = new StringBuilder();
            Assembly assembly = getAssembly();

            foreach (string fileName in assembly.GetManifestResourceNames().Where(resourceName => resourceName.EndsWith(".sqf", StringComparison.CurrentCultureIgnoreCase)))
            {
                using (Stream stream = assembly.GetManifestResourceStream(fileName))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result.AppendLine(string.Format("diag_log 'File {0} loading...';", fileName));
                    result.AppendLine(reader.ReadToEnd());
                    result.AppendLine(string.Format("diag_log 'File {0} loaded!';", fileName));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// convenience function
        /// </summary>
        public int callbackError(Action<string, object[]> execCallback, string errorMsg)
        {
            execCallback.Invoke("error", new object[] { errorMsg });
            return -1;
        }

        /// <summary>
        /// invoke methode
        /// </summary>
        public static T invoke<T>(Type type, string methodenName, object[] args = null)
        {
            return (T)type.GetMethod(methodenName).Invoke(Activator.CreateInstance(type), args);
        }

    }
}
