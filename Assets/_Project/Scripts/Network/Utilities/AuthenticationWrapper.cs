using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    Timeout
}

public enum AuthKind
{
    Anonymously
}

public class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async UniTask<AuthState> DoAuth (AuthKind authKind, int tries = 5)
    {
        if (AuthState == AuthState.Authenticated)
            return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating!");
            await Authenticating();
            return AuthState;
        }

        switch (authKind)
        {
            case AuthKind.Anonymously:
                await AuthenticateAnonymouslyAsync(tries);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(authKind), authKind, null);
        }

        return AuthState;
    }

    private static async UniTask AuthenticateAnonymouslyAsync (int tries)
    {
        var triedAmount = 0;
        AuthState = AuthState.Authenticating;
        while (AuthState == AuthState.Authenticating && triedAmount < tries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn
                    && AuthenticationService.Instance.IsAuthorized)
                    AuthState = AuthState.Authenticated;
            }
            catch (AuthenticationException e)
            {
                Debug.Log(e);
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException e)
            {
                Debug.Log(e);
                AuthState = AuthState.Error;
            }

            triedAmount++;
            await UniTask.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning("Players was not signed din successfully");
            AuthState = AuthState.Timeout;
        }
    }

    private static async UniTask Authenticating()
    {
        while (AuthState is AuthState.Authenticating or AuthState.NotAuthenticated)
            await UniTask.Delay(200);
    }
}