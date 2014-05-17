using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSVTOLrotator : PartModule
{
    [KSPField(guiActive = true, guiName = "Max angle", isPersistant = true)]
    public float deployedAngle = 90f;
    [KSPField(isPersistant = true)]
    public float maxDownAngle = 0f;
    [KSPField(isPersistant = true)]
    public Vector3 availableAngles1 = new Vector3(45, 90, 130);
    [KSPField(isPersistant = true)]
    public Vector3 availableAngles2 = new Vector3(180, 0, 0);
    [KSPField(isPersistant = true)]
    public float stepAngle = 10f;
    [KSPField]
    public string targetPartObject = "mount";
    
    public float currentAngle = 0f;

    [KSPField(guiActive = true, isPersistant = true, guiName = "Current Angle")]
    public float targetAngle = 0f;

    [KSPField]
    private int selectedListAngle = 0;
    private float animationIncrement = 1f;
    //private bool firstActivation = true;
    private List<float> availableAnglesList = new List<float>
    {
    };
    Transform partTransform = new GameObject().transform;
    private FSpropellerAtmosphericNerf atmosphericNerf = new FSpropellerAtmosphericNerf();

    FSGUIPopup popup;
    PopupElement elementInfoText;
    PopupElement elementTestAngle;
    PopupElement elementStepSize;
    PopupElement elementMaxDownAngle;
    //PopupElement elementDebug;
    //PopupElement elementDebug2;
    PopupElement elementSteerRoll;
    PopupElement elementSteerYaw;
    PopupElement elementSteerPitch;
    PopupElement elementSteerPitchType;
    PopupElement elementVTOLSteeringMode;
    List<PopupElement> elementPresets = new List<PopupElement>();
    public int popupWindowID = 0;

    [KSPField(guiName="VTOL steering mode", isPersistant = true, guiActive = true)]
    public string VTOLSteeringMode = "Off";
    [KSPField(isPersistant = true)]
    public bool VTOLsteeringActive = true;
    [KSPField]
    public float SteeringMaxPitch = 20f; // in degrees
    [KSPField]
    public float SteeringMaxPitchThrottle = 0.15f;
    [KSPField]
    public float SteeringMaxYaw = 5f; // in degrees
    [KSPField]
    public float SteeringMaxRoll = 0.15f; // in normalized thrust output variation
    [KSPField(isPersistant=true)]
    public bool steerPitch = false;
    [KSPField(isPersistant = true)]
    public bool steerYaw = false;
    [KSPField(isPersistant = true)]
    public bool steerRoll = false;
    [KSPField(isPersistant = true)]
    public bool steerThrottlePitch = false;    
    [KSPField(isPersistant = true)]
    public bool isInFrontOfCoM;
    public float steerAngle = 0f;

    [KSPField(guiActive = true, guiName = "VTOL rotation inverted", isPersistant = true)]
    public bool invertRotation;
    [KSPField(guiActive = false, isPersistant = true)]
    public bool invertSet;
    [KSPField(guiActive = false, isPersistant = true)]
    public bool isInFrontOfCoMSet = false;
    //public bool invertInHangar = false;
    [KSPField]
    public bool startInverted = false;

    [KSPField]
    public int steerDirection = 1; // -1 is for inverted steering for silly b9 engines

    [KSPEvent(name = "invertVTOLrotation", active = true, guiActive = true, guiName = "invert VTOL rotation", guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 4f)]
    public void toggleInvertRotation()
    {
        invertRotation = !invertRotation;        
    }

    [KSPEvent(name = "toggleVTOL", active = true, guiActive = true, guiName = "toggle VTOL")]
    public void toggleVTOL()
    {
        toggleAngle();
    }

    [KSPEvent(name = "cycleAnglesEvent", active = true, guiActive = true, guiName = "Cycle max angle")]
    public void cycleAnglesEvent()
    {
        cycleAngles();
    }

    [KSPEvent(name = "toggleAdvancedSteering", active = true, guiActive = true, guiName = "Next VTOL Steering Mode")]
    public void nextVTOLSteeringModeEvent()
    {
        switch (VTOLSteeringMode)
        {
            case "Off":
                updateSteeringSetup("Single Pair");                
                break;
            case "Single Pair":
                updateSteeringSetup("Double Pair");
                break;
            case "Double Pair":
                updateSteeringSetup("Custom");                              
                break;
            case "Custom":
                updateSteeringSetup("Off");
                break;
            default:
                updateSteeringSetup("Off");
                break;
        }
        //updateSteeringSetup(VTOLSteeringMode);
    }

    [KSPEvent(guiName = "Control Setup", guiActive = false, guiActiveEditor=true)]
    public void showPopupEvent()
    {
        if (popup != null)
        {
            hideAllExternalPopups();
            popup.showMenu = true;
        }
    }    

    [KSPAction("Cycle max Angles")]
    public void cycleAnglesAction(KSPActionParam param)
    {
        cycleAngles();
    }

    [KSPAction("toggle VTOL rotation")]
    public void toggleVTOLAction(KSPActionParam param)
    {
        toggleAngle();
    }

    [KSPAction("raise engine")]
    public void raiseVTOLAction(KSPActionParam param)
    {
        if (!invertRotation)
        {
            targetAngle += stepAngle;
            if (targetAngle > deployedAngle) targetAngle = deployedAngle;
        }
        else
        {
            targetAngle -= stepAngle;
            if (targetAngle < -deployedAngle) targetAngle = -deployedAngle;
        }
    }

    [KSPAction("lower engine")]
    public void lowerVTOLAction(KSPActionParam param)
    {
        if (!invertRotation)
        {
            targetAngle -= stepAngle;
            if (targetAngle < maxDownAngle) targetAngle = maxDownAngle;
        }
        else
        {
            targetAngle += stepAngle;
            if (targetAngle > -maxDownAngle) targetAngle = -maxDownAngle;
        }
    }

    [KSPAction("VTOL steering toggle")]
    public void toggleVTOLsteeringAction(KSPActionParam param)
    {
        VTOLsteeringActive = !VTOLsteeringActive;
    }

    private void toggleAngle()
    {
        if (!invertRotation)
        {
            if (targetAngle > 0) targetAngle = 0;
            else
                if (targetAngle <= deployedAngle) targetAngle = deployedAngle;
        }
        else
        {
            if (targetAngle < 0) targetAngle = 0;
            else
                if (targetAngle >= -deployedAngle) targetAngle = -deployedAngle;
        }
    }

    private void cycleAngles()
    {
        selectedListAngle ++;
        if (selectedListAngle > availableAnglesList.Count-1) selectedListAngle = 0;
        if (availableAnglesList.Count > 0)
            deployedAngle = availableAnglesList[selectedListAngle];
    }

    public void buildAngleList()
    {
        availableAnglesList.Clear();
        if (availableAngles1.x != 0) availableAnglesList.Add(availableAngles1.x);
        if (availableAngles1.y != 0) availableAnglesList.Add(availableAngles1.y);
        if (availableAngles1.z != 0) availableAnglesList.Add(availableAngles1.z);
        if (availableAngles2.x != 0) availableAnglesList.Add(availableAngles2.x);
        if (availableAngles2.y != 0) availableAnglesList.Add(availableAngles2.y);
        if (availableAngles2.z != 0) availableAnglesList.Add(availableAngles2.z); 
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        if (popupWindowID == 0)
        {
            popupWindowID = FSGUIwindowID.getNextID();
            //Debug.Log("Assigned window ID: " + popupWindowID);
        }

        partTransform = part.FindModelTransform(targetPartObject);
        buildAngleList();

        elementSteerRoll = new PopupElement("Use roll steering", new PopupButton("Yes", "No", 0f, toggleSteerRoll));
        elementSteerYaw = new PopupElement("Use yaw steering", new PopupButton("Yes", "No", 0f, toggleSteerYaw));
        elementSteerPitch = new PopupElement("Use pitch steering", new PopupButton("Yes", "No", 0f, toggleSteerPitch));
        elementSteerPitchType = new PopupElement("Pitch type", new PopupButton("Throttle", "Rotation", 0f, toggleSteerPitchType));
        elementSteerPitchType.buttons.Add(new PopupButton("Front", "Rear", 0f, toggleSteerPitchLocation));

        elementVTOLSteeringMode = new PopupElement("Mode", new PopupButton("Off", 0f, buttonSetVTOLSteering));
        elementVTOLSteeringMode.buttons.Add(new PopupButton("Single", 0f, buttonSetVTOLSteering));
        elementVTOLSteeringMode.buttons.Add(new PopupButton("Double", 0f, buttonSetVTOLSteering));
        elementVTOLSteeringMode.buttons.Add(new PopupButton("Custom", 0f, buttonSetVTOLSteering));
        elementVTOLSteeringMode.titleSize = 30f;

        #region in editor
        if (HighLogic.LoadedSceneIsEditor)
        {                                   
            elementInfoText = new PopupElement("Presets set to 0 are excluded.");
            popup = new FSGUIPopup(part, "FSVTOLrotator", 0, popupWindowID, new Rect(550f, 200f, 325f, 150f), "VTOL presets", elementInfoText); //FSGUIwindowID.VTOLrotator
            popup.sections[0].elements.Add(new PopupElement("Settings are per engine!"));

            elementTestAngle = new PopupElement(new PopupButton("Use Preset " + selectedListAngle, 100f, testUseAngle));
            popup.sections[0].elements.Add(elementTestAngle);
            elementTestAngle.buttons.Add(new PopupButton("Next", 40f, testNextAngle));
            elementTestAngle.buttons.Add(new PopupButton("Reset", 50f, testReset));

            elementStepSize = new PopupElement("Step size", stepAngle.ToString());
            elementStepSize.titleSize = 135f;
            elementStepSize.inputSize = 55f;
            popup.sections[0].elements.Add(elementStepSize);

            elementMaxDownAngle = new PopupElement("Max negative rot.,", maxDownAngle.ToString());
            elementMaxDownAngle.titleSize = 135f;
            elementMaxDownAngle.inputSize = 55f;
            popup.sections[0].elements.Add(elementMaxDownAngle);

            for (int i = 0; i <= 5; i++) // ------------- hard coded ---------------------------
            {
                elementPresets.Add(new PopupElement());
                float presetAngle = 0f;
                if (i < availableAnglesList.Count)
                {
                    presetAngle = availableAnglesList[i];
                }
                elementPresets[i] = new PopupElement("Preset " + i, presetAngle.ToString());
                elementPresets[i].titleSize = 135f;
                elementPresets[i].inputSize = 55f;
                popup.sections[0].elements.Add(elementPresets[i]);
            }

            popup.sections[0].elements.Add(new PopupElement("--- VTOL steering ---"));
            popup.sections[0].elements.Add(elementVTOLSteeringMode);
            popup.sections[0].elements.Add(elementSteerRoll);
            popup.sections[0].elements.Add(elementSteerYaw);
            popup.sections[0].elements.Add(elementSteerPitch);
            popup.sections[0].elements.Add(elementSteerPitchType);            
            updateButtonTexts();

            popup.useInFlight = false;
            popup.useInEditor = true;
            //popup.useInActionEditor = true;            
        }
        #endregion

        #region in flight
        if (HighLogic.LoadedSceneIsFlight)
        {
            atmosphericNerf = part.Modules.OfType<FSpropellerAtmosphericNerf>().FirstOrDefault();
            // --- moved the element creation to the top, for use in both editor and flight
            popup = new FSGUIPopup(part, "FSVTOLrotator", 0, popupWindowID, new Rect(500f, 300f, 250f, 100f), "VTOL steering", elementSteerRoll); //FSGUIwindowID.VTOLrotator
            popup.sections[0].elements.Add(elementSteerYaw);
            popup.sections[0].elements.Add(elementSteerPitch);
            popup.sections[0].elements.Add(elementSteerPitchType);            
            
            popup.useInFlight = true;
            popup.useInActionEditor = false;

            if (VTOLSteeringMode != "Custom")
            {
                updateSteeringSetup(VTOLSteeringMode);
            }
            else
            {
                Events["showPopupEvent"].guiActive = true;
            }

            updateButtonTexts();
        }
        #endregion        
    }

    #region GUI button functions

    private void updateButtonTexts()
    {
        if (elementSteerPitch == null)
            return;
        elementSteerPitch.buttons[0].toggle(steerPitch);
        elementSteerYaw.buttons[0].toggle(steerYaw);
        elementSteerRoll.buttons[0].toggle(steerRoll);
        elementSteerPitchType.buttons[0].toggle(steerThrottlePitch);
        if (steerThrottlePitch)
        {
            if (isInFrontOfCoMSet)
            {
                elementSteerPitchType.buttons[1].toggle(isInFrontOfCoM);
            }
            else
            {
                elementSteerPitchType.buttons[1].buttonText = "Auto";
            }
            elementSteerPitchType.buttons[1].genericFunction = toggleSteerPitchLocation;
        }
        else
        {
            elementSteerPitchType.buttons[1].buttonText = "N/A";
            elementSteerPitchType.buttons[1].genericFunction = emptyFunction;
        }
    }

    public void testNextAngle()
    {
        buildAngleList();
        selectedListAngle++;
        if (selectedListAngle > availableAnglesList.Count - 1)
        {
            selectedListAngle = 0;
        }
    }

    public void testUseAngle()
    {
        buildAngleList();
        List<Part> vtolParts = new List<Part>(part.symmetryCounterparts);
        vtolParts.Add(part);
        foreach (Part p in vtolParts)
        {
            FSVTOLrotator vtol = p.GetComponent<FSVTOLrotator>();
            if (vtol != null)
            {
                vtol.targetAngle = availableAnglesList[selectedListAngle];
                float dot = Vector3.Dot(vtol.partTransform.position.normalized, Vector3.right);
                if (dot < 0) // check for orientation of the part, relative to world directions, since there is no vessel transfrom to compare to
                {
                    vtol.targetAngle *= -1;                    
                }                

                vtol.partTransform.transform.localEulerAngles = new Vector3(-vtol.targetAngle, 0f, 0f);
            }
        }
    }

    public void testReset()
    {
        targetAngle = 0f;
        selectedListAngle = 0;
        partTransform.transform.localEulerAngles = new Vector3(targetAngle, 0f, 0f);
    }

    public void toggleSteerPitch()
    {
        steerPitch = !steerPitch;
        elementSteerPitch.buttons[0].toggle(steerPitch);
        updateSteeringSetup(steerPitch, steerYaw, steerRoll, steerThrottlePitch);
    }

    public void toggleSteerYaw()
    {
        steerYaw = !steerYaw;
        elementSteerYaw.buttons[0].toggle(steerYaw);
        updateSteeringSetup(steerPitch, steerYaw, steerRoll, steerThrottlePitch);
    }

    public void toggleSteerRoll()
    {
        steerRoll = !steerRoll;
        elementSteerRoll.buttons[0].toggle(steerRoll);
        updateSteeringSetup(steerPitch, steerYaw, steerRoll, steerThrottlePitch);
    }

    public void toggleSteerPitchType()
    {
        steerThrottlePitch = !steerThrottlePitch;
        elementSteerPitchType.buttons[0].toggle(steerThrottlePitch);
        if (steerThrottlePitch)
        {            
            elementSteerPitchType.buttons[1].toggle(isInFrontOfCoM);
            elementSteerPitchType.buttons[1].genericFunction = toggleSteerPitchLocation;            
        }
        else
        {            
            elementSteerPitchType.buttons[1].buttonText = "N/A";
            elementSteerPitchType.buttons[1].genericFunction = emptyFunction;
        }
        updateSteeringSetup(steerPitch, steerYaw, steerRoll, steerThrottlePitch);
    }

    public void toggleSteerPitchLocation()
    {
        isInFrontOfCoM = !isInFrontOfCoM;
        isInFrontOfCoMSet = true;
        elementSteerPitchType.buttons[1].toggle(isInFrontOfCoM);
        updateSteeringSetup(steerPitch, steerYaw, steerRoll, steerThrottlePitch);
    }

    public void emptyFunction()
    {
    }

    #endregion

    private void updateSteeringSetup(bool pitch, bool yaw, bool roll, bool throttlePitch)
    {
        steerPitch = pitch;
        steerYaw = yaw;
        steerRoll = roll;
        steerThrottlePitch = throttlePitch;        

        foreach (Part p in part.symmetryCounterparts)
        {
            FSVTOLrotator vtol = p.GetComponent<FSVTOLrotator>();
            if (vtol != null)
            {
                vtol.steerPitch = pitch;
                vtol.steerYaw = yaw;
                vtol.steerRoll = roll;
                vtol.steerThrottlePitch = throttlePitch;
                vtol.VTOLSteeringMode = this.VTOLSteeringMode;
                vtol.isInFrontOfCoM = this.isInFrontOfCoM;
                vtol.updateButtonTexts();
            }
        }
        updateButtonTexts();
        hideAllExternalPopups();
    }

    public void updateSteeringSetup(string newSetup)
    {
        Events["showPopupEvent"].guiActive = false;
        VTOLSteeringMode = newSetup;
        switch (newSetup)
        {
            case "Single Pair":                
                updateSteeringSetup(true, true, true, false);                
                break;
            case "Double Pair":                
                updateSteeringSetup(true, true, true, true);                
                break;
            case "Custom":                
                updateSteeringSetup(true, true, true, true);
                popup.showMenu = true;                
                //updateButtonTexts();
                Events["showPopupEvent"].guiActive = true;
                break;
            case "Off":                
                updateSteeringSetup(false, false, false, false);                
                break;
            default:                
                //updateSteeringSetup(false, false, false, false);                
                break;
        }        
    }

    private void buttonSetVTOLSteering(PopupButton button)
    {
        string newSetup = button.buttonText;
        if (newSetup == "Single" || newSetup == "Double")
            newSetup += " Pair";
        updateSteeringSetup(newSetup);
        foreach (PopupButton b in elementVTOLSteeringMode.buttons)
        {
            b.styleSelected = false;
        }
        button.styleSelected = true;        
    }

    private void hideAllExternalPopups()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            foreach (Part p in vessel.parts)
            {
                if (p != part)
                {
                    FSVTOLrotator vtol = p.GetComponent<FSVTOLrotator>();
                    if (vtol != null)
                    {
                        if (vtol.popup != null)
                        {
                            vtol.popup.showMenu = false;
                        }
                    }
                }
            }
        }
    }

    public void FixedUpdate() // moved angle update to fixed update to make rotation speed indpendent of framerate
    {
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;   

        float angleChange = targetAngle - currentAngle;

        if (angleChange > animationIncrement)
        {
            angleChange = animationIncrement;
        }
        else if (angleChange < -animationIncrement)
        {
            angleChange = -animationIncrement;
        }

        currentAngle += angleChange;

        //partTransform.transform.Rotate(-angleChange, 0, 0);

        float finalAngle = -currentAngle + steerAngle;
        partTransform.transform.localEulerAngles = new Vector3(finalAngle, 0, 0);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight)
            return;

        if (!invertSet) //run only the first time the craft is loaded
        {
            // test whether the engine is on the left or right side of the craft, for inverting the engine rotation and thrust based roll.
            if (Vector3.Dot(partTransform.position.normalized, vessel.ReferenceTransform.right) < 0) // below 0 means the engine is on the left side of the craft
            {
                invertRotation = true;
                //Debug.Log("Inverting left side VTOL rotation");
            }
            else
            {
                invertRotation = false;
            }

            if (startInverted)
                invertRotation = !invertRotation;
        }
        invertSet = true;        

        if (!isInFrontOfCoMSet)
        {
            // test whether the engine is in the front or rear of the craft, for using double engine pair thrust based pitch.
            Transform CoMTransform = new GameObject().transform;
            CoMTransform.position = vessel.CoM;
            CoMTransform.rotation = vessel.transform.rotation;
            Vector3 relativePosition = CoMTransform.InverseTransformPoint(part.transform.position);
            if (relativePosition.y < 0)
            {
                //Debug.Log("FSVTOLrotator: Engine is behind CoM: " + relativePosition);
                isInFrontOfCoM = false;
            }
            else
            {
                //Debug.Log("FSVTOLrotator: Engine is in front of CoM: " + relativePosition);
                isInFrontOfCoM = true;
            }
        }
        isInFrontOfCoMSet = true;
        updateButtonTexts();

        #region VTOL steering

        FlightCtrlState ctrl = vessel.ctrlState;

        steerAngle = 0f;
        atmosphericNerf.steeringModifier = 1f;

        if (VTOLsteeringActive)
        {

            if (steerPitch)
            {
                if (steerThrottlePitch)
                {
                    float steerModifier = ctrl.pitch * SteeringMaxPitchThrottle;
                    if (isInFrontOfCoM)
                        steerModifier *= -1;
                    atmosphericNerf.steeringModifier -= steerModifier * steerDirection;
                }
                else
                {
                    steerAngle -= ctrl.pitch * SteeringMaxPitch * steerDirection;
                    if (invertRotation)
                        steerAngle *= -1;
                }
            }
            if (steerYaw)
            {
                steerAngle -= ctrl.yaw * SteeringMaxYaw * steerDirection;
            }
            if (steerRoll)
            {
                float steerModifier = ctrl.roll * SteeringMaxRoll;
                if (invertRotation)
                    steerModifier *= -1;
                atmosphericNerf.steeringModifier -= steerModifier * steerDirection;
            }
        }

        #endregion
    }

    public void OnGUI()
    {
        if (popup != null)
        {
            if (HighLogic.LoadedSceneIsEditor)
            {                
                //elementCurrentAngle.inputText = "Current rot.: " + currentAngle;
                popup.popup();
                stepAngle = float.Parse(elementStepSize.inputText);
                maxDownAngle = float.Parse(elementMaxDownAngle.inputText);

                // fill the kspfield preset values from the popup
                if (elementPresets.Count > 0)
                    availableAngles1.x = float.Parse(elementPresets[0].inputText);
                if (elementPresets.Count > 1)
                    availableAngles1.y = float.Parse(elementPresets[1].inputText);
                if (elementPresets.Count > 2)
                    availableAngles1.z = float.Parse(elementPresets[2].inputText);
                if (elementPresets.Count > 3)
                    availableAngles2.x = float.Parse(elementPresets[3].inputText);
                if (elementPresets.Count > 4)
                    availableAngles2.y = float.Parse(elementPresets[4].inputText);
                if (elementPresets.Count > 5)
                    availableAngles2.z = float.Parse(elementPresets[5].inputText);
                elementTestAngle.buttons[0].buttonText = "Use Preset " + selectedListAngle;
            }            

            if (HighLogic.LoadedSceneIsFlight && popup.useInFlight)
            {                
                popup.popup();
            }
        }
        //Debug elements
        //elementDebug.titleText = "selected: " + selectedListAngle;
        //elementDebug2.titleText = "2: " + availableAngles2;
    }
}
