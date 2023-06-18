using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace MagicSandbox.TileTerrain
{
    [CustomEditor(typeof(TileTerrainHeightmap))]
    public class TileTerrainHeightmapInspector : Editor
    {
        TileTerrainHeightmap _target;
        public override void OnInspectorGUI()
        {
            _target = (TileTerrainHeightmap)target;
            if (_target == null)
                return;


            EditorGUILayout.HelpBox("Make sure that the heightmap texture is Read/Write enabled and NOT compressed (Texture Settings -> Compression -> None)", MessageType.Warning);
            base.OnInspectorGUI();


            if (GUILayout.Button("Add to Terrain"))
            {
                _target.ApplyToTerrain(false, false);
            }

            if (GUILayout.Button("Subtract from Terrain"))
            {
                _target.ApplyToTerrain(false, true);
            }


        }


    }
}