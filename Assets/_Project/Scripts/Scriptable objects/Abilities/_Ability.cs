using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public abstract void Register();
    public abstract void Remove();
}