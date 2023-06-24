using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using CustomMagicSandbox.UI;

namespace CustomMagicSandbox.TileTerrain
{
    [CustomEditor(typeof(TileTerrain))]
    public class TileTerrainInspector : Editor
    {
        Color _lineColor = new Color(0.15f, 0.15f, 0.15f);
        TileTerrain _target;

        Texture[] _atlasTextures;
        TMesh.Shading _tempShading;

        Texture2D _tileEditModeIcon;
        Texture2D _heightEditModeIcon;
        Texture2D _vertexPaintModeIcon;
        Texture2D _paintTextureIcon;
        Texture2D _rotateUVIcon;
        Texture2D _rotateTriIcon;


        Transform _currentCameraTransform;
        float _currentCameraViewAngle;
        bool _drawHandles;

        Rect _toolbarRect;
        Rect _inputInfoRect;

        //bool _mouseOverTerrain;
        Vector3 _mouseOverTerrainHitPoint; //Contains the last point where we hit the Terrain with a raycast when the mouse was over it.
        Vector3 _mouseOverTerrainNormal;
        int[] _brushSelectedPointIndices; //contains the indices of all gridPoints that are currently selected by the brush
        int _brushSelectedPointCount;

        Quaternion _lastBrushRotation; //For smoothing the brush

        float _h = 0, _s = 1, _v = 1; //Color info for HSV scene view sliders.

        Texture2D _colorTexture;

        void OnEnable()
        {
            _target = (TileTerrain)target;
            _target.terrainChanged += OnTileTerrainChanged;
            OnTileTerrainChanged();

            SceneView.beforeSceneGui += OnBeforeSceneGUI;

            string _packagePath = EditorSettings.GetPackagePath();
            //Debug.Log(_packagePath);

            //Vertex Color stuff
            if (_colorTexture == null)
                _colorTexture = CreateColorTexture(100, 30, _target.targetVertexColor);


            if (_packagePath != string.Empty)
            {
                if (_tileEditModeIcon == null)
                    _tileEditModeIcon = EditorGUIUtility.Load(_packagePath + "/Editor/Textures/TileEditModeIcon.png") as Texture2D;
                if (_heightEditModeIcon == null)
                    _heightEditModeIcon = EditorGUIUtility.Load(_packagePath + "/Editor/Textures/HeightEditModeIcon.png") as Texture2D;
                if (_vertexPaintModeIcon == null)
                    _vertexPaintModeIcon = EditorGUIUtility.Load(_packagePath + "/Editor/Textures/VertexPaintModeIcon.png") as Texture2D;
                if (_rotateTriIcon == null)
                    _rotateTriIcon = EditorGUIUtility.Load(_packagePath + "/Editor/Textures/RotateTriIcon.png") as Texture2D;
                if (_rotateUVIcon == null)
                    _rotateUVIcon = EditorGUIUtility.Load(_packagePath + "/Editor/Textures/RotateUVIcon.png") as Texture2D;
                if (_paintTextureIcon == null)
                    _paintTextureIcon = EditorGUIUtility.Load(_packagePath + "/Editor/Textures/PaintTextureIcon.png") as Texture2D;
            }
        }

        void OnDisable()
        {
            _target.terrainChanged -= OnTileTerrainChanged;
            SceneView.beforeSceneGui -= OnBeforeSceneGUI;
        }

        void OnTileTerrainChanged()
        {
            if (_target.textureSet == null)
                return;

            _atlasTextures = new Texture[_target.texturesCount];
            for (int i = 0; i < _target.texturesCount; i++)
            {
                _atlasTextures[i] = _target.textureSet.GetTexture(i);
            }

            //make sure the brush point indices are not out of sync with the terrains grid points.
            _brushSelectedPointIndices = new int[_target.tMesh.gridPointsCount];
        }

        /// <summary>
        /// Handles the Scene View before Unity stuff takes over.
        /// </summary>
        /// <param name="view"></param>
        void OnBeforeSceneGUI(SceneView view)
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(_target))
                return;

            //Check if the mouse cursor is on the Toolbar.
            //If it is, stop handling anything 3D scene view related.
            if (_toolbarRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            HandleEditorEvents();
        }

        void OnSceneGUI()
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(_target))
                return;
            if (_target.tMesh == null)
                return;
            if (!_target.tMesh.valid)
                return;
            Handles.BeginGUI();
            DrawSceneGUI();
            Handles.EndGUI();

            if (!_target.editFoldout)
                return;

            if (_target.heightEditTab || _target.vertexPaintTab)
                DrawGridEditHandles();

            //Repaint the Scene view if the mouse is moving. Otherwise we would not get the Handle highlights when the mouse is hovering over them.
            if (Event.current.type == EventType.MouseMove) SceneView.RepaintAll();
        }

        void HandleEditorEvents()
        {
            #region Tile Selection

            //Texture Painting 
            if (_target.tileEditTab)
            {
                #region MOUSE DOWN
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
                {
                    #region MOUSE 0
                    if (Event.current.button == 0)
                    {
                        int _terrainLayer = _target.gameObject.layer;
                        _target.gameObject.layer = 31;
                        RaycastHit _hit = new RaycastHit();
                        if (RaycastOnTerrain(1 << 31, out _hit))
                        {
                            _target.mousePosition = _hit.point;

                            int _tileIndex = _target.tMesh.GetTileIndex(_target.transform, _hit.point);
                            if (_tileIndex >= 0)
                            {

                                if (_target.gizmoPosition != _tileIndex)
                                {
                                    //Decide what happens in each tileMode
                                    if (_target.tilePaintMode == 0)
                                    {
                                        //Paint the selected Texture on the terrain.
                                        _target.SetTileTexture(_tileIndex, _target.selectedTextureAtlasIndex);
                                    }
                                    else if (_target.tilePaintMode == 1)
                                    {
                                        _target.RotateTileUV(_tileIndex, false);
                                    }
                                    else if (_target.tilePaintMode == 2)
                                    {
                                        _target.RotateTriangles(_tileIndex);
                                    }

                                    _target.gizmoPosition = _tileIndex;
                                    _target.SetToUnityMesh();
                                }


                                //Move the Scene View to look at the position we are editing
                                if (_target.moveSceneViewToSelection)
                                {
                                    Vector3 _direction = _hit.point - SceneView.lastActiveSceneView.camera.transform.position;
                                    float _distance = _direction.magnitude;
                                    SceneView.lastActiveSceneView.LookAt(_hit.point, _distance, Quaternion.LookRotation(_direction));
                                }

                                Event.current.Use(); //Consume the click event so that nothing else is selected.
                            }
                            _target.gameObject.layer = _terrainLayer;

                        }
                    }
                    #endregion

                }
            }
            #endregion
            else if (_target.heightEditTab)
            {
                RaycastHit _hit = new RaycastHit();
                if (RaycastOnTerrain(1 << _target.gameObject.layer, out _hit))
                {
                    _mouseOverTerrainHitPoint = _hit.point;
                    _mouseOverTerrainNormal = _hit.normal;
                }

                //Do the actual height change
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0 && _brushSelectedPointCount > 0)
                    {
                        if (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.None)
                        {
                            bool _up = Event.current.modifiers == EventModifiers.None;

                            SetSelectedGridPoints(_up);
                            _target.SetToUnityMesh();

                            //Move the Scene View to look at the position we are editing
                            if (_target.moveSceneViewToSelection)
                            {
                                Vector3 _direction = _mouseOverTerrainHitPoint - SceneView.lastActiveSceneView.camera.transform.position;
                                float _distance = _direction.magnitude;
                                SceneView.lastActiveSceneView.LookAt(_mouseOverTerrainHitPoint, _distance, Quaternion.LookRotation(_direction));
                            }

                            Event.current.Use(); //Consume the click event so that nothing else is selected.
                        }
                    }
                }
            }
            else if (_target.vertexPaintTab)
            {
                RaycastHit _hit = new RaycastHit();
                if (RaycastOnTerrain(1 << _target.gameObject.layer, out _hit))
                {
                    _mouseOverTerrainHitPoint = _hit.point;
                    _mouseOverTerrainNormal = _hit.normal;
                }

                //Do the actual height change
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0 && _brushSelectedPointCount > 0)
                    {
                        if (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.None)
                        {
                            bool _up = Event.current.modifiers == EventModifiers.None;

                            //Set Color here
                            VertexPaintSelectedGridPoints();
                            _target.SetToUnityMesh();

                            //Move the Scene View to look at the position we are editing
                            if (_target.moveSceneViewToSelection)
                            {
                                Vector3 _direction = _mouseOverTerrainHitPoint - SceneView.lastActiveSceneView.camera.transform.position;
                                float _distance = _direction.magnitude;
                                SceneView.lastActiveSceneView.LookAt(_mouseOverTerrainHitPoint, _distance, Quaternion.LookRotation(_direction));
                            }

                            Event.current.Use(); //Consume the click event so that nothing else is selected.
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Do a raycast from the Editor camera through mouse position onto the terrain. Returns the hit world position
        /// </summary>
        /// <returns></returns>
        bool RaycastOnTerrain(LayerMask mask, out RaycastHit hit)
        {
            hit = new RaycastHit();
            bool _result = false;
            Vector2 _mousePos = Event.current.mousePosition;

            Ray _ray = HandleUtility.GUIPointToWorldRay(_mousePos);

            //Debug.DrawRay(_ray.origin, _ray.direction * 1000, Color.red, 5);
            if (Physics.Raycast(_ray.origin, _ray.direction, out hit, 1000.0f, mask, QueryTriggerInteraction.Ignore))
            {
                //Debug.Log("Hit something at: " + hit.point);
                _result = true;
            }
            return _result;
        }

        void DrawSceneGUI()
        {
            if (!_target.editFoldout)
                return;

            #region Construct Rects
            if (_target.sceneViewToolsFoldout)
                _toolbarRect = new Rect(0, 0, 110, Screen.height);
            else
                _toolbarRect = new Rect(0, 0, 110, 20);

            _inputInfoRect = new Rect(_toolbarRect.width, 0, Screen.width - _toolbarRect.width, 20);
            #endregion


            //Draw the backgrounds for our toolbar and input helper
            GUI.Box(_toolbarRect, "");
            GUI.Box(_inputInfoRect, "");

            //Draw the Toolbar
            GUILayout.BeginArea(_toolbarRect);
            if (GUILayout.Button(GetSceneViewToolbarFoldoutName(_target.sceneViewToolsFoldout)))
            {
                _target.sceneViewToolsFoldout = !_target.sceneViewToolsFoldout;
            }
            if (_target.sceneViewToolsFoldout)
            { DrawSceneViewTools(); }
            GUILayout.EndArea();


            //Draw the input info helper.
            GUILayout.BeginArea(_inputInfoRect);
            {
                if (_target.heightEditTab)
                    GUILayout.Label(" Raise Terrain: Left Mouse   |   Lower Terrain: CTRL + Left Mouse");
                if (_target.tileEditTab)
                    GUILayout.Label(" Select Tile: Left Mouse");
                if (_target.vertexPaintTab)
                    GUILayout.Label(" Paint Color: Left Mouse");
            }
            GUILayout.EndArea();
        }

        void DrawSceneViewTools()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(_heightEditModeIcon, GUILayout.Width(50), GUILayout.Height(30)))
                {
                    SetTab(0);
                }
                if (GUILayout.Button(_tileEditModeIcon, GUILayout.Width(50), GUILayout.Height(30)))
                {
                    SetTab(1);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button(_vertexPaintModeIcon, GUILayout.Width(50), GUILayout.Height(30)))
            {
                SetTab(2);
            }
            MGUI.Line(_lineColor, 0, 2, 10);

            if (_target.tileEditTab)
            {
                GUILayout.Space(10);
                if (GUILayout.Button(_paintTextureIcon, GUILayout.Width(100), GUILayout.Height(30)))
                {
                    _target.tilePaintMode = 0;
                }
                if (GUILayout.Button(_rotateUVIcon, GUILayout.Width(100), GUILayout.Height(30)))
                {
                    _target.tilePaintMode = 1;

                    /* _target.RotateTileUV(_target.gizmoPosition, false);
                    _target.SetToUnityMesh(); */
                }
                if (GUILayout.Button(_rotateTriIcon, GUILayout.Width(100), GUILayout.Height(30)))
                {
                    _target.tilePaintMode = 2;

                    /*  _target.RotateTriangles(_target.gizmoPosition);
                     _target.SetToUnityMesh(); */
                }

                if (_target.tilePaintMode == 0)
                    DrawSceneViewTextureList();
                else if (_target.tilePaintMode == 1)
                    EditorGUILayout.HelpBox("Texture Rotation Mode. Draw on the Terrain to rotate the Texture at any Tile.", MessageType.Info);
                else if (_target.tilePaintMode == 2)
                    EditorGUILayout.HelpBox("Triangle Rotation Mode. Draw on the Terrain to rotate the Mesh Triangle at any Tile.", MessageType.Info);


            }
            else if (_target.heightEditTab)
            {
                SceneViewSnapModifier();
                SceneViewToolsBrushHandleSize();
            }
            else if (_target.vertexPaintTab)
            {
                GUILayout.Space(10);
                GUILayout.Label("Vertex Color:");
                SceneViewToolsColor();

                SceneViewToolsBrushHandleSize();
            }
        }

        void SceneViewSnapModifier()
        {
            GUILayout.Space(10);
            GUILayout.Label("Snap Multiplier:");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _target.snapMultiplier--;
                }

                GUILayout.Box(_target.snapMultiplier.ToString(), GUILayout.Width(40), GUILayout.Height(30));

                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _target.snapMultiplier++;
                }
            }
            GUILayout.EndHorizontal();
            _target.snapMultiplier = Mathf.RoundToInt(GUILayout.HorizontalSlider((float)_target.snapMultiplier, 1, 16));
        }

        void SceneViewToolsBrushHandleSize()
        {
            GUILayout.Space(20);
            GUILayout.Label("Brush Size:");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _target.brushHandleSize -= 0.5f;
                }

                GUILayout.Box(_target.brushHandleSize.ToString("0.0"), GUILayout.Width(40), GUILayout.Height(30));

                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _target.brushHandleSize += 0.5f;
                }
            }
            GUILayout.EndHorizontal();
            _target.brushHandleSize = GUILayout.HorizontalSlider((float)_target.brushHandleSize, 0.1f, 50);
        }

        void SceneViewToolsColor()
        {
            UpdateColorTexture(_target.targetVertexColor, _colorTexture);
            GUILayout.Box(_colorTexture);
            //HUE
            GUILayout.Space(20);
            GUILayout.Label("Hue:");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _h -= 0.1f;
                }

                GUILayout.Box(_h.ToString("0.0"), GUILayout.Width(40), GUILayout.Height(30));

                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _h += 0.1f;
                }
            }
            GUILayout.EndHorizontal();
            _h = GUILayout.HorizontalSlider((float)_h, 0f, 1f);

            //SAT
            GUILayout.Space(20);
            GUILayout.Label("Saturation:");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _s -= 0.1f;
                }

                GUILayout.Box(_s.ToString("0.0"), GUILayout.Width(40), GUILayout.Height(30));

                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _s += 0.1f;
                }
            }
            GUILayout.EndHorizontal();
            _s = GUILayout.HorizontalSlider((float)_s, 0f, 1f);

            //BRIGHTNESS (value)
            GUILayout.Space(20);
            GUILayout.Label("Brightness:");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _v -= 0.1f;
                }

                GUILayout.Box(_v.ToString("0.0"), GUILayout.Width(40), GUILayout.Height(30));

                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    _v += 0.1f;
                }
            }
            GUILayout.EndHorizontal();
            _v = GUILayout.HorizontalSlider((float)_v, 0f, 1f);

            GUILayout.Space(20);
            if (GUI.changed)
                _target.targetVertexColor = ColorUtility.HSVToRGB(_h, _s, _v);
        }

        void UpdateColorTexture(Color c, Texture2D tex)
        {
            ColorUtility.RGBToHSV(c, out _h, out _s, out _v);

            var size = tex.width;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
        }

        Texture2D CreateColorTexture(int width, int height, Color color)
        {
            var tex = new Texture2D(width, height);
            for (int y = 0; y < height; y++)
            {
                var h = 1f * y / height;
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            return tex;
        }




        void DrawSceneViewSelectedTexture()
        {

        }

        void DrawSceneViewTextureList()
        {
            if (_atlasTextures != null)
            {
                int _texturesPerRow = 2;
                int _rows = 1 + Mathf.CeilToInt(_atlasTextures.Length / _texturesPerRow);
                int _texIndex = 0;

                for (int i = 0; i < _rows; i++)
                {
                    GUILayout.BeginHorizontal();
                    for (int n = 0; n < _texturesPerRow; n++)
                    {
                        if (_texIndex >= _atlasTextures.Length)
                            continue;

                        if (_texIndex == _target.selectedTextureAtlasIndex)
                            GUI.color = Color.green;
                        else
                            GUI.color = Color.white;

                        if (GUILayout.Button(_atlasTextures[_texIndex], GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            _target.selectedTextureAtlasIndex = _texIndex;
                        }

                        _texIndex++;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                if (GUILayout.Button("Refresh Textures"))
                {
                    OnTileTerrainChanged();
                }
            }
        }

        void DrawGridEditHandles()
        {
            Handles.matrix = _target.transform.localToWorldMatrix;
            Handles.color = new Color(0, 0.5f, 0.8f, 0.3f);
            _currentCameraTransform = Camera.current.gameObject.transform;
            _currentCameraViewAngle = Camera.current.fieldOfView;

            //Only Accept left mouse button as input method to manipulate handles
            if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button != 0)
                    return;
            }
            DrawEditBrushHandle();

            Vector3 _terrainPos = _target.gameObject.transform.position;
            _brushSelectedPointCount = 0;
            //_target.tileSize * 0.12f * _target.handleSize
            for (int i = 0; i < _target.tMesh.gridPoints.Count; i++)
            {
                Color _handleColor = new Color(0, 0.5f, 0.8f, 0.3f);
                if (_target.brushHandleSize > 0)
                {
                    if (Utility.SphereContains(_mouseOverTerrainHitPoint, _target.brushHandleSize + (_target.GetDynamicHandleSize() / 2), _terrainPos + _target.tMesh.gridPoints[i]))
                    {
                        if (_brushSelectedPointCount >= _brushSelectedPointIndices.Length)
                            break;

                        _handleColor = new Color(1, 1, 0, 0.6f);
                        _brushSelectedPointIndices[_brushSelectedPointCount] = i;
                        _brushSelectedPointCount++;
                    }
                }

                //Only draw handles to a certain distance from the camera.
                if (Vector3.Distance(_terrainPos + _target.tMesh.gridPoints[i], _currentCameraTransform.position) > _target.handleDisplayDistance)
                    continue;
                //Only draw handles that are in the fov of our camera.
                if (Vector3.Angle(_terrainPos + _target.tMesh.gridPoints[i] - _currentCameraTransform.position, _currentCameraTransform.forward) > _currentCameraViewAngle)
                    continue;
                MHandles.Cube(_target.tMesh.gridPoints[i], _target.GetDynamicHandleSize(), _handleColor);
            }

        }

        void SetSelectedGridPoints(bool up)
        {
            //We need to snap all points to the grid 
            float _normDist = 1;
            for (int i = 0; i < _brushSelectedPointCount; i++)
            {
                if (_brushSelectedPointCount > 1)
                {
                    _normDist = Utility.GetNormalizedFloat(0.1f, _target.brushHandleSize, Vector3.Distance(_target.tMesh.gridPoints[_brushSelectedPointIndices[i]], _mouseOverTerrainHitPoint));
                    _normDist = 1 - _normDist;
                }
                int _falloffSnapMultiplier = Mathf.RoundToInt((float)_target.snapMultiplier * _normDist);
                if (_falloffSnapMultiplier <= 0)
                    _falloffSnapMultiplier = 1;

                if (up)
                    _target.tMesh.SetGridPointPosition(_brushSelectedPointIndices[i], _target.tMesh.gridPoints[_brushSelectedPointIndices[i]] += Vector3.up * _target.snap * _falloffSnapMultiplier);
                else
                    _target.tMesh.SetGridPointPosition(_brushSelectedPointIndices[i], _target.tMesh.gridPoints[_brushSelectedPointIndices[i]] += Vector3.down * _target.snap * _falloffSnapMultiplier);
            }
        }

        void VertexPaintSelectedGridPoints()
        {
            for (int i = 0; i < _brushSelectedPointCount; i++)
            {
                _target.tMesh.SetVertexColor(_brushSelectedPointIndices[i], _target.targetVertexColor);
            }
        }

        /// <summary>
        /// Draws the current edit tool like a circle or a box that shows which area will be affected when you click.
        /// </summary>
        void DrawEditBrushHandle()
        {
            if (_target.brushHandleSize <= 0)
                return;

            Color _old = Handles.color;
            Handles.color = Color.yellow;
            Quaternion _rotation = Quaternion.identity;

            if (_mouseOverTerrainNormal != Vector3.zero)
            {
                if (_lastBrushRotation.eulerAngles != Vector3.zero)
                    _rotation = Quaternion.Slerp(_lastBrushRotation, Quaternion.LookRotation(_mouseOverTerrainNormal), Time.deltaTime * 0.5f);
                else
                {
                    _rotation = Quaternion.LookRotation(_mouseOverTerrainNormal);
                }
            }

            MHandles.Circle(_mouseOverTerrainHitPoint - _target.gameObject.transform.position, _rotation, _target.brushHandleSize);
            MHandles.Circle(_mouseOverTerrainHitPoint - _target.gameObject.transform.position, _rotation, _target.brushHandleSize + 0.1f);

            Handles.color = _old;
            _lastBrushRotation = _rotation;
        }

        public override void OnInspectorGUI()
        {
            _target = (TileTerrain)target;
            if (_target == null)
                return;


            EditorGUILayout.PropertyField(serializedObject.FindProperty("textureSet"));
            if (_target.textureSet == null)
            {
                EditorGUILayout.HelpBox("Tile Terrains need a Texture Set to render correctly. Create a TileSet by right clicking in your Project Window and select /Create/Magic Tile Terrain/New Texture Set", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            //Checking the textureSet changes
            if (_target.textureSet.changeset != _target.lastTextureSetChangeset)
            { _target.UpdateRendererMaterial(); _target.lastTextureSetChangeset = _target.textureSet.changeset; }


            if (_target.tMesh == null || !_target.tMesh.valid)
            {
                DrawCreateNewTerrainArea();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            #region Create New Terrain
            if (_target.createNewTerrainFoldout)
            {
                DrawCreateNewTerrainArea();
                serializedObject.ApplyModifiedProperties();
                return;
            }
            if (GUILayout.Button("Create new Terrain"))
            {
                _target.createNewTerrainFoldout = !_target.createNewTerrainFoldout;
            }
            GUILayout.Space(10);
            #endregion

            #region Shading Popout
            GUILayout.BeginHorizontal();
            {
                _tempShading = (TMesh.Shading)EditorGUILayout.EnumPopup("Terrain Shading:", _tempShading);
                if (_tempShading != _target.tMesh.shading)
                {
                    _target.tMesh.shading = _tempShading;
                    _target.SetToUnityMesh();
                }
            }
            GUILayout.EndHorizontal();
            #endregion

            #region Info Foldout
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.HelpBox("Terrain Info:", MessageType.None);
                if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.showInfoFoldout), GUILayout.Width(20)))
                { _target.showInfoFoldout = !_target.showInfoFoldout; }
            }
            GUILayout.EndHorizontal();

            if (_target.showInfoFoldout)
            {
                GUILayout.Space(10);
                EditorGUI.indentLevel++;
                #region Mesh Info Section
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.HelpBox("Mesh Info:", MessageType.None);
                    if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.meshInfoFoldout), GUILayout.Width(20)))
                    { _target.meshInfoFoldout = !_target.meshInfoFoldout; }
                }
                GUILayout.EndHorizontal();

                if (_target.meshInfoFoldout)
                    DrawMeshInfo();

                GUILayout.Space(10);
                #endregion


                #region Texture Atlas Info Section
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.HelpBox("Texture Atlas Info:", MessageType.None);
                    if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.textureAtlasInfoFoldout), GUILayout.Width(20)))
                    { _target.textureAtlasInfoFoldout = !_target.textureAtlasInfoFoldout; }
                }
                GUILayout.EndHorizontal();

                /*  if (_target.textureSet == null)
                 {
                     EditorGUILayout.HelpBox("There is no TextureSet assigned to this TileTerrain. Create one by right clicking in the Project view and then 'Create/Magic Tile Editor/New Texture Set'.", MessageType.Error);
                 }
      */
                if (_target.textureAtlasInfoFoldout)
                    DrawTextureAtlasInfo();

                GUILayout.Space(10);
                #endregion


                #region Selected Tile Info Section
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.HelpBox("Selected Tile:", MessageType.None);
                    if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.selectedTileInfoFoldout), GUILayout.Width(20)))
                    { _target.selectedTileInfoFoldout = !_target.selectedTileInfoFoldout; }
                }
                GUILayout.EndHorizontal();

                if (_target.selectedTileInfoFoldout)
                    DrawSelectedTileInfo();

                GUILayout.Space(10);
                #endregion

                EditorGUI.indentLevel--;
            }
            #endregion

            #region Edit Section
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.HelpBox("Edit Terrain:", MessageType.None);
                if (GUILayout.Button(MGUI.GetFoldoutLabel(_target.editFoldout), GUILayout.Width(20)))
                { _target.editFoldout = !_target.editFoldout; }
            }
            GUILayout.EndHorizontal();

            if (_target.editFoldout)
                DrawEditArea();

            GUILayout.Space(10);
            #endregion


            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawCreateNewTerrainArea()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSize"));

            if (_target.tMesh.gridPointsCount > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("The current Terrain will be deleted. All of your progress will be gone. Are you sure you want to proceed?", MessageType.Warning);
                GUILayout.Space(10);
                if (GUILayout.Button("No - close this", GUILayout.Height(40)))
                {
                    _target.createNewTerrainFoldout = false;
                }
                GUILayout.Space(50);

                if (GUILayout.Button("Yes - go ahead!"))
                {
                    _target.CreateTerrain();
                    _target.createNewTerrainFoldout = false;
                }
            }
            else
            {   //Will be shown when theres no prior terrain data
                if (GUILayout.Button("Create Terrain"))
                {
                    _target.CreateTerrain();
                    _target.createNewTerrainFoldout = false;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawMeshInfo()
        {
            string _meshInfo = "Grid Points: " + _target.tMesh.gridPointsCount + Environment.NewLine +
             "Tiles: " + _target.tMesh.tileCount + Environment.NewLine +
            "Triangle Indices: " + _target.tMesh.trianglesCount + Environment.NewLine +
            "Vertex Indices: " + _target.tMesh.vericesCount + Environment.NewLine +
            "UV Indices: " + _target.tMesh.uvCount;

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(_meshInfo, MessageType.Info);
            EditorGUI.indentLevel--;
        }

        void DrawTextureAtlasInfo()
        {
            if (_target.textureSet == null)
                return;
            if (_target.textureSet.atlasRects == null)
                return;

            string _atlasInfo = "Atlas Rects: " + _target.textureSet.atlasRects.Length + Environment.NewLine;
            foreach (Rect _r in _target.textureSet.atlasRects)
            {
                _atlasInfo += "(" + _r.x.ToString("0.000") + "," + _r.y.ToString("0.000") + "," + _r.width.ToString("0.000") + "," + _r.height.ToString("0.000") + ")" + Environment.NewLine;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(_atlasInfo, MessageType.Info);
            EditorGUI.indentLevel--;

        }

        void DrawSelectedTileInfo()
        {
            Vector2[] _tileUVs = _target.tMesh.GetUVs(_target.gizmoPosition);
            string _messageString = "UVs: " + Environment.NewLine;
            if (_tileUVs != null)
            {
                _messageString +=
                   "(" + _tileUVs[0].x.ToString("0.000") + "," + _tileUVs[0].y.ToString("0.000") + ")" + " " +
                   "(" + _tileUVs[1].x.ToString("0.000") + "," + _tileUVs[1].y.ToString("0.000") + ")" + " " +
                   "(" + _tileUVs[2].x.ToString("0.000") + "," + _tileUVs[2].y.ToString("0.000") + ")" + " " +
                   "(" + _tileUVs[3].x.ToString("0.000") + "," + _tileUVs[3].y.ToString("0.000") + ")" + Environment.NewLine;
            }

            int[] _tris = _target.tMesh.GetTriangleIndicesAtTile(_target.gizmoPosition);
            if (_tris != null)
            {
                _messageString += "Tri Indices: " + Environment.NewLine +
               "(" + _tris[0].ToString() + "," + _tris[1].ToString() + "," + _tris[2].ToString() + ")" + " " +
               "(" + _tris[3].ToString() + "," + _tris[4].ToString() + "," + _tris[5].ToString() + ")";
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(_messageString, MessageType.Info);
            EditorGUI.indentLevel--;
        }

        void DrawEditArea()
        {
            GUILayout.Space(10);
            EditorGUI.indentLevel++;

            #region  TABS
            GUILayout.BeginHorizontal();
            {
                if (MGUI.Button(_heightEditModeIcon, EditorGUI.indentLevel, GUILayout.Width(60), GUILayout.Height(30)))
                {
                    SetTab(0);
                }

                if (MGUI.Button(_tileEditModeIcon, EditorGUI.indentLevel, GUILayout.Width(60), GUILayout.Height(30)))
                {
                    SetTab(1);
                }
                if (MGUI.Button(_vertexPaintModeIcon, EditorGUI.indentLevel, GUILayout.Width(60), GUILayout.Height(30)))
                {
                    SetTab(2);
                }
            }
            GUILayout.EndHorizontal();
            #endregion

            MGUI.Line(_lineColor, EditorGUI.indentLevel, 2, 15);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSceneViewToSelection"));

            if (_target.heightEditTab)
                DrawHeightEditTab();
            if (_target.tileEditTab)
                DrawTileEditTab();
            if (_target.vertexPaintTab)
                DrawVertexPaintTab();

            EditorGUI.indentLevel--;
        }

        void DrawHeightEditTab()
        {
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handleSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("brushHandleSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handleDisplayDistance"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_snapMultiplier"));
            GUILayout.Space(10);
        }

        void DrawTileEditTab()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (MGUI.Button(_paintTextureIcon, EditorGUI.indentLevel, GUILayout.Width(125), GUILayout.Height(40)))
            {
                _target.tilePaintMode = 0;
            }

            if (MGUI.Button(_rotateUVIcon, EditorGUI.indentLevel, GUILayout.Width(125), GUILayout.Height(40)))
            {
                _target.tilePaintMode = 1;

                /*    _target.RotateTileUV(_target.gizmoPosition, false);
                   _target.SetToUnityMesh(); */
            }

            if (MGUI.Button(_rotateTriIcon, EditorGUI.indentLevel, GUILayout.Width(125), GUILayout.Height(40)))
            {
                _target.tilePaintMode = 2;

                /*   _target.RotateTriangles(_target.gizmoPosition);
                  _target.SetToUnityMesh(); */
            }
            GUILayout.EndHorizontal();

            MGUI.Line(_lineColor, EditorGUI.indentLevel, 2, 15);


            if (_target.tilePaintMode == 0)
                DrawTextureList();
            else if (_target.tilePaintMode == 1)
                EditorGUILayout.HelpBox("Texture Rotation Mode. Draw on the Terrain to rotate the Texture at any Tile.", MessageType.Info);
            else if (_target.tilePaintMode == 2)
                EditorGUILayout.HelpBox("Triangle Rotation Mode. Draw on the Terrain to rotate the Mesh Triangle at any Tile.", MessageType.Info);
        }

        void DrawVertexPaintTab()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetVertexColor"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("currentVertexColor"));
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handleSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("brushHandleSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handleDisplayDistance"));

            if (GUI.changed)
                ColorUtility.RGBToHSV(_target.targetVertexColor, out _h, out _s, out _v);
        }

        void DrawTextureList()
        {
            EditorGUILayout.HelpBox("Texture Paint Mode. Draw on the Terrain to apply the selected Texture on the Terrain.", MessageType.Info);

            if (_atlasTextures != null)
            {
                int _texturesPerRow = Screen.width / 170;
                int _rows = 1 + Mathf.CeilToInt(_atlasTextures.Length / _texturesPerRow);
                int _texIndex = 0;

                for (int i = 0; i < _rows; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    for (int n = 0; n < _texturesPerRow; n++)
                    {
                        if (_texIndex >= _atlasTextures.Length)
                            continue;

                        if (_texIndex == _target.selectedTextureAtlasIndex)
                            GUI.color = Color.green;
                        else
                            GUI.color = Color.white;

                        if (GUILayout.Button(_atlasTextures[_texIndex], GUILayout.Width(100), GUILayout.Height(100)))
                        {
                            _target.selectedTextureAtlasIndex = _texIndex;
                        }

                        _texIndex++;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                if (GUILayout.Button("Refresh Textures"))
                {
                    OnTileTerrainChanged();
                }
            }
        }

        void SetTab(int value)
        {
            switch (value)
            {
                case 0:
                    {
                        _target.heightEditTab = true;
                        _target.tileEditTab = false;
                        _target.vertexPaintTab = false;
                        break;
                    }
                case 1:
                    {
                        _target.heightEditTab = false;
                        _target.tileEditTab = true;
                        _target.vertexPaintTab = false;
                        break;
                    }
                case 2:
                    {
                        _target.heightEditTab = false;
                        _target.tileEditTab = false;
                        _target.vertexPaintTab = true;
                        break;
                    }
            }
        }

        string GetSceneViewToolbarFoldoutName(bool active)
        {
            if (active)
                return "Close";
            else
                return "Terrain Tools";
        }
    }
}