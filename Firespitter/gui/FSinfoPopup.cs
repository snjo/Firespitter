using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSinfoPopup : PartModule
{
    [KSPField(isPersistant=true)]
    public string textHeading = "Craft Info";
    [KSPField(isPersistant=true)]
    public string textBody1 = "";
    [KSPField(isPersistant = true)]
    public string textBody2 = "";
    [KSPField(isPersistant = true)]
    public string textBody3 = "";
    [KSPField(isPersistant = true)]
    public string textBody4 = "";
    [KSPField(isPersistant = true)]
    public string textBody5 = "";
    [KSPField(isPersistant = true)]
    public string textBody6 = "";
    [KSPField(isPersistant = true)]
    public string textBody7 = "";
    [KSPField(isPersistant = true)]
    public string textBody8 = "";
    [KSPField(isPersistant = true)]
    public string textBody9 = "";
    [KSPField(isPersistant = true)]
    public string textBody10 = "";
    [KSPField(isPersistant = true)]
    public string textBody11 = "";

    public int numLines = 11;

    [KSPField(isPersistant = true)]
    public bool showAtFlightStart = true;
    [KSPField(isPersistant = true)]
    public bool hideAfterCountdown = true;
    [KSPField(isPersistant = true)]
    public bool showOnEachFlightStart = false;
    [KSPField(isPersistant = true)]
    public bool hasBeenShown = false;
    [KSPField(isPersistant = true)]
    public float countDownDuration = 20f;
    [KSPField(isPersistant = true)]
    public bool hideMeshInFlight = false;
    [KSPField(isPersistant = true)]
    public string toggleKey = "i";
    [KSPField(isPersistant = true)]
    public bool useHotkey = true;

    private float countDown;
    private bool showInfo = false;
    private bool shownByUser = false;
    private bool editMode = false;
    private float oldTime;
    private string windowTitle;
    private int editorButtonCooldown;

    //Vector2 menuBasePosition = new Vector2(300f, 300f);
    Vector2 menuItemPosition = new Vector2(0f, 0f);
    Vector2 menuItemSize = new Vector2(300f, 22f);
    Vector2 buttonSize = new Vector2(25f, 20f); 
    Rect windowRect = new Rect(300f, 300f, 320f, 320f);


    [KSPEvent(name = "showInfo", active = true, guiActive = true, guiActiveEditor=true, guiName = "Show Info")]
    public void showInfoEvent()
    {
        showInfo = true;
        shownByUser = true;
    }

    [KSPAction("Show Info")]
    public void showInfoAction(KSPActionParam param)
    {
        showInfo = !showInfo;
        shownByUser = true;
    }

    private string writeLine(Rect rect, string text)
    {
        if (editMode)
        {
            return GUI.TextField(rect, text);
        }
        else
        {
            GUI.Label(rect, text);
            return text;
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel)
        {
            showInfo = false;
            return;
        }

        showInfo = false;

        if (showAtFlightStart && showOnEachFlightStart)
            showInfo = true;
        if (showAtFlightStart && !showOnEachFlightStart && !hasBeenShown)
        {
            showInfo = true;
            showAtFlightStart = false;
            hasBeenShown = true;
        }

        oldTime = Time.time;
        countDown = countDownDuration;
        //GUI.skin = skin;
    }

    void drawWindow(int WindowID)
    {
        Rect menuItemRect = new Rect(10f, 8f, menuItemSize.x, menuItemSize.y);

        //textHeading = writeLine(menuItemRect, textHeading);

        if (GUI.Button(new Rect(menuItemRect.x + menuItemSize.x - buttonSize.x, menuItemRect.y, buttonSize.x, buttonSize.y), "X"))
        {
            showInfo = false;
            editMode = false;
        }        

        if (editMode)
        {
            windowTitle = "";
            textHeading = writeLine(new Rect(menuItemRect.x, menuItemRect.y, menuItemRect.width - buttonSize.x - 10f, menuItemRect.height), textHeading);
        }
        else
            windowTitle = textHeading;

        menuItemRect.y += menuItemSize.y;

        //GUIStyle style = new GUIStyle();
        //style.normal.background = 

        textBody1 = writeLine(menuItemRect, textBody1);
        menuItemRect.y += menuItemSize.y;
        textBody2 = writeLine(menuItemRect, textBody2);
        menuItemRect.y += menuItemSize.y;
        textBody3 = writeLine(menuItemRect, textBody3);
        menuItemRect.y += menuItemSize.y;
        textBody4 = writeLine(menuItemRect, textBody4);
        menuItemRect.y += menuItemSize.y;
        textBody5 = writeLine(menuItemRect, textBody5);
        menuItemRect.y += menuItemSize.y;
        textBody6 = writeLine(menuItemRect, textBody6);
        menuItemRect.y += menuItemSize.y;
        textBody7 = writeLine(menuItemRect, textBody7);
        menuItemRect.y += menuItemSize.y;
        textBody8 = writeLine(menuItemRect, textBody8);
        menuItemRect.y += menuItemSize.y;
        textBody9 = writeLine(menuItemRect, textBody9);
        menuItemRect.y += menuItemSize.y;
        textBody10 = writeLine(menuItemRect, textBody10);
        menuItemRect.y += menuItemSize.y;
        textBody11 = writeLine(menuItemRect, textBody11);
        menuItemRect.y += menuItemSize.y;

        // show on start toggle
        string showOnStartString;
        if (showAtFlightStart)
        {
            showOnStartString = "Y";
        }
        else
        {
            showOnStartString = "N";
        }
        if (GUI.Button(new Rect(menuItemRect.x, menuItemRect.y, buttonSize.x, buttonSize.y), showOnStartString))
        {
            showAtFlightStart = !showAtFlightStart;
            if (showAtFlightStart)
                showOnEachFlightStart = true;
        }
        GUI.Label(new Rect(menuItemRect.x + buttonSize.x + 10f, menuItemRect.y, menuItemSize.x - buttonSize.x - 10f, buttonSize.y), "Show on start");
        
        // hotkey toggle
        string useHotkeyString;
        if (useHotkey)
        {
            useHotkeyString = "Y";
        }
        else
        {
            useHotkeyString = "N";
        }
        if (GUI.Button(new Rect(menuItemRect.x + (menuItemSize.x / 2), menuItemRect.y, buttonSize.x, buttonSize.y), useHotkeyString))
        {
            useHotkey = !useHotkey;
        }
        GUI.Label(new Rect(menuItemRect.x + buttonSize.x + 10f + (menuItemSize.x / 2), menuItemRect.y, menuItemSize.x - buttonSize.x - 10f, buttonSize.y), "Use hotkey (" + toggleKey + ")");

        menuItemRect.y += menuItemSize.y;
        if (GUI.Button(new Rect(menuItemRect.x, menuItemRect.y, buttonSize.x * 2, buttonSize.y), "Edit"))
        {
            editMode = !editMode;
            shownByUser = true;
        }
        //menuItemRect.y += menuItemSize.y;
        if (!shownByUser)
            GUI.Label(new Rect(menuItemRect.x + (buttonSize.x * 2) + 20f, menuItemRect.y, menuItemRect.width, menuItemRect.height), "Hiding this window in " + (int)countDown);
        GUI.DragWindow();
    }

    public void OnGUI()
    {        

        if (showInfo)
        {
            float timeChange = Time.time - oldTime;
            oldTime = Time.time;
            if (countDown > 0f)
                countDown -= timeChange;
            if (countDown <= 0f && !shownByUser && hideAfterCountdown)            
                showInfo = false;
                        
            windowRect = GUI.Window(FSGUIwindowID.infoPopup, windowRect, drawWindow, windowTitle);            

            //GUI.Box(new Rect(menuBasePosition.x - 10f, menuBasePosition.y - 10f, menuItemSize.x + 20f, (menuItemSize.y * 14) + 20f), "");

            
        }

        if (HighLogic.LoadedSceneIsEditor)
        {
            EditorLogic editor = EditorLogic.fetch;
            if (editor)
            {
                if (editorButtonCooldown > 0)
                    editorButtonCooldown--;
                if (Input.GetKeyDown(toggleKey) && editorButtonCooldown <= 0)
                {
                    showInfo = !showInfo;
                    shownByUser = true;
                    editorButtonCooldown = 20;
                }
                shownByUser = true;
                //if (editor.editorScreen == EditorLogic.EditorScreen.Actions)
                //{
                //    if (EditorActionGroups.Instance.GetSelectedParts().Find(p => p.Modules.Contains("FSinfoPopup")))
                //    {
                //        showInfo = true;
                //    }
                //}
            }
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel)
        {
            showInfo = false;
            return;
        }

        if (Input.GetKeyDown(toggleKey) && useHotkey)
        {
            showInfo = !showInfo;
            shownByUser = true;
        }
    }
}

