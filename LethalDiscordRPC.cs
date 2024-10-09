using UnityEngine;
using Discord;
using System;
using UnityEngine.SceneManagement;
using Steamworks;

namespace LethalDiscord
{
    public class LethalDiscordRPC : MonoBehaviour
    {
        public static LethalDiscordRPC Instance { get; private set; }
        private Discord.Discord DiscordClient;

        private float timeAtLastStatusUpdate = 0;

        void Awake()
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

        void Start()
        {
            DiscordClient = new Discord.Discord(1291262348975411311, (ulong)CreateFlags.NoRequireDiscord);

            ActivityManager activityManager = DiscordClient.GetActivityManager();
            activityManager.RegisterSteam(1966720);

            DiscordClient.SetLogHook(LogLevel.Debug, LogDiscord);

            SceneManager.sceneLoaded += OnSceneLoaded;

            SteamMatchmaking.OnLobbyDataChanged += (lobby) =>
            {
                GameValues.SetMaxPartySize(lobby.MaxMembers);
                GameValues.SetPartyId(lobby.Id.ToString());
            };

            activityManager.OnActivityJoin += (secret) =>
            {
                SteamId lobbyId = ulong.Parse(secret);
                Steamworks.Data.Lobby lobby = new Steamworks.Data.Lobby(lobbyId);

                try
                {
                    GameNetworkManager.Instance?.JoinLobby(lobby, lobbyId);
                }
                catch (Exception e)
                {
                    LethalDiscordPlugin.LOG.LogError($"Failed to join lobby: {e.Message}");
                }
            };

            UpdateDiscordActivity(true);
        }

        void Update()
        {
            DiscordClient.RunCallbacks();
        }

        void FixedUpdate()
        {
            UpdateDiscordActivity(false);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateDiscordActivity(true);
        }

        private void UpdateDiscordActivity(bool isLoad)
        {
            if (Time.realtimeSinceStartup - timeAtLastStatusUpdate < ConfigManager.Instance.DiscordUpdateInterval.Value && !isLoad)
                return;

            timeAtLastStatusUpdate = Time.realtimeSinceStartup;

            if (isLoad)
            {
                DiscordClient.GetActivityManager().ClearActivity((result) =>
                {
                    if (result != Discord.Result.Ok)
                    {
                        LethalDiscordPlugin.LOG.LogError($"Failed to clear activity: {result}");
                    }
                });

                GameValues.StartNewRoundTimestamp();

                if (GameValues.GetActivityType() == LethalDiscordActivityType.MainMenu)
                {
                    GameValues.SetPartyId(null);
                    GameValues.SetMaxPartySize(1);
                }
            }

            Activity activity = new Activity
            {
                Details = GameValues.GetActivityName(),
                State = GameValues.GetPartyState() ?? "",
                Assets = new ActivityAssets
                {
                    LargeImage = GameValues.GetLargeAssetImage() ?? "",
                    LargeText = GameValues.GetLargeAssetText() ?? "",
                    SmallImage = GameValues.GetSmallAssetImage() ?? "",
                    SmallText = GameValues.GetSmallAssetText() ?? "",
                },
                Timestamps = new ActivityTimestamps
                {
                    Start = GameValues.GetRoundStartTimestamp(),
                },
                Party = GameValues.GetMaxPartySize() > 1 ? new ActivityParty
                {
                    Id = GameValues.GetPartyOwnerId() ?? "",
                    Size = new PartySize
                    {
                        CurrentSize = GameValues.GetPartySize(),
                        MaxSize = GameValues.GetMaxPartySize(),
                    },
                    Privacy = GameValues.GetPartyPrivacy(),
                } : new ActivityParty(),
                Secrets = (GameValues.GetPartyId() != null && GameValues.GetIsPartyJoinable()) ? new ActivitySecrets
                {
                    Join = GameValues.GetPartyId(),
                } : new ActivitySecrets(),
                Instance = true,
            };

            DiscordClient.GetActivityManager().UpdateActivity(activity, (result) =>
            {
                if (result != Discord.Result.Ok)
                {
                    LethalDiscordPlugin.LOG.LogError($"Failed to update activity: {result}");
                }
            });
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

        void OnDestroy()
        {
            LethalDiscordPlugin.LOG.LogInfo("LethalDiscordRPC has been destroyed!");
            LethalDiscordPlugin.IsDiscordRPCActive = false;
            LethalDiscordPlugin.CreateDiscordRPC();
            DiscordClient?.Dispose();
        }

        void OnApplicationQuit()
        {
            LethalDiscordPlugin.LOG.LogInfo("LethalDiscordRPC has been destroyed!");
            LethalDiscordPlugin.IsDiscordRPCActive = false;
            LethalDiscordPlugin.CreateDiscordRPC();
            DiscordClient?.Dispose();
        }
    }
}
