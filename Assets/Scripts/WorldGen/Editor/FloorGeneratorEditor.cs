using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.WorldGen
{
  [CustomEditor(typeof(FloorGenerator))]
  public class FloorGeneratorEditor : Editor
  {
    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      var sp = serializedObject.FindProperty("tiles");
      EditorGUILayout.PropertyField(sp, true);

      var generator = (FloorGenerator)target;

      EditorGUI.BeginChangeCheck();
      generator.map = (Sprite)EditorGUILayout.ObjectField("Map", generator.map, typeof(Sprite), true);
      if (EditorGUI.EndChangeCheck())
      {
        generator.matrix = MatrixManager.PixelsToWallMatrix(generator.map);
        generator.Generate();
      }

      if (GUILayout.Button("Generate"))
      {
        generator.matrix = MatrixManager.PixelsToWallMatrix(generator.map);
        generator.Generate();
      }

      serializedObject.ApplyModifiedProperties();
    }
  }
}
