using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using MagicSandbox.UI;

namespace MagicSandbox.TileTerrain
{
    [CustomEditor(typeof(TileTerrainDetails))]
    public class TileTerrainDetailsInspector : Editor
    {
        TileTerrainDetails _target;

        Color _lineColor = new Color(0.15f, 0.15f, 0.15f);

        public override void OnInspectorGUI()
        {
            _target = (TileTerrainDetails)target;
            if (target == null)
                return;
            //base.OnInspectorGUI();

            if (GUILayout.Button(GetHelpButtonText()))
            {
                _target.helpFoldout = !_target.helpFoldout;
            }

            if (_target.helpFoldout)
            {
                DrawHelp();
            }

            DrawGizmoSettings();
            DrawDetailGeneration();
            DrawGPUInstanceDetailInfos();

            GUILayout.Space(40);
            GUILayout.Label("Generated Texture Size: " + _target.generatedTexture.width.ToString() + "x" + _target.generatedTexture.height.ToString());
            GUILayout.Label("GameObject Detail Instances: " + _target.detailInstancesCount.ToString());
            GUILayout.Label("GPU Mesh Detail Instances: " + _target.GetGPUInstancedDetailsCount().ToString());

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawGizmoSettings()
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.HelpBox("Gizmo Settings:", MessageType.None);
                if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.gizmoSettingsFoldout), GUILayout.Width(20)))
                { _target.gizmoSettingsFoldout = !_target.gizmoSettingsFoldout; }
            }
            GUILayout.EndHorizontal();

            if (!_target.gizmoSettingsFoldout)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showResolutionGrid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showPixelOverlay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmosHeightOffset"));

            EditorGUI.indentLevel--;
        }

        void DrawDetailGeneration()
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.HelpBox("Detail Generation:", MessageType.None);
                if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.detailGenerationFoldout), GUILayout.Width(20)))
                { _target.detailGenerationFoldout = !_target.detailGenerationFoldout; }
            }
            GUILayout.EndHorizontal();

            if (!_target.detailGenerationFoldout)
                return;

            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox("To determine the Terrain height, this component Raycasts onto the terrain. To do this, the terrain will be temporarily set to Layer 31.", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolution"));

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("details"));
            for (int i = 0; i < serializedObject.FindProperty("details").arraySize; i++)
            {
                SerializedProperty _prop = serializedObject.FindProperty("details").GetArrayElementAtIndex(i);
                DrawDetailElement(_prop, i);
            }

            MGUI.Line(_lineColor, EditorGUI.indentLevel);
            if (MGUI.Button("Add new Detail", EditorGUI.indentLevel))
            {
                if (_target.details.Count > 0)
                {
                    _target.details.Add(new TileTerrainDetails.Detail(_target.details[_target.details.Count - 1]));
                }
                else
                    _target.details.Add(new TileTerrainDetails.Detail());
            }

            if (MGUI.Button("Generate All Details", EditorGUI.indentLevel))
            {
                _target.Generate();
            }

            /*   GUILayout.Space(20);

              if (MGUI.Button("Clear Details", EditorGUI.indentLevel))
              {
                  _target.Clear();
              }
              if (MGUI.Button("Generate All Details", EditorGUI.indentLevel))
              {
                  _target.Generate();
              } */
            EditorGUI.indentLevel--;
            GUILayout.Space(20);
        }

        void DrawDetailElement(SerializedProperty property, int arrayIndex)
        {
            int _remove = -1;
            GUILayout.BeginHorizontal();
            {
                string _name = "Detail";
                if (property.FindPropertyRelative("prefab").objectReferenceValue != null)
                    _name = property.FindPropertyRelative("prefab").objectReferenceValue.name;

                bool _foldout = property.FindPropertyRelative("foldout").boolValue;

                if (_foldout)
                {
                    if (MGUI.Button("X", EditorGUI.indentLevel, GUILayout.Width(20)))
                    { _remove = arrayIndex; }
                }

                EditorGUILayout.HelpBox(_name, MessageType.None);

                if (GUILayout.Button(MGUI.GetFoldoutLabel(_foldout), GUILayout.Width(20)))
                { property.FindPropertyRelative("foldout").boolValue = !_foldout; }

            }
            GUILayout.EndHorizontal();

            if (!property.FindPropertyRelative("foldout").boolValue)
            {
                return;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(property);
            GUILayout.BeginHorizontal();
            {
                if (_target.details[arrayIndex].gpuDetailChunks.Count > 0)
                {
                    if (MGUI.Button("Clear", EditorGUI.indentLevel))
                    {
                        Undo.RecordObject(_target, "Clear Detail Instances");
                        _target.RemoveGPUInstancedDetails(arrayIndex);
                    }
                }

                if (MGUI.Button("Generate", EditorGUI.indentLevel))
                { _target.Generate(new int[] { arrayIndex }); }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUI.indentLevel--;

            if (_remove >= 0)
            {
                Undo.RecordObject(_target, "Remove Detail Element");
                _target.RemoveDetail(arrayIndex);
            }
        }

        void DrawHelp()
        {
            EditorGUILayout.HelpBox(
                "This component places Detail Prefabs on your terrain. First it generates a low resolution texture of the whole Terrain in order to use the pixel colors later. This way big chunks of the terrain can be reduced to a single color. To place details, add one to the list in 'Detail Generation'. Then assign a prefab and select the color this prefab is supposed to sit on. You can display the generated texture by enabling 'Show Pixel Overlay' in the Gizmo Settings. Now set a 'Color Threshold' which defines how much the color on the terrain may differ from your previously selected color. Density will define how likely it is that the prefab will be placed when all other parameters are met."
                , MessageType.Info
            );
        }

        void DrawGPUInstanceDetailInfos()
        {
            if (_target.GetGPUInstancedDetailsCount() > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.HelpBox("Generated GPU Instance Chunks:", MessageType.None);
                    if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.gpuInstancedDetailsFoldout), GUILayout.Width(20)))
                    { _target.gpuInstancedDetailsFoldout = !_target.gpuInstancedDetailsFoldout; }
                }
                GUILayout.EndHorizontal();
            }

            if (!_target.gpuInstancedDetailsFoldout)
                return;

            EditorGUI.indentLevel++;
            GUILayout.Space(10);
            int i = 0;
            foreach (string _s in _target.GetGPUInstancedDetailsInfo())
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(_s);
                    /* if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        //_target.RemoveGPUDetailInstanceChunk(i);
                    } */
                }
                GUILayout.EndHorizontal();
                i++;
            }
            EditorGUI.indentLevel--;
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