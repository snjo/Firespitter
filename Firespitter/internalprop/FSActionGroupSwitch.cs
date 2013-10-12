using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Controls an internal prop switch in a cockpit.
/// Can control most default action groups, and some custom ones. Will animate the switch with rotation or emissive texture.
/// </summary>

public class FSActionGroupSwitch : InternalModule
{
    /// <summary>
    /// the action group managed by this switch
    /// </summary>
    [KSPField]    
    public string groupName = "Gear";
    /// <summary>
    /// should the object defines by switchObjectName be animated by rotation?
    /// </summary>
    [KSPField]
    public string switchType = "flipSwitch";
    /// <summary>
    /// the part that animates, if switchtype is "flipswitch", or useEmissiveToggle is 1
    /// </summary>
    [KSPField]
    public string switchObjectName = "lever";
    /// <summary>
    /// the collider that catches the mouse click
    /// </summary>
    [KSPField]
    public string buttonTrigger = "trigger";
    /// <summary>
    /// if set to 1, it will use the colour set in onEmissiveColor when the button is on/pushed
    /// </summary>
    [KSPField]
    public int useEmissiveToggle = 0;
    /// <summary>
    /// reverse flip direction? 1 is normal, -1 is reversed
    /// </summary>
    [KSPField]
    public float flipDirection = 1f;
    /// <summary>
    /// will post a message in the middle of the screen when setting hover mode, or other custom stuff.
    /// </summary>
    [KSPField]
    public int postMessagesToScreen = 0;
    /// <summary>
    /// how long the button will be lit if it's just a click (not a toggle on/off)
    /// </summary>
    [KSPField]
    public int resetEmissiveTime = 0;
    /// <summary>
    /// an RGB value for when the button is on
    /// </summary>
    [KSPField]
    public Vector3 onEmissiveColor;
    /// <summary>
    /// an RGB value for when the button is off
    /// </summary>
    [KSPField]
    public Vector3 offEmissiveColor;


    public KSPActionGroup actionGroup = KSPActionGroup.Gear;
    public Transform switchObjectTransform;

    private FSgenericButtonHandler buttonHandler;
    private GameObject buttonObject;
    private bool customAction = false;
    private int actionGroupNumber = 0;
    private int emissiveCountdown = -1;
    //private Color defaultEmissiveColor;
    //private bool clicked = false;

    /// <summary>
    /// triggered by FSswitchButtonHandler
    /// </summary>
    /// <param name="buttonNumber">If there are several buttons tied to this module, this would tell them apart</param>
    public void buttonClick() //(int buttonNumber)
    {        
        //set emissive color if used when clicked, start countdown to reset emissive if that applies
        if (useEmissiveToggle == 1)
        {
            switchObjectTransform.renderer.material.SetColor("_EmissiveColor", new Color(onEmissiveColor.x, onEmissiveColor.y, onEmissiveColor.z));            
            //switchObjectTransform.renderer.material.SetColor("_EmissiveColor", Color.yellow);            
            emissiveCountdown = resetEmissiveTime;
        }


        //Debug.Log("FS switch " + buttonNumber + " clicked");
        if (!customAction)
        {
            FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(actionGroup);
        }
        else
        {
            switch (groupName)
            {
                case "engine":
                    foreach (Part part in vessel.Parts)
                    {                        
                        ModuleEngines engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
                        if (engine != null) engine.EngineIgnited = !engine.EngineIgnited;
                    }
                    break;
                case "hover":
                    foreach (Part part in vessel.Parts)
                    {
                        FScopterThrottle copterEngine = part.Modules.OfType<FScopterThrottle>().FirstOrDefault();
                        if (copterEngine != null)
                        {
                            copterEngine.toggleHover();
                            if (postMessagesToScreen == 1)
                                ScreenMessages.PostScreenMessage(new ScreenMessage("hover height set to " + Math.Round(copterEngine.hoverHeight,1), 2f, ScreenMessageStyle.UPPER_CENTER));
                            Debug.Log("FS: hover height set to " + copterEngine.hoverHeight);
                        }
                    }
                    break;
                case "Stage":
                    //FlightGlobals.ActiveVessel.ResumeStaging();
                    //StageManager.ActivateNextStage();                   
                    break;
                case "resetTrim":
                    FlightInputHandler.state.yawTrim = 0f;
                    break;
            }
        }
    }    

    public void Start()
    {
        switch (groupName)
        {
            case "Gear":
                actionGroup = KSPActionGroup.Gear;
                break;
            case "Brakes":
                actionGroup = KSPActionGroup.Brakes;
                break;
            case "Light":
                actionGroup = KSPActionGroup.Light;
                break;
            case "RCS":
                actionGroup = KSPActionGroup.RCS;
                break;
            case "SAS":
                actionGroup = KSPActionGroup.SAS;
                break;
            case "Stage":
                customAction = true;
                //actionGroup = KSPActionGroup.Stage;
                break;
            case "Abort":
                actionGroup = KSPActionGroup.Abort;
                break;
            case "Custom01":
                actionGroup = KSPActionGroup.Custom01;
                break;
            case "Custom02":
                actionGroup = KSPActionGroup.Custom02;
                break;
            case "Custom03":
                actionGroup = KSPActionGroup.Custom03;
                break;
            case "Custom04":
                actionGroup = KSPActionGroup.Custom04;
                break;
            case "Custom05":
                actionGroup = KSPActionGroup.Custom05;
                break;
            case "Custom06":
                actionGroup = KSPActionGroup.Custom06;
                break;
            case "Custom07":
                actionGroup = KSPActionGroup.Custom07;
                break;
            case "Custom08":
                actionGroup = KSPActionGroup.Custom08;
                break;
            case "Custom09":
                actionGroup = KSPActionGroup.Custom09;
                break;
            case "Custom10":
                actionGroup = KSPActionGroup.Custom10;
                break;            
            default:
                customAction = true;
                break;
        }
        actionGroupNumber = BaseAction.GetGroupIndex(actionGroup);
        switchObjectTransform = base.internalProp.FindModelTransform(switchObjectName);

        buttonObject = base.internalProp.FindModelTransform(buttonTrigger).gameObject;
        buttonHandler = buttonObject.AddComponent<FSgenericButtonHandler>();
        buttonHandler.mouseDownFunction = buttonClick;
        //buttonObject.AddComponent<FSswitchButtonHandler>();
        //buttonObject.GetComponent<FSswitchButtonHandler>().buttonNumber = 1;
        //buttonObject.GetComponent<FSswitchButtonHandler>().target = base.internalProp.gameObject;
    }

    public void Update()
    {
        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA
            || CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
        {
            bool groupState = FlightGlobals.ActiveVessel.ActionGroups.groups[actionGroupNumber];        

            if (switchType == "flipSwitch")
            {
                if (groupState)
                {
                    switchObjectTransform.localRotation = Quaternion.Euler(new Vector3(-90f - (50f*flipDirection), 0f, 0f));
                }
                else
                {
                    switchObjectTransform.localRotation = Quaternion.Euler(new Vector3(-90f + (50f * flipDirection), 0f, 0f));
                }
            }
            if (useEmissiveToggle == 1)
            {
                if (emissiveCountdown == 0)
                {
                    switchObjectTransform.renderer.material.SetColor("_EmissiveColor", new Color(offEmissiveColor.x, offEmissiveColor.y, offEmissiveColor.z, 1f));
                    //switchObjectTransform.renderer.material.SetColor("_EmissiveColor", Color.green);
                    emissiveCountdown--;
                }
                else if (emissiveCountdown > 0)
                {
                    emissiveCountdown--;
                }
            }
        }
    }
}

