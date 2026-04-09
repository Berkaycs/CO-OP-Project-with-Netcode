using UnityEngine;
using System;

public class WaitingForPlayersUI : MonoBehaviour
{
    public static WaitingForPlayersUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(true);
        StartingGameUI.Instance.OnAllPlayersConnected += StartingGameUI_OnAllPlayersConnected;
    }

    private void StartingGameUI_OnAllPlayersConnected()
    {
        Hide();
        StartingGameUI.Instance.OnAllPlayersConnected -= StartingGameUI_OnAllPlayersConnected;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
