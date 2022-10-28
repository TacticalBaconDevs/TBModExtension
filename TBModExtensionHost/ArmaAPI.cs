using RGiesecke.DllExport;
using System;
using System.Linq;
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
    internal class ArmaAPI
    {
        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        public static ExtensionCallback callback;

        [DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
        public static void RVExtensionRegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)] ExtensionCallback func)
        {
            callback = func;
            init();
        }

        [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            output.Append(string.Format("TBModExtensionHost v{0}", Assembly.GetExecutingAssembly().GetName().Version));
        }

        [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
        public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            HostAPI.handleRvExtension(output, outputSize, function);
        }

        [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
        public static int RvExtensionArgs(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
        {
            return HostAPI.handleRvExtensionArgs(output, outputSize, function, args, argCount);
        }

        private static void init()
        {
            // load embedded ressources
            loadEmbedded();

            // start own logic
            HostAPI.init();
        }

        private static void loadEmbedded()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
            {
                string dllName = new AssemblyName(bargs.Name).Name + ".dll";
                var assem = Assembly.GetExecutingAssembly();
                string resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName, StringComparison.CurrentCultureIgnoreCase));

                if (resourceName == null)
                    return null; // Not found, maybe another handler will find it

                // load own embedded dll
                using (var stream = assem.GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);
                }
            };
        }

    }
}
