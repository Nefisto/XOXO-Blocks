using System;
using UnityEngine;

public class AuthOperations : ScriptableObject
{
    public void LoginAsGuest() => GameEvents.NetworkEvents.LoggingAsGuest?.Invoke(this, EventArgs.Empty);
}