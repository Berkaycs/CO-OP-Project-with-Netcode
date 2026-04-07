using UnityEngine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton _clientSingletonPrefab;
    [SerializeField] private HostSingleton _hostSingletonPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null); // null ise server, diğer ise host client
    }
    
    private async UniTask LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            // DEDICATED SERVER
        }
        else
        {
            // HOST CLIENT
            HostSingleton hostSingletonInstance = Instantiate(_hostSingletonPrefab);
            hostSingletonInstance.CreateHost();

            ClientSingleton clientSingletonInstance = Instantiate(_clientSingletonPrefab);
            bool isAuthenticated = await clientSingletonInstance.CreateClient();

            if (isAuthenticated)
            {
                // LOAD MAIN MENU
                clientSingletonInstance.ClientGameManager.GoToMainMenu();
            }
            else
            {
                // SHOW AUTH ERROR
            }
        }
    }
}
