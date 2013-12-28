using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSinputVisualizer
{
    public Rect windowRect = new Rect(500f, 200f, 150f, 30f);
    public int windowID;
    public Vector2 buttonSize = new Vector2(30f, 30f);
    public float padding = 8f;
    public Rect drawPosition = new Rect(0f, 0f, 30f, 30f);

    private GUIStyle buttonPassive;
    private GUIStyle buttonActive;
    private bool stylesCreated = false;

    private Color faintBlack = new Color(0f, 0f, 0f, 0.1f);
    private Color opaqueBlack = new Color(0f, 0f, 0f, 1f);

    private void createStyles()
    {
        buttonPassive = new GUIStyle(GUI.skin.button);
        buttonActive = new GUIStyle(GUI.skin.button);

        buttonPassive.normal.textColor = Color.gray;
        buttonActive.normal.textColor = Color.white;

        buttonPassive.normal.background = new Texture2D(1, 1);
        buttonPassive.normal.background.SetPixel(0, 0, faintBlack);        
        buttonPassive.normal.background.Apply();

        buttonActive.normal.background = new Texture2D(1, 1);
        buttonActive.normal.background.SetPixel(0, 0, opaqueBlack);
        buttonActive.normal.background.Apply();
        
        buttonPassive.padding = new RectOffset(2, 2, 2, 2);
        buttonActive.padding = new RectOffset(2, 2, 2, 2);

        stylesCreated = true;
    }

    private GUIStyle getButtonStyle(KeyCode key)
    {
        if (Input.GetKey(key))
            return buttonActive;
        else
            return buttonPassive;
    }

    private GUIStyle getButtonStyle(bool state)
    {
        if (state)
            return buttonActive;
        else
            return buttonPassive;
    }

    private Rect longButtonRect(float length)
    {                
            return new Rect(drawPosition.x, drawPosition.y, buttonSize.x * length, buttonSize.y);        
    }    

    public FSinputVisualizer()
    {
        windowID = FSGUIwindowID.getNextID();
    }

    private void nextButtonPos()
    {
        drawPosition.x += buttonSize.x + padding;
    }

    private void newLine(float x)
    {
        drawPosition.x = x;
        drawPosition.y += buttonSize.y + padding;
    }

    public void drawWindow(int ID)
    {
        GUI.DragWindow();
    }

    public void OnGUI()
    {
        if (!stylesCreated)
            createStyles();
        windowRect = GUI.Window(windowID, windowRect, drawWindow, "Input Visualizer", buttonActive);

        float letterKeysXpos = windowRect.x + buttonSize.x*2 + padding*2;
        drawPosition.x = letterKeysXpos;
        drawPosition.y = windowRect.y + windowRect.height + padding;

        GUI.Button(drawPosition, "Q", getButtonStyle(KeyCode.Q));
        nextButtonPos();
        GUI.Button(drawPosition, "W", getButtonStyle(KeyCode.W));
        nextButtonPos();
        GUI.Button(drawPosition, "E", getButtonStyle(KeyCode.E));
        newLine(letterKeysXpos);

        GUI.Button(drawPosition, "A", getButtonStyle(KeyCode.A));
        nextButtonPos();
        GUI.Button(drawPosition, "S", getButtonStyle(KeyCode.S));
        nextButtonPos();
        GUI.Button(drawPosition, "D", getButtonStyle(KeyCode.D));
        newLine(letterKeysXpos);

        GUI.Button(drawPosition, "Z", getButtonStyle(KeyCode.Z));
        nextButtonPos();
        GUI.Button(drawPosition, "X", getButtonStyle(KeyCode.X));
        nextButtonPos();
        GUI.Button(drawPosition, "C", getButtonStyle(KeyCode.C));
        newLine(letterKeysXpos);

        drawPosition.x = windowRect.x;
        drawPosition.y = windowRect.y + windowRect.height + padding*2 + buttonSize.y;
        GUI.Button(longButtonRect(2), "Caps", getButtonStyle(KeyCode.CapsLock));
        drawPosition.y += buttonSize.y + padding;
        GUI.Button(longButtonRect(2), "Shift", getButtonStyle(KeyCode.LeftShift));
        drawPosition.y += buttonSize.y + padding;
        GUI.Button(longButtonRect(2), "Ctrl", getButtonStyle(KeyCode.LeftControl));
    }
}
