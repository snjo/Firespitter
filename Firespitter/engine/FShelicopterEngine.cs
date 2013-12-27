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
    public float positiveSpeedChangeRate = 0.05f;
    [KSPField]
    public float negativeSpeedChangeRate = 0.02f;
    [KSPField(guiActive = true, guiName = "Req. Climb rate")]
    public float requestedVerticalSpeed = 0f;
    
    //private float upAlignment = 1f;

    [KSPField(guiActive = true, guiName = "Allowed")]
    public float allowedVerticalSpeed = 0f;
    public bool inputProvided = false;
    public bool provideLift = false;

    public override void OnUpdate()
    {
        if (FlightGlobals.ActiveVessel == vessel)
        {
            //upAlignment = Mathf.Sign(Vector3.Dot(thrustTransforms[0].forward, vessel.upAxis));

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

            if (!inputProvided && requestedVerticalSpeed != 0f)
            {
                requestedVerticalSpeed = 0f;
                //requestedVerticalSpeed += verticalSpeedChangeRate * -Mathf.Sign(requestedVerticalSpeed);
                //if (Mathf.Abs(requestedVerticalSpeed) < verticalSpeedChangeRate) requestedVerticalSpeed = 0f;
            }

            requestedVerticalSpeed = Mathf.Clamp(requestedVerticalSpeed, minVerticalSpeed, maxVerticalSpeed);

            //requestedVerticalSpeed *= upAlignment;

            if (provideLift)
            {
                if (requestedVerticalSpeed >= 0f)
                {
                    //allowedVerticalSpeed = Mathf.Clamp(Mathf.Min(requestedVerticalSpeed, maxVerticalSpeed - (float)vessel.verticalSpeed), 0f, maxVerticalSpeed);
                    //requestedThrottle = allowedVerticalSpeed / maxVerticalSpeed;
                    if (vessel.verticalSpeed < requestedVerticalSpeed)
                        requestedThrottle = 1f;
                    else
                        requestedThrottle = 0f;
                }
                else if (requestedVerticalSpeed < 0f && minVerticalSpeed < 0f)
                {
                    //allowedVerticalSpeed = Mathf.Clamp(Mathf.Max(requestedVerticalSpeed, maxVerticalSpeed - (float)vessel.verticalSpeed), minVerticalSpeed, 0f);
                    //requestedThrottle = allowedVerticalSpeed / minVerticalSpeed;
                    if (vessel.verticalSpeed > requestedVerticalSpeed)
                        requestedThrottle = -1f;
                    else
                        requestedThrottle = 0f;
                }
            }
            else
                requestedThrottle = 0f;
         
        }
        else
        {
            requestedVerticalSpeed = 0f;
        }
    }
}