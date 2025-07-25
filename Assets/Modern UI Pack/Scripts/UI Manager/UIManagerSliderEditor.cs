#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(UIManagerSlider))]
    public class UIManagerSliderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var UIManagerSlider = target as UIManagerSlider;

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerSlider.hasLabel)))
            {
                if (group.visible)
                    UIManagerSlider.label =
                        EditorGUILayout.ObjectField("Label", UIManagerSlider.label, typeof(TextMeshProUGUI), true) as
                            TextMeshProUGUI;
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerSlider.hasPopupLabel)))
            {
                if (group.visible)
                    UIManagerSlider.popupLabel =
                        EditorGUILayout.ObjectField("Popup Label", UIManagerSlider.popupLabel, typeof(TextMeshProUGUI),
                            true) as TextMeshProUGUI;
            }
        }
    }
}
#endif