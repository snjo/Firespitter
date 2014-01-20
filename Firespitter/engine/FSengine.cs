using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSengine : PartModule
{    
    [KSPField]
    public float maxThrust = 100f;
    [KSPField]
    public FloatCurve atmosphericThrust;
    [KSPField]
    public FloatCurve velocityCurve;
    [KSPField]
    public FloatCurve fuelConsumption;
    [KSPField]
    public string thrustTransformName = "thrustTransform";
    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max throttle", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.01f)]
    public float maxThrottle = 100f;

    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;    

    public bool flameOut = false;
    public bool staged = false;
    public List<Propellant> propellants;
    public FlightCtrlState ctrl;
    public Transform[] thrustTransforms;

    public float finalThrust = 0f;
    public float finalThrustNormalized = 0f;
    public float thrustPerTransform = 0f;

    [KSPField(guiActive = true, guiName = "Requested throttle")]
    public float requestedThrottle = 0f;
    public float smoothFxThrust = 0f;
    [KSPField]
    public float smoothFXSpeed = 0.1f;

    [KSPField]
    public float powerProduction = 0.01f;
    //[KSPField]
    //public float fallingPowerProduction = 0.01f;
    [KSPField(guiActive = true, guiName = "Stored Momentum")]
    public float momentum = 0f;
    [KSPField(guiActive = true, guiName = "Power Drain"), UI_FloatRange(minValue = 0f, maxValue = 0.02f, stepIncrement = 0.0001f)]
    public float powerDrain = 0.01f;

    [KSPEvent(guiName = "Activate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void Activate()
    {
        engineIgnited = true;
        staged = true;
        Debug.Log("igniting engine");
    }

    [KSPEvent(guiName = "Deactivate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void Deactivate()
    {
        engineIgnited = false;
    }

    public void updateFX()
    {
        if (engineIgnited && !flameOut)
        {
            part.Effect("running", Mathf.Clamp(smoothFxThrust, 0.01f, 1f));
        }
        else
            part.Effect("running", 0f);
    }

    public float maxThottleNormalized
    {
        get
        {
            return maxThrottle / 100f;
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        //part.stackIcon.SetIcon(DefaultIcons.LIQUID_ENGINE);
        part.stagingIcon = "LIQUID_ENGINE";
        thrustTransforms = part.FindModelTransforms(thrustTransformName);
    }

    public override void OnFixedUpdate()
    {
        if (engineIgnited && !flameOut)
            momentum += powerProduction;
        else
            momentum -= 0.0001f;
        //if (vessel.verticalSpeed < 1f)
        //    momentum -= (float)vessel.verticalSpeed * fallingPowerProduction;        
        momentum -= Mathf.Abs(requestedThrottle) * powerDrain;
        momentum = Mathf.Clamp(momentum, 0f, 2f);
        float useMomentum = Mathf.Clamp(momentum, 0f, 1f);

        //if (engineIgnited)
        //{            
            finalThrust = momentum * maxThrust * Mathf.Clamp(requestedThrottle, -maxThottleNormalized, maxThottleNormalized);
            thrustPerTransform = finalThrust / thrustTransforms.Length;
            for (int i = 0; i < thrustTransforms.Length; i++)
            {
                rigidbody.AddForceAtPosition(-thrustTransforms[i].forward * thrustPerTransform, thrustTransforms[i].position);                
            }
            finalThrustNormalized = finalThrust / maxThrust;
            smoothFxThrust = Mathf.Lerp(smoothFxThrust, finalThrustNormalized, smoothFXSpeed);
        //}
    }

    public override void OnUpdate()
    {
        maxThrottle = Mathf.Round(maxThrottle);

        if (HighLogic.LoadedSceneIsFlight)
        {
            requestedThrottle = vessel.ctrlState.mainThrottle;
            updateFX();
        }
    }

    public override void OnActive()
    {
        Activate();
    }
}
