using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class SessionApi : ISessionApi
{
    private readonly BackendClient r_Client;
    private const string k_Route = "session";

    public SessionApi(BackendClient i_Client)
    {
        r_Client = i_Client;    
    }
    
    public Task<GameSessionDto?> GetSessionAsync(string key, CancellationToken ct = default)
    {
        string path = $"api/{k_Route}?key={key}";

        return r_Client.GetJsonAsync<GameSessionDto>(path, ct);
    }

    public Task<StartSessionResponse> StartSessionAsync(CancellationToken ct = default)
    {
        return r_Client.PostJsonAsync<StartSessionResponse, object>($"api/{k_Route}", null, ct);
    }
}
