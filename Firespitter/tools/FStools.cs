using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace Firespitter
{
    public static class Tools
    {
        public static Vector3 WorldUp(Vessel vessel)
        {
            if (vessel != null)
                return (vessel.rigidbody.position - vessel.mainBody.position).normalized;
            else
                return Vector3.zero;
        }

        public static T Clamp<T>(T value, T min, T max)
         where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        public static double Clamp(double value, double min, double max)
        {     
            double result = value;
            if (value > max)
                result = max;
            if (value < min)
                result = min;
            return result;
        }

        public static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        public static String PlugInDataPath = AppPath + "GameData/Firespitter/PluginData/";

        public static FloatCurve stringToFloatCurve(string curveString)
        {
            FloatCurve resultCurve = new FloatCurve();

            string[] keyString = curveString.Split(';');
            for (int i = 0; i < keyString.Length; i++)
            {
                string[] valueString = keyString[i].Split(',');
                if (valueString.Length >= 2)
                {                   
                    Vector4 key = Vector4.zero;
                    float.TryParse(valueString[0], out key.x);
                    float.TryParse(valueString[1], out key.y);
                    if (valueString.Length == 4)
                    {
                        float.TryParse(valueString[2], out key.z);
                        float.TryParse(valueString[3], out key.w);
                    }

                    resultCurve.Add(key.x, key.y, key.z, key.w);                    
                }
            }
            return resultCurve;
        }

        public static List<int> parseIntegers(string stringOfInts)
        {
            List<int> newIntList = new List<int>();
            string[] valueArray = stringOfInts.Split(';');
            for (int i = 0; i < valueArray.Length; i++)
            {
                int newValue = 0;
                if (int.TryParse(valueArray[i], out newValue))
                {
                    newIntList.Add(newValue);
                }
                else
                {
                    Debug.Log("invalid integer: " + valueArray[i]);
                }
            }
            return newIntList;
        }


        public static List<float> parseFloats(string stringOfFloats)
        {
            System.Collections.Generic.List<float> list = new System.Collections.Generic.List<float>();
            string[] array = stringOfFloats.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                float item = 0f;
                if (float.TryParse(array[i].Trim(), out item))
                {
                    list.Add(item);
                }
                else
                {
                    Debug.Log("invalid float: " + array[i]);
                }
            }
            return list;
        }

        public static List<double> parseDoubles(string stringOfDoubles)
        {
            System.Collections.Generic.List<double> list = new System.Collections.Generic.List<double>();
            string[] array = stringOfDoubles.Trim().Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                double item = 0f;
                if (double.TryParse(array[i].Trim(), out item))
                {
                    list.Add(item);
                }
                else
                {
                    Debug.Log("FStools: invalid float: [len:" + array[i].Length + "] '" + array[i]+ "']");
                }
            }
            return list;
        }


        public static List<string> parseNames(string names)
        {
            return parseNames(names, false, true, string.Empty);
        }

        public static List<string> parseNames(string names, bool replaceBackslashErrors)
        {
            return parseNames(names, replaceBackslashErrors, true, string.Empty);
        }

        public static List<string> parseNames(string names, bool replaceBackslashErrors, bool trimWhiteSpace, string prefix)
        {
            List<string> source = names.Split(';').ToList<string>();
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] == string.Empty)
                {
                    source.RemoveAt(i);
                }
            }
            if (trimWhiteSpace)
            {
                for (int i = 0; i < source.Count; i++)
                {                    
                    source[i] = source[i].Trim(' ');                    
                }
            }
            if (prefix != string.Empty)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    source[i] = prefix + source[i];
                }
            }
            if (replaceBackslashErrors)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    source[i] = source[i].Replace('\\', '/');
                }
            }            
            return source.ToList<string>();
        }

        #region refresh tweakable GUI
        // Code from https://github.com/Swamp-Ig/KSPAPIExtensions/blob/master/Source/Utils/KSPUtils.cs#L62

        private static FieldInfo windowListField;

        /// <summary>
        /// Find the UIPartActionWindow for a part. Usually this is useful just to mark it as dirty.
        /// </summary>
        public static UIPartActionWindow FindActionWindow(this Part part)
        {
            if (part == null)
                return null;

            // We need to do quite a bit of piss-farting about with reflection to 
            // dig the thing out. We could just use Object.Find, but that requires hitting a heap more objects.
            UIPartActionController controller = UIPartActionController.Instance;
            if (controller == null)
                return null;

            if (windowListField == null)
            {
                Type cntrType = typeof(UIPartActionController);
                foreach (FieldInfo info in cntrType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (info.FieldType == typeof(List<UIPartActionWindow>))
                    {
                        windowListField = info;
                        goto foundField;
                    }
                }
                Debug.LogWarning("*PartUtils* Unable to find UIPartActionWindow list");
                return null;
            }
        foundField:

            List<UIPartActionWindow> uiPartActionWindows = (List<UIPartActionWindow>)windowListField.GetValue(controller);
            if (uiPartActionWindows == null)
                return null;

            return uiPartActionWindows.FirstOrDefault(window => window != null && window.part == part);
        }

        #endregion
    }

    public class MouseEventHandler : MonoBehaviour
    {
        public delegate void GenericDelegate();
        public GenericDelegate mouseDownEvent;

        public void OnMouseDown()
        {
            mouseDownEvent();
        }
    }

    public struct IntVector2
    {
        public int x;
        public int y;

        public IntVector2(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public IntVector2(float _x, float _y)
        {
            x = (int)_x;
            y = (int)_y;
        }

        public Vector2 toVector2()
        {
            return new Vector2((float)x, (float)y);
        }

        public float magnitude()
        {
            return new Vector2((float)x, (float)y).magnitude;
        }

        public override string ToString()
        {
            return ("(" + x + ", " + y + ")");
        }
    }    
}
