using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSrudder : PartModule
{
    [KSPField]
	public string animatedPart = "obj_ctrlSrf";
    [KSPField]
	public float range = 15f;
    [KSPField]
	public float power = 0.5f;
    [KSPField]
	public Vector3 pivotAxis = new Vector3(1f,0f,0f);
    [KSPField]
	public Vector3 useInputAxis = new Vector3(0f,0f,1f); //pitch, roll or yaw
    [KSPField]
    public string forceAxis = "left";
    [KSPField]
    public int requiresWaterContact = 1;
    [KSPField]
    public float speedModifierMin = 0.5f; 
    [KSPField]
    public float speedModifierMax = 5.0f;
    [KSPField]
    public int debugMode = 0;

    private Transform rudderTransform;
    private Transform rudderDefaultTransform;
    private FlightCtrlState ctrl;
    private float input = 0;
    private bool firstRun = true;

    [KSPEvent(name = "IncreasePower", active = true, guiActive = false, guiName = "Increase Power")]
    public void increasePower()
    {
        power += 0.01f;        
        Debug.Log("FSrudder power: " + power);
    }

    [KSPEvent(name = "DecreasePower", active = true, guiActive = false, guiName = "Decrease Power")]
    public void decreasePower()
    {
        power -= 0.01f;
        if (power < 0f) power = 0f;
        Debug.Log("FSrudder power: " + power);
    }

    [KSPEvent(name = "IncreaseRange", active = true, guiActive = false, guiName = "Increase Range")]
    public void increaseRange()
    {
        range += 1f;
        if (range > 60f) range = 60f;
        Debug.Log("FSrudder range: " + range);
    }

    [KSPEvent(name = "DecreaseRange", active = true, guiActive = false, guiName = "Decrease Range")]
    public void decreaseRange()
    {
        range -= 1f;
        if (range < 0f) range = 0f;
        Debug.Log("FSrudder range: " + range);
    }

    [KSPEvent(name = "CycleForceAxis", active = true, guiActive = false, guiName = "Cycle Force Axis")]
    public void cycleForceAxis()
    {
        switch (forceAxis)
        {
            case "left":
                forceAxis = "right";
                break;
            case "right":
                forceAxis = "up";
                break;
            case "up":
                forceAxis = "down";
                break;
            case "down":
                forceAxis = "forward";
                break;
            case "forward":
                forceAxis = "back";
                break;
            case "back":
                forceAxis = "left";
                break;
        }
        
        Debug.Log("FSrudder forceAxis: " + forceAxis);
    }

    [KSPEvent(name = "ToggleRequiresWaterContact", active = true, guiActive = false, guiName = "Toggle Req. Water Contact")]
    public void toggleRequiresWaterContact()
    {
        if (requiresWaterContact == 0)
            requiresWaterContact = 1;
        else requiresWaterContact = 0;
        Debug.Log("FSrudder req. water contact: " + requiresWaterContact);
    }

    [KSPAction("Toggle Debug Mode")]
    public void toggleDebugMode(KSPActionParam param)
    {
        if (debugMode == 0)
        {
            debugMode = 1;
            setDebugMode(true);
        }
        else
        {
            debugMode = 0;
            setDebugMode(false);
        }
    }

    private void setDebugMode(bool newState)
    {
        Events["increasePower"].guiActive = newState;
        Events["decreasePower"].guiActive = newState;
        Events["increaseRange"].guiActive = newState;
        Events["decreaseRange"].guiActive = newState;
        Events["increasePower"].guiActive = newState;
        Events["cycleForceAxis"].guiActive = newState;
        Events["toggleRequiresWaterContact"].guiActive = newState;
    }


    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        rudderTransform = part.FindModelTransform(animatedPart);
        if (rudderTransform != null)
        {
            rudderDefaultTransform = new GameObject().transform;
            rudderDefaultTransform.localRotation = rudderTransform.localRotation;
        }        
    }

    public void FixedUpdate()
    {
        //base.OnFixedUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;

        if (firstRun)
        {
            if (debugMode == 1)
            {
                setDebugMode(true);
            }
            firstRun = false;
        }

        

        ctrl = vessel.ctrlState;

        Vector3 ctrlInputVector = new Vector3(ctrl.pitch, ctrl.roll, ctrl.yaw);
        Vector3 inputVector = new Vector3(ctrlInputVector.x * useInputAxis.x, ctrlInputVector.y * useInputAxis.y, ctrlInputVector.z * useInputAxis.z);
        if (inputVector.x != 0) input = inputVector.x;
            else if (inputVector.y != 0) input = inputVector.y;
                else input = inputVector.z;

        if (input != 0f)
        {
            if (part.WaterContact || requiresWaterContact == 0)
            {
                float forcetoAdd = input * power * range;
                float speedModifier = (float)FlightGlobals.ship_srfSpeed;
                speedModifier = Mathf.Clamp(speedModifier, speedModifierMin, speedModifierMax);
                forcetoAdd *= speedModifier;

                Vector3 transformDirection = new Vector3();
                switch (forceAxis){
                    case "right":
                        transformDirection = transform.right;
                        break;                        
                    case "left":                        
                        transformDirection = -transform.right;
                        break;                        
                    case "up":
                        transformDirection = transform.up;
                        break;
                    case "down":
                        transformDirection = -transform.up;
                        break;
                    case "forward":
                        transformDirection = transform.forward;
                        break;
                    case "back":
                        transformDirection = -transform.forward;
                        break;
                }                
                base.rigidbody.AddForceAtPosition(transformDirection * forcetoAdd, base.transform.position);

                if (rudderTransform != null)
                {
                    rudderTransform.localRotation = rudderDefaultTransform.localRotation;
                    rudderTransform.Rotate(pivotAxis * input * range);
                }
            }
        }
        else
        {
            if (rudderTransform != null)
            {
                rudderTransform.localRotation = rudderDefaultTransform.localRotation;
            }
        }
    }
}

