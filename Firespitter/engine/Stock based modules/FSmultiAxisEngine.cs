using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
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
    public Vector3 axisMultiplier = new Vector3(10f, 10f, 10f); // pitch, roll, yaw
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
    [KSPField]
    public Vector3 pitchResponseAxis = new Vector3(0f, 1f, 0f);
    [KSPField]
    public Vector3 yawResponseAxis = new Vector3(1f, 0f, 0f);
    [KSPField]
    public Vector3 rollResponseAxis = new Vector3(0f, 0f, 1f);

    [KSPField]
    public bool useReferenceGimbal = false;
    [KSPField]
    public string gimbalTransformName = string.Empty;
    [KSPField]
    public string pitchGimbalExtremeTransformName = string.Empty;
    [KSPField]
    public string yawGimbalExtremeTransformName = string.Empty;
    [KSPField]
    public string rollGimbalExtremeTransformName = string.Empty;
    [KSPField]
    public bool useGimbalResponseSpeed = true;
    [KSPField]
    public float gimbalRange = 10f;
    [KSPField]
    public float gimbalResponseSpeed = 0.1f;

    [KSPField]
    public bool lockWhenEngineIdle = false;

    private bool usePitch;
    private bool useRoll;
    private bool useYaw;
    private Transform pitchTransform = new GameObject().transform;
    private Transform rollTransform = new GameObject().transform;
    private Transform yawTransform = new GameObject().transform;
    private Transform gimbalTransform = new GameObject().transform;
    private Transform pitchGimbalExtreme = new GameObject().transform;
    private Transform rollGimbalExtreme = new GameObject().transform;
    private Transform yawGimbalExtreme = new GameObject().transform;

    private ModuleEngines engine;

    private Quaternion gimbalDefaultRotation = new Quaternion();

    private float gimbalAngleYaw = 0f;
    private float gimbalAnglePitch = 0f;
    private float gimbalAngleRoll = 0f;

    [KSPEvent(name = "invertPitch", active = true, guiActive = false, guiName = "Invert pitch")]
    public void toggleInvertPitch()
    {
        invertPitch = !invertPitch;
    }

    [KSPEvent(name = "invertRoll", active = true, guiActive = false, guiName = "Invert roll")]
    public void toggleInvertRoll()
    {
        invertRoll = !invertRoll;
    }

    [KSPEvent(name = "invertYaw", active = true, guiActive = false, guiName = "Invert yaw")]
    public void toggleInvertYaw()
    {
        invertYaw = !invertYaw;
    }

    private void updateGimbal()
    {
        if (useGimbalResponseSpeed)
        {
            float toYaw = vessel.ctrlState.yaw * gimbalRange;
            float toPitch = vessel.ctrlState.pitch * gimbalRange;
            //float toRoll = vessel.ctrlState.roll * gimbalRange;
            this.gimbalAngleYaw = Mathf.Lerp(gimbalAngleYaw, toYaw, gimbalResponseSpeed * TimeWarp.deltaTime);
            this.gimbalAnglePitch = Mathf.Lerp(gimbalAnglePitch, toPitch, gimbalResponseSpeed * TimeWarp.deltaTime);
            //this.gimbalAngleRoll = Mathf.Lerp(gimbalAngleRoll, toRoll, gimbalResponseSpeed * TimeWarp.deltaTime);
        }
        else
        {
            this.gimbalAngleYaw = base.vessel.ctrlState.yaw * this.gimbalRange;
            this.gimbalAnglePitch = base.vessel.ctrlState.pitch * this.gimbalRange;
        }
        gimbalTransform.localRotation = gimbalDefaultRotation * Quaternion.AngleAxis(this.gimbalAnglePitch, gimbalTransform.InverseTransformDirection(vessel.ReferenceTransform.right)) * Quaternion.AngleAxis(gimbalAngleYaw, gimbalTransform.InverseTransformDirection(vessel.ReferenceTransform.forward));
    }

    private Vector3 getGimbalOffset() // returns xyz as pitch, roll, yaw
    {
        Vector3 result = new Vector3(0f, 0f, 0f);

        if (usePitch)
        {
            float pitchOffset = Quaternion.Dot(pitchGimbalExtreme.rotation, gimbalTransform.rotation);
            result.x = pitchOffset;
            //pitchTransform.localRotation = Quaternion.Euler(new Vector3(0, rotation.x, 0) + pitchDefaultRotation);
        }
        if (useRoll)
        {            
            //rollTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation.y) + rollDefaultRotation);
        }
        if (useYaw)
        {
            float yawOffset = Quaternion.Dot(yawGimbalExtreme.rotation, gimbalTransform.rotation);
            result.z = yawOffset;
            //yawTransform.localRotation = Quaternion.Euler(new Vector3(rotation.z, 0, 0) + yawDefaultRotation);
        }

        return result;
    }

    private void rotateParts(Vector3 rotation)
    {
        if (lockWhenEngineIdle)
        {
            if (engine.flameout || !engine.EngineIgnited || engine.currentThrottle < 0.01f)
                rotation = Vector3.zero;
        }
        if (usePitch)
        {           
            //pitchTransform.localRotation = Quaternion.Euler(new Vector3(0, rotation.x, 0) + pitchDefaultRotation);
            pitchTransform.localRotation = Quaternion.Euler((pitchResponseAxis * rotation.x) + pitchDefaultRotation);
        }
        if (useRoll)
        {
            rollTransform.localRotation = Quaternion.Euler((rollResponseAxis * rotation.y) + rollDefaultRotation);
            //rollTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation.y) + rollDefaultRotation);
        }
        if (useYaw)
        {
            yawTransform.localRotation = Quaternion.Euler((yawResponseAxis * rotation.z) + yawDefaultRotation);
            //yawTransform.localRotation = Quaternion.Euler(new Vector3(rotation.z, 0, 0) + yawDefaultRotation);
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

        if (useReferenceGimbal)
        {
            if (gimbalTransformName == string.Empty)
            {
                useReferenceGimbal = false;
                Debug.Log("FSmultiAxisEngine: gimbal transform name empty");
            }
            else
            {
                gimbalTransform = part.FindModelTransform(gimbalTransformName);
                if (gimbalTransform != null)
                {
                    pitchGimbalExtreme = part.FindModelTransform(pitchGimbalExtremeTransformName);
                    //rollGimbalExtreme = part.FindModelTransform(rollGimbalExtremeTransformName);
                    yawGimbalExtreme = part.FindModelTransform(yawGimbalExtremeTransformName);

                    if (pitchGimbalExtreme == null)
                    {
                        usePitch = false;
                        Debug.Log("FSmultiAxisEngine: pitch gimbal extreme not found");
                    }

                    if (yawGimbalExtreme == null)
                    {
                        usePitch = false;
                        Debug.Log("FSmultiAxisEngine: yaw gimbal extreme not found");
                    }

                    gimbalDefaultRotation = gimbalTransform.rotation;
                }
                else
                {
                    useReferenceGimbal = false;
                    Debug.Log("FSmultiAxisEngine: gimbal transform not found: " + gimbalTransformName);
                }
            }
        }

        if (lockWhenEngineIdle)
        {
            engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
            if (engine == null)
                lockWhenEngineIdle = false;
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

        if (useReferenceGimbal)
        {
            updateGimbal();
            //steeringInput = new Vector3(pitch * axisMultiplier.x, roll * axisMultiplier.y, yaw * axisMultiplier.z);
            Vector3 gimbalResult = getGimbalOffset();
            rotateParts(new Vector3(gimbalResult.x * axisMultiplier.x, gimbalResult.y * axisMultiplier.y, gimbalResult.z * axisMultiplier.z));
        }
        else
        {
            steeringInput = new Vector3(pitch * axisMultiplier.x, roll * axisMultiplier.y, yaw * axisMultiplier.z);
            rotateParts(steeringInput);            
        }        
    }
}
