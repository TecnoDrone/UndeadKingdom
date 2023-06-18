using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace MagicSandbox.TileTerrain
{
    [InitializeOnLoad]
    [CreateAssetMenu(menuName = "Magic Tile Terrain/ Create Info Asset")]
    public class PackageInfo : ScriptableObject
    {
        [SerializeField] bool _startupMessage;  //Was the startup message shown?

        void OnEnable()
        {
            if (!_startupMessage)
            {
                PackageInfoWindow.ShowWindow();
                _startupMessage = true;
            }

        }
    }
}