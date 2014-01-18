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
