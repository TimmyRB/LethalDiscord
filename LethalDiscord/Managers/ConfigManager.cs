using BepInEx.Configuration;

namespace LethalDiscord.Managers
{
    internal class ConfigManager
    {
        public static ConfigManager Instance { get; private set; }

        public static void Init(ConfigFile config)
        {
            if (Instance == null)
            {
                Instance = new ConfigManager(config);
            }
        }

        public ConfigEntry<float> DiscordUpdateInterval { get; private set; }

        public ConfigEntry<bool> ShowWaitingForPlayers { get; private set; }
        public ConfigEntry<bool> ShowPartyState { get; private set; }
        public ConfigEntry<bool> ShowCurrentTime { get; private set; }
        public ConfigEntry<bool> ShowProfitQuota { get; private set; }

        public ConfigEntry<bool> AllowDiscordJoins { get; private set; }
        public ConfigEntry<bool> MirrorPartyPrivacy { get; private set; }

        private ConfigManager(ConfigFile config)
        {
            DiscordUpdateInterval = config.Bind(new ConfigDefinition("General", "Discord Update Interval"), 2f, new ConfigDescription("The interval in seconds between Discord Rich Presence updates"));

            ShowWaitingForPlayers = config.Bind(new ConfigDefinition("Activity", "Show Waiting For Players"), false, new ConfigDescription("Should show whether player is in ship lobby waiting for players"));
            ShowPartyState = config.Bind(new ConfigDefinition("Activity", "Show Party State"), true, new ConfigDescription("Shows the current time or quota status"));
            ShowCurrentTime = config.Bind(new ConfigDefinition("Activity", "Show Current Time"), true, new ConfigDescription("Shows the current time when exploring a Moon, overriden by Show Party State"));
            ShowProfitQuota = config.Bind(new ConfigDefinition("Activity", "Show Profit Quota"), true, new ConfigDescription("Shows the current profit quota when orbiting a Moon, overriden by Show Party State"));

            AllowDiscordJoins = config.Bind(new ConfigDefinition("Lobby", "Allow Discord Joins"), true, new ConfigDescription("Allows players to join your lobby through Discord"));
            MirrorPartyPrivacy = config.Bind(new ConfigDefinition("Lobby", "Mirror Party Privacy"), false, new ConfigDescription("Mirrors your Lethal Company lobby privacy settings to Discord"));
        }
    }
}
