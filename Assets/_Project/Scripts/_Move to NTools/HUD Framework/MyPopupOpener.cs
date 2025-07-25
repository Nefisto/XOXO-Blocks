using UnityEngine;

public class PopupSettings { }

public class MyPopupOpener : MonoBehaviour
{
    public MyPopup popupPrefab;

    private Canvas canvas;

    protected void Start() => canvas = GetComponentInParent<Canvas>();

    public virtual void OpenPopup (PopupSettings settings = null)
    {
        settings ??= new PopupSettings();

        var popup = Instantiate(popupPrefab, canvas.transform, false);
        popup.Setup(settings);

        popup.gameObject.SetActive(true);
        popup.transform.localScale = Vector3.zero;

        popup.Open();
    }
}