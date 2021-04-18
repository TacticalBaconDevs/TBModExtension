using System;
using System.Linq;

namespace TBModExtensionHost.Tools
{
    public class ArmaToolbox
    {
        private const char SEPARATOR = (char)007;

        public static string convert2Arma(object value)
        {
            if (value == null)
                return "nil";
            if (value is string || value is String)
                return convertString2Arma(value as string);
            if (value is object[])
                return convertArray2Arma(value as object[]);
            if (value is bool || value is Boolean)
                return ((bool)value ? "true" : "false");

            return Convert.ToString(value);
        }

        public static object convert2C(string value)
        {
            if (value == "nil")
                return null;
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return convertArma2String(value);
            if (value.StartsWith("[") && value.EndsWith("]"))
                return convertArma2Array(value);
            if (value == "true" || value == "false")
                return value == "true";
            if (value.Contains("."))
                return Convert.ToDouble(value.Replace(".", ","));

            return Convert.ToInt64(value);
        }

        private static string convertArma2String(string text)
        {
            return text.Trim().Substring(1, text.Trim().Length - 2).Trim();
        }

        private static object[] convertArma2Array(string text)
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

            return new string(charArray).Split(SEPARATOR).Select(element => convert2C(element.Trim())).ToArray();
        }

        private static string convertString2Arma(string text)
        {
            return '"' + text + '"';
        }

        private static string convertArray2Arma(params object[] arguments)
        {
            if (arguments == null)
                return "[]";

            return "[" + string.Join(",", arguments.Select(x => convert2Arma(x)).ToArray()) + "]";
        }

    }
}
