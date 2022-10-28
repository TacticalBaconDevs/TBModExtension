using System.Reflection;

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
    public interface ITBModExtensionAPI
    {
        /// <summary>
        /// Version of your plugin DLL
        /// </summary>
        string getPluginVersion();

        /// <summary>
        /// The alias which connects fnc to this API plugin
        /// </summary>
        /// <returns></returns>
        string getCallAlias();

        /// <summary>
        /// SQF code (unscheduled) executed if plugin is loaded
        /// </summary>
        /// <returns>SQF Code</returns>
        string init();

        /// <summary>
        /// This method is executed if a synchronous function is called<br/>
        /// synchronous function: input and ouput at the same time<br/>
        /// !!! Only fast and short functions should be used !!!
        /// </summary>
        /// <param name="args">arguments from Arma3</param>
        /// <param name="execCallback">response callback back to Arma - more in the interface description</param>
        /// <param name="output">direct result</param>
        /// <returns>0 - nothing happend, 1 = successful, -1 = failed</returns>
        //int syncFncs(object[] args, Action<string, object[]> execCallback, StringBuilder output);

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
        //int assyncFncs(object[] args, long taskId, Action<string, object[]> execCallback, Action<long, string> setTaskStatus);

        /// <summary>
        /// the assembly for sqf file searching in embedded resources
        /// </summary>
        /// <returns></returns>
        Assembly getAssembly();

        /// <summary>
        /// is called if plugin is loaded<br/>
        /// first it call the init() function<br/>
        /// after this it search for *.sqf files in the embedded files
        /// </summary>
        string initAPI();

        /// <summary>
        /// loading *.sqf files in the embedded files
        /// </summary>
        /// <returns>content of *.sqf files</returns>
        string loadSQFFiles();

        /// <summary>
        /// convenience function
        /// </summary>
        //int callbackError(Action<string, object[]> execCallback, string errorMsg);

    }
}
