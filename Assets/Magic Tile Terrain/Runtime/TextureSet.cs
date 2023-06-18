using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicSandbox.TileTerrain
{
    /// <summary>
    /// A Texture Set is a container that holds a list of textures and converts them to a texture Atlas when needed.
    /// </summary>
    [CreateAssetMenu(menuName = "Magic Tile Terrain/ New Texture Set")]
    public class TextureSet : ScriptableObject
    {
        public int padding = 0;
        public int maxSize = 1024;
        public FilterMode filterMode = FilterMode.Bilinear;
        public Material material;

        /// <summary>
        /// Returns the amount of textures assigned to this set. This is not the amount of textures in a baked atlas.
        /// </summary>
        /// <value></value>
        public int texturesCount { get { return _textures.Count; } }
        [SerializeField] List<Texture2D> _textures = new List<Texture2D>();

        public Rect[] atlasRects { get { return _atlasRects; } }
        [HideInInspector][SerializeField] Rect[] _atlasRects = new Rect[0];

        public Texture2D textureAtlas { get { return _textureAtlas; } }
        [HideInInspector][SerializeField] Texture2D _textureAtlas;

        public Material atlasMaterial { get { return _atlasMaterial; } }
        [HideInInspector][SerializeField] Material _atlasMaterial;

#if UNITY_EDITOR
        /// <summary>
        /// Whenever there are changes made in this asset this number increases. Other components are then able to check if they are up to date.
        /// </summary>
        /// <value></value>
        public int changeset { get; private set; }

        [HideInInspector] public bool atlasOutOfSync;
        public void CheckSync()
        {
            if (texturesCount != atlasRects.Length)
                atlasOutOfSync = true;
            else
                atlasOutOfSync = false;
        }

        void OnEnable()
        {
            CheckSync();
        }


        public bool TryCreateTextureAtlas()
        {
            if (material == null)
            {
                Debug.LogError("Assign a Material to the TextureSet first.");
                return false;
            }

            //Remove previous Texture
            if (_textureAtlas != null)
                AssetDatabase.RemoveObjectFromAsset(_textureAtlas);
            //Remove previous Mat
            if (_atlasMaterial != null)
                AssetDatabase.RemoveObjectFromAsset(_atlasMaterial);

            _textureAtlas = new Texture2D(maxSize, maxSize);
            _atlasRects = new Rect[0];

            _textureAtlas.filterMode = filterMode;

            _atlasRects = _textureAtlas.PackTextures(_textures.ToArray(), padding, maxSize);
            if (atlasRects.Length <= 0)
                return false;

            _atlasMaterial = new Material(material);
            _atlasMaterial.mainTexture = _textureAtlas;

            AssetDatabase.AddObjectToAsset(_textureAtlas, this);
            AssetDatabase.AddObjectToAsset(_atlasMaterial, this);

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this);
            changeset++;
            return true;
        }
#endif

        /// <summary>
        /// Returns a texture from the original texture list. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Texture2D GetTexture(int index)
        {
            try
            {
                return _textures[index];
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
    }
}