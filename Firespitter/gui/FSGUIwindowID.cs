using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using System.Threading.Tasks;

public static class FSGUIwindowID
{
	public static int housingProgram = 5600;
    public static int baseNumber = 5879;
    public static int infoPopup = 5879;
    public static int animateGeneric = 5880; // 10 reserved
    public static int switchEngineThrustTransform = 5890;
    public static int toggleSurfaceAttach = 5891;
    public static int test = 5892;
    public static int trimAdjustment = 5893; // 4 reserved for pitch/yaw/roll/wheel
    public static int moveCraftAtLaunch = 5898;
    public static int VTOLrotator = 5899;
    public static int textureSwitch = 5900; // 10 reserved
    public static int wheel = 5911;
    public static int partTurner = 5912;
    public static int wing = 5913;
    public static int flightPath = 5916; // 5 reserved

    public static Rect standardRect = new Rect(500f, 300f, 300f, 100f);
    public static Rect tallRect = new Rect(500f, 300f, 300f, 500f);

    public static int lastUsedID = 6050;

    public static int getNextID()
    {
        lastUsedID++;
        return lastUsedID;
    }
}