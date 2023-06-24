using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomMagicSandbox.TileTerrain
{
  /// <summary>
  /// Creates a Mesh where you can assign unique texture coordinates to each Quad face. This results in more vertices but fewer drawcalls.
  /// </summary>
  [ExecuteInEditMode]
  public class TileTerrain : MonoBehaviour
  {
    public int xSize = 10;   //Size * tileSize 
    public int zSize = 10;
    public float snap => _snap;
    [SerializeField] float _snap = 0.25f;

    [SerializeField]
    [Tooltip("Sets how much the terrain will be deformed in one click.")]
    [Range(1, 16)]
    int _snapMultiplier = 1;
    public int snapMultiplier
    {
      get { return _snapMultiplier; }
      set
      {
        if (value <= 0)
        {
          _snapMultiplier = 1;
          return;
        }
        else if (value > 16)
        {
          _snapMultiplier = 16;
          return;
        }

        _snapMultiplier = value;
      }
    }

    public float tileSize = 1;
    public int subdivisions = 0;
    public Material material;
    public TextureSet textureSet;
    public int texturesCount
    {
      get
      {
        if (textureSet.atlasRects != null)
        {
          if (textureSet.atlasRects.Length > textureSet.texturesCount)
            return textureSet.texturesCount;

          return textureSet.atlasRects.Length;
        }
        return 0;
      }
    }

    public TMesh tMesh => _tMesh;
    //public TMeshDTO tMesh => _tMesh;

    #region Events
    /// <summary>
    /// Will be called whenever the TileTerrain updates its values to the Unity Mesh.
    /// </summary>
    public Action terrainChanged;
    #endregion

    [SerializeField] TMesh _tMesh;
    //[SerializeField] TMeshDTO _tMesh;

    MeshFilter _filter;
    MeshRenderer _renderer;
    [SerializeField] Mesh _mesh;
    MeshCollider _collider;

    // Start is called before the first frame update
    void OnEnable()
    {
      SetupComponents();
    }

    void SetupComponents()
    {
      //Setup Mesh Filter
      _filter = gameObject.GetComponent<MeshFilter>();
      if (_filter == null)
        _filter = gameObject.AddComponent<MeshFilter>();
      _filter.sharedMesh = _mesh;

      //Setup Mesh Renderer
      _renderer = gameObject.GetComponent<MeshRenderer>();
      if (_renderer == null)
        _renderer = gameObject.AddComponent<MeshRenderer>();
      if (textureSet != null)
        if (textureSet.atlasMaterial != null)
          _renderer.sharedMaterial = textureSet.atlasMaterial;

      //Setup Mesh Collider
      _collider = gameObject.GetComponent<MeshCollider>();
      if (_collider == null)
        _collider = gameObject.AddComponent<MeshCollider>();
      _collider.sharedMesh = _filter.sharedMesh;
    }

    public void CreateTerrain()
    {
      if (xSize * zSize * 4 >= 65000)
      {
        Debug.LogError("Terrain Vertex count is limited to 65k. If you need bigger Terrains, use multiple Terrain Objects.");
        return;
      }

      if (xSize <= 0)
        xSize = 1;
      if (zSize <= 0)
        zSize = 1;

      _snap = tileSize / 16f; //Set the snap to 1/16 of a tile. We can later modify the Multiplier to make editing easier.
      _mesh = new Mesh();

      _tMesh = new TMesh();
      _tMesh.Create(new Vector2Int(xSize, zSize), tileSize);
      //var _tMesh = new TMeshFactory()
      //  .WithTileSize(tileSize)
      //  .WithMeshSize(xSize, zSize)
      //  .Build();

      //Set all tiles to the first texture in the atlas
      //Optional - may be done after setting the textureSet
      if (textureSet != null)
      {
        for (int i = 0; i < _tMesh.tileCount; i++)
        {
          SetTileTexture(i, 0);
        }
      }

      _tMesh.SetUnityMesh(_mesh);

      SetupComponents();

#if UNITY_EDITOR
      gizmoPosition = 0;
#endif
    }

    public void ChangeGridPointHeight(int index, int snapSteps, bool up)
    {
      if (index >= tMesh.gridPointsCount)
      {
        //Debug.LogError("Index ( " + index + " ) is greater than tMesh grid points count ( " + tMesh.gridPointsCount + " ).");
        return;
      }
      if (index < 0)
      {
        //Debug.LogError("Index is < 0 ");
        return;
      }

      if (up)
        tMesh.SetGridPointPosition(index, tMesh.gridPoints[index] += Vector3.up * snap * snapSteps);
      else
        tMesh.SetGridPointPosition(index, tMesh.gridPoints[index] += Vector3.down * snap * snapSteps);
    }

    public void ResetTerrainHeight()
    {
      for (int i = 0; i < tMesh.gridPointsCount; i++)
      {
        tMesh.SetGridPointPosition(i, new Vector3(tMesh.gridPoints[i].x, 0, tMesh.gridPoints[i].z));
      }
    }

    public void RotateTileUV(int tileIndex, bool counterClockwise = false)
    {
      Vector2[] _currentUVs = _tMesh.GetUVs(tileIndex);
      Vector2 _pivot = GetUVPivotPoint(_currentUVs);
      for (int i = 0; i < _currentUVs.Length; i++)
      {
        if (counterClockwise)
          _currentUVs[i] = RotatePointAroundPivot(_currentUVs[i], _pivot, new Vector3(0, 0, -90));
        else
          _currentUVs[i] = RotatePointAroundPivot(_currentUVs[i], _pivot, new Vector3(0, 0, 90));
      }

      _tMesh.SetUVCoordinates(tileIndex, _currentUVs, 0);
    }

    public void RotateTileUV(int tileIndex, float degrees = 90)
    {
      Vector2[] _currentUVs = _tMesh.GetUVs(tileIndex);
      Vector2 _pivot = GetUVPivotPoint(_currentUVs);
      for (int i = 0; i < _currentUVs.Length; i++)
      {
        _currentUVs[i] = RotatePointAroundPivot(_currentUVs[i], _pivot, new Vector3(0, 0, degrees));
      }

      _tMesh.SetUVCoordinates(tileIndex, _currentUVs, 0);
    }

    public void RotateTriangles(int tileIndex)
    {
      _tMesh.RotateTriangles(tileIndex);
    }

    public void SetTileTexture(int tileIndex, int textureAtlasIndex)
    {
      if (textureSet == null)
      { Debug.LogError("There is no TextureSet assigned."); return; }
      if (textureSet.atlasRects == null)
      { Debug.LogError("Make sure that you have created a Texture Atlas from your TextureSet."); return; }
      if (textureSet.atlasRects.Length <= 0)
      { Debug.LogError("Make sure that you have created a Texture Atlas from your TextureSet."); return; }

      if (textureAtlasIndex > textureSet.atlasRects.Length || textureAtlasIndex < 0)
      { Debug.LogError("Texture Atlas index is out of range."); return; }

      if (tileIndex >= _tMesh.tileCount || tileIndex < 0)
      { Debug.LogError("Tile index is out of range."); return; }

      Vector2[] _newUV = new Vector2[]
      {
            new Vector2(textureSet.atlasRects[textureAtlasIndex].x,textureSet.atlasRects[textureAtlasIndex].y),
            new Vector2(textureSet.atlasRects[textureAtlasIndex].x,textureSet.atlasRects[textureAtlasIndex].y+ textureSet.atlasRects[textureAtlasIndex].height),
            new Vector2(textureSet.atlasRects[textureAtlasIndex].x+ textureSet.atlasRects[textureAtlasIndex].width,textureSet.atlasRects[textureAtlasIndex].y),
            new Vector2(textureSet.atlasRects[textureAtlasIndex].x+ textureSet.atlasRects[textureAtlasIndex].width,textureSet.atlasRects[textureAtlasIndex].y+ textureSet.atlasRects[textureAtlasIndex].height),
      };

      _tMesh.SetUVCoordinates(tileIndex, _newUV, 0);
    }

    public void SetToUnityMesh()
    {

      if (terrainChanged != null)
        terrainChanged.Invoke();

      _tMesh.SetUnityMesh(_mesh);

      GenerateLightmappingUVs();

      _filter.sharedMesh = _mesh;

      _collider.sharedMesh = _mesh;
    }

    /// <summary>
    /// Sets the Mesh UVs on UV channel 2 and 3 to enable lightmapping and GI.
    /// </summary>
    public void GenerateLightmappingUVs()
    {
      List<Vector2> _uvs = new List<Vector2>();

      for (int i = 0; i < _tMesh.vericesCount; i++)
      {
        Vector3 _currentVPos = _tMesh.gridPoints[_tMesh.vertices[i]];
        float _x = GetNormalizedFloat(0, xSize * tileSize, _currentVPos.x);
        float _y = GetNormalizedFloat(0, zSize * tileSize, _currentVPos.z);

        _uvs.Add(new Vector2(_x, _y));
      }

      _mesh.SetUVs(1, _uvs);
      _mesh.SetUVs(2, _uvs);
    }

    public void UpdateRendererMaterial()
    {
      if (_renderer != null)
        if (textureSet != null)
          if (textureSet.atlasMaterial != null)
            _renderer.sharedMaterial = textureSet.atlasMaterial;
    }

    #region  Utility
    int GetTileIndex(Vector3 worldPosition)
    {

      return -1;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
      Vector3 dir = point - pivot; // get point direction relative to pivot
      dir = Quaternion.Euler(angles) * dir; // rotate it
      point = dir + pivot; // calculate rotated point
      return point; // return it
    }

    Vector2 GetUVPivotPoint(Vector2[] uv)
    {
      //Position 0 and 3 are always on the opposide side of the Quad. So the midpoint is always between them on a quad that has 4 sides with the same lenght.
      Vector2 _midpoint = (uv[0] + uv[3]) * 0.5f;
      return new Vector2(_midpoint.x, _midpoint.y);
    }

    /// <summary>
    /// Returns a float between 0 and 1 based on the min and max parameter.
    /// </summary>
    /// <returns>The normalized float.</returns>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Max.</param>
    /// <param name="value">Value.</param>
    public static float GetNormalizedFloat(float min, float max, float value)
    {
      if (value == 0)
        return 0;
      if (value > max)
        return 1;
      if (value < min)
        return 0;

      float _range = Mathf.Abs(max - min);

      float _returnValue = Mathf.Abs(value - min) / _range;
      if (float.IsNaN(_returnValue))
        return 0;
      else
        return _returnValue;
    }
    #endregion

#if UNITY_EDITOR
    public bool showInfoFoldout;
    public bool createNewTerrainFoldout;
    public bool meshInfoFoldout;
    public bool textureAtlasInfoFoldout;
    public bool selectedTileInfoFoldout;
    public bool editFoldout;
    public bool textureListFoldout;
    public bool tileEditTab;
    public bool vertexPaintTab;
    public bool heightEditTab = true;
    public bool sceneViewToolsFoldout;

    public int gizmoPosition;
    public Vector3 mousePosition;

    public int selectedTextureAtlasIndex; //When the user selects a texture through UI, this stores the index of that texture inside the texture atlas.
    public int tilePaintMode; //0 is Texture painting. 1 is Texture rotation. 2 is Triangle rotation.

    [Tooltip("If enabled the SceneView will look at what you have selected after each mouse click. This is useful if you are working with a Laptop touchpad.")]
    public bool moveSceneViewToSelection;

    [Tooltip("Scale of the terrain editor handles.")]
    [Range(1, 4)]
    public float handleSize = 1;
    public float GetDynamicHandleSize()
    {
      return tileSize * 0.08f * handleSize;
    }

    [Tooltip("Reduce the draw distance for handles to increase performance in editor.")]
    [Range(1f, 100)]
    public float handleDisplayDistance = 50;

    [Tooltip("Scale of the brush handle.")]
    [Range(0.1f, 50f)]
    public float brushHandleSize = 0.5f;

    [Tooltip("The Vertex Color you want to apply to the Terrain")]
    public Color targetVertexColor;

    [Tooltip("The Vertex Color of the selected grid point")]
    [NonSerialized] public Color currentVertexColor;


    [HideInInspector] public int lastTextureSetChangeset;  //Cheks against the TextureSet if we need to update some stuff

    void OnDrawGizmos()
    {
      if (Selection.activeTransform == null)
        return;

      if (Selection.activeTransform.gameObject != gameObject)
        return;


      if (_tMesh == null)
        return;
      if (!_tMesh.valid)
        return;

      Gizmos.matrix = transform.localToWorldMatrix;
      Handles.matrix = transform.localToWorldMatrix;

      if (_tMesh.gridPointsCount < gizmoPosition)
        return;
      Gizmos.color = new Color(0, 0.5f, 0.8f, 0.1f);
      // Vector3 _centerPos = _tMesh.gridPoints[_tMesh.GridPointIndexToTileIndex(gizmoPosition)] + new Vector3(1, 0, 1) * tileSize * 0.5f;

      DrawSelectedTile();
      //DrawVertexNormals();  //Use for debug 
    }

    void DrawSelectedTile()
    {
      if (!editFoldout)
        return;
      if (!tileEditTab)
        return;

      int[] _tri = _tMesh.GetTriangleIndicesAtTile(gizmoPosition);

      Handles.DrawLine(_tMesh.gridPoints[_tMesh.vertices[_tri[0]]], _tMesh.gridPoints[_tMesh.vertices[_tri[1]]]);
      Handles.DrawLine(_tMesh.gridPoints[_tMesh.vertices[_tri[1]]], _tMesh.gridPoints[_tMesh.vertices[_tri[2]]]);
      Handles.DrawLine(_tMesh.gridPoints[_tMesh.vertices[_tri[2]]], _tMesh.gridPoints[_tMesh.vertices[_tri[0]]]);

      Handles.DrawLine(_tMesh.gridPoints[_tMesh.vertices[_tri[2]]], _tMesh.gridPoints[_tMesh.vertices[_tri[5]]]);
      Handles.DrawLine(_tMesh.gridPoints[_tMesh.vertices[_tri[5]]], _tMesh.gridPoints[_tMesh.vertices[_tri[1]]]);

    }

    void DrawVertexNormals()
    {
      int i = 0;
      foreach (Vector3 _pos in _mesh.vertices)
      {
        Handles.DrawLine(_pos, _pos + _mesh.normals[i]);
        i++;
      }
    }

#endif
  }
}