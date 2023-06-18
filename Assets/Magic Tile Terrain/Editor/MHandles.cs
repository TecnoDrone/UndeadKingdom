using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MagicSandbox.UI
{
    public static class MHandles
    {
        public static bool Button(Vector3 handlePosition, float size, Color color, Color hoverColor)
        {
            int _controlID = GUIUtility.GetControlID(FocusType.Passive); // Gets a new ControlID for the handle
            bool _buttonClicked = false;

            switch (Event.current.GetTypeForControl(_controlID))
            {
                case EventType.Layout:
                    //Set the Size of this button
                    HandleUtility.AddControl(
                        _controlID,
                        HandleUtility.DistanceToCube(Handles.matrix.MultiplyPoint(handlePosition), Quaternion.identity, size)
                    );
                    break;

                case EventType.MouseUp:
                    if (HandleUtility.nearestControl == _controlID)
                    {
                        GUIUtility.hotControl = _controlID;
                        _buttonClicked = true;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    //Handle the Hover state
                    if (HandleUtility.nearestControl == _controlID)
                    {
                        Handles.color = hoverColor;
                        Handles.CubeHandleCap(_controlID, handlePosition, Quaternion.identity, size, EventType.Repaint);
                    }
                    else
                    {
                        Handles.color = color;
                        Handles.CubeHandleCap(_controlID, handlePosition, Quaternion.identity, size, EventType.Repaint);
                    }
                    break;
            }

            return _buttonClicked;
        }

        public static void Cube(Vector3 handlePosition, float size, Color color)
        {
            Handles.color = color;
            Handles.CubeHandleCap(GUIUtility.GetControlID(FocusType.Passive), handlePosition, Quaternion.identity, size, EventType.Repaint);
        }

        public static void Circle(Vector3 position, Quaternion rotation, float size)
        {
            int _controlID = GUIUtility.GetControlID(FocusType.Passive); // Gets a new ControlID for the handle
            Handles.CircleHandleCap(_controlID, position, rotation, size, EventType.Repaint);
        }
    }
}