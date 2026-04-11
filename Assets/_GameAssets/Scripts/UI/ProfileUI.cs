using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ProfileUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _bgSelectProfile;
    [SerializeField] private Toggle[] _profileToggles;
    [SerializeField] private Button _selectProfileButton;
    [SerializeField] private Image _selectedProfileImage;

    [Header("Audios")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _charactersSfx;


    private int _selectedProfileIndex;

    private void Awake()
    {
        _selectProfileButton.onClick.AddListener(OnSelectProfileButtonClicked);

        foreach (Toggle profileToggle in _profileToggles)
        {
            profileToggle.onValueChanged.AddListener(OnProfileToggleValueChanged);
        }
    }

    private void Start()
    {
        if (_profileToggles == null || _profileToggles.Length == 0)
        {
            return;
        }

        _selectedProfileIndex = Mathf.Clamp(
            PlayerPrefs.GetInt(Consts.PlayerData.PROFILE_INDEX, 0),
            0,
            _profileToggles.Length - 1);

        ApplyToggleSelectionFromIndex(_selectedProfileIndex);
        UpdateSelectedProfileImage(_selectedProfileIndex);
    }

    private void ApplyToggleSelectionFromIndex(int index)
    {
        if (_profileToggles == null || _profileToggles.Length == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, _profileToggles.Length - 1);
        for (int i = 0; i < _profileToggles.Length; i++)
        {
            _profileToggles[i].SetIsOnWithoutNotify(i == index);
        }
    }

    private void UpdateSelectedProfileImage(int index)
    {
        _selectedProfileImage.sprite = _profileToggles[index].GetComponent<Image>().sprite;
    }

    private void OnSelectProfileButtonClicked()
    {
        bool willShow = !_bgSelectProfile.activeSelf;
        _bgSelectProfile.SetActive(willShow);

        if (willShow)
        {
            ApplyToggleSelectionFromIndex(_selectedProfileIndex);
        }
    }

    private void OnProfileToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            _selectedProfileIndex = _profileToggles.ToList().IndexOf(_profileToggles.FirstOrDefault(t => t.isOn));

            UpdateSelectedProfileImage(_selectedProfileIndex);

            _audioSource.PlayOneShot(_charactersSfx[_selectedProfileIndex]);

            PlayerPrefs.SetInt(Consts.PlayerData.PROFILE_INDEX, _selectedProfileIndex);
            _bgSelectProfile.SetActive(false);
        }
    }
}
