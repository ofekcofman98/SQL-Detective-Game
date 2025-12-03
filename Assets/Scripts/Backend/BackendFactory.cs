using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackendFactory
{
    private static BackendClient s_Client;

    public static BackendClient Client
        => s_Client ??= new BackendClient(BackendConfig.BaseUrl);

    public static IGameProgressApi CreateGameProgressApi()
        => new GameProgressApi(Client);

    public static IQueryRelayApi CreateQueryRelayApi()
        => new QueryRelayApi(Client);

    public static ISessionApi CreateSessionApi()
        => new SessionApi(Client);

    public static ISchemaApi CreateSchemaApi()
        => new SchemaApi(Client);
}
