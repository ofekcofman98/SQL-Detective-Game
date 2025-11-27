using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.ServerIntegration;
using UnityEngine;

public class GameProgressClient
{
    private readonly IGameProgressApi r_Api;

    public GameProgressClient(IGameProgressApi i_Api)
    {
        r_Api = i_Api;
    }

    public async Task SaveAsync(GameProgressDto dto, CancellationToken ct = default)
    {
        GameProgressSaveRequest SaveRequest = new GameProgressSaveRequest
        {
            Key = UniqueKeyManager.Instance.gameKey,
            Progress = dto
        } ;

        ApiMessageResponse response = await r_Api.SaveGame(SaveRequest, ct);
        Debug.Log($"Progress saved, server said: {response.Message}");
    }

    public async Task<GameProgressDto?> LoadProgressAsync(string key, CancellationToken ct = default)
    {
        GameProgressDto dto = await r_Api.LoadGame(key, ct);

        if (dto == null)
        {
            Debug.LogWarning("No saved progress for this key");
            return null;
        }

        // SequenceManager.Instance.SetSequence(dto.SequenceIndex);
        // MissionsManager.Instance.LoadMissionSequence(SequenceManager.Instance.Current);
        // MissionsManager.Instance.SetStatsFromLoadedGame(dto.SequenceIndex, dto.Lives, dto.CurrentMissionIndex);
        // GameManager.Instance.StartMissions();

        return dto;
    }
}
