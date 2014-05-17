using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

class FSengineHandCrank : PartModule
{
    private ModuleEngines engine;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        engine = part.GetComponent<ModuleEngines>();
        engine.Events["Activate"].guiActiveUnfocused = true;
        engine.Events["Activate"].unfocusedRange = 5f;
        engine.Events["Shutdown"].guiActiveUnfocused = true;
        engine.Events["Shutdown"].unfocusedRange = 5f;
    }

    /*
    [KSPEvent(name = "ignitionOn", guiActive = false, active = true, guiName = "Ignition On", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void ignitionOnEvent()
    {
        if (engine != null)
        {
            //gameObject.active = true;
            //engine.staged = true;
            engine.EngineIgnited = true;            
        }
    }

    [KSPEvent(name = "ignitionOff", guiActive = false, active = true, guiName = "Ignition Off", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void ignitionOffEvent()
    {
        if (engine != null)
        {
            //engine.staged = true;
            engine.EngineIgnited = false;
        }
    }*/
}
