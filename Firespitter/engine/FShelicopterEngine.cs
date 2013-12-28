using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FShelicopterEngine : FSengine
{
    [KSPField]
    public float maxVerticalSpeed = 10f;
    [KSPField]
    public float minVerticalSpeed = -5f;
    [KSPField]
    public float positiveSpeedChangeRate = 0.1f;
    [KSPField]
    public float negativeSpeedChangeRate = 0.05f;
    [KSPField(guiActive = true, guiName = "Req. Climb rate")]
    public float requestedVerticalSpeed = 0f;
    
    private float upAlignment = 1f;

    [KSPField(guiActive = true, guiName = "Allowed")]
    public float allowedVerticalSpeed = 0f;
    public bool inputProvided = false;
    public bool provideLift = false;
    private float verticalSpeed = 0f;
    private float requestThrottleRaw = 0f;

    private FSinputVisualizer inputVisualizer = new FSinputVisualizer();

    [KSPField(isPersistant = true)]
    public bool autoThrottle = true;

    [KSPEvent(guiName = "Normal Throttle", guiActive = true, guiActiveEditor = true)]
    public void toggleThrottleMode()
    {
        autoThrottle = !autoThrottle;
        if (autoThrottle)
            Events["toggleThrottleMode"].guiName = "Normal Throttle";
        else
            Events["toggleThrottleMode"].guiName = "Auto Throttle";
    }

    public override void OnUpdate()
    {
        if (autoThrottle)
        {
            if (FlightGlobals.ActiveVessel == vessel)
            {
                upAlignment = -Mathf.Sign(Vector3.Dot(thrustTransforms[0].forward, vessel.upAxis));
                verticalSpeed = (float)vessel.verticalSpeed * upAlignment;                

                getThrottleInput();

                if (provideLift)
                {
                    if (verticalSpeed > requestedVerticalSpeed)
                            requestThrottleRaw = -1f;
                        else if (verticalSpeed < requestedVerticalSpeed)
                            requestThrottleRaw = 1f;
                        else
                            requestThrottleRaw = 0f;
                }
                else
                    requestThrottleRaw = 0f;

                requestedThrottle = Mathf.Lerp(requestedThrottle, requestThrottleRaw, 0.5f);

            }
            else
            {
                requestedVerticalSpeed = 0f;
            }
            updateFX();
        }
        else
            base.OnUpdate();
    }

    private void getThrottleInput()
    {
        inputProvided = false;
        if (Input.GetKey(GameSettings.THROTTLE_UP.primary))
        {
            requestedVerticalSpeed += positiveSpeedChangeRate;
            provideLift = true;
            inputProvided = true;
        }
        if (Input.GetKey(GameSettings.THROTTLE_DOWN.primary))
        {
            requestedVerticalSpeed -= negativeSpeedChangeRate;
            provideLift = true;
            inputProvided = true;
        }
        if (Input.GetKey(GameSettings.THROTTLE_CUTOFF.primary))
        {
            provideLift = false;
            requestedVerticalSpeed = 0f;
            inputProvided = true;
        }

        if (!inputProvided || !vessel.IsControllable)
        {
            requestedVerticalSpeed = 0f;
        }

        requestedVerticalSpeed = Mathf.Clamp(requestedVerticalSpeed, minVerticalSpeed, maxVerticalSpeed);
    }

    public void OnGUI()
    {
        inputVisualizer.OnGUI();
    }
}