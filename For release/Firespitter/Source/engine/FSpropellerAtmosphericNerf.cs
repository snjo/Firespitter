using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSpropellerAtmosphericNerf : PartModule
{
    [KSPField]
    public int useHover = 0;
    [KSPField]
    public float thrustModifier = 1.1f; // the current atmosphere is multiplied by this number to get the engine effectiveness multiplier.
    //1 means you start losing power from 0m, linearly till you reach 0 atm.
    //1.1 means you stay constant up till you get to ca. 0.9 atm. At 0.5 atm you will have 0.55 thrust
    //2 means you will have full thrust until you reach 0.5 atm.
    [KSPField]
    public float hoverThrottle = 0.5f; // used by the Hover Throttle Action
    [KSPField]
    public float cargoThrottle = 1.5f; // used by the Cargo Throttle Action
    //[KSPField(guiActive = false, guiName = "modeModifier")]
    public float engineModeModifier = 1f; // allows for hover, normal and cargo throttle presets in copterThrottle.cs.
    public float steeringModifier = 1f; // used by the VTOL roll steering mode

    [KSPField]
    public bool disableAtmosphericNerf = false;

    private ModuleEngines engine = new ModuleEngines();
    private float fullThrottle;

    [KSPAction("Hover Throttle")]
    public void hoverThrottleAction(KSPActionParam param)
    {
        engineModeModifier = hoverThrottle;
    }
    [KSPAction("Normal Throttle")]
    public void normalThrottleAction(KSPActionParam param)
    {
        engineModeModifier = 1;
    }
    [KSPAction("Cargo Throttle")]
    public void cargoThrottleAction(KSPActionParam param)
    {
        engineModeModifier = cargoThrottle;
    }

    public override void OnStart(PartModule.StartState state)
    {            
        base.OnStart(state);
        engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
        fullThrottle = engine.maxThrust;
    }

    public override void OnUpdate() {
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        float atmosphericModifier = ((float)part.staticPressureAtm * thrustModifier);
        if ((atmosphericModifier > 1f && thrustModifier > 1f) || disableAtmosphericNerf) atmosphericModifier = 1f; // not setting modifier to 1 at thrustModifier 1 or lower allows for engine that are better than normal in atmospeheres above 1
        float newThrust = fullThrottle * atmosphericModifier * engineModeModifier * steeringModifier;
        if (newThrust <= 0) newThrust = 0.001f;
        engine.maxThrust = newThrust;
        
    }
}
