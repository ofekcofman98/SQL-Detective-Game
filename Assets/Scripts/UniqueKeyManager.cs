using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Text;
using System;
using Newtonsoft.Json;
using Assets.Scripts.ServerIntegration;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.ServerIntegration
{
    public class UniqueKeyManager : Singleton<UniqueKeyManager>
    {
        public string gameKey { get; private set; }

        private ISessionApi m_SessionApi;
        private GameSessionDto m_CurrentSession;

        private bool isKeyInWaitlist = false;


        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            gameKey = string.Empty;
            m_SessionApi = BackendFactory.CreateSessionApi();
        }

        public void SetGameKeyFromSavedGame(string i_GameKey)
        {
            gameKey = i_GameKey;
            Debug.Log($"📌 Game key set from saved game: {gameKey}");
        }


        public async Task StartNewSessionAsync(CancellationToken ct = default)
        {
            if(!string.IsNullOrWhiteSpace(gameKey))
            {
                Debug.Log($"📌 Game key already exists: {gameKey}");
                return;
            }

    Debug.Log("🌐 StartNewSessionAsync: sending request to backend...");

            StartSessionResponse response = await m_SessionApi.StartSessionAsync(ct);
            if (response == null || string.IsNullOrWhiteSpace(response.Key))
            {
                Debug.LogError("❌ StartNewSessionAsync: backend did not return a valid key");
                return;
            }

            gameKey = response.Key;
            Debug.Log($"🔑 Received session key from backend: {gameKey}");
        }

        public async Task CompareKeys(string key, Action<bool> onResult)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    Debug.Log("[CompareKeys]: Empty key");
                    onResult?.Invoke(false);
                    return;
                }   

                GameSessionDto session = await m_SessionApi.GetSessionAsync(key);

                if (session == null)
                {
                    Debug.Log($"No session found for key {key}");
                    onResult?.Invoke(false);
                    return;
                }

                gameKey = session.key;
                m_CurrentSession = session;

                Debug.Log($"✅ Session found for key '{key}', adopted as gameKey");

                GameManager.Instance.InitMobile();

                onResult?.Invoke(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error while validating key '{key}': {ex.Message}");
                onResult?.Invoke(false);
            }
        }

    }
}
