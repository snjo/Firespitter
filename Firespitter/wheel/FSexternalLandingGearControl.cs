
using ModuleWheels;

public class FSexternalLandingGearControl : PartModule
{
    private ModuleWheelDeployment gear;
    private ModuleWheelBrakes brakes;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        gear = part.GetComponent<ModuleWheelDeployment>();
        brakes = part.GetComponent<ModuleWheelBrakes>();
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
        if (gear != null && brakes != null)
        {
            brakes.BrakeAction(new KSPActionParam(KSPActionGroup.Brakes, KSPActionType.Activate));
        }
    }

    [KSPEvent(name = "brakesOff", guiActive = true, active = true, guiName = "Brakes Off", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void brakesOffEvent()
    {
        if (gear != null && brakes != null)
        {
            brakes.BrakeAction(new KSPActionParam(KSPActionGroup.Brakes, KSPActionType.Deactivate));
        }
    }

}
