using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSengineMenuCleaner : PartModule
{

    private ModuleEngines engine;
    private ModuleResourceIntake intake;
    //private bool firstRun = true;

    //[KSPEvent(name = "cleanMenus", active = true, guiActive = true, guiName = "Clean up menu")]
    //public void cleanMenuEvent()
    //{
    //    toggleMenuItems(false);
    //}


    public void toggleMenuItems(bool newState)
    {
        if (engine != null)
        {
            engine.Fields["fuelFlowGui"].guiActive = newState;
            engine.Fields["realIsp"].guiActive = newState;
            engine.Fields["statusL2"].guiActive = newState;
            engine.Fields["statusL2"].guiActive = newState;
        }
        if (intake != null)
        {
            intake.Fields["airFlow"].guiActive = newState;
            intake.Fields["intakeDrag"].guiActive = newState;
            intake.Fields["status"].guiActive = newState;
            intake.Fields["airSpeedGui"].guiActive = newState;
            intake.Events["Deactivate"].guiActive = newState;
            intake.Events["Activate"].guiActive = newState;
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
        intake = part.Modules.OfType<ModuleResourceIntake>().FirstOrDefault();
        toggleMenuItems(false);
    }
}
