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
            return text.Trim().Substring(1, text.Trim().Length - 2).Trim();
        }

        public static object[] getAry(string text)
        {
            string array = text.Trim().Substring(1, text.Trim().Length - 2).Trim();
            
            //List<object> result = new List<object>();
            // TODO: Komma in Strings sind derzeit BÖSE

            return array.Split(',').Select(element => convert2C(element.Trim())).ToArray();
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
            if (arguments == null)
                return "";

            arguments = arguments.Select(x => convert2Arma(x)).ToArray();

            return "[" + String.Join(",", arguments) + "]";
        }

        public static object convert2Arma(object value)
        {
            if (value is string || value is String)
                return toStr(value as string);
            if (value is object[])
                return toAry(value as object[]);
            if (value is bool || value is Boolean)
                return ((bool)value ? "true" : "false");

            return value;
        }

        public static object convert2C(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return getStr(value);
            if (value.StartsWith("[") && value.EndsWith("]"))
                return getAry(value);
            if (value == "true" || value == "false")
                return value == "true";

            return Convert.ToDouble(value.Replace(".", ","));
        }
    }
}
