using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TBModExtensionHost
{
    /*
     * API to the Arma3 process
     * Infos:   https://community.bistudio.com/wiki/Extensions
     *          https://community.bistudio.com/wiki/callExtension
     *          
     * Tester:  http://killzonekid.com/arma-3-extension-tester-callextension-exe-callextension_x64-exe/
     *          http://killzonekid.com/callextension-v2-0/
     *     
     * DllExport:
     *          #if WIN64
     *          #else
     *          #endif
     *      [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("_RVExtensionRegisterCallback@4", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
     *      [DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
     */
    class ArmaAPI
    {
        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        public static ExtensionCallback callback;

        [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            output.Append(string.Format("TBModExtensionHost v{0}", Assembly.GetExecutingAssembly().GetName().Version));
        }

        [DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
        public static void RVExtensionRegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)] ExtensionCallback func)
        {
            callback = func;
        }

        [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
        public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            output.Append("Te123st");
        }

        [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
        public static int RvExtensionArgs(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
        {
            output.Append("Te456st");
            return 1;
        }

    }
}
