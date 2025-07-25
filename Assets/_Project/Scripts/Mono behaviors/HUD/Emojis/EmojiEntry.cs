using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class EmojiEntry : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private Image emojiIcon;

    [TitleGroup("Settings")]
    [SerializeField]
    private Button button;

    public void Setup (EmojiData data, Action onClickCallback)
    {
        emojiIcon.sprite = data.Icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke());
    }
}