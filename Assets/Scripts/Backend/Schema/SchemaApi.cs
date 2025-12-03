using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SchemaApi : ISchemaApi
{
    private readonly BackendClient r_Client;
    private const string k_Route = "api/schema";

    public SchemaApi(BackendClient client)
    {
        r_Client = client;
    }

    public Task<SchemaDto> GetSchemaAsync(CancellationToken ct = default)
    {
        return r_Client.GetJsonAsync<SchemaDto>(k_Route, ct);
    }
}
