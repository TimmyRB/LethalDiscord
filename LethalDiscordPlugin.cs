﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace LethalDiscord
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalDiscordPlugin : BaseUnityPlugin
    {
        private const string modGUID = "com.jacobbrasil.LethalDiscord";
        private const string modName = "LethalDiscord";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static LethalDiscordPlugin Instance { get; private set; }
        internal static ManualLogSource LOG;
        internal static bool IsDiscordRPCActive;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            LOG = BepInEx.Logging.Logger.CreateLogSource(modName);
            LOG.LogInfo("LethalDiscord has been loaded!");

            CreateDiscordRPC();

            harmony.PatchAll();
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
