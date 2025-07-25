using UnityEngine;
using UnityEngine.UI;

public class GameOptionsPopUp : MyPopup
{
    [Header("References")]
    [SerializeField]
    private Image background;

    public override void Setup (PopupSettings settings) { }

    public class GameOptionsPopupSettings : PopupSettings
    {
        public Color backgroundColor = Color.cyan;
    }
}