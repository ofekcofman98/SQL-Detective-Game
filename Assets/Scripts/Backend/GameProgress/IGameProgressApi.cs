using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IGameProgressApi
{
    Task<GameProgressDto?> LoadGame(string key, CancellationToken ct = default);
    Task<ApiMessageResponse> SaveGame(GameProgressSaveRequest progressSaveRequest, CancellationToken ct = default);
}
