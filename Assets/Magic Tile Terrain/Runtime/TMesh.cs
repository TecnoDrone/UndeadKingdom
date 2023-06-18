using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicSandbox.TileTerrain
{
    /// <summary>
    ///TMesh is a Grid based mesh that features individually modifiable Quads. This however comes at a cost. The mesh has significantly more verticies
    ///since it wont share them with neighboring tris/quads. However  -this enables us to give each tri or quad a unique texture position and makes working
    ///with texture atlases easy. So we are trading vertices for draw calls. 
    /// </summary>
    [System.Serializable]
    public class TMesh
    {
        public enum Shading { Smooth, Flat }
        public Shading shading { get { return _shading; } set { _shading = value; } }
        Shading _shading = Shading.Smooth;

        public bool valid
        {
            get { if (gridPoints.Count > 0) { return true; } return false; }
        }
        /// <summary>
        /// Grid points basicly are what vertices are in a regular mesh. They are called differently to avoid confusion.
        /// Each grid point may hold a number of vertices. This is to enable iE seperate UVs for each face/quad.
        /// Moving a grid point will automaticly move all attached vertices as well.
        /// </summary>
        /// <value></value>
        public List<Vector3> gridPoints { get { return _gridPoints; } }
        public int gridPointsCount { get { return _gridPoints.Count; } }
        public int tileCount { get { return gridPointsCount - _sizeZ - _sizeX - 1; } }
        /// <summary>
        /// Vertices that point to a position in the gridPoints list.
        /// </summary>
        /// <value></value>
        public List<int> vertices { get { return _vertices; } }
        public int vericesCount { get { return vertices.Count; } }

        /// <summary>
        /// Triangles is a 3-tuple (vert0Index, vert1Index, vert2Index) pointing into the vertices list
        /// </summary>
        /// <value></value>
        public List<int> triangles { get { return _triangles; } }
        public int trianglesCount { get { return _triangles.Count; } }

        public int uvCount { get { return _uv1.Count; } }

        [SerializeField] List<Vector3> _gridPoints = new List<Vector3>();
        [SerializeField] List<int> _vertices = new List<int>();
        [SerializeField] List<int> _triangles = new List<int>();
        [SerializeField] List<int> _gridIndexToTriangle = new List<int>(); //This list contains indices to triangles for each tile on the grid. Starting bottom left to top right.
                                                                           //  List<Bounds> _tileBounds = new List<Bounds>(); //One Bounds for each tile position encapsulating all positions belonging to that quad 

        [SerializeField] List<List<Vector2>> _uvs = new List<List<Vector2>>();   //Lists that contains all UVs mapped 1to1 onto the vertices list. First index is the UV set itself ( UV0, UV1, UV2)
        [SerializeField] List<int> _gridIndexToUV = new List<int>(); //Grid index points into the UV list. 

        //Naming like in Unity Mesh - UV1 is often called UV0
        [SerializeField] List<Vector2> _uv1 = new List<Vector2>(); //Main Texture UV
        [SerializeField] List<Vector2> _uv2 = new List<Vector2>();//Baked Lightmaps
        [SerializeField] List<Vector2> _uv3 = new List<Vector2>(); //Baked GI

        [SerializeField] List<Color> _vertexColors = new List<Color>();

        [SerializeField] int _sizeX; //How many tiles on the X axis.  
        [SerializeField] int _sizeZ;
        [SerializeField] float _tileSize;

        /// <summary>
        /// Create a new Tile Mesh with the following dimensions in tile size.
        /// </summary>
        /// <param name="dimensions"></param>
        public TMesh()
        {
            _gridPoints = new List<Vector3>();
            _vertices = new List<int>();
            _triangles = new List<int>();
            _uvs = new List<List<Vector2>>();
        }

        #region Initialization
        public void Create(Vector2Int dimensions, float tileSize)
        {
            _sizeX = dimensions.x;
            _sizeZ = dimensions.y;
            _tileSize = tileSize;
            CreateGridMesh(dimensions, tileSize);    //Initialize the TMesh with its grid positions.
        }

        void CreateGridMesh(Vector2Int dimensions, float tileSize)
        {
            int _gridPointCount = (dimensions.x + 1) * (dimensions.y + 1);
            _gridPoints = new List<Vector3>(new Vector3[_gridPointCount]);
            _gridIndexToTriangle = new List<int>(new int[_gridPointCount]);

            _uvs.Clear();
            _uvs.Add(new List<Vector2>()); //UV0 Textures
            _uvs.Add(new List<Vector2>()); //UV1 Lightmapping

            _uv1 = new List<Vector2>(tileCount * 4);
            _uv2 = new List<Vector2>(tileCount * 4);
            _uv3 = new List<Vector2>(tileCount * 4);

            _gridIndexToUV = new List<int>(new int[_gridPointCount]);

            //Define Grid point positions
            for (int z = 0, i = 0; z <= dimensions.y; z++) //Z axis
            {
                for (int x = 0; x <= dimensions.x; x++, i++) //X axis
                {
                    if (i > _gridPoints.Count)
                    {
                        Debug.Log("i=" + i + " gridPoints.Count=" + _gridPoints.Count);
                        continue;
                    }
                    _gridPoints[i] = new Vector3((float)x * tileSize, 0, (float)z * tileSize);
                }
            }

            //Create all Vertices and Triangles 
            for (int i = 0; i < _sizeX * _sizeZ; i++)
            {
                CreateQuad(i);
            }

            //Create vertex Color list 
            _vertexColors = new List<Color>(vericesCount);
            for (int i = 0; i < vericesCount; i++)
            {
                _vertexColors.Add(new Color(1, 1, 1));
            }
        }

        void CreateQuad(int gridPointIndex)
        {
            int _index = GridPointIndexToTileIndex(gridPointIndex);
            //Create vertices for the new Quad.
            //Vertices are arranged in a N shape into the vertices list.
            //Because grid points are ordered in a linear fashion row by row we need to map the vertices to that format.
            int[] _verts = new int[]
            {
            _index,
            _index + _sizeX +1,
            _index +1,
            _index + _sizeX +2
            };
            _vertices.AddRange(_verts);

            //Create triangles.
            //Triangles are mapped to the Vertices List and do not need to take the gridPoints ordering into account.
            //
            int _vertIndex = gridPointIndex * 4; //The starting vertex of each quad
            int[] _tris = new int[]
            {
            //_verts[0],_verts[1],_verts[2],  _verts[2],_verts[1],_verts[3]
            _vertIndex +0,
            _vertIndex +1,
            _vertIndex+2,

            _vertIndex + 2,
            _vertIndex+1,
            _vertIndex +3
            };
            _gridIndexToTriangle[_index] = _triangles.Count;    //store the first tri index for this tile position
            _triangles.AddRange(_tris);


            //Create Texture UVs
            Vector2[] _uv = new Vector2[]
            {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1)
            };
            _gridIndexToUV[_index] = _uv1.Count;
            _uv1.AddRange(_uv);
            _uv2.AddRange(_uv);
            _uv3.AddRange(_uv);

            //Create the Grid Rect for this tile
            float _xMidpoint = _gridPoints[_verts[0]].x + _gridPoints[_verts[2]].x * 0.5f;
            float _yMidpoint = _gridPoints[_verts[0]].y + _gridPoints[_verts[2]].y * 0.5f;
        }
        #endregion

        #region Final
        public void SetUnityMesh(Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Trying to modify a NULL mesh.");
                return;
            }
            /*  Debug.Log("GridPoints: " + _gridPoints.Count);
             Debug.Log("Verts: " + _vertices.Count);
             Debug.Log("Tris: " + _triangles.Count);
             Debug.Log("UVs: " + _uvs.Count);
             Debug.Log("Quads: " + (_vertices.Count / 4).ToString() + " " + (_triangles.Count / 6).ToString()); */

            Vector3[] _uVerts = new Vector3[_vertices.Count];
            for (int i = 0; i < _uVerts.Length; i++)
            {
                //Debug.Log("Vertex at gridPoint index " + _vertices[i]);
                _uVerts[i] = _gridPoints[_vertices[i]];
            }
            mesh.vertices = _uVerts;



            mesh.triangles = _triangles.ToArray();
            mesh.SetUVs(0, _uv1);
            mesh.SetUVs(1, _uv2);
            mesh.SetUVs(2, _uv3);

            GenerateNormalsAsync(mesh);

            //Check if the Vertex Colors List is in sync with the vertices List.
            if (_vertexColors.Count != vertices.Count)
            {
                _vertexColors = new List<Color>(vericesCount);
                for (int i = 0; i < vericesCount; i++)
                {
                    _vertexColors.Add(new Color(1, 1, 1));
                }
            }

            mesh.SetColors(_vertexColors);
        }

        #endregion

        #region Modification
        public void SetGridPointPosition(int gridIndex, Vector3 position)
        {
            if (gridIndex > _gridPoints.Count || gridIndex < 0)
            {
                Debug.LogError("Index is out of range. The grid has " + _gridPoints + " points and you are trying to access index " + gridIndex);
                return;
            }
            _gridPoints[gridIndex] = position;
        }

        /// <summary>
        /// Sets the UV coordinates for a Tile (Quad).
        /// </summary>
        /// <param name="gridIndex"></param>
        /// <param name="uv"></param>
        public void SetUVCoordinates(int tileIndex, Vector2[] uv, int uvSet = 0)
        {
            //UV FORMAT - following the N pattern like the vertices list:
            //    new Vector2(0,0),
            //    new Vector2(0,1),
            //    new Vector2(1,0),
            //    new Vector2(1,1)
            if (uv.Length != 4)
            { Debug.LogError("Only UV arrays of lenght 4 are allowed"); return; }
            if (tileIndex >= _gridIndexToUV.Count || tileIndex < 0)
            { Debug.LogError("Index is out of bounds. Select an index that is inside the grid"); return; }

            int _gridIndex = GridPointIndexToTileIndex(tileIndex);
            int _startIndex = _gridIndexToUV[_gridIndex];
            switch (uvSet)
            {
                case 0:
                    {
                        if (uvSet >= _uv1.Count)
                        {
                            Debug.LogError("UV1 index out of range.");
                            return;
                        }

                        _uv1[_startIndex] = uv[0];
                        _uv1[_startIndex + 1] = uv[1];
                        _uv1[_startIndex + 2] = uv[2];
                        _uv1[_startIndex + 3] = uv[3];
                        break;
                    }
                case 1:
                    {
                        if (uvSet >= _uv2.Count)
                        {
                            Debug.LogError("UV1 index out of range.");
                            return;
                        }
                        _uv2[_startIndex] = uv[0];
                        _uv2[_startIndex + 1] = uv[1];
                        _uv2[_startIndex + 2] = uv[2];
                        _uv2[_startIndex + 3] = uv[3];
                        break;
                    }
                case 2:
                    {
                        if (uvSet >= _uv3.Count)
                        {
                            Debug.LogError("UV1 index out of range.");
                            return;
                        }
                        _uv3[_startIndex] = uv[0];
                        _uv3[_startIndex + 1] = uv[1];
                        _uv3[_startIndex + 2] = uv[2];
                        _uv3[_startIndex + 3] = uv[3];
                        break;
                    }
                default:
                    {
                        Debug.LogError("UV channel not supported.");
                        return;
                    }
            }
            /* 
                    _uvs[uvSet][_startIndex] = uv[0];
                    _uvs[uvSet][_startIndex + 1] = uv[1];
                    _uvs[uvSet][_startIndex + 2] = uv[2];
                    _uvs[uvSet][_startIndex + 3] = uv[3]; */
        }

        /// <summary>
        /// Rotates the orientation of the 2 Triangles that are part of the Quad at gridIndex.
        /// </summary>
        /// <param name="gridIndex"></param>
        public void RotateTriangles(int tileIndex)
        {
            int _index = GridPointIndexToTileIndex(tileIndex);
            if (_index > _gridIndexToTriangle.Count || _index < 0)
                return;
            //TODO: ich muss vertices tauschen und nicht tri indices
            int _triStart = _gridIndexToTriangle[_index];

            //Create a triangle index conversion.
            int[] _newTriangle = new int[]
            {
            _triangles[_triStart +1],
            _triangles[_triStart +5],
            _triangles[_triStart ],
            _triangles[_triStart ],
            _triangles[_triStart +5],
            _triangles[_triStart +2],
            };
            //Debug.Log(_newTriangle[0] + "," + _newTriangle[1] + "," + _newTriangle[2] + "," + _newTriangle[3] + "," + _newTriangle[4] + "," + _newTriangle[5]);


            for (int i = _triStart, n = 0; i < _triStart + 6; n++, i++)
            {
                _triangles[i] = _newTriangle[n];
            }

        }

        /// <summary>
        /// Sets the vertex color of all vertices at grid position.
        /// </summary>
        /// <param name="gridIndex"></param>
        public void SetVertexColor(int gridIndex, Color color)
        {
            //Check if the Vertex Colors List is in sync with the vertices List.
            if (_vertexColors.Count != vertices.Count)
            {
                _vertexColors = new List<Color>(vericesCount);
                for (int i = 0; i < vericesCount; i++)
                {
                    _vertexColors.Add(new Color(1, 1, 1));
                }
            }

            //TODO: worst possible implementation - make performant when everything else works.
            //Find all vertices at grid index
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i] == gridIndex)
                {
                    //Change color;
                    _vertexColors[i] = color;
                }
            }
        }

        #endregion


        #region Get Data

        /// <summary>
        /// Returns the 6 vertex positions belonging to the Quad at index in the grid list. 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3[] GetQuadTriangles(int gridPointIndex)
        {
            if (gridPointIndex >= _gridIndexToTriangle.Count || gridPointIndex < 0)
                return null;

            //tri format:  0,1,2  2,1,3
            int _tri = _gridIndexToTriangle[gridPointIndex];
            return new Vector3[]
            {
            _gridPoints[ _vertices[_tri]],
            _gridPoints[_vertices[_tri +1]],
            _gridPoints[_vertices[_tri +2]],
            _gridPoints[_vertices[_tri +3]],
            _gridPoints[_vertices[_tri +4]],
            _gridPoints[_vertices[_tri +5]],
            };
        }

        public int[] GetTriangleIndicesAtTile(int tileIndex)
        {
            int _index = GridPointIndexToTileIndex(tileIndex);
            if (_index > _gridIndexToTriangle.Count || _index < 0)
                return null;

            int _triStart = _gridIndexToTriangle[_index];
            return new int[]
            {
            _triangles[_triStart],_triangles[_triStart+1],_triangles[_triStart+2],_triangles[_triStart+3],_triangles[_triStart+4],_triangles[_triStart+5]
            };
        }

        /// <summary>
        /// Returns the 4 grid points that belong to a tile
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetGridPoints(int tileIndex)
        {
            int[] _tris = GetTriangleIndicesAtTile(tileIndex);

            int[] _verts = new int[] { _vertices[_tris[0]], _vertices[_tris[1]], _vertices[_tris[2]], _vertices[_tris[5]] };

            return new Vector3[] { _gridPoints[_verts[0]], _gridPoints[_verts[1]], _gridPoints[_verts[2]], _gridPoints[_verts[3]] };
        }

        public Vector2[] GetUVs(int tileIndex, int uvSet = 0)
        {
            /* if (uvSet >= _uvs.Count)
            {
                Debug.LogError("UV index out of range.");
                return null;
            } */
            int _index = GridPointIndexToTileIndex(tileIndex);
            if (_index + 3 > _gridIndexToUV.Count || _index < 0)
            { Debug.LogError("Index out of range."); return null; }


            switch (uvSet)
            {
                case 0:
                    {
                        if (_uv1.Count < uvSet)
                        { Debug.LogError("UV1 out of bounds"); return null; }
                        return new Vector2[]
                        {
                        _uv1[_gridIndexToUV[_index]],
                        _uv1[_gridIndexToUV[_index]+1],
                        _uv1[_gridIndexToUV[_index]+2],
                        _uv1[_gridIndexToUV[_index]+3],
                        };
                    }
                case 1:
                    {
                        if (_uv2.Count < uvSet)
                        { Debug.LogError("UV2 out of bounds"); return null; }
                        return new Vector2[]
                        {
                        _uv2[_gridIndexToUV[_index]],
                        _uv2[_gridIndexToUV[_index]+1],
                        _uv2[_gridIndexToUV[_index]+2],
                        _uv2[_gridIndexToUV[_index]+3],
                        };
                    }
                case 2:
                    {
                        if (_uv3.Count < uvSet)
                        { Debug.LogError("UV3 out of bounds"); return null; }
                        return new Vector2[]
                        {
                        _uv3[_gridIndexToUV[_index]],
                        _uv3[_gridIndexToUV[_index]+1],
                        _uv3[_gridIndexToUV[_index]+2],
                        _uv3[_gridIndexToUV[_index]+3],
                        };
                    }
                default:
                    {
                        Debug.LogError("UV" + uvSet + " not supported."); return null;
                    }
            }

            /*  return new Vector2[]
                     {
             _uvs[uvSet][_gridIndexToUV[_index]],
             _uvs[uvSet][_gridIndexToUV[_index]+1],
             _uvs[uvSet][_gridIndexToUV[_index]+2],
             _uvs[uvSet][_gridIndexToUV[_index]+3],
                     }; */
        }

        /// <summary>
        /// Returns the Tile index at world position. Mesh Transform needs to be provided to calculate the offset, the transform may have from Vector3.zero since all mesh data is in local space.
        /// </summary>
        /// <param name="meshTransform"></param>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public int GetTileIndex(Transform meshTransform, Vector3 worldPosition)
        {
            worldPosition = worldPosition - meshTransform.position;
            float _minX = 10000;
            float _maxX = 0;

            float _minY = 10000;
            float _maxY = 0;

            float _minZ = 10000;
            float _maxZ = 0;

            float _temp = 0;

            int n = 0;
            for (int _tri = 0; _tri < _triangles.Count; _tri += 6)
            {
                _minX = 10000;
                _maxX = 0;

                _minY = 10000;
                _maxY = 0;

                _minZ = 10000;
                _maxZ = 0;

                for (int i = 0; i < 6; i++)
                {
                    _temp = _gridPoints[_vertices[_triangles[_tri + i]]].x;
                    if (_temp < _minX)
                        _minX = _temp;
                    if (_temp > _maxX)
                        _maxX = _temp;

                    _temp = _gridPoints[_vertices[_triangles[_tri + i]]].y;
                    if (_temp < _minY)
                        _minY = _temp;
                    if (_temp > _maxY)
                        _maxY = _temp;

                    _temp = _gridPoints[_vertices[_triangles[_tri + i]]].z;
                    if (_temp < _minZ)
                        _minZ = _temp;
                    if (_temp > _maxZ)
                        _maxZ = _temp;
                }

                if (worldPosition.x <= _maxX && worldPosition.x >= _minX)
                {
                    if (worldPosition.y <= _maxY && worldPosition.y >= _minY)
                    {
                        if (worldPosition.z <= _maxZ && worldPosition.z >= _minZ)
                        {
                            return n;
                        }
                    }
                }
                n++;
            }

            return -1;
        }
        #endregion

        #region Utility
        /// <summary>
        /// We have one gridPointPosition per Row more than we have tiles.
        /// This returns the first point inside the gridPoints List of a Tile.
        /// It basicly just skips the last point of every row. 
        /// </summary>
        /// <param name="gridIndex"></param>
        /// <returns></returns>
        public int GridPointIndexToTileIndex(int gridpointIndex)
        {
            if (_sizeX > 0)
            {
                gridpointIndex += Mathf.FloorToInt(gridpointIndex / _sizeX);
                return gridpointIndex;
            }
            return -1;
        }

        public int GridIndexToTile(int gridIndex)
        {
            int _z = Mathf.FloorToInt(gridIndex / _sizeZ);
            return gridIndex - _z;
        }

        async void GenerateNormalsAsync(Mesh mesh)
        {
            Vector3[] _normals = await Task.Factory.StartNew(() => CalculateNormals(shading));
            mesh.SetNormals(_normals);
        }
        /// <summary>
        /// Calculate normals to have smooth shading.
        /// </summary>
        /// <returns></returns>
        public Vector3[] CalculateNormals(Shading shading)
        {
            Vector3[] _normals = new Vector3[_vertices.Count];
            int _triCount = _triangles.Count / 3;

            for (var i = 0; i < _triangles.Count; i += 3)
            {
                //Get the triangle normal direction
                Vector3 _triNormal = (GetTriangleNormal(i));
                //Add this normal value to each vertex that belongs to that tri
                _normals[_triangles[i]] += _triNormal;
                _normals[_triangles[i + 1]] += _triNormal;
                _normals[_triangles[i + 2]] += _triNormal;
            }

            if (shading == Shading.Smooth)
            {
                //We now have multiple normals at each grid point position.
                //Lets go through each position and average the normals there
                for (int i = 0; i < _gridPoints.Count; i++)
                {
                    Vector3 _average = new Vector3();
                    List<int> _found = new List<int>();
                    for (int n = 0; n < _vertices.Count; n++)
                    {
                        if (_vertices[n] == i)
                        {
                            _average += _normals[n];
                            _found.Add(n);
                        }
                    }

                    //Now that we have all the normals at this grid point and they are combined, assign the average vector bavk to each index in the normal array
                    foreach (int _f in _found)
                    {
                        _normals[_f] = _average;
                    }
                }
            }

            //Last step is to normalize the direction vectors
            for (var i = 0; i < _normals.Length; i++)
            {
                _normals[i].Normalize();
            }

            return _normals;
        }

        /// <summary>
        /// Takes the start index of a triangle and returns the normal vector of that face.
        /// </summary>
        /// <param name="triIndexStart"></param>
        /// <returns></returns>
        Vector3 GetTriangleNormal(int triIndexStart)
        {
            // Find vectors corresponding to two of the sides of the triangle.
            Vector3 side1 = _gridPoints[_vertices[_triangles[triIndexStart + 1]]] - _gridPoints[_vertices[_triangles[triIndexStart]]];
            Vector3 side2 = _gridPoints[_vertices[_triangles[triIndexStart + 2]]] - _gridPoints[_vertices[_triangles[triIndexStart]]];

            // Cross the vectors to get a perpendicular vector, then normalize it.
            return Vector3.Cross(side1, side2).normalized;
        }
        #endregion

#if UNITY_EDITOR
        public void DrawGridGizmos()
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < _gridPoints.Count; i++)
            {
                Handles.Label(_gridPoints[i], i.ToString());
                Gizmos.DrawCube(_gridPoints[i], Vector3.one * 0.25f);
            }

        }
#endif
    }
}