using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameProgressApi : IGameProgressApi
{
    private readonly BackendClient r_Client;
    private const string k_Route = "api/game-progress";

    public GameProgressApi(BackendClient i_BackendClient)
    {
        r_Client = i_BackendClient;
    } 

    public Task<GameProgressDto?> LoadGame(string key, CancellationToken ct = default)
    {
        string path = $"{k_Route}?key={key}";

        return r_Client.GetJsonAsync<GameProgressDto>(path, ct);
    }

    public Task<ApiMessageResponse> SaveGame(GameProgressSaveRequest progressSaveRequest, CancellationToken ct = default)
    {   
        string path = $"{k_Route}";
        return r_Client.PostJsonAsync<ApiMessageResponse, GameProgressSaveRequest>(path, progressSaveRequest, ct);
    }
}

