using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSengine : PartModule
{    
    [KSPField(guiName = "Max Thrust", guiActiveEditor=true)]
    public float maxThrust = 100f;
    [KSPField]
    public string atmosphericThrust;
    [KSPField]
    public string velocityLimit;
    [KSPField]
    public string fuelConsumption = "0,0;1,0.01";
    [KSPField]
    public string resources = "LiquidFuel,1;IntakeAir,15";
    [KSPField]
    public string thrustTransformName = "thrustTransform";
    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max throttle", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.01f)]
    public float maxThrottle = 100f;

    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;    

    public bool flameOut = false;
    public bool staged = false;
    //public List<Propellant> propellants;    
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

    [KSPField(guiActive = true, guiName = "Power Production")]
    public float powerProduction = 0.1f;
    [KSPField]
    public float engineBreak = 0.1f;
    [KSPField(guiActive = true, guiName = "Stored Momentum")]
    public float momentum = 0f;
    [KSPField(guiActive = true, guiName = "Power Drain")]
    public float powerDrain = 0.05f;

    private FloatCurve velocityCurve = new FloatCurve();
    private FloatCurve fuelConsumptionCurve = new FloatCurve();
    private List<FSresource> resourceList = new List<FSresource>();

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

    private void fillResourceList(string resourceString)
    {
        string[] keyString = resourceString.Split(';');
        for (int i = 0; i < keyString.Length; i++)
        {
            string[] valueString = keyString[i].Split(',');
            if (valueString.Length > 0)
            {
                try
                {
                    resourceList.Add(new FSresource(valueString[0], float.Parse(valueString[1])));
                    Debug.Log("FSengine: Added resource " + valueString[0] + ", ratio " + valueString[1]);
                }
                catch
                {
                    Debug.Log("FSengine: could not add resource to list: " + valueString[0]);
                }
            }
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        //part.stackIcon.SetIcon(DefaultIcons.LIQUID_ENGINE);
        part.stagingIcon = "LIQUID_ENGINE";
        thrustTransforms = part.FindModelTransforms(thrustTransformName);

        velocityCurve = Firespitter.Tools.stringToFloatCurve(velocityLimit);
        fuelConsumptionCurve = Firespitter.Tools.stringToFloatCurve(fuelConsumption);
        fillResourceList(resources);

        Debug.Log("FSengine: " + velocityCurve.ToString());
    }

    public override void OnFixedUpdate()
    {
        if (engineIgnited && !flameOut)
            momentum += powerProduction*TimeWarp.deltaTime;
        else
            momentum -= engineBreak * TimeWarp.deltaTime;
      
        momentum -= Mathf.Abs(requestedThrottle) * powerDrain * TimeWarp.deltaTime;
        momentum = Mathf.Clamp(momentum, 0f, 2f);
        float useMomentum = Mathf.Clamp(momentum, 0f, 1f);

        //if (engineIgnited)
        //{            
        finalThrust = useMomentum * maxThrust * Mathf.Clamp(requestedThrottle, -maxThottleNormalized, maxThottleNormalized) * velocityCurve.Evaluate(part.rigidbody.velocity.magnitude);
        thrustPerTransform = finalThrust / thrustTransforms.Length;
        for (int i = 0; i < thrustTransforms.Length; i++)
        {
            rigidbody.AddForceAtPosition(-thrustTransforms[i].forward * thrustPerTransform, thrustTransforms[i].position);                
        }
        finalThrustNormalized = finalThrust / maxThrust;
        smoothFxThrust = Mathf.Lerp(smoothFxThrust, finalThrustNormalized, smoothFXSpeed);

        //burn fuel
        for (int i = 0; i < resourceList.Count; i++)
        {
            part.RequestResource(resourceList[i].name, fuelConsumptionCurve.Evaluate(finalThrustNormalized) * maxThrust * resourceList[i].ratio * TimeWarp.deltaTime);
        }                    
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

public class FSresource
{
    //public PartResource resource;
    public string name;
    public float ratio;

    public FSresource(string _name, float _ratio)
    {
        name = _name;
        ratio = _ratio;
    }
}
