using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Scripts.WorldGen
{
  [CustomEditor(typeof(Generator))]
  public class GeneratorEditor : Editor
  {
    public Sprite[] caps;

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      var sp = serializedObject.FindProperty("caps");
      EditorGUILayout.PropertyField(sp, true);

      var generator = (Generator)target;

      generator.wall = (GameObject)EditorGUILayout.ObjectField("Wall", generator.wall, typeof(GameObject), true);
      generator.cap = (GameObject)EditorGUILayout.ObjectField("Cap", generator.cap, typeof(GameObject), true);
      generator.floor = (GameObject)EditorGUILayout.ObjectField("Floor", generator.floor, typeof(GameObject), true);

      EditorGUI.BeginChangeCheck();
      generator.map = (Sprite)EditorGUILayout.ObjectField("Map", generator.map, typeof(Sprite), true);
      if (EditorGUI.EndChangeCheck())
      {
        generator.matrix = MatrixManager.PixelsToMatrix(generator.map);
        generator.Generate();
      }

      if(GUILayout.Button("Generate"))
      {
        generator.matrix = MatrixManager.PixelsToMatrix(generator.map);
        generator.Generate();
      }

      serializedObject.ApplyModifiedProperties();
    }
  }
}
