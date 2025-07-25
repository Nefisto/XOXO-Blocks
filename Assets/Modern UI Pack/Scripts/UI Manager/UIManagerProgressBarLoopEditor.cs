#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(UIManagerProgressBarLoop))]
    public class UIManagerProgressBarLoopEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var UIManagerProgressBarLoop = target as UIManagerProgressBarLoop;

            using (var group =
                   new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerProgressBarLoop.hasBackground)))
            {
                if (group.visible)
                    UIManagerProgressBarLoop.background = EditorGUILayout.ObjectField("Background",
                        UIManagerProgressBarLoop.background, typeof(Image), true) as Image;
            }
        }
    }
}
#endif