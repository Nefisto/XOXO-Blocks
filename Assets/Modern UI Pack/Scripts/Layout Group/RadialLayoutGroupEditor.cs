#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(RadialLayoutGroup))]
    public class RadialLayoutGroupEditor : Editor
    {
        private SerializedProperty angleCenter;
        private SerializedProperty angleRange;
        private SerializedProperty angleStart;
        private SerializedProperty childRotate;
        private int currentTab;
        private GUISkin customSkin;

        private SerializedProperty layoutDir;
        private SerializedProperty radiusRange;
        private SerializedProperty radiusStart;
        private RadialLayoutGroup rlgTarget;

        private void OnEnable()
        {
            if (target == null)
                return;

            rlgTarget = target as RadialLayoutGroup;

            if (EditorGUIUtility.isProSkin)
                customSkin = MUIPEditorHandler.GetDarkEditor(customSkin);
            else
                customSkin = MUIPEditorHandler.GetLightEditor(customSkin);

            var serObj = serializedObject;
            layoutDir = serObj.FindProperty("refLayoutDir");
            radiusStart = serObj.FindProperty("refRadiusStart");
            radiusRange = serObj.FindProperty("refRadiusRange");
            angleStart = serObj.FindProperty("refAngleStart");
            angleCenter = serObj.FindProperty("refAngleCenter");
            angleRange = serObj.FindProperty("refAngleRange");
            childRotate = serObj.FindProperty("refChildRotate");
        }

        public override void OnInspectorGUI()
        {
            MUIPEditorHandler.DrawComponentHeader(customSkin, "RLG Top Header");

            var toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = MUIPEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();
            serializedObject.Update();

            MUIPEditorHandler.DrawHeader(customSkin, "Options Header", 6);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            MUIPEditorHandler.DrawPropertyPlain(layoutDir, customSkin, "Layout Direction");
            EditorGUI.indentLevel = 1;

            if (rlgTarget.layoutDir != RadialLayoutGroup.Direction.Bidirectional)
                EditorGUILayout.PropertyField(angleStart, new GUIContent("Angle Start"));
            else
                EditorGUILayout.PropertyField(angleCenter, new GUIContent("Angle Center"));

            EditorGUILayout.PropertyField(angleRange, new GUIContent("Angle Range"));
            EditorGUILayout.PropertyField(radiusStart, new GUIContent("Radius Start"));
            EditorGUILayout.PropertyField(radiusRange, new GUIContent("Radius Range"));

            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();

            childRotate.boolValue = MUIPEditorHandler.DrawToggle(childRotate.boolValue, customSkin, "Rotate Child");

            if (Application.isPlaying == false)
                Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif