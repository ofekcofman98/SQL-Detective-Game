using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.ServerIntegration;
using Unity.VisualScripting;
using UnityEngine;

public class GameSaver : Singleton<GameSaver>
{
    private GameProgressClient m_ProgressClient;
    private GameProgressDto m_LastSaved;
    private GameProgressDto m_LastLoaded;
    private string m_LastLoadedKey;


    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        var api = BackendFactory.CreateGameProgressApi();
        m_ProgressClient = new GameProgressClient(api);
    }

    public async Task SaveGame(Action<string> onGameKeyAvailable = null)
    {
        if (string.IsNullOrWhiteSpace(UniqueKeyManager.Instance.gameKey))
        {
            Debug.LogWarning("No game key yet, cannot save");
            onGameKeyAvailable?.Invoke(null);
            return;
        }

        var progressDto = new GameProgressDto
        {
            SequenceIndex = SequenceManager.Instance.CurrentSequenceIndex,
            CurrentMissionIndex = MissionsManager.Instance.GetLastValidMissionIndex(),
            Lives = LivesManager.Instance.Lives
        };

        if(isSameAsLastSave(progressDto))
        {
            Debug.Log("No changes since last save");
            onGameKeyAvailable?.Invoke(UniqueKeyManager.Instance.gameKey);
            return;
        }
        

        await m_ProgressClient.SaveAsync(progressDto);
        m_LastSaved = progressDto;
        onGameKeyAvailable?.Invoke(UniqueKeyManager.Instance.gameKey);
    }

    private bool isSameAsLastSave(GameProgressDto progressDto)
    {
        if (m_LastSaved == null) return false;

        return m_LastSaved.SequenceIndex == progressDto.SequenceIndex
               && m_LastSaved.CurrentMissionIndex == progressDto.CurrentMissionIndex
               && m_LastSaved.Lives == progressDto.Lives;
    }

    public async void ValidateKeyAndLoadGame(string key, Action<bool> onValidationComplete)
    {
        try
        {
            GameProgressDto dto = await m_ProgressClient.LoadProgressAsync(key);

            if (dto == null)
            {
                Debug.LogWarning($"No saved game found for key {key}");
                onValidationComplete?.Invoke(false);
                return;
            }

            m_LastLoaded = dto;
            m_LastLoadedKey = key;

            UniqueKeyManager.Instance.SetGameKeyFromSavedGame(key);


            onValidationComplete?.Invoke(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error validating or loading saved game for key {key}: {ex.Message}");
            onValidationComplete?.Invoke(false);
        }
    }

    public void ApplyLoadedGame()
    {
        if (m_LastLoaded == null)
        {
            Debug.LogError("ApplyLoadedGame called but no loaded progress is cached. Did you call ValidateKeyAndLoadGame first?");
            return;
        }
        
        GameProgressDto dto = m_LastLoaded;

        SequenceManager.Instance.SetSequence(dto.SequenceIndex);
        MissionsManager.Instance.LoadMissionSequence(SequenceManager.Instance.Current);
        MissionsManager.Instance.SetStatsFromLoadedGame(dto.SequenceIndex, dto.Lives, dto.CurrentMissionIndex);

        if (!string.IsNullOrWhiteSpace(m_LastLoadedKey))
        {
            UniqueKeyManager.Instance.SetGameKeyFromSavedGame(m_LastLoadedKey);
        }

        GameManager.Instance.StartMissions();
        _ = SaveGame();

        // StateSender.Instance.UpdatePhone();
    }

}
