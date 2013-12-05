using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

class FStrimAdjustment : PartModule
{
    FSGUIPopup popup;

    [KSPField]
    public string axis = "Pitch"; // Pitch, Yaw, Roll
    [KSPField]
    public int moduleID = 0;
    [KSPField]
    public float buttonIncrements = 1f;
    [KSPField]
    public float MoreTrimActionMultiplier = 10f;
    [KSPField(isPersistant = true)]
    public bool showOnFlightStart = true;

    [KSPField(isPersistant = true)]
    public float presetTrim0 = 0f;
    [KSPField(isPersistant = true)]
    public float presetTrim1 = 0f;
    [KSPField(isPersistant = true)]
    public float presetTrim2 = 0f;

    PopupElement elementShowOnFlightStart;
    public int firstPresetLine = 1;
    private float showGUICountDown = 2f;

    [KSPAction("Toggle Popup")]
    public void togglePopupAction(KSPActionParam param)
    {
        popup.showMenu = !popup.showMenu;
    }

    [KSPEvent(guiName = "Toggle Popup", guiActive = true, guiActiveEditor=true)]
    public void togglePopupEvent()
    {
        popup.showMenu = !popup.showMenu;
    }

    [KSPAction("Use Preset 1")]
    public void usePreset1Action(KSPActionParam param)
    {
        if (popup.sections[0].elements.Count > 1)
            setTrimValue(popup.sections[0].elements[1].buttons[0]);
    }

    [KSPAction("Increase Trim")]
    public void increaseTrimAction(KSPActionParam param)
    {
        adjustTrim(buttonIncrements);
    }

    [KSPAction("Decrease Trim")]
    public void decreaseTrimAction(KSPActionParam param)
    {
        adjustTrim(-buttonIncrements);
    }

    [KSPAction("Increase Trim")]
    public void increaseTrimMoreAction(KSPActionParam param)
    {
        adjustTrim(buttonIncrements * MoreTrimActionMultiplier);
    }

    [KSPAction("Decrease Trim")]
    public void decreaseTrimMoreAction(KSPActionParam param)
    {
        adjustTrim(-buttonIncrements * MoreTrimActionMultiplier);
    }

    public int popupWindowID = 0;

    #region trim actions

    /*
    [KSPAction("Trim Pitch Down 2%")]
    public void trimDownAction2(KSPActionParam param)
    {
        adjustTrim(-0.02f);
    }

    [KSPAction("Trim Pitch Up 2%")]
    public void trimUpAction2(KSPActionParam param)
    {
        adjustTrim(+0.02f);
    }

    [KSPAction("Trim Pitch Down 4%")]
    public void trimDownAction4(KSPActionParam param)
    {
        adjustTrim(-0.04f);
    }

    [KSPAction("Trim Pitch Up 4%")]
    public void trimUpAction4(KSPActionParam param)
    {
        adjustTrim(+0.04f);
    }

    [KSPAction("Trim Pitch Down 8%")]
    public void trimDownAction8(KSPActionParam param)
    {
        adjustTrim(-0.08f);
    }

    [KSPAction("Trim Pitch Up 8%")]
    public void trimUpAction8(KSPActionParam param)
    {
        adjustTrim(+0.08f);
    }

    [KSPAction("Trim Pitch Down 16%")]
    public void trimDowActionn16(KSPActionParam param)
    {
        adjustTrim(-0.16f);
    }

    [KSPAction("Trim Pitch Up 16%")]
    public void trimUpAction16(KSPActionParam param)
    {
        adjustTrim(+0.16f);
    }

    [KSPAction("Trim Pitch Down 32%")]
    public void trimDownAction32(KSPActionParam param)
    {
        adjustTrim(-0.32f);
    }

    [KSPAction("Trim Pitch Up 32%")]
    public void trimUpAction32(KSPActionParam param)
    {
        adjustTrim(+0.32f);
    }

    [KSPAction("Trim Pitch Down 64%")]
    public void trimDownAction64(KSPActionParam param)
    {
        adjustTrim(-0.64f);
    }

    [KSPAction("Trim Pitch Up 64%")]
    public void trimUpAction64(KSPActionParam param)
    {
        adjustTrim(+0.64f);
    }

     */
    #endregion

    private void addPresetLine()
    {
        addPresetLine(0f);
    }

    private void addPresetLine(float preset)
    {
        PopupElement element = new PopupElement(axis, preset.ToString(), new PopupButton("Use", 0f, setTrimValue));
        element.buttons[0].popupElement = element;
        popup.sections[0].elements.Add(element);
    }

    private void removePresetLine()
    {
        if (popup.sections[0].elements.Count > firstPresetLine)
            popup.sections[0].elements.RemoveAt(popup.sections[0].elements.Count - 1);
    }

    private void adjustTrim(float amount)
    {
        trim = Mathf.Clamp(trim + amount, -1f, 1f);
        //switch (axis)
        //{
        //    case "Pitch":
        //        FlightInputHandler.state.pitchTrim = Mathf.Clamp(FlightInputHandler.state.pitchTrim  + amount, -1f, 1f); 
        //        break;
        //    case "Yaw":
        //        FlightInputHandler.state.yawTrim = Mathf.Clamp(FlightInputHandler.state.yawTrim  + amount, -1f, 1f); 
        //        break;
        //    case "Roll":
        //        FlightInputHandler.state.rollTrim = Mathf.Clamp(FlightInputHandler.state.rollTrim  + amount, -1f, 1f); 
        //        break;
        //    case "Wheel":
        //        FlightInputHandler.state.wheelSteerTrim = Mathf.Clamp(FlightInputHandler.state.wheelSteerTrim  + amount, -1f, 1f); 
        //        break;
        //}
    }

    private void adjustTrimFromButton(PopupButton button)
    {
        if (button.buttonText == "+")
            adjustTrim(buttonIncrements);
        else if (button.buttonText == "-")
            adjustTrim(-buttonIncrements);
    }

    private void setTrimValue(PopupButton button)
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            float newTrim = 0f;
            string trimString = button.popupElement.inputText;
            if (float.TryParse(trimString, out newTrim))
            {
                trim = Mathf.Clamp(newTrim / 100f, -1f, 1f);
                //switch (axis)
                //{
                //    case "Pitch":
                //        FlightInputHandler.state.pitchTrim = Mathf.Clamp(newTrim / 100f, -1f, 1f);
                //        break;
                //    case "Yaw":
                //        FlightInputHandler.state.yawTrim = Mathf.Clamp(newTrim / 100f, -1f, 1f);
                //        break;
                //    case "Roll":
                //        FlightInputHandler.state.rollTrim = Mathf.Clamp(newTrim / 100f, -1f, 1f);
                //        break;
                //    case "Wheel":
                //        FlightInputHandler.state.wheelSteerTrim = Mathf.Clamp(newTrim / 100f, -1f, 1f);
                //        break;
                //}
            }
        }
    }

    public float trim
    {
        get
        {
            switch (axis)
            {
                case "Pitch":
                    return FlightInputHandler.state.pitchTrim;
                case "Yaw":
                    return FlightInputHandler.state.yawTrim;
                case "Roll":
                    return FlightInputHandler.state.rollTrim;
                case "Wheel":
                    return FlightInputHandler.state.wheelSteerTrim;
                default:
                    return 0f;
            }
        }

        set
        {
            switch (axis)
            {
                case "Pitch":
                    FlightInputHandler.state.pitchTrim = value;
                    break;
                case "Yaw":
                    FlightInputHandler.state.yawTrim = value;
                    break;
                case "Roll":
                    FlightInputHandler.state.rollTrim = value;
                    break;
                case "Wheel":
                    FlightInputHandler.state.wheelSteerTrim = value;
                    break;
            }
        }
    }

    public void toggleShowOnFlightStart()
    {
        showOnFlightStart = !showOnFlightStart;
        elementShowOnFlightStart.buttons[0].toggle(showOnFlightStart);
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (popupWindowID == 0)
            popupWindowID = FSGUIwindowID.getNextID();
        firstPresetLine = 1;
        PopupElement element = new PopupElement(new PopupButton("+", 25f, adjustTrimFromButton));        
        element.buttons.Add(new PopupButton("-", 25f, adjustTrimFromButton));
        element.buttons.Add(new PopupButton("Add", 43f, addPresetLine));
        element.buttons.Add(new PopupButton("Del", 43f, removePresetLine));
        popup = new FSGUIPopup(part, "FStrimAdjustment", moduleID, popupWindowID, new Rect(100f, 200f, 180f, 150f), axis + " Trim Adjustment", element);//FSGUIwindowID.trimAdjustment + moduleID

        
        if (HighLogic.LoadedSceneIsEditor)
        {
            //popup.showMenu = true;
            elementShowOnFlightStart = new PopupElement("Show on Launch?", new PopupButton("Y", "N", 30f, toggleShowOnFlightStart));
            elementShowOnFlightStart.titleSize = 120f;
            popup.sections[0].elements.Add(elementShowOnFlightStart);
            elementShowOnFlightStart.buttons[0].toggle(showOnFlightStart);
            firstPresetLine = 2;
        }
        else
        {            
            if (HighLogic.LoadedSceneIsFlight && showOnFlightStart)
            {
                popup.showMenu = true;
            }
            else
            {
                popup.showMenu = false;
            }
        }

        popup.useInFlight = false;
        popup.useInEditor = true;
        //popup.useInActionEditor = true;

        addPresetLine(presetTrim0);
        if (presetTrim1 != 0f)
            addPresetLine(presetTrim1);
        if (presetTrim2 != 0f)
            addPresetLine(presetTrim2);

        if (HighLogic.LoadedSceneIsEditor)
        {
            popup.windowRect.x = 450f;
        }        
        
        Actions["togglePopupAction"].guiName = "Toggle " + axis + " Popup";
        Actions["usePreset1Action"].guiName = "Use " + axis + " Preset 1";
        Events["togglePopupEvent"].guiName = "Toggle " + axis + " Popup";
        Actions["increaseTrimAction"].guiName = axis + " trim + 1 step";
        Actions["decreaseTrimAction"].guiName = axis + " trim - 1 step";
        Actions["increaseTrimMoreAction"].guiName = "" + axis + " trim +" + MoreTrimActionMultiplier + " steps";
        Actions["decreaseTrimMoreAction"].guiName = "" + axis + " trim -" + MoreTrimActionMultiplier + " steps";
        
    }

    public void OnGUI()
    {
        if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
            return;

        if (HighLogic.LoadedSceneIsFlight) // This gets rid of some mysterious nullrefs when going from the action editor with the GUI showing, to flight.
        {
            if (showGUICountDown > 0f)
            {
                showGUICountDown -= Time.deltaTime;
            }
            else
            {
                popup.useInFlight = true;
            }
        }
        else if (HighLogic.LoadedSceneIsEditor)
        {
            showGUICountDown = 2f;            
        }

        if (popup != null)
        {
            popup.popup();
            if (popup.sections[0].elements.Count > firstPresetLine)
            {
                float.TryParse(popup.sections[0].elements[firstPresetLine].inputText, out presetTrim0);
                if (popup.sections[0].elements.Count > firstPresetLine + 1)
                {
                    float.TryParse(popup.sections[0].elements[firstPresetLine + 1].inputText, out presetTrim1);
                    if (popup.sections[0].elements.Count > firstPresetLine + 2)
                    {
                        float.TryParse(popup.sections[0].elements[firstPresetLine + 2].inputText, out presetTrim2);
                    }
                }
            }
            if (popup.showMenu)
            {
                if(HighLogic.LoadedSceneIsFlight)
                    popup.windowTitle = axis + ": " + Math.Round(trim*100f, 3);
            }
        }
    }
}
