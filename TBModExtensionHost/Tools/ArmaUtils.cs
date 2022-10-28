using System;
using System.Linq;

namespace TBModExtensionHost.Tools
{
    public class ArmaUtils
    {
        private const char SEPARATOR = (char)007;

        public static string convert2Arma(object value)
        {
            try
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
            catch (Exception e)
            {
                Logger.logError(string.Format("Problem bei convert2Arma beim Wert: {0}", value), e);
                return null;
            }
        }

        public static object convert2C(string value)
        {
            try
            {
                if (value == "" || value == "nil" || value == "any")
                    return null;
                if (value.StartsWith("[") && value.EndsWith("]"))
                    return convertArma2Array(value);
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    return convertArma2String(value);
                if (value == "true" || value == "false")
                    return value == "true";
                if (value.Contains("."))
                    return Convert.ToDouble(value.Replace(".", ","));

                return Convert.ToInt64(value);
            }
            catch (Exception e)
            {
                Logger.logError(string.Format("Problem bei convert2C beim Wert: {0}", value), e);
                return null;
            }
        }

        private static string convertArma2String(string text)
        {
            return text.Trim().Substring(1, text.Trim().Length - 2).Trim();
        }

        /*
         * Ungelöst: "[\"deadcauses\",\"PVP_KILLED\",\"Gen. Shukari(B_Soldier_lite_F) von Gen. Shukari(B_Soldier_lite_F) durch #scripted  --->  [bob2,\"\"#scripted\"\",bob2,<NULL-object>]\"]"
         */
        private static object[] convertArma2Array(string text)
        {
            if (text.Contains(SEPARATOR))
                return new object[] { }; // input wird abgelehnt

            string array = text.Trim().Substring(1, text.Trim().Length - 2).Trim();
            char[] charArray = array.ToCharArray();

            if (charArray.Length == 0)
                return new object[] { }; // leerer array

            // Kommas in Strings von den richtigen Separatoren unterscheiden
            char stringStart = ' ';
            int arrayInArray = 0;
            bool arrayInStringIgnore = false;
            for (int i = 0; i < charArray.Length; i++)
            {
                char zeichen = charArray[i];

                // Stringanfang mit " oder '
                if (stringStart == ' ' && (zeichen == '"' || zeichen == '\''))
                {
                    stringStart = zeichen;
                    continue;
                }

                // Stringende zum passenden Stringanfang
                if (stringStart != ' ' && zeichen == stringStart && !arrayInStringIgnore)
                {
                    stringStart = ' ';
                    continue;
                }

                // in einem String ein Array
                if (stringStart != ' ' && (zeichen == '[' || zeichen == ']'))
                {
                    arrayInStringIgnore = zeichen == '[';
                    continue;
                }

                // in einem Array ein Array
                if (stringStart == ' ' && (zeichen == '[' || zeichen == ']'))
                {
                    arrayInArray += zeichen == '[' ? 1 : -1;
                    continue;
                }

                // derzeit in Subarray
                if (arrayInArray != 0)
                    continue;

                // Alle Kommas 
                if (zeichen == ',' && stringStart == ' ' && !arrayInStringIgnore)
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
