using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

/// <summary>
/// Logs Netcode scene sync and client connect/disconnect for multiplayer debugging.
/// </summary>
public class NetworkSceneDiagnostics : MonoBehaviour
{
    private bool _subscribed;

    private void Start()
    {
        StartCoroutine(SubscribeWhenNetworkManagerReady());
    }

    private IEnumerator SubscribeWhenNetworkManagerReady()
    {
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }

        NetworkManager nm = GetComponent<NetworkManager>() ?? NetworkManager.Singleton;

        int waited = 0;
        const int maxFramesWaitSceneManager = 600;
        while (nm.SceneManager == null && waited < maxFramesWaitSceneManager)
        {
            waited++;
            yield return null;
        }

        if (nm.SceneManager == null)
        {
            Debug.LogWarning(
                "[NetSceneDiag] NetworkManager.SceneManager is still null after waiting. " +
                "Is 'Enable Scene Management' enabled on the NetworkManager / NetworkConfig?");
            yield break;
        }

        nm.SceneManager.OnSceneEvent += OnSceneEvent;
        nm.OnClientDisconnectCallback += OnClientDisconnected;
        nm.OnClientConnectedCallback += OnClientConnected;
        _subscribed = true;

        Debug.Log(
            $"[NetSceneDiag] Subscribed. IsServer={nm.IsServer} IsClient={nm.IsClient} IsHost={nm.IsHost} " +
            $"LocalClientId={nm.LocalClientId} ConnectedCount={nm.ConnectedClientsList.Count}");
    }

    private void OnDestroy()
    {
        if (!_subscribed || NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager nm = NetworkManager.Singleton;
        if (nm.SceneManager != null)
        {
            nm.SceneManager.OnSceneEvent -= OnSceneEvent;
        }

        nm.OnClientDisconnectCallback -= OnClientDisconnected;
        nm.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;

        var extra = new StringBuilder();
        if (sceneEvent.ClientsThatCompleted is { Count: > 0 } done)
        {
            extra.Append(" completed=[");
            extra.Append(string.Join(",", done));
            extra.Append(']');
        }

        if (sceneEvent.ClientsThatTimedOut is { Count: > 0 } timedOut)
        {
            extra.Append(" timedOut=[");
            extra.Append(string.Join(",", timedOut));
            extra.Append(']');
        }

        if (sceneEvent.AsyncOperation != null)
        {
            extra.Append($" asyncProgress={sceneEvent.AsyncOperation.progress:F2} asyncDone={sceneEvent.AsyncOperation.isDone}");
        }

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete && sceneEvent.Scene.IsValid())
        {
            extra.Append($" loadedScene={sceneEvent.Scene.name}");
        }

        Debug.Log(
            $"[NetSceneDiag] SceneEvent type={sceneEvent.SceneEventType} scene={sceneEvent.SceneName} " +
            $"clientId={sceneEvent.ClientId} loadMode={sceneEvent.LoadSceneMode}{extra} " +
            $"activeSceneNow={activeScene} connected={nm.ConnectedClientsList.Count} " +
            $"(local IsServer={nm.IsServer} IsClient={nm.IsClient} LocalClientId={nm.LocalClientId})");

        if (sceneEvent.ClientsThatTimedOut is { Count: > 0 })
        {
            Debug.LogWarning(
                "[NetSceneDiag] One or more clients timed out during this scene event. " +
                "Check client Player.log for load errors or increase LoadSceneTimeOut on NetworkManager.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetSceneDiag] ClientConnected clientId={clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        NetworkManager nm = NetworkManager.Singleton;
        string activeScene = nm != null ? SceneManager.GetActiveScene().name : "?";
        Debug.Log(
            $"[NetSceneDiag] ClientDisconnected clientId={clientId} activeScene={activeScene} " +
            $"localClientId={nm?.LocalClientId} isListening={nm?.IsListening}");
    }
}
