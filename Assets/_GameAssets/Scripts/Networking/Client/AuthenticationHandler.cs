using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationHandler 
{
    public static AuthenticationState AuthenticationState { get; private set; } = AuthenticationState.NotAuthenticated;

    public static async UniTask<AuthenticationState> DoAuth(int maxTries = 5)
    {
        if (AuthenticationState == AuthenticationState.Authenticated)
        {
            return AuthenticationState;
        }

        if (AuthenticationState == AuthenticationState.Authenticating)
        {
            Debug.LogWarning("Authentication is already in progress.");
            await Authenticating();
            return AuthenticationState;
        }

        await SignInAnonymouslyAsync(maxTries);

        return AuthenticationState;
    }

    // wait until authentication is complete
    private static async UniTask<AuthenticationState> Authenticating()
    {
        while (AuthenticationState == AuthenticationState.Authenticating || AuthenticationState == AuthenticationState.NotAuthenticated)
        {
            await UniTask.Delay(200);
        }

        return AuthenticationState;
    }

    private static async UniTask SignInAnonymouslyAsync(int maxTries)
    {
        AuthenticationState = AuthenticationState.Authenticating;

        int tries = 0;

        while (AuthenticationState == AuthenticationState.Authenticating && tries < maxTries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthenticationState = AuthenticationState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException authenticationException)
            {
                AuthenticationState = AuthenticationState.Error;
                Debug.LogError($"Authentication failed: {authenticationException.Message}");
            }
            catch (RequestFailedException requestFailedException)
            {
                AuthenticationState = AuthenticationState.Error;
                Debug.LogError($"Authentication failed: {requestFailedException.Message}");
            }

            tries++;
            await UniTask.Delay(1000);
        }

        if (AuthenticationState != AuthenticationState.Authenticated)
        {
            AuthenticationState = AuthenticationState.Timeout;
            Debug.LogWarning($"Authentication timed out after {maxTries} tries.");
        }
    }
}
