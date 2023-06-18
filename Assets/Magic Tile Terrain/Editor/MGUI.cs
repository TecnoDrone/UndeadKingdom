using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MagicSandbox.UI
{
    public static class MGUI
    {
        public static string GetFoldoutLabel(bool foldout)
        {
            if (foldout)
                return "-";
            return "+";
        }

        /// <summary>
        /// Draws a regular GUILayout Button with indent level
        /// </summary>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static bool Button(string text, int indent, params GUILayoutOption[] options)
        {
            bool _result = false;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15 * indent);    //Unity Editor indent is 15 px
            if (GUILayout.Button(text, options))
            {
                _result = true;
            }
            GUILayout.EndHorizontal();

            return _result;
        }

        /// <summary>
        /// Draws a regular GUILayout Button with indent level
        /// </summary>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static bool Button(Texture texture, int indent, params GUILayoutOption[] options)
        {
            bool _result = false;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15 * indent);    //Unity Editor indent is 15 px
            if (GUILayout.Button(texture, options))
            {
                _result = true;
            }
            GUILayout.EndHorizontal();

            return _result;
        }

        public static void Line(Color color, int indent, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x += (15 * indent);
            r.width -= (15 * indent);
            EditorGUI.DrawRect(r, color);
        }
    }
}