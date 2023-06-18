using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSandbox.TileTerrain
{
    /// <summary>
    /// Apply Heightmaps to your Tile Terrain
    /// </summary>
    [RequireComponent(typeof(TileTerrain))]
    public class TileTerrainHeightmap : MonoBehaviour
    {
        public Texture2D heightmap;
        [Tooltip("The amount of steps the terrain will deform")] public int steps = 32;
        TileTerrain _terrain;

        Texture2D _tempHeightmap;

        // Start is called before the first frame update
        void Start()
        {
            _terrain = gameObject.GetComponent<TileTerrain>();
        }

        public void ApplyToTerrain(bool resetBeforeApply, bool subtract)
        {
            if (resetBeforeApply)
                _terrain.ResetTerrainHeight();

            if (_terrain == null)
            {
                _terrain = gameObject.GetComponent<TileTerrain>();
                if (_terrain == null)
                    return;
            }


            _tempHeightmap = new Texture2D(heightmap.width, heightmap.height, heightmap.format, heightmap.mipmapCount, true);
            //_tempHeightmap.LoadRawTextureData(heightmap.GetRawTextureData());

            Graphics.CopyTexture(heightmap, _tempHeightmap);
            TextureScale.Bilinear(_tempHeightmap, _terrain.xSize + 1, _terrain.zSize + 1);

            int i = 0;
            for (int x = 0; x < _tempHeightmap.width; x++)
            {
                for (int y = 0; y < _tempHeightmap.height; y++)
                {
                    if (subtract)
                        _terrain.ChangeGridPointHeight(i, Mathf.RoundToInt(_tempHeightmap.GetPixel(x, y).grayscale * (float)-steps), true);
                    else
                        _terrain.ChangeGridPointHeight(i, Mathf.RoundToInt(_tempHeightmap.GetPixel(x, y).grayscale * (float)steps), true);

                    i++;
                }
            }

            _terrain.SetToUnityMesh();
        }

    }
}