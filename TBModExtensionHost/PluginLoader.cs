using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TBModExtensionHost.PluginAPI;
using TBModExtensionHost.Tools;

namespace TBModExtensionHost
{
    internal class PluginLoader
    {
        public ConcurrentDictionary<string, Plugin> plugins = new ConcurrentDictionary<string, Plugin>();
        public Dictionary<string, bool> userEnabledSetting = new Dictionary<string, bool>();
        private const int NEEDED_TRUSTLVL = 1; //3

        //public volatile Dictionary<string, Version> aliasAPIs = new Dictionary<string, Version>();
        //public ConcurrentDictionary<string, Assembly> embeddedDlls = new ConcurrentDictionary<string, Assembly>();
        //public ConcurrentDictionary<string, List<TBModExtensionCommand>> apiFncs = new ConcurrentDictionary<string, List<TBModExtensionCommand>>();

        public string loadPlugin(string filePath, ref StringBuilder outputSb)
        {
            // get the name of the dll without extension
            string dllName = new FileInfo(filePath).Name.Replace(".dll", "").ToLower();

            try
            {
                // load Host normal but plugins as reference
                Assembly dll = null;
                if (Assembly.GetExecutingAssembly().Location.Equals(filePath))
                {
                    dll = Assembly.LoadFile(filePath);
                }
                else
                {
                    dll = Assembly.Load(File.ReadAllBytes(filePath));
                }

                if (!checkSignedPlugin(dll, filePath))
                {
                    outputSb.AppendLine(string.Format(" - Cant load plugin '{0}' trustlevel not high enough [path: {1}]", dllName, filePath));
                    return null;
                }

                // load plugin informations
                Plugin newPlugin = new Plugin(dll);
                if (!newPlugin.fetchFromApi())
                    return null; // DLL hat keine API Klasse

                // check if the version is there or newer
                plugins.TryGetValue(newPlugin.alias, out Plugin loadedPlugin);
                if (loadedPlugin != null && loadedPlugin.version >= newPlugin.version)
                {
                    outputSb.AppendLine(string.Format("- Plugin '{0}' is allready loaded [Alias: {1}, Version: {2}, location: {3}]", dllName, newPlugin.alias, newPlugin.version, filePath));
                    return null;
                }

                // check if user disabled this plugin
                if (!"tbmodextensionhost_x64".Equals(dllName, StringComparison.OrdinalIgnoreCase) && !checkSettingEnabled(dllName))
                {
                    outputSb.AppendLine(string.Format("- Plugin '{0}' is disabled by the user [Alias: {1}, Version: {2}, location: {3}]", dllName, newPlugin.alias, newPlugin.version, filePath));
                    return null;
                }

                outputSb.AppendLine(string.Format("- Plugin '{0}' successful {4}loaded [Alias: {1}, Version: {2}, location: {3}]", dllName, newPlugin.alias, newPlugin.version, filePath, loadedPlugin != null ? "re" : ""));
                plugins.AddOrUpdate(newPlugin.alias, newPlugin, (k, v) => newPlugin);

                // execute SQF code
                if (newPlugin.initCode != null)
                    HostAPI.execCallback(CallbackModes.CALL, newPlugin.initCode);

                return newPlugin.initCode;
            }
            catch (Exception e)
            {
                string msg = string.Format("- Extension '{0}' cant be loaded -> {1}", dllName, e.Message);
                outputSb.AppendLine(msg);
                Logger.logError(msg, e);
                return null;
            }
        }

        /// <summary>
        /// load file for enabled checks
        /// </summary>
        public void loadPluginEnableFile()
        {
            if (!File.Exists("TBModExtention_userEnabledSetting.json"))
                return;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;

            userEnabledSetting = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText("TBModExtention_userEnabledSetting.json"), settings);
        }

        /// <summary>
        /// load file for enabled checks
        /// </summary>
        public void savePluginEnableFile()
        {
            File.WriteAllText("TBModExtention_userEnabledSetting.json", JsonConvert.SerializeObject(userEnabledSetting));
        }

        /// <summary>
        /// Check if dll is allowed
        /// </summary>
        public bool checkSettingEnabled(string dllName)
        {
            // no setting set it to default
            if (!userEnabledSetting.ContainsKey(dllName))
                userEnabledSetting.Add(dllName, true);

            return userEnabledSetting[dllName];
        }

        /// <summary>
        /// Check if the assembly is a verified one
        /// </summary>
        public bool checkSignedPlugin(Assembly dll, string filePath)
        {
            int trustLevel = 0;

            // check the signature of the file
            if (SignUtils.checkSignature(filePath))
                trustLevel += 1;
            else
                HostAPI.execCallback(CallbackModes.DEBUG, string.Format("[DEBUG][checkSignedPlugin] SignUtils.checkSignature failed for: {0}", filePath)); // FIXME: muss wieder raus

            // check the file cert
            if (SignUtils.checkFile(filePath))
                trustLevel += 1;
            else
                HostAPI.execCallback(CallbackModes.DEBUG, string.Format("[DEBUG][checkSignedPlugin] SignUtils.checkFile failed for: {0}", filePath)); // FIXME: muss wieder raus

            // check strongname of dll
            if (SignUtils.checkWithWindowsLib(filePath))
                trustLevel += 1;
            else
                HostAPI.execCallback(CallbackModes.DEBUG, string.Format("[DEBUG][checkSignedPlugin] SignUtils.checkWithWindowsLib failed for: {0}", filePath)); // FIXME: muss wieder raus

            return trustLevel >= NEEDED_TRUSTLVL;
        }

        /// <summary>
        /// Tries to get a command from the apiFncs
        /// </summary>
        public TBModExtensionCommand getCmd(string alias, string fncName)
        {
            plugins.TryGetValue(alias, out Plugin plugin);
            if (plugin == null)
                return null;

            return plugin.commands.FirstOrDefault(cmd => cmd.getName().Equals(fncName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Tries to load an embeddedDll from an plugin
        /// </summary>
        public Assembly pluginAssemblyResolve(string name)
        {
            string dllName = (new AssemblyName(name).Name + ".dll").ToLower();

            if (Assembly.GetExecutingAssembly().GetName().Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                return Assembly.GetExecutingAssembly();

            Plugin plugin = plugins.Values.FirstOrDefault(x => x.embeddedDlls.Any(y => y.Key.ToLower().Equals(dllName, StringComparison.CurrentCultureIgnoreCase)));
            if (plugin != null && plugin.embeddedDlls != null)
                return plugin.embeddedDlls[dllName];

            return null;
        }

    }
}
