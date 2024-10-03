using UnityEngine;
using Discord;
using System;

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

    public class LethalDiscordRPC : MonoBehaviour
    {
        public static LethalDiscordRPC Instance { get; private set; }
        private Discord.Discord DiscordClient;

        private LethalDiscordActivityType prevActivity = LethalDiscordActivityType.MainMenu;
        private LethalDiscordActivityType currentActivity = LethalDiscordActivityType.MainMenu;

        private string currentMoon;

        private string state;

        private string partyId;

        private int currentPartySize = 1;

        private int maxPartySize = 4;

        private bool isPartyJoinable = false;

        private string partyJoinSecret;

        private long roundStartTimestamp;

        private float timeAtLastStatusUpdate;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }

            DontDestroyOnLoad(this);

            LethalDiscordPlugin.LOG.LogInfo("LethalDiscordRPC has been loaded!");
        }

        private void Start()
        {
            DiscordClient = new Discord.Discord(1291262348975411311, (ulong)CreateFlags.NoRequireDiscord);

            ActivityManager activityManager = DiscordClient.GetActivityManager();
            activityManager.RegisterSteam(1966720);

            DiscordClient.SetLogHook(LogLevel.Debug, LogDiscord);

            roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (StartOfRound.Instance != null)
            {
                string levels = "";
                foreach (var level in StartOfRound.Instance.levels)
                {
                    levels += level.PlanetName + ", ";
                }
                LethalDiscordPlugin.LOG.LogInfo($"Possible Levels: {levels}");
            }
        }

        private void Update()
        {
            DiscordClient.RunCallbacks();
        }

        private void FixedUpdate()
        {
            if (StartOfRound.Instance != null)
            {
                if (StartOfRound.Instance.currentLevel != null)
                {
                    if (StartOfRound.Instance.firingPlayersCutsceneRunning)
                    {
                        SetPreviousActivity();
                        currentActivity = LethalDiscordActivityType.Ejected;
                    }
                    else if (GameNetworkManager.Instance != null && !GameNetworkManager.Instance.gameHasStarted)
                    {
                        SetPreviousActivity();
                        currentActivity = LethalDiscordActivityType.WaitingForPlayers;
                    }
                    else if (StartOfRound.Instance.inShipPhase)
                    {
                        SetPreviousActivity();
                        currentActivity = LethalDiscordActivityType.InOrbit;
                        currentMoon = StartOfRound.Instance.currentLevel.PlanetName;
                    }
                    else if (HUDManager.Instance != null)
                    {
                        state = HUDManager.Instance.SetClock(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, createNewLine: false);
                        SetPreviousActivity();
                        currentActivity = LethalDiscordActivityType.OnMoon;
                        currentMoon = StartOfRound.Instance.currentLevel.PlanetName;
                    }
                }
                else
                {
                    SetPreviousActivity();
                    currentActivity = LethalDiscordActivityType.MainMenu;
                    partyJoinSecret = null;
                    isPartyJoinable = false;
                }

                currentPartySize = StartOfRound.Instance.connectedPlayersAmount + 1;
                if (GameNetworkManager.Instance != null)
                {
                    maxPartySize = GameNetworkManager.Instance.maxAllowedPlayers;
                    isPartyJoinable = currentPartySize < maxPartySize;

                    if (GameNetworkManager.Instance.currentLobby.HasValue)
                    {
                        partyId = Convert.ToString(GameNetworkManager.Instance.currentLobby.Value.Owner.Id);
                        partyJoinSecret = GameNetworkManager.Instance.steamLobbyName;
                    }
                }

                if (RoundManager.Instance != null && StartOfRound.Instance.inShipPhase)
                {
                    float num = (float)StartOfRound.Instance.GetValueOfAllScrap() / (float)TimeOfDay.Instance.profitQuota * 100f;
                    state = $"{(int)num}% of quota | {TimeOfDay.Instance.daysUntilDeadline} days left";
                }
            }

            SetStatus();
        }

        public void SetStatus()
        {
            if (Time.realtimeSinceStartup - timeAtLastStatusUpdate < 2f)
                return;

            timeAtLastStatusUpdate = Time.realtimeSinceStartup;

            string details = "";

            string largeAsset = "";
            string largeAssetText = "";

            string smallAsset = "";
            string smallAssetString = "";

            switch (currentActivity)
            {
                case LethalDiscordActivityType.MainMenu:
                    details = "Main Menu";
                    largeAsset = "mainmenu";
                    largeAssetText = "Main Menu";
                    break;
                case LethalDiscordActivityType.WaitingForPlayers:
                    details = "Waiting for players";
                    largeAsset = "orbit";
                    largeAssetText = "Orbiting";
                    break;
                case LethalDiscordActivityType.InOrbit:
                    details = $"Orbiting {currentMoon}";
                    largeAsset = currentMoon.ToLower().Replace(" ", "-");
                    largeAssetText = currentMoon;
                    break;
                case LethalDiscordActivityType.OnMoon:
                    details = $"Exploring {currentMoon}";
                    largeAsset = currentMoon.ToLower().Replace(" ", "-");
                    largeAssetText = currentMoon;
                    break;
                case LethalDiscordActivityType.Ejected:
                    details = "Ejected";
                    largeAsset = "orbit";
                    largeAssetText = "Ejected";
                    break;
            }

            ActivityParty party = new ActivityParty
            {
                Id = partyId,
                Size = new PartySize
                {
                    CurrentSize = currentPartySize,
                    MaxSize = maxPartySize,
                },
                Privacy = ActivityPartyPrivacy.Public,
            };

            ActivityTimestamps timestamps = new ActivityTimestamps
            {
                Start = roundStartTimestamp,
            };

            ActivityAssets assets = new ActivityAssets
            {
                LargeImage = largeAsset,
                LargeText = largeAssetText,
                SmallImage = smallAssetString,
                SmallText = smallAsset,
            };

            ActivitySecrets secrets = new ActivitySecrets
            {
                Join = isPartyJoinable ? partyJoinSecret : null,
            };

            Activity activity = new Activity
            {
                Details = details,
                State = state,
                Party = party,
                Timestamps = timestamps,
                Assets = assets,
                //Secrets = secrets,
                Instance = true,
            };

            DiscordClient.GetActivityManager().UpdateActivity(activity, result =>
            {
                if (result != Result.Ok)
                {
                    LogDiscord(LogLevel.Error, $"Failed to update activity: {result}");
                }
            });
        }

        private void SetPreviousActivity()
        {
            if (prevActivity != currentActivity)
            {
                prevActivity = currentActivity;

                if (prevActivity == LethalDiscordActivityType.MainMenu)
                {
                    roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
                else if (prevActivity == LethalDiscordActivityType.WaitingForPlayers)
                {
                    roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
                else if (prevActivity == LethalDiscordActivityType.InOrbit)
                {
                    roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
                else if (prevActivity == LethalDiscordActivityType.OnMoon)
                {
                    roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
                else if (prevActivity == LethalDiscordActivityType.Ejected)
                {
                    roundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
            }
        }

        private void LogDiscord(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Error:
                    LethalDiscordPlugin.LOG.LogError($"Discord: {message}");
                    break;
                case LogLevel.Warn:
                    LethalDiscordPlugin.LOG.LogWarning($"Discord: {message}");
                    break;
                case LogLevel.Info:
                    LethalDiscordPlugin.LOG.LogMessage($"Discord: {message}");
                    break;
                case LogLevel.Debug:
                    LethalDiscordPlugin.LOG.LogDebug($"Discord: {message}");
                    break;
            }
        }

        private void OnDestroy()
        {
            LethalDiscordPlugin.LOG.LogInfo("LethalDiscordRPC has been destroyed!");
            LethalDiscordPlugin.IsDiscordRPCActive = false;
            LethalDiscordPlugin.CreateDiscordRPC();
            if (DiscordClient != null)
            {
                DiscordClient.Dispose();
            }
        }

        private void OnApplicationQuit()
        {
            if (DiscordClient != null)
            {
                DiscordClient.Dispose();
            }
        }
    }
}
