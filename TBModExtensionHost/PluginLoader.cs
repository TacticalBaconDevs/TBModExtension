using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TBModExtensionHost.PluginAPI;
using TBModExtensionHost.Tools;

namespace TBModExtensionHost
{
    internal class PluginLoader
    {
        public volatile List<string> loadedAPIs = new List<string>();
        public volatile List<string> aliasAPIs = new List<string>();
        public ConcurrentDictionary<string, Assembly> embeddedDlls = new ConcurrentDictionary<string, Assembly>();
        public ConcurrentDictionary<string, List<TBModExtensionCommand>> apiFncs = new ConcurrentDictionary<string, List<TBModExtensionCommand>>();

        public void addCmd(string alias, TBModExtensionCommand command)
        {
            List<TBModExtensionCommand> commands = apiFncs.GetOrAdd(alias, new List<TBModExtensionCommand>());
            commands.Add(command);
        }

        public TBModExtensionCommand getCmd(string alias/*, bool isSync*/, string fncName)
        {
            if (!apiFncs.TryGetValue(alias, out List<TBModExtensionCommand> commands))
                return null;

            return commands.FirstOrDefault(cmd => /*cmd.isSync() == isSync &&*/ cmd.getName().Equals(fncName, StringComparison.OrdinalIgnoreCase));
        }

        public Assembly pluginAssemblyResolve(string name)
        {
            string dllName = (new AssemblyName(name).Name + ".dll").ToLower();

            if (embeddedDlls.ContainsKey(dllName))
                return embeddedDlls[dllName];

            return null;
        }

        public bool checkSignedPlugin(Assembly DLL, string filePath)
        {
            int trustLevel = 0;

            try
            {
                Signature signature = getSignature(filePath);
                if (signature != null)
                {
                    if (signature.Status == SignatureStatus.NotTrusted && "CN=TacticalBaconRootCA, O=TacticalBacon.de, OU=TacticalBaconDevs, E=kontakt@tacticalbacon.de".Equals(signature.SignerCertificate.Issuer) &&
                            "7F225134E91038284C1E69C7CB896077D04BB7FD".Equals(signature.SignerCertificate.Thumbprint))
                        trustLevel += 1;
                    if (signature.Status == SignatureStatus.Valid)
                        trustLevel += 2;
                }

                // check the signature first
                byte[] key = DLL.GetName().GetPublicKeyToken();
                if (key.Length > 0 && "912aa95efe157609".Equals(getHash(key)))
                    trustLevel += 1;
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }

            return trustLevel >= 2;
        }

        private string getHash(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }

        public static Signature getSignature(string filepath)
        {
            using (Runspace runspace = RunspaceFactory.CreateRunspace(RunspaceConfiguration.Create()))
            {
                runspace.Open();
                using (var pipeline = runspace.CreatePipeline())
                {
                    pipeline.Commands.AddScript("Get-AuthenticodeSignature -FilePath \"" + filepath + "\"");
                    var results = pipeline.Invoke();
                    runspace.Close();

                    var signature = results[0].BaseObject as Signature;
                    return signature == null || signature.SignerCertificate == null ? null : signature;
                }
            }
        }

        public string loadPlugin(string dllName, string filePath, ref StringBuilder sb)
        {
            if (loadedAPIs.Contains(dllName))
            {
                sb.AppendLine(string.Format(" - Cant load plugin '{0}' is already loaded [path: {1}]", dllName, filePath));
                return null;
            }

            // Extension vars
            string extensionAlias = null;
            string initCode = null;

            try
            {
                Assembly DLL = Assembly.LoadFile(filePath);

                if (!checkSignedPlugin(DLL, filePath))
                {
                    sb.AppendLine(string.Format(" - Cant load plugin '{0}' trustlevel not high enough [path: {1}]", dllName, filePath));
                    return null;
                }

                Type typeAPI = DLL.GetTypes().FirstOrDefault(type => typeof(TBModExtensionAPI).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);
                TBModExtensionAPI extensionAPI = typeAPI != null ? Activator.CreateInstance(typeAPI) as TBModExtensionAPI : null;
                //extensionAPI = extensionAPI == null ? DLL.CreateInstance(typeAPI.FullName) as TBModExtensionAPI : extensionAPI;
                if (extensionAPI != null)
                {
                    extensionAlias = extensionAPI.getAlias().ToLower();

                    foreach (KeyValuePair<string, Assembly> dependencies in extensionAPI.loadPluginDependencies())
                        embeddedDlls.TryAdd(dependencies.Key, dependencies.Value);

                    initCode = extensionAPI.initAPI();
                }
                else
                {
                    // DLL hat keine API Klasse
                    return null;
                }

                // Commands laden
                foreach (Type typeCmd in DLL.GetTypes().Where(type => typeof(TBModExtensionCommand).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).ToList())
                {
                    TBModExtensionCommand command = Activator.CreateInstance(typeCmd) as TBModExtensionCommand;
                    //command = command == null ? DLL.CreateInstance(typeCmd.FullName) as TBModExtensionCommand : command;
                    addCmd(extensionAlias, command);
                }

                // finalise loading
                loadedAPIs.Add(dllName);
                aliasAPIs.Add(extensionAlias);
                sb.AppendLine(string.Format("- Plugin '{0}' successful loaded [Alias: {1}, location: {2}]", dllName, extensionAlias, filePath));

                // execute SQF code
                if (initCode != null)
                    HostAPI.execCallback(CallbackModes.CALL, initCode);
                
                return initCode;
            }
            catch (Exception e)
            {
                string msg = string.Format("- Extension '{0}' cant be loaded -> {1}", dllName, e.Message);
                sb.AppendLine(msg);
                Logger.logError(msg, e);
                return null;
            }
        }

    }
}
