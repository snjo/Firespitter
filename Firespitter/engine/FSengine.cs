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

    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;

    public bool flameOut = false;
    public bool staged = false;
    public List<Propellant> propellants;
    public FlightCtrlState ctrl;
    public Transform[] thrustTransforms;

    public float finalThrust = 0f;
    public float thrustPerTransform = 0f;

    [KSPField(guiActive = true, guiName = "Requested throttle")]
    public float requestedThrottle = 0f;

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

    public override void OnStart(PartModule.StartState state)
    {
        //part.stackIcon.SetIcon(DefaultIcons.LIQUID_ENGINE);
        part.stagingIcon = "LIQUID_ENGINE";
        thrustTransforms = part.FindModelTransforms(thrustTransformName);
    }

    public override void OnFixedUpdate()
    {        
        if (engineIgnited)
        {
            finalThrust = maxThrust * requestedThrottle;
            thrustPerTransform = finalThrust / thrustTransforms.Length;
            for (int i = 0; i < thrustTransforms.Length; i++)
            {
                rigidbody.AddForceAtPosition(-thrustTransforms[i].forward * thrustPerTransform, thrustTransforms[i].position);                
            }                        
        }
    }

    public override void OnUpdate()
    {
        requestedThrottle = vessel.ctrlState.mainThrottle;
    }

    public override void OnActive()
    {
        Activate();
    }
}
