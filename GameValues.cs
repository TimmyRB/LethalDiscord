using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace LethalDiscord
{

    public enum LethalDiscordActivityType
    {
        MainMenu,
        WaitingForPlayers,
        InOrbit,
        OnMoon,
        Ejected,
    }

    internal class GameValues
    {
        private static string? lobbySecret;

        private static int maxPartySize = 1;

        private static long roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static LethalDiscordActivityType GetActivityType()
        {
            if (StartOfRound.Instance != null)
            {
                if (StartOfRound.Instance.firingPlayersCutsceneRunning)
                {
                    return LethalDiscordActivityType.Ejected;
                }
                else if (ConfigManager.Instance.ShowWaitingForPlayers.Value && (!GameNetworkManager.Instance?.gameHasStarted ?? false))
                {
                    return LethalDiscordActivityType.WaitingForPlayers;
                }
                else if (StartOfRound.Instance.inShipPhase)
                {
                    return LethalDiscordActivityType.InOrbit;
                }

                return LethalDiscordActivityType.OnMoon;
            }
            else
            {
                return LethalDiscordActivityType.MainMenu;
            }
        }

        public static string GetActivityName()
        {
            return GetActivityType() switch
            {
                LethalDiscordActivityType.MainMenu => "Main Menu",
                LethalDiscordActivityType.WaitingForPlayers => "Waiting for Players",
                LethalDiscordActivityType.InOrbit => "Orbiting " + GetMoonName(),
                LethalDiscordActivityType.OnMoon => "Exploring " + GetMoonName(),
                LethalDiscordActivityType.Ejected => "Getting fired",
                _ => "Unknown",
            };
        }

        public static string? GetPartyState()
        {
            if (!ConfigManager.Instance.ShowPartyState.Value)
            {
                return null;
            }

            if (ConfigManager.Instance.ShowCurrentTime.Value && GetActivityType() == LethalDiscordActivityType.OnMoon && HUDManager.Instance != null && TimeOfDay.Instance != null)
            {
                return HUDManager.Instance.SetClock(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, createNewLine: false);
            }
            else if (ConfigManager.Instance.ShowProfitQuota.Value && GetActivityType() == LethalDiscordActivityType.InOrbit && StartOfRound.Instance != null && TimeOfDay.Instance != null)
            {
                float num = (float)StartOfRound.Instance.GetValueOfAllScrap() / (float)TimeOfDay.Instance.profitQuota * 100f;
                return $"{(int)num}% of quota | {TimeOfDay.Instance.daysUntilDeadline} days left";
            }

            return null;
        }

        public static string? GetLargeAssetImage()
        {
            switch (GetActivityType())
            {
                case LethalDiscordActivityType.MainMenu:
                    return "mainmenu";
                case LethalDiscordActivityType.WaitingForPlayers:
                case LethalDiscordActivityType.InOrbit:
                    return "orbit";
                case LethalDiscordActivityType.OnMoon:
                    return GetMoonName()?.ToLower().Replace(" ", "-");
                case LethalDiscordActivityType.Ejected:
                    return "fired";
                default:
                    return null;
            }
        }

        public static string? GetLargeAssetText()
        {
            switch (GetActivityType())
            {
                case LethalDiscordActivityType.MainMenu:
                    return "Main Menu";
                case LethalDiscordActivityType.WaitingForPlayers:
                case LethalDiscordActivityType.InOrbit:
                    return "In Orbit";
                case LethalDiscordActivityType.OnMoon:
                    return GetMoonName();
                case LethalDiscordActivityType.Ejected:
                    return "Ejected";
                default:
                    return null;
            }
        }

        public static string? GetSmallAssetImage()
        {
            switch (GetActivityType())
            {
                case LethalDiscordActivityType.InOrbit:
                    return GetMoonName()?.ToLower().Replace(" ", "-");
                default:
                    return null;
            }
        }

        public static string? GetSmallAssetText()
        {
            switch (GetActivityType())
            {
                case LethalDiscordActivityType.InOrbit:
                    return GetMoonName();
                default:
                    return null;
            }
        }

        public static string? GetMoonName()
        {
            switch (GetActivityType())
            {
                case LethalDiscordActivityType.InOrbit:
                case LethalDiscordActivityType.OnMoon:
                    return StartOfRound.Instance?.currentLevel.PlanetName;
                default:
                    return null;
            }
        }

        public static void StartNewRoundTimestamp()
        {
            roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long GetRoundStartTimestamp()
        {
            return roundStartTimestamp;
        }

        public static bool GetIsPartyJoinable()
        {
            if (ConfigManager.Instance.AllowDiscordJoins.Value)
            {
                return GetPartySize() < GetMaxPartySize();
            }

            return false;
        }

        public static string? GetPartyId()
        {
            return lobbySecret;
        }

        public static void SetPartyId(string? id)
        {
            lobbySecret = id;
        }

        public static string? GetPartyOwnerId()
        {
            return GameNetworkManager.Instance?.currentLobby?.Owner.Id.ToString();
        }

        public static ActivityPartyPrivacy GetPartyPrivacy()
        {
            if (ConfigManager.Instance.MirrorPartyPrivacy.Value)
            {
                return (GameNetworkManager.Instance?.lobbyHostSettings?.isLobbyPublic ?? false) ? ActivityPartyPrivacy.Public : ActivityPartyPrivacy.Private;
            }

            return ActivityPartyPrivacy.Public;
        }

        public static int GetPartySize()
        {
            return StartOfRound.Instance?.connectedPlayersAmount + 1 ?? 1;
        }

        public static int GetMaxPartySize()
        {
            return maxPartySize;
        }

        public static void SetMaxPartySize(int size)
        {
            maxPartySize = size;
        }
    }
}
