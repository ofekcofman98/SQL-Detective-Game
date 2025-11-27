using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IQueryRelayApi
{
    Task<ApiMessageResponse> SendQuery(string key, Query query, CancellationToken ct = default);
    Task<Query> GetNextQuery(string key, CancellationToken ct = default);
}
