using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.ServerIntegration;
using UnityEngine;

public class QueryRelayPollingChannel : IBackendPollingChannel
{
    private readonly IQueryRelayApi r_Api;
    public event Action<Query> OnQueryReceived;

    public string Name => "QueryRelay";

    public bool IsEnabled {get; private set;} = true;

    public float IntervalSeconds { get; } = 0.5f;

    public QueryRelayPollingChannel(IQueryRelayApi i_Api)
    {
        r_Api = i_Api;
    }

    public async Task PollAsync(CancellationToken ct)
    {
        Debug.Log($"[QueryRelayPollingChannel] Polling...");

        string sessionKey = UniqueKeyManager.Instance.gameKey;

        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            Debug.Log($"[QueryRelayPollingChannel] string.IsNullOrWhiteSpace(r_SessionKey) == true");
            return;
        }

        Query query = await r_Api.GetNextQuery(sessionKey, ct);
                
        if (query == null)
        {
            Debug.Log("[QueryRelayPollingChannel] No query available.");
            return;
        }

        Debug.Log($"[QueryRelayPollingChannel] Query Received: {query.QueryString}");

        query.PostDeserialize();
        OnQueryReceived?.Invoke(query);
    }

}
