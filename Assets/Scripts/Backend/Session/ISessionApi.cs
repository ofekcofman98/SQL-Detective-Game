using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface ISessionApi 
{
    Task<StartSessionResponse> StartSessionAsync(CancellationToken ct = default);
    Task<GameSessionDto?> GetSessionAsync(string key, CancellationToken ct = default);
}
