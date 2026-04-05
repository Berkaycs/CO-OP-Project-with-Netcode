using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance { get; private set; }

    [SerializeField] private TMP_Text _timerText;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTimer(int timer)
    {
        _timerText.text = timer.ToString();
    }
}
