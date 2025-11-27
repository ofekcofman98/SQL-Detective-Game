using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class QueryRelayApi : IQueryRelayApi
{
    private readonly BackendClient r_BackendClient;
    private const string k_Route = "api/relay";

    public QueryRelayApi(BackendClient i_BackendClient)
    {
        r_BackendClient = i_BackendClient;
    } 

    public async Task<Query> GetNextQuery(string key, CancellationToken ct = default)
    {
        string path = $"{k_Route}?key={key}";

        string? queryJson = await r_BackendClient.GetJsonAsync<string>(path, ct);

        if (string.IsNullOrWhiteSpace(queryJson))
        {
            return null;
        }

        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new OperatorConverter());

        Query query = JsonConvert.DeserializeObject<Query>(queryJson, settings);

        return query;
    }

    public Task<ApiMessageResponse> SendQuery(string key, Query query, CancellationToken ct = default)
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new OperatorConverter());

        string queryJson = JsonConvert.SerializeObject(query, settings);
        
        string path = $"{k_Route}?key={key}";

        return r_BackendClient.PostJsonAsync<ApiMessageResponse, string>(path, queryJson, ct);    }
}
