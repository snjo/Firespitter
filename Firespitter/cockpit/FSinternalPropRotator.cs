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
    public Vector3 pitchDefaultRotation = Vector3.zero;
    [KSPField]
    public Vector3 rollDefaultRotation = Vector3.zero;
    [KSPField]
    public Vector3 yawDefaultRotation = Vector3.zero;
    [KSPField]
    public Vector3 brakeMultiplier = Vector3.zero;
    private bool usePitch;
    private bool useRoll;
    private bool useYaw;
    private Transform pitchTransform;
    private Transform rollTransform;
    private Transform yawTransform;
    private Vector3 currentRotation;
    private Vector3 oldRotation;
    private float smoothBrake = 0f;
    private int brakeActionInt = 0;
    private bool firstRun = true;
    Firespitter.info.FSdebugMessages debug = new Firespitter.info.FSdebugMessages(true, "FSinternalPropRotator");
         


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
        if (pitchTransform != null) return;

        if (pitchObject != "none")
        {
            usePitch = true;
            pitchTransform = base.internalProp.FindModelTransform(pitchObject);
            if (pitchTransform == null)
                debug.debugMessage("Could not find pitch transform " + pitchObject);
        }
        if (rollObject != "none")
        {
            useRoll = true;
            rollTransform = base.internalProp.FindModelTransform(rollObject);
            if (rollTransform == null)
                debug.debugMessage("Could not find roll transform " + rollTransform);
        }
        if (yawObject != "none")
        {
            useYaw = true;
            yawTransform = base.internalProp.FindModelTransform(yawObject);
            if (yawTransform == null)
                debug.debugMessage("Could not find yaw transform " + yawObject);
        }

        brakeActionInt = BaseAction.GetGroupIndex(KSPActionGroup.Brakes);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;

        smoothBrake = Mathf.Lerp(smoothBrake, (FlightGlobals.ActiveVessel.ActionGroups.groups[brakeActionInt] ? 1 : 0), 0.1f);

        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA
            || CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
        {
            FlightCtrlState ctrl = vessel.ctrlState;
            Vector3 steeringInput = new Vector3(0, 0, 0);

            steeringInput = new Vector3(ctrl.pitch * axisMultiplier.x, ctrl.roll * axisMultiplier.y, ctrl.yaw * axisMultiplier.z) + brakeMultiplier * smoothBrake;
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

