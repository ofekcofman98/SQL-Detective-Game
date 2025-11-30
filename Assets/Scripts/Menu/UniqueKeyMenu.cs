using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UniqueKeyMenu : MenuBase
{
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private GameObject waitingLabel;
    [SerializeField] private Button continueOnPcButton;

    private bool confirmNoMobileClicked = false;
    [SerializeField] private TextMeshProUGUI continueOnPcButtonText;

    private Action m_OnKeyAccepted;
    public bool registerExistingKey = false;


    private void Awake()
    {
        continueOnPcButton.onClick.AddListener(() =>
        {
            if (!confirmNoMobileClicked)
            {
                confirmNoMobileClicked = true;
                continueOnPcButtonText.text = "⚠️ Continue without mobile";
                Debug.Log("🟡 Confirm 'no mobile' clicked once — awaiting second click");
                return;
            }

            Debug.Log("🧍‍♂️ Player confirmed 'continue on PC'");
            GameManager.Instance.ForceStartGameFromPC();
            CloseAndProceed();
        });
    }

    private void OnEnable()
    {
        Debug.Log($"🧷 UniqueKeyMenu.OnEnable, registerExistingKey={registerExistingKey}, current gameKey='{UniqueKeyManager.Instance.gameKey}'");

        waitingLabel.SetActive(true);
        keyLabel.text = "";

        if (registerExistingKey)
            StartCoroutine(WaitForKeyRegistration());
        else
            StartCoroutine(WaitForKeyGeneration());
            
        confirmNoMobileClicked = false;
        continueOnPcButtonText.text = "I don't have mobile";
    }

    public void Show(Action onKeyAccepted)
    {
        m_OnKeyAccepted = onKeyAccepted;
        MenuManager.Instance.ShowMenu(eMenuType.Key);
    }


    private IEnumerator WaitForKeyGeneration()
    {
        Debug.Log("🔄 WaitForKeyGeneration: starting new session...");

        var task = UniqueKeyManager.Instance.StartNewSessionAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"❌ Failed to start new session: {task.Exception.InnerException?.Message ?? task.Exception.Message}");
            waitingLabel.SetActive(false);
            keyLabel.text = "Error getting key";
            yield break;
        }

        if (string.IsNullOrEmpty(UniqueKeyManager.Instance.gameKey))
        {
            Debug.LogError("❌ WaitForKeyGeneration: gameKey is still empty after StartNewSessionAsync");
            waitingLabel.SetActive(false);
            keyLabel.text = "No key received";
            yield break;
        }

        waitingLabel.SetActive(false);
        keyLabel.text = UniqueKeyManager.Instance.gameKey;

        Debug.Log($"✅ WaitForKeyGeneration: key ready and shown on screen: {UniqueKeyManager.Instance.gameKey}");

        // // ConnectListener.Instance.StartListening();
        // // Wait for mobile to connect (you need to call OnMobileConnected externally)
        // // yield return new WaitUntil(() => GameManager.Instance.MobileConnected ||
        // //                                  GameManager.Instance.SkipMobileWaiting); // or any other flag for mobile connection  

        // MenuManager.Instance.HideMenu(eMenuType.Key);
        // // GameManager.Instance.TurnOffSkipOnMobile();

        // // if (GameManager.Instance.MobileConnected)
        // // {
        // //     UIManager.Instance.HideSQLButton();
        // //     // Debug.Log("GameManager.Instance.MobileConnected = true");
        // // }
        // // else if (GameManager.Instance.SkipMobileWaiting)
        // // {
        // //     UIManager.Instance.ShowSQLButton();
        // //     // Debug.Log("GameManager.Instance.SkipMobileWaiting = true");
        // // }

        // m_OnKeyAccepted?.Invoke();
    }

    private IEnumerator WaitForKeyRegistration()
    {
        yield return new WaitUntil(() => !string.IsNullOrEmpty(UniqueKeyManager.Instance.gameKey));

        keyLabel.text = $"{UniqueKeyManager.Instance.gameKey}";

        MenuManager.Instance.HideMenu(eMenuType.Key);

        GameManager.Instance.StartSavedGame();

        // yield return new WaitUntil(() => !string.IsNullOrEmpty(UniqueKeyManager.Instance.gameKey));

        // keyLabel.text = $"{UniqueKeyManager.Instance.gameKey}";

        // ConnectListener.Instance.StartListening();
        // // Wait for mobile to connect (you need to call OnMobileConnected externally)
        // yield return new WaitUntil(() => GameManager.Instance.MobileConnected ||
        //                                  GameManager.Instance.SkipMobileWaiting); // or any other flag for mobile connection  

        // MenuManager.Instance.HideMenu(eMenuType.Key);

        // // m_OnKeyAccepted?.Invoke();
        // GameManager.Instance.StartSavedGame();
    }

    private void CloseAndProceed()
    {
        MenuManager.Instance.HideMenu(eMenuType.Key);
        m_OnKeyAccepted?.Invoke();
    }

    public void OnMobileConnected()
    {
        Debug.Log("📱 Mobile connected, closing key menu");
        CloseAndProceed();
    }


}
