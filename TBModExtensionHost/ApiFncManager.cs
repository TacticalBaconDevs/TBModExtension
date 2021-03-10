using System;
using System.Collections.Concurrent;

namespace TBModExtensionHost
{
    class ApiFncManager
    {
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, Func<object[], int>>> apiFncs = new ConcurrentDictionary<string, ConcurrentDictionary<string, Func<object[], int>>>();

        public bool addFnc(string name, string alias, Func<object[], int> action)
        {
            ConcurrentDictionary<string, Func<object[], int>> fncs = getFncs(name);
            return fncs.TryAdd(alias, action);
        }

        public Func<object[], int> getFnc(string name, string alias)
        {
            ConcurrentDictionary<string, Func<object[], int>> fncs = getFncs(name);

            if (fncs.ContainsKey(alias))
                return fncs[alias];

            return null;
        }

        public ConcurrentDictionary<string, Func<object[], int>> getFncs(string name)
        {
            return apiFncs.GetOrAdd(name, new ConcurrentDictionary<string, Func<object[], int>>());
        }
    }
}
