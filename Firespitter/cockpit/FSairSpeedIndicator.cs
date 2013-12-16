using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSairSpeedIndicator : InternalModule
{
    [KSPField]
    public string needleMSName = "needleMS";
    [KSPField]
    public string needleKnotName = "needleKnot";
    [KSPField]
    public float minSpeed = 10f;
    [KSPField]
    public float maxSpeed = 180f;
    [KSPField]
    public float minAngle = -20f;
    [KSPField]
    public float maxAngle = 340f;
    [KSPField]
    public float rotationDirection = 1f;

    private Transform needleMS;
    private Transform needleKnot;
    private float speed = 0f;
    //private float degreeRange;
    private float needleAngle = 0f;
    //private float anglesPerUnit = 0f;
    private float toKnots = 1.94384449f;
    private bool useMS = true;
    private bool useKnot = true;

    public void Start()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            needleMS = base.internalProp.FindModelTransform(needleMSName);
            needleKnot = base.internalProp.FindModelTransform(needleKnotName);
            if (needleMS == null) useMS = false;
            if (needleKnot == null) useKnot = false;
            //float range = maxAngle - minAngle;
            //anglesPerUnit = range / maxSpeed;
        }
    }

    public override void OnUpdate()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            if (vessel != null)
            {
                speed = (float)FlightGlobals.ship_srfSpeed;

                if (useMS)
                {
                    setAngle(needleMS, speed);
                }
                if (useKnot)
                {
                    speed *= toKnots;
                    setAngle(needleKnot, speed);
                }                             
            }
        }
    }

    private void setAngle(Transform needle, float _speed)
    {
        _speed = Mathf.Clamp(_speed, minSpeed, maxSpeed);
        needleAngle = Mathf.Lerp(minAngle, maxAngle, _speed / maxSpeed);
        //needleAngle = Mathf.Clamp((speed * anglesPerUnit) + minAngle, minAngle, maxAngle);
        needle.localRotation = Quaternion.Euler(0f, 0f, needleAngle * rotationDirection);
    }
}
