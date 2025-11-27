using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackendPollingManager : MonoBehaviour
{
    [SerializeField] private float m_PollingIntervalSeconds = 0.5f;
    private readonly List<IBackendPollingChannel> r_Channels = new();
    private CancellationTokenSource m_Cts;


    private void Awake()
    {
        IQueryRelayApi queryApi = BackendFactory.CreateQueryRelayApi();
        r_Channels.Add(new QueryRelayPollingChannel(queryApi));
        // r_Channels.Add(new StatePollingChannel(client));
        // r_Channels.Add(new ResetPollingChannel(client));
    }

    private void OnEnable()
    {
        m_Cts = new CancellationTokenSource();
        StartCoroutine(PollLoop(m_Cts.Token));
    }

    private void OnDisable()
    {
        m_Cts?.Cancel();
        m_Cts = null;
    }


    private IEnumerator PollLoop(CancellationToken ct)
    {
        WaitForSeconds wait = new WaitForSeconds(m_PollingIntervalSeconds);

        while (!ct.IsCancellationRequested)
        {
            foreach (var channel in r_Channels)
            {
                // אם אתה עדיין רוצה אפשרות לכבות ערוץ
                if (!channel.IsEnabled)
                {
                    continue;
                }

                // fire and forget
                _ = channel.PollAsync(ct);
            }

            yield return wait;
        }
    }

    public TChannel GetChannel<TChannel>() where TChannel : class, IBackendPollingChannel
    {
        foreach (var channel in r_Channels)
        {
            if (channel is TChannel typed)
            {
                return typed;
            }
        }

        return null;
    }

}
