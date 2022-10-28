using System;
using System.Text;

namespace TBModExtensionHost.PluginAPI
{
    /// <summary>
    /// Entrypoint for a command
    /// </summary>
    public abstract class TBModExtensionCommand
    {
        /// <summary>
        /// Arma can send max this bytes to the engine
        /// </summary>
        protected const int OUTPUT_SIZE = 10240 - 1;

        /// <summary>
        /// The name of the command
        /// </summary>
        /// <returns>command name</returns>
        public abstract string getName();

        /// <summary>
        /// Defines if this command is executed synchronous or asynchronous.<br/>
        /// true - is synchronous the command is executed immediately<br/>
        /// false - is asynchronous the command is executed later and the taskid gets returned
        /// </summary>
        /// <returns>if it is synchronous</returns>
        public abstract bool isSync();

        /// <summary>
        /// This method is executed if the command is called.<br/>
        /// - "output" is only defined for sync commands<br/>
        /// - "taskId" is only defined for async commands<br/><br/>
        /// Helper method for "argument" is "getArgsEntry"<br/>
        /// Helper method for "execCallback" is "callbackError"
        /// </summary>
        /// <param name="argument">the transfered argument can be null, an object or a object array</param>
        /// <param name="execCallback">this is the delagate to arma3 an can be invoked</param>
        /// <param name="output">!!! Only sync !!! the string output</param>
        /// <param name="taskId">!!! Only async !!! the taskId of the async task</param>
        /// <returns></returns>
        public abstract int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId);

        /// <summary>
        /// convenience function<br/>
        /// Example: <code>return callbackError(execCallback, "Error message");</code>
        /// </summary>
        public int callbackError(Action<CallbackModes, object[]> execCallback, string errorMsg)
        {
            execCallback.Invoke(CallbackModes.ERROR, new object[] { errorMsg });
            return -1;
        }

        /// <summary>
        /// convenience function<br/>
        /// Example: <code>string name = getArgsEntry(argument, 0);</code>
        /// </summary>
        /*public T getArgsEntry<T>(object argument, int entryId) where T : class
        {
            if (!(argument is object[]))
                return null;

            object[] args = argument as object[];
            if (entryId >= args.Length)
                return null;

            try
            {
                return args[entryId] as T;
            }
            catch (Exception)
            {
                return null;
            }
        }*/

        /// <summary>
        /// convenience function<br/>
        /// Example: <code>string name = getArgsEntry(argument, 0);</code>
        /// </summary>
        public T getArgsEntry<T>(object argument, int entryId)
        {
            if (!(argument is object[]))
                return default(T);

            object[] args = argument as object[];
            if (entryId >= args.Length)
                return default(T);

            try
            {
                return (T)args[entryId];
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
