using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MagicSandbox.TileTerrain
{
    /// <summary>
    /// Automaticly Texture your Tile Terrain based on the hight of the individual tile
    /// </summary>
    [RequireComponent(typeof(TileTerrain))]
    public class TileTerrainHeightTexturing : MonoBehaviour
    {
        [System.Serializable]
        public class HeightTexture : IComparable<HeightTexture>
        {
            public int textureIndex = 0; //Index into the texture set atlas
            public int stepHeight = 0; // at what step height is this texture applied
            public bool randomRotation;

            int IComparable<HeightTexture>.CompareTo(HeightTexture other)
            {
                if (this.stepHeight > other.stepHeight)
                    return 1;
                else if (this.stepHeight == other.stepHeight)
                    return 0;
                else return -1;
            }
        }
        public List<HeightTexture> heightTextures = new List<HeightTexture>();

#if UNITY_EDITOR
        public bool helpFoldout;
#endif

        public void ApplyTextures()
        {
            TileTerrain _terrain = gameObject.GetComponent<TileTerrain>();
            if (_terrain == null)
                return;

            List<HeightTexture> _tempHeightTex = new List<HeightTexture>(heightTextures);
            _tempHeightTex.Sort();

            float[] _rotations = new float[] { 0, 90, 180, 270 };
            Vector3[] _gridPoints = null;
            float _height = 0;
            for (int i = 0; i < _terrain.tMesh.tileCount; i++)
            {
                _height = 0;
                _gridPoints = _terrain.tMesh.GetGridPoints(i);
                foreach (Vector3 _v in _gridPoints)
                {
                    _height += _v.y;
                }
                _height = _height / 4;

                for (int n = 0; n < _tempHeightTex.Count; n++)
                {
                    if (_height >= _tempHeightTex[n].stepHeight * _terrain.snap)
                    {
                        _terrain.SetTileTexture(i, _tempHeightTex[n].textureIndex);
                        //Rotate the UV if the user wants random texture rotation
                        if (_tempHeightTex[n].randomRotation)
                            _terrain.RotateTileUV(i, _rotations[UnityEngine.Random.Range(0, 4)]);
                    }

                }
            }

            _terrain.SetToUnityMesh();
        }
    }
}