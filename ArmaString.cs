using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TBModExtension
{
    class ArmaString
    {
        public static string getPath()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return path.Substring(0, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).LastIndexOf('\\'));
        }

        public static string getStr(string text)
        {
            return text.Trim().Replace("\"", "").Trim();
        }

        public static string[] getAry(string text)
        {
            return text.Trim().Replace("\"", "").Replace("[", "").Replace("]", "").Trim().Split(',').Select(element => element.Trim()).ToArray();
        }

        public static string toStr(string text)
        {
            return "\"" + text + "\"";
        }

        public static string toStrAry(params string[] arguments)
        {
            return "[" + String.Join(",", arguments.Select(element => toStr(element)).ToArray()) + "]";
        }

        public static string toAry(params object[] arguments)
        {
            arguments = arguments.Select(x => {
                if (x is string || x is String)
                    return toStr(x as string);
                if (x is object[])
                    return toAry(x as object[]);
                if (x is bool || x is Boolean)
                    return ((x as bool) ? "true" : "false");

                return x;
            }).ToArray();

            return "[" + String.Join(",", arguments) + "]";
        }
    }
}
