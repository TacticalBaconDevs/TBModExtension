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
        private const char SEPARATOR = (char)007;

        public static string getString(string text)
        {
            return text.Trim().Substring(1, text.Trim().Length - 2).Trim();
        }

        public static object[] getArray(string text)
        {
            if (text.Contains(SEPARATOR))
                return new object[] { }; // input wird abgelehnt

            string array = text.Trim().Substring(1, text.Trim().Length - 2).Trim();
            char[] charArray = array.ToCharArray();

            // Kommas in Strings von den richtigen Separatoren unterscheiden
            char inString = ' ';
            for (int i = 0; i < charArray.Length; i++)
            {
                char zeichen = charArray[i];

                // Stringanfang mit " oder '
                if (inString == ' ' && (zeichen == '"' || zeichen == '\''))
                    inString = zeichen;

                // Stringende zum passenden Stringanfang
                if (inString != ' ' && zeichen == inString)
                    inString = ' ';

                // Alle Kommas 
                if (zeichen == ',' && inString == ' ')
                    charArray[i] = SEPARATOR;
            }

            return new String(charArray).Split(SEPARATOR).Select(element => convert2C(element.Trim())).ToArray();
        }

        public static string toString(string text)
        {
            return '"' + text + '"';
        }

        public static string toStrArray(params string[] arguments)
        {
            if (arguments == null)
                return "[]";

            return "[" + String.Join(",", arguments.Select(element => toString(element)).ToArray()) + "]";
        }

        public static string toArray(params object[] arguments)
        {
            if (arguments == null)
                return "[]";

            return "[" + String.Join(",", arguments.Select(x => convert2Arma(x)).ToArray()) + "]";
        }

        public static object convert2Arma(object value)
        {
            if (value == null)
                return "nil";
            if (value is string || value is String)
                return toString(value as string);
            if (value is object[])
                return toArray(value as object[]);
            if (value is bool || value is Boolean)
                return ((bool)value ? "true" : "false");

            return value;
        }

        public static object convert2C(string value)
        {
            if (value == "nil")
                return null;
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return getString(value);
            if (value.StartsWith("[") && value.EndsWith("]"))
                return getArray(value);
            if (value == "true" || value == "false")
                return value == "true";
            
            return Convert.ToDouble(value.Replace(".", ","));
        }
    }
}
