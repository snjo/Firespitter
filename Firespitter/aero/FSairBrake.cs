using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;


public class FSairBrake : PartModule // Inspired by Vlad Just Vlad's airbrake plugin from here http://forum.kerbalspaceprogram.com/showthread.php/13898-Airbrake?
{
    [KSPField]
    public float deployedDrag = 75f;
    [KSPField]
    public float deployedAngle = 30f;
    [KSPField]
    public float stepAngle = 10f; //the amount to increase the brakes when using action group buttons
    [KSPField]
    public string targetPartObject = "airBrake";    

    [KSPField(guiActive=false, isPersistant = true)]
    public float targetAngle = 0f;

    private float normalMinDrag;
    private float normalMaxDrag;
    private float currentAngle = 0f;    
    private float animationIncrement = 1f;
    private bool firstActivation = true;
    Transform partTransform = new GameObject().transform;
    Transform defaultRotation = new GameObject().transform;
    Transform deployedRotation = new GameObject().transform;       

    [KSPField(guiActive = true, guiName = "drag", isPersistant = false)]
    public string currentDrag;

    [KSPEvent(name = "toggleAirBrake", active = true, guiActive = true, guiName = "toggle Air Brake")]
    public void toggleAltInputMode()
    {
        toggleAngle();
    }

    [KSPAction("toggle Air Brake")]
    public void toggleAirBrakeAction(KSPActionParam param)
    {
        toggleAngle();
    }

    [KSPAction("raise Air Brake")]
    public void raiseAirBrakeAction(KSPActionParam param)
    {
        targetAngle += stepAngle;
        if (targetAngle > deployedAngle) targetAngle = deployedAngle;
    }

    [KSPAction("lower Air Brake")]
    public void lowerAirBrakeAction(KSPActionParam param)
    {
        targetAngle -= stepAngle;
        if (targetAngle < 0) targetAngle = 0;
    }

    private void toggleAngle()
    {
        if (targetAngle > 0) targetAngle = 0;
        else
            if (targetAngle <= deployedAngle) targetAngle = deployedAngle;
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (firstActivation)
        {
            partTransform = part.FindModelTransform(targetPartObject);
            normalMinDrag = part.minimum_drag;
            normalMaxDrag = part.maximum_drag;
            defaultRotation.rotation = partTransform.transform.localRotation;
            deployedRotation.rotation = defaultRotation.rotation;
            deployedRotation.Rotate(new Vector3(-deployedAngle, 0, 0));
            firstActivation = false;
        }
    }

    public void FixedUpdate()
    {
        base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;

        float angleChange = targetAngle - currentAngle;

        if (angleChange > animationIncrement)
        {
            angleChange = animationIncrement;
        }
        else if (angleChange < -animationIncrement)
        {
            angleChange = -animationIncrement;
        }

        currentAngle += angleChange;

        partTransform.transform.Rotate(-angleChange, 0, 0);

        part.maximum_drag = deployedDrag * (currentAngle / deployedAngle);
        part.minimum_drag = part.maximum_drag;

        currentDrag = "" + Math.Ceiling(part.maximum_drag);
    }
}

