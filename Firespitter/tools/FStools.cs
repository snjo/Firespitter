using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter
{
    class Tools
    {
        public static Vector3 WorldUp(Vessel vessel)
        {
            if (vessel != null)
                return (vessel.rigidbody.position - vessel.mainBody.position).normalized;
            else
                return Vector3.zero;
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
                    Debug.Log("FStools: Added key to FloatCurve: " + key.ToString());
                }
            }

            return resultCurve;
        }
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
