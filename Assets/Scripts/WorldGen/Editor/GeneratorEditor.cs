using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.WorldGen
{
  [CustomEditor(typeof(Generator))]
  public class GeneratorEditor : Editor
  {
    Generator generator;

    public void OnEnable()
    {
      generator = (Generator)target;
      generator.wallFiller = Resources.Load<GameObject>("WorldGen/Infrastructure/Filler");
      generator.door = Resources.Load<GameObject>("WorldGen/Infrastructure/Door");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      var sp = serializedObject.FindProperty("caps");
      EditorGUILayout.PropertyField(sp, true);

      generator.wall = (GameObject)EditorGUILayout.ObjectField("Wall", generator.wall, typeof(GameObject), true);
      generator.material = (Material)EditorGUILayout.ObjectField("Material", generator.material, typeof(Material), true);
      generator.tileSprite = (Sprite)EditorGUILayout.ObjectField("Tile", generator.tileSprite, typeof(Sprite), true);
      generator.wallSprite = (Sprite)EditorGUILayout.ObjectField("WallSprite", generator.wallSprite, typeof(Sprite), true);
      generator.level = (GameObject)EditorGUILayout.ObjectField("Level", generator.level, typeof(GameObject), true);

      EditorGUI.BeginChangeCheck();
      generator.map = (Sprite)EditorGUILayout.ObjectField("Map", generator.map, typeof(Sprite), true);
      if (EditorGUI.EndChangeCheck())
      {
        UpdateMatrix();
      }

      if(GUILayout.Button("Generate"))
      {
        UpdateMatrix();
      }

      serializedObject.ApplyModifiedProperties();
    }

    void UpdateMatrix()
    {
      generator.matrix = MatrixManager.PixelsToMatrix(generator.map);
      generator.width = generator.map.texture.width;
      generator.depth = generator.map.texture.height;
      generator.GenerateLevel();
    }
  }
}
