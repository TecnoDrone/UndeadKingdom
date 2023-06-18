using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using MagicSandbox.UI;

namespace MagicSandbox.TileTerrain
{
    public class PackageInfoWindow : EditorWindow
    {
        static PackageInfoWindow _window;

        bool _helpFoldout;
        Color _lineColor = new Color(0.15f, 0.15f, 0.15f);


        [MenuItem("Tools/Magic Tile Terrain/Open Info Window")]
        public static void ShowWindow()
        {
            if (_window == null)
            {

                _window = GetWindow<PackageInfoWindow>();
                //_window = CreateInstance<ResourceImporterWindow>();
                _window.titleContent = new GUIContent("Magic Tile Terrain Info");
                _window.Focus();
            }
        }

        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();
        }

        void OnDestroy()
        {

        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("You can open this Window at any time at 'Tools / Magic Tile Terrain / Open Info Window", MessageType.None);
            EditorGUILayout.Space(15);

            //GUI.Label(new Rect(0, 0, _window.minSize.x, _window.minSize.y), "Thanks for buying the Magic Tile Terrain Package. If you have any problems using this Tool please reach out to me using the 'Get Help' button below. I am adding new features almost weekly so keep an eye on the Package Mangers Update Button :).");
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Thanks for buying the Magic Tile Terrain Package. If you have any problems \nusing this Tool please reach out to me using the 'Get Help' button below.\nI am adding new features almost weekly so keep an eye on the Package Mangers\nUpdate Button :).");
            GUILayout.EndVertical();

            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                MGUI.Line(_lineColor, EditorGUI.indentLevel);
                string _help = "Your Project is setup to use the Unity Build-In Render Pipeline. The Magic Tile Terrain Asset" +
                 "is not officially supported on this Render Pipeline. To use a supported Pipeline create a Project with the '3D (URP)'" +
                 "name tag from the Unity Hub.";
                EditorGUILayout.HelpBox(_help, MessageType.Warning);

                string _textureSetHelp = "If you want to stick with the Build-In Renderer, just create a new Material and add it to your" +
                " TextureSet Asset. Then hit the 'Update Texture Atlas' button to apply this Material to the Atlas. Now you need to assign" +
                " this TextureSet to the TileTerrain that you are working with. Everything else should update accordingly." +
                " Keep in mind that you cannot use the Demo Assets out of the box with the Build-In Renderer.";
                EditorGUILayout.HelpBox(_textureSetHelp, MessageType.Info);
            }

            MGUI.Line(_lineColor, EditorGUI.indentLevel);

            if (GUILayout.Button(GetHelpButtonText(_helpFoldout), GUILayout.Width(100), GUILayout.Height(30)))
            {
                _helpFoldout = !_helpFoldout;
            }

            if (_helpFoldout)
            {
                MGUI.Line(_lineColor, EditorGUI.indentLevel);
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    if (MGUI.Button("Discord", EditorGUI.indentLevel, GUILayout.Width(100)))
                    {
                        Application.OpenURL("https://discord.gg/BPR3p84ZFH");
                    }
                    if (MGUI.Button("Mail", EditorGUI.indentLevel, GUILayout.Width(100)))
                    {
                        Application.OpenURL("mailto:support@magicsandbox.net");
                    }
                }
                GUILayout.EndHorizontal();
            }

        }

        string GetRenderPipelineString()
        {
            string _pipelineText = "You are currently using the ";
            if (GraphicsSettings.defaultRenderPipeline == null)
                _pipelineText += "Build-In ";
            else
                _pipelineText += GraphicsSettings.defaultRenderPipeline.name + " ";

            _pipelineText += "Render Pipeline";
            return _pipelineText;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 windowSize = new Vector2(524, 450);
            editorWindow.minSize = windowSize;
            editorWindow.maxSize = windowSize;
        }

        string GetHelpButtonText(bool foldout)
        {
            if (foldout)
                return "Close Help";
            else
                return "Get Help";
        }
    }

}
