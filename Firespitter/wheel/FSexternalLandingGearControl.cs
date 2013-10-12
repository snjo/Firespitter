using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSexternalLandingGearControl : PartModule
{
    private ModuleLandingGear gear;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        gear = part.GetComponent<ModuleLandingGear>();
        if (gear != null)
        {
            gear.Events["LowerLandingGear"].guiActiveUnfocused = true;
            gear.Events["LowerLandingGear"].unfocusedRange = 5f;
            gear.Events["RaiseLandingGear"].guiActiveUnfocused = true;
            gear.Events["RaiseLandingGear"].unfocusedRange = 5f;
        }
    }

    [KSPEvent(name = "brakesOn", guiActive = true, active = true, guiName = "Brakes On", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void brakesOnEvent()
    {
        if (gear != null)
        {
            gear.brakesEngaged = true;
        }
    }

    [KSPEvent(name = "brakesOff", guiActive = true, active = true, guiName = "Brakes Off", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void brakesOffEvent()
    {
        if (gear != null)
        {
            gear.brakesEngaged = false;
        }
    }

}
