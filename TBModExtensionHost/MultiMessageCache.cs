using System;
using System.Collections.Concurrent;

namespace TBModExtensionHost
{
    public class MultiMessageCache
    {
        private static ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

        public static bool addToCache(string key, string content)
        {
            return cache.TryAdd(key, content);
        }

        public static int getFromCache(string key, out string output)
        {
            bool result = cache.TryGetValue(key, out string content);
            if (result && content.Length >= HostAPI.OUTPUT_SIZE)
            {
                string trimmed = content.Substring(0, HostAPI.OUTPUT_SIZE);
                string rest = content.Substring(HostAPI.OUTPUT_SIZE);
                cache.TryUpdate(key, rest, content);
                output = trimmed;
                return 1 + (int)Math.Ceiling((double)rest.Length / (double)HostAPI.OUTPUT_SIZE);
            }
            else
            {
                cache.TryRemove(key, out _);
                output = content;
                return result ? 1 : -1;
            }
        }
    }
}
