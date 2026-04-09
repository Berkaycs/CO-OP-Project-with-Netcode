using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;

public class ScoreTablePlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _killsText;
    [SerializeField] private Color _ownerColor;
    [SerializeField] private Image _profileImage;
    [SerializeField] private Sprite[] _profileSprites;

    public void SetScoreTableData(string rank, FixedString32Bytes playerName, string kills, int profileIndex, bool isOwner)
    {
        _rankText.text = rank;
        _playerNameText.text = playerName.ToString();
        _killsText.text = kills;
        _profileImage.sprite = _profileSprites[profileIndex];

        if (isOwner)
        {
            _playerNameText.color = _ownerColor;
        }
    }
}
