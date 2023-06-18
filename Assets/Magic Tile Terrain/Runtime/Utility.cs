

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System;

namespace MagicSandbox.TileTerrain
{
    /// <summary>
    /// Utitly Dump. One day i will reorganize all of this into its own classes. I promise ....
    /// </summary>
    public static class Utility
    {
        static Dictionary<string, Stopwatch> _timerDictionary = new Dictionary<string, Stopwatch>();

        public static Color GetColor(float r, float g, float b, float a)
        {
            return new Color(r / 255, g / 255, b / 255, a / 255);
        }

        public static bool IsValidFileName(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0;
        }

        /// <summary>
        /// Starts the time measure.
        /// </summary>
        /// <param name="instanceName">Instance name.</param>
        public static void StartTimeMeasure(string instanceName)
        {

            if (instanceName != null)
            {
                if (!_timerDictionary.ContainsKey(instanceName))
                {
                    Stopwatch _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    _timerDictionary.Add(instanceName, _stopWatch);
                }
            }
        }

        /// <summary>
        /// Stops the time measure and returns the time between start and stop.
        /// </summary>
        /// <returns>The time measure.</returns>
        /// <param name="instanceName">Instance name.</param>
        public static double StopTimeMeasure(string instanceName, bool printResult = true)
        {
            if (instanceName != null)
            {
                Stopwatch _stopWatch = new Stopwatch();
                if (!_timerDictionary.TryGetValue(instanceName, out _stopWatch))
                {
                    return 0;
                }

                _timerDictionary.Remove(instanceName);
                TimeSpan _ts = _stopWatch.Elapsed;

                if (printResult)
                {
                    UnityEngine.Debug.Log(instanceName + " " + _ts.TotalSeconds);
                }

                return (_ts.TotalSeconds);
            }
            return 0;
        }

        /// <summary>
        /// Returns the shortest euler Angle between two euler Angles. Return value can be negative
        /// </summary>
        /// <returns>The short angle.</returns>
        /// <param name="eulerAngle_A">Euler angle_ a.</param>
        /// <param name="eulerAngle_B">Euler angle_ b.</param>
        public static float GetShortAngle(float eulerAngle_From, float eulerAngle_To)
        {
            float _a = 0;
            float _b = 0;

            if (eulerAngle_From > eulerAngle_To)
            {
                _a = eulerAngle_To - eulerAngle_From;
                _b = eulerAngle_To - eulerAngle_From + 360;
            }
            else
            {
                _a = eulerAngle_To - eulerAngle_From;
                _b = eulerAngle_To - eulerAngle_From - 360;
            }

            if (Math.Abs(_a) > Mathf.Abs(_b))
            {
                return _b;
            }
            return _a;

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

        /// <summary>
        /// Returns a value between min and max.
        /// </summary>
        /// <returns>The normalized float.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        /// <param name="value">Value.</param>
        public static float GetValueFromNormalizedFloat(float normalizedValue, float rangeMin, float rangeMax)
        {
            if (normalizedValue < 0 || normalizedValue > 1)
                return 0;

            float _range = rangeMax - rangeMin;
            return rangeMin + (_range * normalizedValue);
        }

        /// <summary>
        /// Maps a value "value" from range "input" to range "output"
        /// Example: Map a color from range 0-1 to range 0-255
        /// </summary>
        public static float MapValue(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
        }

        public static bool EnumArrayContains(int[] array, int searchFor)
        {
            if (array == null)
                return false;
            if (array.Length <= 0)
                return false;

            foreach (object _o in array)
            {
                if ((int)_o == searchFor)
                    return true;
            }
            return false;
        }

        public static bool IsTypeOf(System.Type t1, System.Type t2)
        {
            if (t1 == t2)
                return true;

            return false;
        }

        /// <summary>
        /// Returns a string of the hierarchy from the transform all the way up to its root
        /// </summary>
        /// <returns>The transform hierarchy name.</returns>
        /// <param name="transform">Transform.</param>
        public static string GetTransformHierarchyName(Transform transform)
        {
            if (transform == null)
                return "transform is NULL";

            Transform _parent = transform.parent;
            if (_parent == null)
                return transform.name;

            List<string> _hierarchy = new List<string>();
            _hierarchy.Add(transform.name);

            int _i = 0;
            while (_parent != null)
            {
                if (_i >= 99)
                    return "Hierarchy lenght > 99 - something is wrong";

                _hierarchy.Add(_parent.name);
                _parent = _parent.parent;
                _i++;
            }

            _hierarchy.Reverse();
            string _ret = "";
            foreach (string _s in _hierarchy)
            {
                _ret += " / " + _s;
            }

            return _ret;
        }

        /// <summary>
        /// Takes any generic list and removes all null entries
        /// </summary>
        public static void CleanList<T>(List<T> list)
        {
            //List<int> _removeAt = new List<int>();

            //Get amount of NULL objects in that list
            int _numberOfNullEntries = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    _numberOfNullEntries++;
                }
            }

            //Keep looping until all null entries are removed
            for (int i = 0; i < _numberOfNullEntries; i++)
            {
                for (int n = 0; n < list.Count; n++)
                {
                    if (list[n] == null)
                    {
                        list.RemoveAt(n);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// DOES NOT WORK - do not use until fixed
        /// </summary>
        public static float ConvertRange(float inputRangeMin, float inputRangeMax, float desiredRangeMin, float desiredRangeMax, float valueToConvert)
        {
            float scale = (desiredRangeMax - desiredRangeMin) / (inputRangeMax - inputRangeMin);
            return (desiredRangeMin + ((valueToConvert - inputRangeMin) * scale));
        }

        public static void Shuffle<T>(this IList<T> list, System.Random rnd)
        {
            for (var i = 0; i < list.Count; i++)
                list.Swap(i, rnd.Next(i, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        /// <summary>
        /// Works only with a 45degree throw angle
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetPhysicsThrowVelocity(float distance, float deltaTargetHeight)
        {
            float _g = distance * (-(Physics.gravity.y + deltaTargetHeight));
            float _speed = Mathf.Sqrt(_g);
            return new Vector3(0, 0, _speed);
        }

        /// <summary>
        /// Converts a screen space value into a value that matches the UI reference resolution.
        /// iE: RefRes: 1280x720
        ///     Input: 1920, 1080
        ///     Output 1280, 720
        /// </summary>
        /// <param name="screenSpaceValue"></param>
        /// <returns></returns>
        public static Vector2 ScreenSpaceToReferenceSpace(Vector2 screenSpaceValue)
        {
            float _widthScale = 1280f / (float)Screen.width;
            float _heightScale = 720f / (float)Screen.height;

            return new Vector2(screenSpaceValue.x * _widthScale, screenSpaceValue.y * _heightScale);
        }

        /// <summary>
        /// Returns the lenght of a given path that consists of positions.
        /// </summary>
        /// <param name="corners"></param>
        /// <returns></returns>
        public static float GetPathLenght(Vector3[] corners)
        {
            if (corners.Length <= 1)
                return 0;

            float _dist = 0;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                _dist += Vector3.Distance(corners[i], corners[i + 1]);
            }

            return _dist;
        }

        public static bool SphereContains(Vector3 spherePosition, float sphereRadius, Vector3 point)
        {
            return Mathf.Pow(point.x - spherePosition.x, 2) + Mathf.Pow(point.y - spherePosition.y, 2) + Mathf.Pow(point.z - spherePosition.z, 2) < Mathf.Pow(sphereRadius, 2);
        }
    }
}