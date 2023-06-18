using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MagicSandbox.TileTerrain
{
    /// <summary>
    /// Add detail objects like grass trees stones etc automaticly to your terrain.
    /// </summary>
    [RequireComponent(typeof(TileTerrain))]
    [ExecuteInEditMode]
    public class TileTerrainDetails : MonoBehaviour
    {
        [System.Serializable]
        public class Detail
        {
            public Detail()
            { }
            public Detail(Detail other)
            {
                this.prefab = other.prefab;
                this.color = other.color;
                this.colorThreshold = other.colorThreshold;
                this.density = other.density;
                this.densityRecovery = other.densityRecovery;
                this.alignWithTerrain = other.alignWithTerrain;
                this.randomRotation = other.randomRotation;
                this.randomScale = other.randomScale;
                this.offset = other.offset;
                this.drawInstanced = other.drawInstanced;
            }

            public GameObject prefab;
            public Color color = new Color();   //If pixel color matches this color, we will spawn a detail

            [Tooltip("Precision helps if the pixel color is just slightly off from the above selected color. 1 means theres no room for error. 0 means we pretty much always spawn something.")]
            [Range(0, 1)]
            public float colorThreshold = 1;

            [Tooltip("Density defines the chance of a prefab spawn when all other requirements are met. 1 means we spawn something, 0 means we do not spawn anything.")]
            [Range(0, 1)]
            public float density = 0.5f;

            [Tooltip("Density will drop to 0 after a successful spawn. This defines how fast the density will go up again.")]
            [Range(0, 1)]
            public float densityRecovery = 0.1f;

            [Tooltip("If enabled the prefab will be rotated to align with the Terrains normal vector at that position. Prefab Y -> Normal Z")]
            public bool alignWithTerrain = false;
            public Vector3 randomRotation = new Vector3();
            public Vector3 randomScale = new Vector3();
            [HideInInspector] public Vector3 offset = new Vector3();

            public float currentDensity { get { return _currentDensity; } set { _currentDensity = value; } }
            float _currentDensity = 0;

            [Tooltip("If enabled, the Detail will be drawn as a GPU instanced mesh. This is great for objects that are drawn in big numbers and usually dont need adjustments later like grass or flowers. Make sure that the Details Material has 'Enable GPU Instancing' set to TRUE.")]
            public bool drawInstanced;

            public bool foldout;

            [HideInInspector] public List<GPUInstancedDetail> gpuDetailChunks = new List<GPUInstancedDetail>(); //Just for editor use. All Chunks of max 1023 gpu instanced positions of this detail are stored here.
        }

        public Camera detailTextureRenderCamera;
        public RenderTexture renderTex;
        [Range(1, 2048)] public int resolution = 32;
        public List<Detail> details = new List<Detail>();


        TileTerrain _terrain;
        [SerializeField][HideInInspector] Transform _detailsRoot;
        public Texture2D generatedTexture { get { return _generatedTex; } }
        Texture2D _generatedTex;

        [SerializeField][HideInInspector] List<Vector2Int> _detailKeys = new List<Vector2Int>();
        [SerializeField][HideInInspector] List<GameObject> _detailInstances = new List<GameObject>();
        public int detailInstancesCount { get { return _detailInstances.Count; } }

        [System.Serializable]
        public class GPUInstancedDetail
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4[] matrixes = new Matrix4x4[1023];
            public int count = 0;
        }
        Matrix4x4 _transformMatrix;

#if UNITY_EDITOR
        public bool helpFoldout;

        public int GetGPUInstancedDetailsCount()
        {
            int _c = 0;
            for (int i = 0; i < details.Count; i++)
            {
                foreach (GPUInstancedDetail _d in details[i].gpuDetailChunks)
                {
                    _c += _d.count;
                }
            }
            return _c;
        }

        /// <summary>
        /// Returns all GPU Detail instances as chunks of max 1023 objects. So there may be the same Detail object but one with 1023 and the other with 200 count. 
        /// This is for EDITOR GUI use.
        /// </summary>
        /// <returns></returns>
        public List<string> GetGPUInstancedDetailsInfo()
        {
            List<string> _list = new List<string>();
            foreach (Detail _d in details)
            {
                foreach (GPUInstancedDetail _gD in _d.gpuDetailChunks)
                {
                    _list.Add("Mesh Name: " + _gD.mesh.name + "   Count: " + _gD.count);
                }
            }
            return _list;
        }

        public void RemoveGPUInstancedDetails(int index)
        {
            if (index >= details.Count)
                return;
            if (index < 0)
                return;
            details[index].gpuDetailChunks.Clear();
        }

        public void RemoveDetail(int index)
        {
            if (index >= details.Count)
                return;
            if (index < 0)
                return;

            details.RemoveAt(index);
        }

#endif

        void Start()
        {
            if (!Application.isPlaying)
                return;


        }

        void Update()
        {
            foreach (Detail _d in details)
            {
                foreach (GPUInstancedDetail _gD in _d.gpuDetailChunks)
                {
                    Graphics.DrawMeshInstanced(_gD.mesh, 0, _gD.material, _gD.matrixes);
                }
            }
        }

#if UNITY_EDITOR

        void OnEnable()
        {
            _terrain = gameObject.GetComponent<TileTerrain>();
            _terrain.terrainChanged += OnTerrainChanged;

            GenerateDetailPlacementTexture();
        }

        void OnDisable()
        {
            if (_terrain != null)
                _terrain.terrainChanged -= OnTerrainChanged;
        }

        /// <summary>
        /// A low res texture of the whole terrain will be created to determine where detail objects will be placed.
        /// To do this we create a Camera and take a snapshot from above. IN order to render only the terrain, it will be temporarily set to layer 31.
        /// </summary>
        void GenerateDetailPlacementTexture()
        {
            int _terrainLayer = _terrain.gameObject.layer;
            _terrain.gameObject.layer = 31;

            //Create a new Render tex if we dont have one
            renderTex = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.Default);

            //Create the Camera to take a snapshot.
            GameObject _renderCameraGO = new GameObject();
            Camera _renderCamera = _renderCameraGO.AddComponent<Camera>();
            SetupRenderCamera(_renderCamera);
            _renderCamera.Render();

            //Read the Pixels of the generated render texture and clean everything up.
            _generatedTex = GetRTPixels(renderTex);
            DestroyImmediate(_renderCameraGO);
            DestroyImmediate(renderTex);

            _terrain.gameObject.layer = _terrainLayer;
        }

        void SetupRenderCamera(Camera camera)
        {
            camera.targetTexture = renderTex;
            if (_terrain.xSize > 0 && _terrain.zSize > 0)
                camera.aspect = (float)_terrain.xSize / (float)_terrain.zSize;
            else
                camera.aspect = 1;

            camera.orthographic = true;
            camera.cullingMask = 1 << 31;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.farClipPlane = 2000;
            Vector2 _terrainWorldSize = new Vector2(_terrain.tileSize * _terrain.xSize, _terrain.tileSize * _terrain.zSize);

            camera.orthographicSize = _terrainWorldSize.y * 0.5f;
            camera.transform.position = transform.position + new Vector3(_terrainWorldSize.x * 0.5f, 1000, _terrainWorldSize.y * 0.5f);
            camera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);
        }

        static public Texture2D GetRTPixels(RenderTexture rt)
        {
            // Remember currently active render texture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rt;

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, 0, true);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

            // Restore previously active render texture
            RenderTexture.active = currentActiveRT;
            return tex;
        }

        /// <summary>
        /// Generates details for all detail elements
        /// </summary>
        public void Generate()
        {
            int[] _indices = new int[details.Count];
            for (int i = 0; i < _indices.Length; i++)
            { _indices[i] = i; }
            Generate(_indices);
        }
        /// <summary>
        /// Generates the Details for the provided indices
        /// </summary>
        /// <param name="detailIndices"></param>
        public void Generate(int[] detailIndices)
        {
            //Make sure we always have a fresh detail texture
            GenerateDetailPlacementTexture();

            for (int i = 0; i < detailIndices.Length; i++)
            {
                if (detailIndices[i] >= details.Count)
                {
                    Debug.LogError("Cant Generate Details because the provided indices are out of range! Details.Count: " + details.Count + "   Index: " + detailIndices[i]);
                    return;
                }
                if (detailIndices[i] < 0)
                {
                    Debug.LogError("Cant Generate Details because the provided indices are out of range! Details.Count: " + details.Count + "   Index: " + detailIndices[i]);
                    return;
                }

                //Clean the gpuInstances list before adding new entries.
                RemoveGPUInstancedDetails(detailIndices[i]);
                //DETAIL _gpuInstancedDetailDictionary.Remove(details[detailIndices[i]]);
            }

            if (_detailsRoot == null)
            {
                GameObject _go = new GameObject("Details");
                _go.transform.SetParent(transform);
                _detailsRoot = _go.transform;
            }

            //loop through the resolution modified version of the terrain and check the pixel value at that position.
            float xWorldSize = _terrain.xSize * _terrain.tileSize;
            float zWorldSize = _terrain.zSize * _terrain.tileSize;

            Vector2 _pixelGridSize = new Vector2(xWorldSize / resolution, zWorldSize / resolution);

            Vector3 _midpoint = new Vector3(0, 0, 0);
            Vector3 _groundNormal = Vector3.zero;
            //Setup density 
            for (int i = 0; i < details.Count; i++)
            {
                details[i].currentDensity = details[i].density;
            }

            //Calculate the Mesh offset if the Detail object is going to be GPU instanced
            foreach (Detail _d in details)
            {
                MeshFilter _filter = _d.prefab.GetComponentInChildren<MeshFilter>();
                Renderer _renderer = _d.prefab.GetComponentInChildren<Renderer>();
                if (_filter != null && _renderer != null)
                {
                    Vector3 _meshOffset = _filter.transform.position - _d.prefab.transform.position;
                    _d.offset = _meshOffset;
                }
            }

            //Go through the resolution grid and spawn details when possible
            int n = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int _index = 0;
                    //Check Details
                    for (int i = 0; i < detailIndices.Length; i++)
                    {
                        _index = detailIndices[i];

                        if (CheckPixelColor(x, z, details[_index]))
                        {
                            if (details[_index].currentDensity > Random.Range(0f, 1f))
                            {
                                details[_index].currentDensity = 0;
                                //Spawn prefab at midpoint
                                _midpoint = new Vector3((x * _pixelGridSize.x) + (_pixelGridSize.x * 0.5f), transform.position.y, (z * _pixelGridSize.y) + (_pixelGridSize.y * 0.5f));
                                _midpoint += transform.position;
                                if (TryGetTerrainHeightAtPosition(_midpoint, out _midpoint, out _groundNormal))
                                {
                                    if (details[_index].drawInstanced)
                                    {
                                        GenerateGPUInstancedDetail(details[_index], _midpoint + details[_index].offset, _groundNormal);
                                    }
                                    else
                                    { SpawnDetail(details[_index], new Vector2Int(x, z), _midpoint, _groundNormal); }
                                }

                            }

                            details[_index].currentDensity += details[_index].densityRecovery;
                            if (details[_index].currentDensity > details[_index].density)
                                details[_index].currentDensity = details[_index].density;
                        }
                    }
                    n++;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _detailInstances.Count; i++)
            {
                if (Application.isPlaying)
                    Destroy(_detailInstances[i]);
                else
                    DestroyImmediate(_detailInstances[i]);
            }
            _detailInstances.Clear();
            _detailKeys.Clear();
        }

        bool CheckPixelColor(int x, int y, Detail detail)
        {
            Color _pixel = _generatedTex.GetPixel(x, y);
            //Debug.Log("Check Pixel " + _pixel + "  At " + x.ToString() + "x" + y.ToString());

            if (CheckColorThreshold(_pixel, detail.color, detail.colorThreshold))
                return true;
            return false;
        }

        bool CheckColorThreshold(Color a, Color z, float threshold = 1)
        {
            float r = a.r - z.r,
                g = a.g - z.g,
                b = a.b - z.b;
            return (r * r + g * g + b * b) <= threshold * threshold;
        }

        void SpawnDetail(Detail detail, Vector2Int resolutionGridPosition, Vector3 worldPos, Vector3 groundNormal)
        {
            Quaternion _rot = new Quaternion();

            // Sets the rotation so that the transform's y-axis goes along the z-axis
            //transform.rotation = Quaternion.FromToRotation(Vector3.up, transform.forward);
            if (detail.alignWithTerrain)
            {
                _rot = Quaternion.FromToRotation(Vector3.up, groundNormal);
            }

            Vector3 _randomRotation = new Vector3(
                        Random.Range(-detail.randomRotation.x, detail.randomRotation.x),
                        Random.Range(-detail.randomRotation.y, detail.randomRotation.y),
                        Random.Range(-detail.randomRotation.z, detail.randomRotation.z)
            );

            Vector3 _randomScale = new Vector3(
                           Random.Range(-detail.randomScale.x, detail.randomScale.x),
                           Random.Range(-detail.randomScale.y, detail.randomScale.y),
                           Random.Range(-detail.randomScale.z, detail.randomScale.z)
               );

            GameObject _go = (GameObject)Instantiate(detail.prefab, worldPos, _rot);
            _go.transform.SetParent(_detailsRoot);
            _go.transform.Rotate(_randomRotation, Space.Self);
            _go.transform.localScale += _randomScale;

            _detailKeys.Add(resolutionGridPosition);
            _detailInstances.Add(_go);
        }

        /*  void GenerateGPUInstancedDetail(Detail detail, Vector3 worldPos, Vector3 groundNormal)
         {
             if (detail.prefab == null)
             {
                 Debug.LogError("There is a null Prefab entry in TileTerrainDetails.");
                 return;
             }

             GPUInstancedDetail _gpuDetail = null;
             int _lastListIndex = 0;

             if (_gpuInstancedDetailDictionary.ContainsKey(detail))
             {
                 _lastListIndex = _gpuInstancedDetailDictionary[detail].Count - 1; //Get the current index that we are working with

                 if (_gpuInstancedDetailDictionary[detail][_lastListIndex].count >= 1023)
                 {
                     //Create a new GPUInstancedDetail object because we can only draw 1023 at once
                     _lastListIndex++;

                     //Only add the new detail if the prefab is valid 
                     if (TryCreateGPUDetail(detail, worldPos, groundNormal, out _gpuDetail))
                     {
                         _gpuInstancedDetailDictionary[detail].Add(_gpuDetail);
                     }
                 }

                 //We already have this detail. Just tell unity that it should draw one more by adding to the matrixes.
                 GPUInstancedDetail _d = _gpuInstancedDetailDictionary[detail][_lastListIndex];

                 //Create the random rotation
                 Vector3 _randomRotation = new Vector3(
                        Random.Range(-detail.randomRotation.x, detail.randomRotation.x),
                        Random.Range(-detail.randomRotation.y, detail.randomRotation.y),
                        Random.Range(-detail.randomRotation.z, detail.randomRotation.z));
                 Quaternion _rotation = Quaternion.Euler(_randomRotation);
                 if (detail.alignWithTerrain)
                     _rotation = Quaternion.FromToRotation(Vector3.up, groundNormal) * _rotation;

                 //Create random scale
                 Vector3 _randomScale = new Vector3(
                                            Random.Range(-detail.randomScale.x, detail.randomScale.x),
                                            Random.Range(-detail.randomScale.y, detail.randomScale.y),
                                            Random.Range(-detail.randomScale.z, detail.randomScale.z)
                                );
                 _randomScale = _d.matrixes[0].lossyScale + _randomScale;


                 _d.matrixes[_d.count] = Matrix4x4.TRS(worldPos, _rotation, _randomScale);
                 _d.count++;
                 _gpuInstancedDetailDictionary[detail][_lastListIndex] = _d;
                 return;
             }


             _gpuInstancedDetailDictionary.Add(detail, new List<GPUInstancedDetail>());
             //Only add the new detail if the prefab is valid 
             if (TryCreateGPUDetail(detail, worldPos, groundNormal, out _gpuDetail))
             {
                 _gpuInstancedDetailDictionary[detail].Add(_gpuDetail);
             }

         } */

        /// <summary>
        /// Generate one instance of a GPU instanced Detail object at world position
        /// </summary>
        void GenerateGPUInstancedDetail(Detail detail, Vector3 worldPos, Vector3 groundNormal)
        {
            if (detail.prefab == null)
            {
                Debug.LogError("There is a null Prefab entry in TileTerrainDetails.");
                return;
            }

            GPUInstancedDetail _gpuDetail = null;
            if (detail.gpuDetailChunks.Count <= 0)
            {
                if (TryCreateGPUDetail(detail, worldPos, groundNormal, out _gpuDetail))
                {
                    detail.gpuDetailChunks.Add(_gpuDetail);
                }
                return;
            }

            _gpuDetail = detail.gpuDetailChunks[detail.gpuDetailChunks.Count - 1];
            if (_gpuDetail.count >= 1023)
            {
                //Create a new GPUInstancedDetail object because we can only draw 1023 at once
                //Only add the new detail if the prefab is valid 
                if (TryCreateGPUDetail(detail, worldPos, groundNormal, out _gpuDetail))
                {
                    detail.gpuDetailChunks.Add(_gpuDetail);
                }
                return;
            }


            //Just add a new Matrix element to this chunk.

            //Create the random rotation
            Vector3 _randomRotation = new Vector3(
                   Random.Range(-detail.randomRotation.x, detail.randomRotation.x),
                   Random.Range(-detail.randomRotation.y, detail.randomRotation.y),
                   Random.Range(-detail.randomRotation.z, detail.randomRotation.z));
            Quaternion _rotation = Quaternion.Euler(_randomRotation);
            if (detail.alignWithTerrain)
                _rotation = Quaternion.FromToRotation(Vector3.up, groundNormal) * _rotation;

            //Create random scale
            Vector3 _randomScale = new Vector3(
                                       Random.Range(-detail.randomScale.x, detail.randomScale.x),
                                       Random.Range(-detail.randomScale.y, detail.randomScale.y),
                                       Random.Range(-detail.randomScale.z, detail.randomScale.z)
                           );
            _randomScale = _gpuDetail.matrixes[0].lossyScale + _randomScale;


            _gpuDetail.matrixes[_gpuDetail.count] = Matrix4x4.TRS(worldPos, _rotation, _randomScale);
            _gpuDetail.count++;
        }

        /// <summary>
        /// Creates a Detail object that is GPU Instancable and returns TRUE if everything is setup properly (Prefab needs a Mesh Filter and Render etc)
        /// </summary>
        bool TryCreateGPUDetail(Detail detail, Vector3 worldPos, Vector3 groundNormal, out GPUInstancedDetail gpuDetail)
        {
            //Create the random rotation
            Vector3 _randomRotation = new Vector3(
                   Random.Range(-detail.randomRotation.x, detail.randomRotation.x),
                   Random.Range(-detail.randomRotation.y, detail.randomRotation.y),
                   Random.Range(-detail.randomRotation.z, detail.randomRotation.z));
            Quaternion _rotation = Quaternion.Euler(_randomRotation);
            if (detail.alignWithTerrain)
                _rotation = Quaternion.FromToRotation(Vector3.up, groundNormal) * _rotation;

            gpuDetail = new GPUInstancedDetail();
            MeshFilter _filter = detail.prefab.GetComponentInChildren<MeshFilter>();
            Renderer _renderer = detail.prefab.GetComponentInChildren<Renderer>();
            if (_filter != null && _renderer != null)
            {
                gpuDetail.mesh = _filter.sharedMesh;
                gpuDetail.material = _renderer.sharedMaterial;
                gpuDetail.matrixes[gpuDetail.count] = Matrix4x4.TRS(worldPos, _rotation, _filter.transform.localScale);
                gpuDetail.count++;

                return true;
            }
            return false;
        }

        void OnTerrainChanged()
        {
            GenerateDetailPlacementTexture();

            //TODO: IMplement auto change detection
            UpdateHeight();
        }

        bool TryGetTerrainHeightAtPosition(Vector3 worldPos, out Vector3 position, out Vector3 normal)
        {
            int _terrainLayer = _terrain.gameObject.layer;
            _terrain.gameObject.layer = 31;
            position = Vector3.zero;
            normal = Vector3.up;

            //Quick and dirty - just raycast on the terrain. Maybe need to come up with something faster.
            RaycastHit _hit = new RaycastHit();
            Ray _ray = new Ray(worldPos + (Vector3.up * 1000), Vector3.down);
            Color _rayColor = Color.red;

            if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, 10000, 1 << _terrain.gameObject.layer, QueryTriggerInteraction.Ignore))
            {
                _rayColor = Color.green;
                //Debug.DrawLine(_ray.origin, _hit.point, _rayColor, 35);
                _terrain.gameObject.layer = _terrainLayer;
                position = _hit.point;
                normal = _hit.normal;
                return true;
            }

            _terrain.gameObject.layer = _terrainLayer;
            //Debug.DrawRay(_ray.origin, _ray.direction * 1000, _rayColor, 5);
            return false;
        }

        /// <summary>
        /// Update the Height for all Details.
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateHeight()
        {
            foreach (Detail _d in details)
            {
                UpdateHeight(_d);
            }
        }

        /// <summary>
        /// Update the Height for one detail.
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateHeight(Detail detail)
        {
            Vector3 _worldPosition;
            Vector3 _newPosition;
            Vector3 _newNormal;

            foreach (GPUInstancedDetail _d in detail.gpuDetailChunks)
            {
                for (int i = 0; i < _d.matrixes.Length; i++)
                {
                    _worldPosition = new Vector3(_d.matrixes[i][0, 3], _d.matrixes[i][1, 3], _d.matrixes[i][2, 3]);
                    if (TryGetTerrainHeightAtPosition(_worldPosition, out _newPosition, out _newNormal))
                    {
                        //Create the random rotation
                        Vector3 _randomRotation = new Vector3(
                               Random.Range(-detail.randomRotation.x, detail.randomRotation.x),
                               Random.Range(-detail.randomRotation.y, detail.randomRotation.y),
                               Random.Range(-detail.randomRotation.z, detail.randomRotation.z));
                        Quaternion _rotation = Quaternion.Euler(_randomRotation);
                        if (detail.alignWithTerrain)
                            _rotation = Quaternion.FromToRotation(Vector3.up, _newNormal) * _rotation;

                        Vector3 _scale = Vector3.one;
                        if (_d.matrixes[i].ValidTRS())
                            _scale = _d.matrixes[i].lossyScale;

                        _d.matrixes[i] = Matrix4x4.TRS(_newPosition, _rotation, _scale);
                    }
                }
            }
        }

#endif

#if UNITY_EDITOR
        public bool gizmoSettingsFoldout;
        public bool detailGenerationFoldout;
        public bool gpuInstancedDetailsFoldout;

        #region GIZMOS

        public bool showPixelOverlay;
        public bool showResolutionGrid;
        [Range(-10f, 10f)] public float gizmosHeightOffset = 0;

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (showResolutionGrid)
                DrawResolutionGrid();

            if (showPixelOverlay)
                DrawPixelOverlay();
        }

        void DrawPixelOverlay()
        {
            Vector2 _pixelGridSize = new Vector2((_terrain.xSize * _terrain.tileSize) / resolution, (_terrain.zSize * _terrain.tileSize) / resolution);

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector3 _midpoint = new Vector3((x * _pixelGridSize.x) + (_pixelGridSize.x * 0.5f), gizmosHeightOffset, (z * _pixelGridSize.y) + (_pixelGridSize.y * 0.5f));
                    Gizmos.color = _generatedTex.GetPixel(x, z);
                    Gizmos.DrawCube(_midpoint, new Vector3(_pixelGridSize.x, 0, _pixelGridSize.y));
                }
            }
        }

        void DrawResolutionGrid()
        {
            if (resolution <= 0)
                return;
            if (resolution > 4096)
                return;

            //Draw resolution grid
            float xWorldSize = _terrain.xSize * _terrain.tileSize;
            float _pixelGridSize = xWorldSize / resolution;
            Vector3 _offset = new Vector3(0, 0, 0);
            Vector3 _yOffset = new Vector3(0, 0.2f, 0);
            for (int i = 0; i < resolution + 1; i++)
            {
                _offset = new Vector3(i * _pixelGridSize, gizmosHeightOffset, 0);
                Gizmos.DrawLine(_offset + _yOffset, _offset + _yOffset + Vector3.forward * xWorldSize);
            }

            for (int i = 0; i < resolution + 1; i++)
            {
                _offset = new Vector3(0, gizmosHeightOffset, i * _pixelGridSize);
                Gizmos.DrawLine(_offset + _yOffset, _offset + _yOffset + Vector3.right * xWorldSize);
            }
        }
        #endregion

#endif
    }
}