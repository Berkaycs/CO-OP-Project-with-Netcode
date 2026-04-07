using UnityEngine;
using Cysharp.Threading.Tasks;

public class ClientSingleton : MonoBehaviour
{
    #region Singleton
    // detailed instance definition is important for client singleton
    private static ClientSingleton _instance;

    public static ClientSingleton Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindFirstObjectByType<ClientSingleton>();

            if (_instance == null)
            {
                Debug.LogError("ClientSingleton not found!");
                return null;
            }

            return _instance;
        }
    }
    #endregion

    public ClientGameManager ClientGameManager { get; private set; }
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async UniTask<bool> CreateClient()
    {
        ClientGameManager = new ClientGameManager();
        return await ClientGameManager.InitAsync();
    }

    private void OnDestroy()
    {
        ClientGameManager?.Dispose();
    }
}
