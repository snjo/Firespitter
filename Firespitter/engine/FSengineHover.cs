using System.Linq;
using UnityEngine;

public class FSengineHover : PartModule
{
    [KSPField]
    public float verticalSpeedIncrements = 1f;
    [KSPField(guiActive = true, guiName = "Vertical Speed")]
    public float verticalSpeed = 0f;
    [KSPField(guiActive = true, guiName = "Hover Active")]
    public bool hoverActive = false;
    [KSPField]
    public float thrustSmooth = 0.1f;
    [KSPField(isPersistant = true)]
    public float maxThrust = 0f;
    [KSPField(isPersistant = true)]
    public bool maxThrustFetched = false;
    [KSPField]
    public bool useHardCodedButtons = true;


    private ModuleEngines engine;
    private float currentThrustNormalized = 0f;
    private float targetThrustNormalized = 0f;
    private float minThrust = 0f;

    [KSPEvent(guiName = "Toggle Hover")]
    public void toggleHoverEvent()
    {
        if (engine != null)
        {
            hoverActive = !hoverActive;
            verticalSpeed = 0f;
            if (hoverActive)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("Hover On", 1f, ScreenMessageStyle.UPPER_CENTER));
            }
            else
            {
                engine.maxThrust = maxThrust;
                ScreenMessages.PostScreenMessage(new ScreenMessage("Hover Off", 1f, ScreenMessageStyle.UPPER_CENTER));
            }


        }
    }

    [KSPAction("Toggle Hover")]
    public void toggleHoverAction(KSPActionParam param)
    {
        toggleHoverEvent();
    }

    [KSPAction("Increase vSpeed")]
    public void increaseVerticalSpeed(KSPActionParam param)
    {
        verticalSpeed += verticalSpeedIncrements;
        printSpeed();
    }

    [KSPAction("Decrease vSpeed")]
    public void decreaseVerticalSpeed(KSPActionParam param)
    {
        verticalSpeed -= verticalSpeedIncrements;
        printSpeed();
    }

    public void printSpeed()
    {
        ScreenMessages.PostScreenMessage(new ScreenMessage("Hover Climb Rate: " + verticalSpeed, 1f, ScreenMessageStyle.UPPER_CENTER));
    }

    public override void OnStart(PartModule.StartState state)
    {
        Debug.Log("KTengineHover OnStart");
        base.OnStart(state);
        if (HighLogic.LoadedSceneIsFlight)
        {
            engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
            if (engine != null)
            {
                if (maxThrustFetched && maxThrust > 0f)
                {
                    engine.maxThrust = maxThrust;
                }
                else
                {
                    maxThrust = engine.maxThrust;
                    maxThrustFetched = true;
                }
                minThrust = engine.minThrust;
            }
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (HighLogic.LoadedSceneIsFlight && vessel == FlightGlobals.ActiveVessel)
        {
            if (hoverActive)
            {
                if (vessel.verticalSpeed >= verticalSpeed)
                    targetThrustNormalized = 0f;
                else if (vessel.verticalSpeed < verticalSpeed)
                    targetThrustNormalized = 1f;

                currentThrustNormalized = Mathf.Lerp(currentThrustNormalized, targetThrustNormalized, thrustSmooth);

                float newThrust = maxThrust * currentThrustNormalized;
                if (newThrust <= minThrust) newThrust = minThrust + 0.001f;
                if (engine != null)
                {
                    //Debug.Log("newThrust is " + newThrust);
                    engine.maxThrust = newThrust;
                }
                else
                {
                    Debug.Log("engine is null");
                }
            }
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (HighLogic.LoadedSceneIsFlight && vessel == FlightGlobals.ActiveVessel)
        {
            if (useHardCodedButtons)
            {
                if (Input.GetKeyDown(KeyCode.PageUp))
                {
                    verticalSpeed += verticalSpeedIncrements;
                    printSpeed();
                }
                if (Input.GetKeyDown(KeyCode.PageDown))
                {
                    verticalSpeed -= verticalSpeedIncrements;
                    printSpeed();
                }
                if (Input.GetKeyDown(KeyCode.Home))
                {
                    verticalSpeed = 0f;
                    printSpeed();
                }
                if (Input.GetKeyDown(KeyCode.End))
                {
                    toggleHoverEvent();
                }
            }
        }
    }
}