using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Image image;

    private void Awake()
    {
        ServiceLocator.FadingManager = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public async UniTask FadeOutAsync (float duration)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        image.gameObject.SetActive(true);
        await DOTween
            .Sequence()
            .Append(image.DOFade(0f, duration))
            .AsyncWaitForCompletion();

        image.gameObject.SetActive(false);
    }

    public async UniTask FadeInAsync (float duration)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.gameObject.SetActive(true);
        await DOTween
            .Sequence()
            .Append(image.DOFade(1f, duration))
            .AsyncWaitForCompletion();
    }
}