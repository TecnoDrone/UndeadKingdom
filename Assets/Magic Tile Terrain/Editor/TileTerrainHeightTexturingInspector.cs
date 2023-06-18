using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicSandbox.TileTerrain
{
    [CustomEditor(typeof(TileTerrainHeightTexturing))]
    public class TileTerrainHeightTexturingInspector : Editor
    {
        TileTerrain _terrain;
        TileTerrainHeightTexturing _target;
        public override void OnInspectorGUI()
        {
            _target = (TileTerrainHeightTexturing)target;
            if (_target == null)
                return;

            //base.OnInspectorGUI();

            if (_terrain == null)
            {
                _terrain = _target.gameObject.GetComponent<TileTerrain>();
                if (_terrain == null)
                    return;
            }

            if (GUILayout.Button(GetHelpButtonText()))
            {
                _target.helpFoldout = !_target.helpFoldout;
            }

            if (_target.helpFoldout)
                DrawHelp();

            DrawHeightTextureSelection();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }

        void DrawHeightTextureSelection()
        {
            GUILayout.Space(10);

            int _remove = -1;
            int i = 0;
            foreach (TileTerrainHeightTexturing.HeightTexture _ht in _target.heightTextures)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        _ht.textureIndex--;
                        if (_ht.textureIndex < 0)
                            _ht.textureIndex = _terrain.textureSet.texturesCount - 1;
                    }

                    GUILayout.Box(_terrain.textureSet.GetTexture(_ht.textureIndex));

                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        _ht.textureIndex++;
                        if (_ht.textureIndex >= _terrain.textureSet.texturesCount)
                            _ht.textureIndex = 0;
                    }

                    GUILayout.Space(10);
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("Step Height:");
                        _ht.stepHeight = EditorGUILayout.IntSlider(_ht.stepHeight, -128, 128);
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Random Texture Rotation:");
                            _ht.randomRotation = EditorGUILayout.Toggle(_ht.randomRotation);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(10);

                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _remove = i;
                    }
                }
                GUILayout.EndHorizontal();
                i++;

                GUILayout.Box("", GUILayout.Height(5), GUILayout.Width(Screen.width));
            }

            if (_remove >= 0)
            {
                _target.heightTextures.RemoveAt(_remove);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Add Height Texture"))
            {
                _target.heightTextures.Add(new TileTerrainHeightTexturing.HeightTexture());
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Apply Textures", GUILayout.Height(30)))
            {
                _target.ApplyTextures();
            }
        }

        void DrawHelp()
        {
            EditorGUILayout.HelpBox("Click 'Add Height Texture' to add more textures that you want to automaticly add to your terrain. Use the 'Step Height' slider to set the height at which this texture is applied to the terrain. 'Step Height' is measured in TileTerrain snapping steps. Each step is 0.5 units. 'Random Texture Rotation' will rotate each tile in a random direction once you hit Apply. ", MessageType.Info);
        }

        string GetHelpButtonText()
        {
            if (_target.helpFoldout)
                return "Close";
            else
                return "Help";
        }
    }
}