using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

class FSwing : FSwingBase
{
    #region kspfields    

    [KSPEvent(guiName = "Help", guiActive = true, guiActiveEditor=true, active=true)]
    public override void showHelpEvent()
    {
        helpPopup.showMenu = true;
    }

    [KSPEvent(guiActive = false, guiName = "Toggle Leading Edge")]
    public override void toggleLeadingEdgeEvent()
    {
        setLeadingEdge(!leadingEdgeExtended);
    }

    [KSPAction("Toggle Leading Edge")]
    public override void toggleLeadingEdgeAction(KSPActionParam param)
    {
        toggleLeadingEdgeEvent();
    }

    [KSPAction("Increase Flap")]
    public override void extendFlapAction(KSPActionParam param)
    {
        flapTarget += flapIncrements;
        if (flapTarget > flapMax)
            flapTarget = flapMax;
    }

    [KSPAction("Decrease Flap")]
    public override void retractFlapAction(KSPActionParam param)
    {
        flapTarget -= flapIncrements;
        if (flapTarget < flapMin)
            flapTarget = flapMin;
    }

    #endregion

    public override bool isMaster()
    {
        return true;
    }


    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        if (FARActive)
        {
            foreach (BaseField f in Fields)
            {
                f.guiActive = false;
            }
            foreach (BaseEvent e in Events)
            {
                e.active = false;
                e.guiActive = false;
                e.guiActiveEditor = false;
            }
            foreach (BaseAction a in Actions)
            {
                a.active = false;
            }
            this.enabled = false;
            return;
        }

        if (!useLeadingEdge || autoDeployLeadingEdge)
        {
            Events["toggleLeadingEdgeEvent"].guiActive = false;
        }

        if (affectStockWingModule || !showHelp)
        {            
            Events["showHelpEvent"].guiActive = false;
            Events["showHelpEvent"].guiActiveEditor = false;
        }

        if (customActionName)
        {
            Fields["pitchResponse"].guiName = displayName + " Pitch";            
            Fields["rollResponse"].guiName = displayName + " Roll";            
            Fields["yawResponse"].guiName = displayName + " Yaw";
            Fields["flapResponse"].guiName = displayName + " Flap";            
        }

        if (leadingEdgeToggleName != string.Empty)
        {
            Events["toggleLeadingEdgeEvent"].guiName = leadingEdgeToggleName;
            Actions["toggleLeadingEdgeAction"].guiName = leadingEdgeToggleName;
        }
    }   

    public void OnGUI()
    {
        if (!FARActive)
        {
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
            {
                helpPopup.draw();
            }
        }
    }
}

/*
public class WingAxisSection : PopupSection
{    
    private bool _collapseSection = true;
    public bool collapseSection
    {
        get
        {
            return _collapseSection;
        }
        set
        {
            _collapseSection = value;
            setCollapseState(value);
        }
    }

    public string displayName = "";
    public PopupElement collapseElement;
    public PopupElement responseElement;
    public PopupElement testElement;
    public PopupElement testElement2;
    private float _response;
    public float response
    {
        get
        {
            return _response;
        }
        set
        {
            _response = value;
            if (value != 0f)
                collapseSection = false;
            if  (responseElement != null)
                responseElement.inputText = value.ToString();
        }
    }
    
    public Firespitter.IntVector2 collapseRange = new Firespitter.IntVector2(1, 0);

    public void setCollapseState(bool newState)
    {
        Firespitter.IntVector2 range = collapseRange;
        if (range.y > elements.Count-1 || range.y < range.x)
        {
            range.y = elements.Count-1;
        }

        if (range.x < elements.Count)
        {
            for (int i = range.x; i <= range.y; i++)
            {
                elements[i].showElement = newState;
            }
        }

        if (newState == false)
        {
            //response = 0f;            
        }
    }

    public WingAxisSection(string title, float axisResponse)
    {
        displayName = title;
        response = axisResponse;
        //collapseElement = new PopupElement(new PopupButton(title, 0f));
        //collapseElement.buttons[0].isGUIToggle = true;
        //elements.Add(collapseElement);
        //if (axisResponse == 0f)
        //{
        //    collapseSection = true;
        //    collapseElement.buttons[0].toggleState = false;
        //}
        //else
        //{
        //    collapseSection = false;
        //    collapseElement.buttons[0].toggleState = true;
        //}

        responseElement = new PopupElement(title, response.ToString());
        responseElement.titleSize = FSGUIwindowID.standardRect.width - 115f;
        responseElement.inputSize = 80f;
        elements.Add(responseElement);
    }

    public WingAxisSection()
    {     
    }

    public void updateCollapseStatus()
    {
        collapseSection = collapseElement.buttons[0].toggleState;
    }
}*/
