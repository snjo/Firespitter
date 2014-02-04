using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FShoverThrottle : PartModule //this is for ground level hover.
{
    private ModuleEngines engine = new ModuleEngines();

    [KSPField]
    public float minimumThrust = 0.001f;
    [KSPField]
    public float hoverHeight = 1f;
    [KSPField]
    public float fallOffHeight = 10f;
    [KSPField]
    public float rayDistance = 30.0f;
    [KSPField]
    public float heightIncrements = 0.25f;
    [KSPField]
    public int hoverActive = 1;
    [KSPField]
    public int allowHoverToggle = 1;
    [KSPField]
    public int useAtmospehricNerfModule = 0;
    [KSPField]
    public int useThrottleLock = 1;
    //[KSPField(guiActive = true, guiName = "Hover thrust")]
    private float thrust;
    [KSPField(guiActive = true, guiName = "Altitude")]
    private float radarAltitude;
    //[KSPField(guiActive = true, guiName = "vectorName")]
    //private string vectorName = "default";

    [KSPField(guiActive = false, isPersistant = true)]
    private float defaultMaxThrust;
    //private int directionInt = 0;
    private Vector3 direction = Vector3.forward;
    private bool firstActivation = true;
    private FSpropellerAtmosphericNerf atmosphericNerf = new FSpropellerAtmosphericNerf();
    Transform thrustTransform;

    //[KSPField(guiActive = true, guiName = "partDistance")]
    private float partDistanceFromVessel = 0f;

    [KSPEvent(name = "toggleHover", active = true, guiActive = true, guiName = "Toggle hover")]
    public void toggleHoverEvent()
    {
        toggleHover();
    }

    [KSPAction("Toggle hover")]
    public void toggleHoverAction(KSPActionParam param)
    {
        toggleHover();
    }

    //[KSPEvent(name = "toggleHover", active = true, guiActive = true, guiName = "Toggle hover")]
    public void toggleHover()
    {
        if (hoverActive == 0)
        {
            hoverActive = 1;
            hoverHeight = radarAltitude;
            if (useThrottleLock == 1) engine.throttleLocked = false;
        } else
        {
            hoverActive = 0;
            if (useThrottleLock == 1) engine.throttleLocked = true;
            if (useAtmospehricNerfModule == 1)
                atmosphericNerf.engineModeModifier = 1f;
        }
    }

    [KSPEvent(name = "toggleLock", active = true, guiActive = true, guiName = "Toggle lock throttle")]
    public void toggleLockEvent()
    {
        engine.throttleLocked = !engine.throttleLocked;        
    }

    [KSPEvent(name = "increaseThrust", active = true, guiActive = true, guiName = "Increase thrust")]
    public void increaseThrustEvent()
    {
        defaultMaxThrust += 50;
    }

    [KSPEvent(name = "decreaseThrust", active = true, guiActive = true, guiName = "Decrease thrust")]
    public void decreaseThrustEvent()
    {
        defaultMaxThrust -= 50;
        if (defaultMaxThrust < minimumThrust) defaultMaxThrust = minimumThrust;
    }

    [KSPEvent(name = "increasehoverHeight", active = true, guiActive = true, guiName = "Increase height")]
    public void increaseHeightEvent()
    {
        hoverHeight += heightIncrements;
    }

    [KSPEvent(name = "decreasehoverHeight", active = true, guiActive = true, guiName = "Decrease heght")]
    public void decreaseHeightEvent()
    {
        hoverHeight -= heightIncrements;
        if (hoverHeight < 0f) hoverHeight = 0f;
    }

    /*[KSPEvent(name = "toggleDirection", active = true, guiActive = true, guiName = "toggle direction")]
    public void toggleDirectionEvent()
    {
        directionInt++;
        if (directionInt > 5) directionInt = 0;
        switch (directionInt)
        {
            case 0:
                direction = Vector3.up;
                vectorName = "up";
                break;
            case 1:
                direction = -Vector3.up;
                vectorName = "-up";
                break;
            case 2:
                direction = Vector3.right;
                vectorName = "right";
                break;
            case 3:
                direction = -Vector3.right;
                vectorName = "-right";
                break;
            case 4:
                direction = Vector3.forward;
                vectorName = "forward";
                break;
            case 5:
                direction = -Vector3.forward;
                vectorName = "-forward";
                break;
            default:
                direction = Vector3.forward;
                vectorName = "default";
                break;        
        }
    }*/

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
        defaultMaxThrust = engine.maxThrust;
        if (useAtmospehricNerfModule == 1)
            atmosphericNerf = part.Modules.OfType<FSpropellerAtmosphericNerf>().FirstOrDefault();
        thrustTransform = part.FindModelTransform("thrustTransform");
    }    

    public override void OnUpdate()
    {        
        base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel || !engine.EngineIgnited) return;        

        double pqsAltitude = vessel.pqsAltitude;        
        if (pqsAltitude < 0) pqsAltitude = 0;
        float pqsRadarAltitude = (float)(vessel.altitude - pqsAltitude);
        
        RaycastHit hit;        
        
        if (Physics.Raycast(thrustTransform.position, direction, out hit, rayDistance))
            radarAltitude = hit.distance;
        else
            radarAltitude = rayDistance;

        if (firstActivation)
        {
            partDistanceFromVessel = pqsRadarAltitude - radarAltitude;
            firstActivation = false;
            if (fallOffHeight <= 0) fallOffHeight = 0.01f; //to avoid division by zero
            if (useThrottleLock == 1) engine.throttleLocked = true;
        }

        radarAltitude = Math.Min(radarAltitude, pqsRadarAltitude - partDistanceFromVessel);

        float heightAboveHover = radarAltitude - hoverHeight;
        /*if (heightAboveHover < 0) heightAboveHover = 0f;*/
        thrust = ((fallOffHeight - heightAboveHover) / fallOffHeight);
        if (thrust < minimumThrust) thrust = minimumThrust;
        if (thrust > 1f) thrust = 1f;

        if (vessel.verticalSpeed > 2.5f) thrust = minimumThrust;
        if (vessel.verticalSpeed > 0.2f && heightAboveHover > -0.5f) thrust = minimumThrust;
        if (heightAboveHover < 0 && vessel.verticalSpeed < -0.2f) thrust = 1f;        
        if (heightAboveHover < fallOffHeight * 4 && vessel.verticalSpeed < -2.5f) thrust = 1f;

        if (hoverActive == 1)
        {
            if (useAtmospehricNerfModule == 1)
                atmosphericNerf.engineModeModifier = thrust;
            else
                engine.maxThrust = defaultMaxThrust * thrust;
        }

    }
}
