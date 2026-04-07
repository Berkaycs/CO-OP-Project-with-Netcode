using UnityEngine;
using System.Collections;

public class HostSingleton : MonoBehaviour
{
     #region Singleton
    // detailed instance definition is important for client singleton
    private static HostSingleton _instance;

    public static HostSingleton Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindFirstObjectByType<HostSingleton>();

            if (_instance == null)
            {
                Debug.LogError("HostSingleton not found!");
                return null;
            }

            return _instance;
        }
    }
    #endregion

    public HostGameManager HostGameManager { get; private set; }
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // we dont need to await because host will be created before the game starts
    public void CreateHost() => HostGameManager = new HostGameManager();

    private void OnDestroy()
    {
        HostGameManager?.Dispose();
    }
}
