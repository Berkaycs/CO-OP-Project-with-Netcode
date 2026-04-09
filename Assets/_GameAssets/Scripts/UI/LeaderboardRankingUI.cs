using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Netcode;

public class LeaderboardRankingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private Color _ownerColor;
    [SerializeField] private Image _profileImage;
    [SerializeField] private Sprite[] _profileSprites;

    private FixedString32Bytes _playerName;
    public ulong ClientId { get; private set; }
    public int Score { get; private set; }
    public int ProfileIndex { get; private set; }

    public void SetData(ulong clientId, FixedString32Bytes playerName, int score, int profileIndex)
    {
        ClientId = clientId;
        _playerName = playerName;
        ProfileIndex = profileIndex;

        _playerNameText.text = _playerName.ToString();
        _profileImage.sprite = _profileSprites[ProfileIndex];

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            _playerNameText.color = _ownerColor;
            _scoreText.color = _ownerColor;
        }

        UpdateRank();
        UpdateScore(score);
    }

    public string GetPlayerName()
    {
        return _playerName.ToString();
    }

    public void UpdateScore(int score)
    {
        Score = score;
        _scoreText.text = Score.ToString();
    }

    public void UpdateRank()
    {
        _rankText.text = $"{transform.GetSiblingIndex() + 1}";
    }
}
