using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using UnityEngine;
using LethalDiscord.Managers;
using LethalDiscord.Components;
 
namespace LethalDiscord
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "com.jacobbrasil.LethalDiscord";
        private const string modName = "LethalDiscord";
        private const string modVersion = "1.4.0";

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource LOG;
        internal static bool IsDiscordRPCActive;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            ConfigManager.Init(Config);

            if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
            {
                LethalConfigManager.Init();
            }

            LOG = BepInEx.Logging.Logger.CreateLogSource(modName);
            LOG.LogInfo("LethalDiscord has been loaded!");

            CreateDiscordRPC();
        }

        public static void CreateDiscordRPC()
        {
            if (!IsDiscordRPCActive)
            {
                new GameObject("DiscordController").AddComponent<LethalDiscordRPC>();
                IsDiscordRPCActive = true;
            }
        }
    }
}
