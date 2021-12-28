using System;
using System.Text;
using TBModExtensionHost.PluginAPI;

namespace TBModExtensionHost.API
{
    class CmdTestString : TBModExtensionCommand
    {
        public override string getName() => "teststring";

        public override bool isSync() => true;

        public override int execute(object argument, Action<CallbackModes, object[]> execCallback, StringBuilder output, long taskId)
        {
            string testString = argument is string ? argument as string : null;
            if (testString == null)
                return callbackError(execCallback, "teststring: benötigt einen Parameter, testString (String)");

            testString = "benötigt einenß Parämßter öüä <|> => " + testString;

            execCallback.Invoke(CallbackModes.LOG, new object[] { testString, convertUtf16(testString, Encoding.UTF8), convertUtf16(testString, Encoding.Default) });

            output.AppendLine(testString);
            output.AppendLine(convertUtf16(testString, Encoding.UTF8));
            output.AppendLine(convertUtf16(testString, Encoding.Default));

            return 1;
        }

        public static string convertUtf16(string utf16String, Encoding toEncoding)
        {
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);
            byte[] bytes = Encoding.Convert(Encoding.Unicode, toEncoding, utf16Bytes);
            return toEncoding.GetString(bytes);
        }
    }
}
