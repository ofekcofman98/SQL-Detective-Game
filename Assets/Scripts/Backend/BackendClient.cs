using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;

public class BackendClient
{
    private readonly string r_BaseUrl;
    
    public BackendClient(string i_BaseUrl)
    {
        r_BaseUrl = i_BaseUrl.TrimEnd('/');
    }

    private string buildUrl(string i_RelativePath)
    {
        return $"{r_BaseUrl}/{i_RelativePath.TrimStart('/')}";
    }

    public async Task<TResponse?> GetJsonAsync<TResponse>(string i_RelativePath, CancellationToken ct = default)
    {
        var url = buildUrl(i_RelativePath);
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        var op = request.SendWebRequest();

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested)
            {
                request.Abort();
                ct.ThrowIfCancellationRequested();
            }
            
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"GET {url} failed: {request.responseCode} | {request.error}");
            throw new Exception($"Backend GET failed with code {request.responseCode}");
        }

        string json = request.downloadHandler.text;
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<TResponse>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to deserialize GET response from {url}: {ex.Message}\nBody: {json}");
            throw;
        }

    }

    public async Task<TResponse?> PostJsonAsync<TResponse, TRequest>(string i_RelativePath, TRequest i_Body, CancellationToken ct = default)
    {
        string url = buildUrl(i_RelativePath);
        
        string jsonBody = i_Body == null ? "" : JsonConvert.SerializeObject(i_Body);

        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.disposeUploadHandlerOnDispose = true;
        request.disposeDownloadHandlerOnDispose = true;
        request.SetRequestHeader("Content-Type", "application/json");

        var op = request.SendWebRequest();

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested)
            {
                request.Abort();
                ct.ThrowIfCancellationRequested();
            }
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
                Debug.LogError($"POST {url} failed: {request.responseCode} | {request.error}");
                throw new Exception($"Backend POST failed with code {request.responseCode}");
        }

        string json = request.downloadHandler.text;
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }
        try
        {
            return JsonConvert.DeserializeObject<TResponse>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to deserialize POST response from {url}: {ex.Message}\nBody: {json}");
            throw;
        }
    }
}
