using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Text;

public class NameSelectorUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private Button _connectButton;

    private void Awake()
    {
        _connectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    private void Start()
    {
        // If we are in a dedicated server, we don't need to show the name selector
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(Consts.Scenes.LOADING_SCENE);
            return;
        }

        _nameInputField.text = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, string.Empty);
    }

    private void OnConnectButtonClicked()
    {
        PlayerPrefs.SetString(Consts.PlayerData.PLAYER_NAME, _nameInputField.text);
        SceneManager.LoadScene(Consts.Scenes.LOADING_SCENE);
    }
}
