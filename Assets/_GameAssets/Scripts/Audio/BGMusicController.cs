using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMusicController : MonoBehaviour
{
    public static BGMusicController Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _bgMenuMusicClip;
    [SerializeField] private AudioClip _bgGameMusicClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Consts.Scenes.GAME_SCENE)
        {
            if (_audioSource.clip != _bgGameMusicClip || !_audioSource.isPlaying)
            {
                _audioSource.clip = _bgGameMusicClip;
                _audioSource.Play();
            }

            _audioSource.volume = .6f;
        }
        else
        {
            if (_audioSource.clip == _bgMenuMusicClip && _audioSource.isPlaying)
            {
                _audioSource.volume = 1f;
                return;
            }

            _audioSource.clip = _bgMenuMusicClip;
            _audioSource.volume = 1f;
            _audioSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        _audioSource.volume = volume;
    }
}
