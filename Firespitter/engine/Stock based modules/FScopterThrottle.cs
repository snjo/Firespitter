using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This is a rewrite of DYJ's baiscPropSpinner from Damned Aerospace, for use in constatnly spinning helicopter rotors

public class FScopterThrottle : PartModule
{
    [KSPField]
    public string rotorparent = "";
    [KSPField]
    public float rotationSpeed = -700f; // in RPM
    [KSPField]
    public int spinUpTime = 5;
    [KSPField]
    public float hoverThrottle = 0.5f;
    [KSPField]
    public float cargoThrottle = 1.5f;
    [KSPField]
    public int usesDeployAnimation = 0;
    [KSPField]
    public float parkedBladeRotation = 0f;
    [KSPField]
    public int useHoverFunction = 1;
    [KSPField]
    public float heightIncrements = 10f;
    [KSPField(guiActive = true, guiName = "Hover Height")]
    public float hoverHeight = 0f;

    [KSPField]
    public bool showDebugGUI = false;

    private bool hoverActive = false;
    //private float currentSpeed;

    //[KSPField(guiActive = false, guiName = "partDistance", isPersistant = true)]
    //private float partDistanceFromVessel = 0;
    //private bool firstActivation = true;
    //private float radarAltitude = 0f;

    private FSpropellerAtmosphericNerf atmosphericNerf = new FSpropellerAtmosphericNerf();
    private FSanimateGeneric deployAnimation = new FSanimateGeneric();
    private ModuleEngines engine = new ModuleEngines();
    private Transform RotorParent;
    private bool spinRotorObject = true;

    //private float desiredVerticalSpeed = 0f;

    /*[KSPAction("Hover Throttle")]
    public void hoverThrottleAction(KSPActionParam param)
    {
        atmosphericNerf.engineModeModifier = hoverThrottle;            
    }
    [KSPAction("Normal Throttle")]
    public void normalThrottleAction(KSPActionParam param)
    {
        atmosphericNerf.engineModeModifier = 1;            
    }
    [KSPAction("Cargo Throttle")]
    public void cargoThrottleAction(KSPActionParam param)
    {
        atmosphericNerf.engineModeModifier = cargoThrottle;            
    }*/

#region hover function

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

    public void toggleHover()
    {
        if (!hoverActive)
        {
            hoverActive = true;
            hoverHeight = radarAltitude();            
        } else
        {
            hoverActive = false;
            hoverHeight = 0;
            atmosphericNerf.engineModeModifier = 1f;
        }
    }

    [KSPEvent(name = "increasehoverHeight", active = true, guiActive = true, guiName = "Increase height")]
    public void increaseHeightEvent()
    {
        hoverHeight += heightIncrements;
    }

    [KSPEvent(name = "decreasehoverHeight", active = true, guiActive = true, guiName = "Decrease height")]
    public void decreaseHeightEvent()
    {
        hoverHeight -= heightIncrements;
        //if (hoverHeight < 0f) hoverHeight = 0f;
    }

    [KSPAction("Increase Hover Height")]
    public void increaseHeightAction(KSPActionParam param)
    {
        increaseHeightEvent();
    }

    [KSPAction("Decrease Hover Height")]
    public void decreaseHeightAction(KSPActionParam param)
    {
        decreaseHeightEvent();
    }

    private float getHoverThrottle(float radarAlt, float verticalSpeed)
    {
        float minimumThrust = 0.001f;            
        float fallOffHeight = 10f;    
        float thrust = 1f;

        Vector3 direction = Vector3.forward;
        
        float heightAboveHover = radarAlt - hoverHeight;        
        thrust = ((fallOffHeight - heightAboveHover) / fallOffHeight);
        if (thrust < minimumThrust) thrust = minimumThrust;
        if (thrust > 1f) thrust = 1f;

        if (verticalSpeed > 2.5f) thrust = minimumThrust;
        if (verticalSpeed > 0.2f && heightAboveHover > -0.5f) thrust = minimumThrust;
        if (heightAboveHover < 0 && vessel.verticalSpeed < -0.2f) thrust = 1f;        
        if (heightAboveHover < fallOffHeight * 4 && vessel.verticalSpeed < -2.5f) thrust = 1f;       
    
        return thrust;
    }

    public float radarAltitude()
    {
        double pqsAltitude = vessel.pqsAltitude;
        if (pqsAltitude < 0) pqsAltitude = 0;
        return (float)(vessel.altitude - pqsAltitude); 
    }

#endregion

    private int timeCount = 0;

    public override void OnStart(PartModule.StartState state)
    {            
        base.OnStart(state);
        if (usesDeployAnimation == 1)
        {
            deployAnimation = part.Modules.OfType<FSanimateGeneric>().FirstOrDefault();
        }
        engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
        atmosphericNerf = part.Modules.OfType<FSpropellerAtmosphericNerf>().FirstOrDefault();
        if (rotorparent != "")
        {
            RotorParent = part.FindModelTransform(rotorparent);
            spinRotorObject = true;
        }
        else
        {
            spinRotorObject = false;
        }
    }

    public override void OnUpdate()
    {

        if (!HighLogic.LoadedSceneIsFlight) return; // || !vessel.isActiveVessel           

        
            
        if (usesDeployAnimation == 1)
        {
             if (deployAnimation.animTime < 1)
                engine.EngineIgnited = false;
        }

        //double pqsAltitude = vessel.pqsAltitude;
        //if (pqsAltitude < 0) pqsAltitude = 0;
        //radarAltitude = (float)(vessel.altitude - pqsAltitude);        

        /*if (firstActivation)
        {            
            partDistanceFromVessel = pqsRadarAltitude - radarAltitude;
            firstActivation = false;            
        }*/

        bool engineActive = engine && engine.getIgnitionState && !engine.getFlameoutState;

        if (vessel != null)
        {
            
            if ((engineActive && !engine.getFlameoutState) && timeCount < 1000)
            {
                timeCount += spinUpTime;
            }
            else if ((engine.getFlameoutState || !engineActive) && timeCount > 0)
            {
                timeCount -= spinUpTime;
            }

            if (timeCount < 0) timeCount = 0; // in case people give the spinUpTime in an unexpected way
                
            float targetSpeed = ((rotationSpeed * 6) * TimeWarp.deltaTime * ((float)timeCount/1000));
            //if (engine.getFlameoutState)
            //    targetSpeed = 0f;
            if (RotorParent != null && spinRotorObject)
            {
                if (targetSpeed != 0f)
                    RotorParent.transform.Rotate(Vector3.forward * targetSpeed);

                if (usesDeployAnimation == 1 && engine.EngineIgnited == false)
                {
                    if (RotorParent.transform.localEulerAngles.y > parkedBladeRotation + 1.1f || RotorParent.transform.localEulerAngles.y < parkedBladeRotation - 1.1f)
                    {
                        RotorParent.transform.localEulerAngles += new Vector3(0, 1, 0);
                        //Debug.Log("resetting blades");                        
                    }
                }
            }

            // ----hover
            if (hoverActive)
                atmosphericNerf.engineModeModifier = getHoverThrottle(radarAltitude(), (float)vessel.verticalSpeed);            
        }
    }

    public void OnGUI()
    {
        if (showDebugGUI)
        {
            GUI.Label(new Rect(100f, 200f, 200f, 60f), "r: " + RotorParent.transform.localEulerAngles);
        }
    }
}
