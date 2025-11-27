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
    private readonly string r_SessionKey;

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
        if (string.IsNullOrWhiteSpace(r_SessionKey))
        {
            return;
        }

        Query query = await r_Api.GetNextQuery(r_SessionKey, ct);

        if (query == null)
        {
            return;
        }

        query.PostDeserialize();
        OnQueryReceived?.Invoke(query);
    }

}
