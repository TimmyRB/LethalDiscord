using BepInEx;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using UnityEngine;

namespace LethalDiscord
{
    [BepInDependency("ainavt.lc.lethalconfig")]
    internal class LethalConfigManager
    {
        public static LethalConfigManager Instance { get; private set; }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new LethalConfigManager();
            }
        }

        private LethalConfigManager()
        {
            LethalConfig.LethalConfigManager.SetModDescription("A mod that adds support for Discord's Rich Presence to share your game stats with friends!");

            BaseConfigItem discordUpdateInterval = new FloatSliderConfigItem(ConfigManager.Instance.DiscordUpdateInterval, new FloatSliderOptions
            {
                RequiresRestart = false,
                Min = 1f,
                Max = 10f,
            });

            BaseConfigItem showWaitingForPlayers = new BoolCheckBoxConfigItem(ConfigManager.Instance.ShowWaitingForPlayers, false);
            BaseConfigItem showPartyState = new BoolCheckBoxConfigItem(ConfigManager.Instance.ShowPartyState, false);
            BaseConfigItem showCurrentTime = new BoolCheckBoxConfigItem(ConfigManager.Instance.ShowCurrentTime, false);
            BaseConfigItem showProfitQuota = new BoolCheckBoxConfigItem(ConfigManager.Instance.ShowProfitQuota, false);

            BaseConfigItem allowDiscordJoins = new BoolCheckBoxConfigItem(ConfigManager.Instance.AllowDiscordJoins, false);
            BaseConfigItem mirrorPartyPrivacy = new BoolCheckBoxConfigItem(ConfigManager.Instance.MirrorPartyPrivacy, false);

            AddConfigItems(discordUpdateInterval, showWaitingForPlayers, showPartyState, showCurrentTime, showProfitQuota, allowDiscordJoins, mirrorPartyPrivacy);
        }

        private void AddConfigItems(params BaseConfigItem[] items)
        {
            foreach (BaseConfigItem item in items)
            {
                LethalConfig.LethalConfigManager.AddConfigItem(item);
            }
        }
    }
}
