using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicSandbox.TileTerrain
{
    [CustomPropertyDrawer(typeof(TileTerrainDetails.Detail))]
    public class DetailPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("prefab"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("color"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("colorThreshold"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("density"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("densityRecovery"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("alignWithTerrain"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("randomRotation"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("randomScale"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("drawInstanced"));
            //EditorGUILayout.PropertyField(property.FindPropertyRelative("gpuDetailChunks"));
        }
    }

}
