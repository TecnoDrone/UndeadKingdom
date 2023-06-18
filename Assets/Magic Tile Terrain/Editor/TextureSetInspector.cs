using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MagicSandbox.TileTerrain
{
    [CustomEditor(typeof(TextureSet))]
    public class TextureSetInspector : Editor
    {
        TextureSet _target;
        public override void OnInspectorGUI()
        {
            _target = (TextureSet)target;
            if (_target == null)
                return;


            base.OnInspectorGUI();
            GUILayout.Space(15);

            if (_target.atlasOutOfSync)
                EditorGUILayout.HelpBox("The Texture Atlas and the above Texture List are out of sync. You may want to Update your Atlas.", MessageType.Warning);

            string _buttonText = "Create New Atlas";
            if (_target.textureAtlas != null)
                _buttonText = "Update Texture Atlas";

            //TODO: When non square texture UV problems are fixed - this can go
            if (_target.textureAtlas != null)
            {
                if (_target.textureAtlas.width != _target.textureAtlas.height)
                    EditorGUILayout.HelpBox("In this Version of Magic Tile Terrain, UV rotations only work on Square texture altases. Please make sure to add more Textures or remove ones to generate a Square (1x1 aspect) Atlas.", MessageType.Warning);
            }

            if (GUILayout.Button(_buttonText))
            {
                _target.TryCreateTextureAtlas();
            }

            if (_target.textureAtlas == null)
                return;

            GUILayout.Label("Atlas size: " + _target.textureAtlas.width.ToString() + "x" + _target.textureAtlas.height);


            if (_target.textureAtlas != null)
            {
                GUILayout.Box(_target.textureAtlas, GUILayout.Width(Screen.width / 1.65f), GUILayout.Height(Screen.width / 1.65f));
            }

            if (GUI.changed)
                _target.CheckSync();
        }
    }
}