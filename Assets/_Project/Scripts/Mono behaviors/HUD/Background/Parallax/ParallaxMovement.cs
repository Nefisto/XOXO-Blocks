using NTools;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ParallaxMovement : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    [MinMaxSlider(0f, 1f)]
    private Vector2 movementRange = new(0.5f, 0.7f);

    [TitleGroup("Debug")]
    [SerializeField]
    private float selectedVelocity;

    private void Start() => selectedVelocity = movementRange.GetRandom();

    private void Update() => transform.Translate(Vector2.left * (selectedVelocity * Time.deltaTime));

    private void OnTriggerEnter2D (Collider2D other)
        => transform.position = new Vector2(ServiceLocator.VisualReferences.ParallaxStartPosition.position.x,
            transform.position.y);
}