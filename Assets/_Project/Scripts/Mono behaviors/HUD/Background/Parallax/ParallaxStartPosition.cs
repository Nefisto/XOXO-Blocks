using UnityEngine;

public class ParallaxStartPosition : MonoBehaviour
{
    private void Awake() => ServiceLocator.VisualReferences.ParallaxStartPosition = transform;
}