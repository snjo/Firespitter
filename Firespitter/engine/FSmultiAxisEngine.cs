using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FSmultiAxisEngine : PartModule // This if for the swamp engine, and other custom gimballing engines
{
    [KSPField]
    public string pitchObject = "none";
    [KSPField]
    public string rollObject = "none";
    [KSPField]
    public string yawObject = "none";
    [KSPField]
    public Vector3 axisMultiplier = new Vector3(1, 1, 1);
    [KSPField]
    public Vector3 pitchDefaultRotation = new Vector3(0, 0, 0);
    [KSPField]
    public Vector3 rollDefaultRotation = new Vector3(0, 0, 0);
    [KSPField]
    public Vector3 yawDefaultRotation = new Vector3(0, 0, 0);
    [KSPField(guiActive = false, guiName = "Pitch inverted", isPersistant = true)]
    public bool invertPitch;
    [KSPField(guiActive = false, guiName = "Roll inverted", isPersistant = true)]
    public bool invertRoll;
    [KSPField(guiActive = false, guiName = "Yaw inverted", isPersistant = true)]
    public bool invertYaw;
    private bool usePitch;
    private bool useRoll;
    private bool useYaw;
    private Transform pitchTransform = new GameObject().transform;
    private Transform rollTransform = new GameObject().transform;
    private Transform yawTransform = new GameObject().transform;

    [KSPEvent(name = "invertPitch", active = true, guiActive = true, guiName = "Invert pitch")]
    public void toggleInvertPitch()
    {
        invertPitch = !invertPitch;
    }

    [KSPEvent(name = "invertRoll", active = true, guiActive = true, guiName = "Invert roll")]
    public void toggleInvertRoll()
    {
        invertRoll = !invertRoll;
    }

    [KSPEvent(name = "invertYaw", active = true, guiActive = true, guiName = "Invert yaw")]
    public void toggleInvertYaw()
    {
        invertYaw = !invertYaw;
    }


    private void rotateParts(Vector3 rotation)
    {        
        if (usePitch)
        {
            //Transform pitchTransform = part.FindModelTransform(pitchObject);
            pitchTransform.localRotation = Quaternion.Euler(new Vector3(0, rotation.x, 0) + pitchDefaultRotation);
        }
        if (useRoll)
        {
            //Transform rollTransform = part.FindModelTransform(rollObject);
            rollTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation.y) + rollDefaultRotation);
        }
        if (useYaw)
        {
            //Transform yawTransform = part.FindModelTransform(yawObject);
            yawTransform.localRotation = Quaternion.Euler(new Vector3(rotation.z, 0, 0) + yawDefaultRotation);
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (pitchObject != "none")
        {            
            pitchTransform = part.FindModelTransform(pitchObject);
            if (pitchTransform != null)
                usePitch = true;
        }
        if (rollObject != "none")
        {            
            rollTransform = part.FindModelTransform(rollObject);
            if (rollTransform != null)
                useRoll = true;
        }
        if (yawObject != "none")
        {            
            yawTransform = part.FindModelTransform(yawObject);
            if (yawTransform != null)
                useYaw = true;
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        FlightCtrlState ctrl = vessel.ctrlState;
        Vector3 steeringInput = new Vector3(0, 0, 0);

        float pitch = ctrl.pitch;
        if (invertPitch) pitch *= -1;
        float roll = ctrl.roll;
        if (invertRoll) roll *= -1;
        float yaw = ctrl.yaw;
        if (invertYaw) yaw *= -1;

        steeringInput = new Vector3(pitch * axisMultiplier.x, roll * axisMultiplier.y, yaw * axisMultiplier.z);
        //Debug.Log("FS SM in: " + steeringInput);
        rotateParts(steeringInput);
    }
}
