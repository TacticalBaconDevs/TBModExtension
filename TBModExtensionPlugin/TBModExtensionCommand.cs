using System;
using System.Text;

namespace TBModExtensionPlugin
{
    public abstract class TBModExtensionCommand : ITBModExtensionCommand
    {
        protected const int outputSize = 10240 - 1;

        public abstract string getName();

        public abstract bool isSync();

        public abstract int execute(object argument, Action<string, object[]> execCallback, StringBuilder output, long taskId);

        /// <summary>
        /// convenience function
        /// </summary>
        public int callbackError(Action<string, object[]> execCallback, string errorMsg)
        {
            execCallback.Invoke("error", new object[] { errorMsg });
            return -1;
        }

        /// <summary>
        /// convenience function
        /// </summary>
        public T getArgsEntry<T>(object argument, int entryId) where T : class
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
        }

        /// <summary>
        /// convenience function
        /// </summary>
        public T getArgsEntryStruct<T>(object argument, int entryId) where T : struct
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
