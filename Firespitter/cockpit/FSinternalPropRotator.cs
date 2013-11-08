using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSinternalPropRotator : InternalModule
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
    private bool usePitch;
    private bool useRoll;
    private bool useYaw;
    private Transform pitchTransform;
    private Transform rollTransform;
    private Transform yawTransform;
    private Vector3 currentRotation;
    private Vector3 oldRotation;
    private bool firstRun = true;
         


    private void rotateParts(Vector3 rotation)
    {
        if (usePitch)
        {
            pitchTransform.transform.localRotation = Quaternion.Euler(new Vector3(0, rotation.x, 0) + pitchDefaultRotation);
        }
        if (useRoll)
        {
            rollTransform.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation.y) + rollDefaultRotation);
        }
        if (useYaw)
        {
            yawTransform.transform.localRotation = Quaternion.Euler(new Vector3(rotation.z, 0, 0) + yawDefaultRotation);
        }
    }

    public override void OnAwake()
    {
        if (this.pitchTransform != null) return;

        if (pitchObject != "none")
        {
            usePitch = true;
            this.pitchTransform = base.internalProp.FindModelTransform(this.pitchObject);
        }
        if (rollObject != "none")
        {
            useRoll = true;
            this.rollTransform = base.internalProp.FindModelTransform(this.rollObject);
        }
        if (yawObject != "none")
        {
            useYaw = true;
            this.yawTransform = base.internalProp.FindModelTransform(this.yawObject);  
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA
            || CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
        {
            FlightCtrlState ctrl = vessel.ctrlState;
            Vector3 steeringInput = new Vector3(0, 0, 0);

            steeringInput = new Vector3(ctrl.pitch * axisMultiplier.x, ctrl.roll * axisMultiplier.y, ctrl.yaw * axisMultiplier.z);
            if (firstRun)
            {
                oldRotation = steeringInput;
                firstRun = false;
            }
            currentRotation = Vector3.Lerp(oldRotation, steeringInput, 0.2f);
            rotateParts(currentRotation);            
            oldRotation = currentRotation;
        }
    }
}

