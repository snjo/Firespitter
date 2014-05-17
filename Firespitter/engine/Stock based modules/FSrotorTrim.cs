using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSrotorTrim : PartModule
{

    [KSPField]
    public string targetPartObject = "thrustTrimObject";
    [KSPField]
    public string hoverKey = "f";
    [KSPField]
    public float rotationDirectionX = 0f;
    [KSPField]
    public float rotationDirectionY = 0f;
    [KSPField]
    public float rotationDirectionZ = 1f;
    [KSPField]
    public float defaultRotationX = 0f;
    [KSPField]
    public float defaultRotationY = 0f;
    [KSPField]
    public float defaultRotationZ = 0f;
    [KSPField]
    public float steerAmount = 20f;
    //[KSPField]
    //public float trimAmount = 0.25f;
    [KSPField]
    public float hoverHeatModifier = 5f;
    //[KSPField]
    //public float warningSound = 0.8f;
    [KSPField]
    public string rootPart = "copterEngineMain";

    [KSPField(guiActive = true, guiName = "Steering", isPersistant = true)]
    public bool steeringEnabled = false;
    [KSPField(guiActive = true, guiName = "use AD, not QE", isPersistant = true)]
    public bool altInputModeEnabled = false;

    private Vector3 currentRotation = new Vector3(0, 0, 0);
    private float defaultHeatProduction = 300f;
    //private int timeCounter = 0;

    //private static AudioSource heatDing;

    ModuleEngines engine = new ModuleEngines();
    Transform rootTransform = new GameObject().transform;

    [KSPAction("Toggle Steering")]
    public void toggleSteeringAction(KSPActionParam param)
    {
        toggleSteering();
    }

    [KSPEvent(name = "toggleSteering", active = true, guiActive = true, guiName = "Toggle Steering")]
    public void toggleSteering()
    {
        steeringEnabled = !steeringEnabled;
    }

    [KSPEvent(name = "toggleAltInputMode", active = true, guiActive = true, guiName = "QE or AD to rotate")]
    public void toggleAltInputMode()
    {
        altInputModeEnabled = !altInputModeEnabled;
    }

    /* //Very useful function for some other part, but works poorly with ASAS on this part
    [KSPEvent(guiName = "Control from Here", guiActive = true)]
    public void MakeReferenceTransform()
    {
        base.vessel.SetReferenceTransform(this.rootTransform);
    }*/

    private double RadianToDegree(double angle)
    {
        return angle * (180.0 / Math.PI);
    }
    private double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    private void resetTrim()
    {
        //currentTrim = new Vector3(0, 0, 0);
        vessel.ctrlState.pitchTrim = 0f;
        if (altInputModeEnabled)
        {
            vessel.ctrlState.yawTrim = 0f;
        }
        else
        {
            vessel.ctrlState.rollTrim = 0f;
        }
    }

    //private void trimPart(float trimDegrees, Vector3 axis)
    //{
    //    currentTrim = currentTrim + (axis * trimDegrees);
    //}
    
    public void steerPart(float steerDegrees, Vector3 axis)
    {
        float steerThrustModifier = engine.currentThrottle / 1.7f;
        //currentRotation = currentTrim + (steerDegrees * axis * (1-steerThrustModifier));
        currentRotation = steerDegrees * axis * (1 - steerThrustModifier);
    }


    private void setPartRotation()
    {
        Transform partTransform = part.FindModelTransform(targetPartObject);
        partTransform.localRotation = Quaternion.Euler(currentRotation + new Vector3(defaultRotationX,defaultRotationY,defaultRotationZ));
    }

    private void autoHover()
    {        
        {
            Vector3 heading = (Vector3d)this.vessel.transform.up;            
            Vector3d up = (this.vessel.rigidbody.position - this.vessel.mainBody.position).normalized;
            
            Transform partTransform = part.FindModelTransform(targetPartObject);

            Transform modifiedUp = new GameObject().transform;
            modifiedUp.rotation = Quaternion.LookRotation(up, heading);
            modifiedUp.Rotate(new Vector3(-90,0,180));

            partTransform.localRotation = Quaternion.Euler(currentRotation + new Vector3(defaultRotationX, defaultRotationY, defaultRotationZ));
            partTransform.rotation = Quaternion.RotateTowards(partTransform.rotation, modifiedUp.rotation, steerAmount*4); 
        }

    }

    /*public bool fieldsEnabled = true;
    [KSPEvent(name = "toggleFields", active = true, guiActive = true, guiName = "Toggle Fields")]
    public void toggleFieldsEvent()
    {
        toggleFields(!fieldsEnabled);
    }
    public void toggleFields(bool toggle)
    {
        BaseFieldList test = part.Fields;
        

        Debug.Log("FS: part.Fields: " + test);
        Debug.Log("FS: part.Fields.Count: " + test.Count);        
        //test["steeringEnabled"].guiActive = false;

        foreach (KSPField field in test)
        {
            //field.guiActive = toggle;
            Debug.Log("FS guiName: " + field.guiName);
            //Debug.Log("FS name   : " + field.name);
        }
    }*/

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
        rootTransform = part.FindModelTransform(targetPartObject);
        defaultHeatProduction = engine.heatProduction;

        // Sound code from kethane kethanedetector.cs
        /*
        #region Sound effects
        heatDing = gameObject.AddComponent<AudioSource>();
        WWW wwwE = new WWW("file://" + KSPUtil.ApplicationRootPath.Replace("\\", "/") + "/sounds/sound_fsheatDing.wav");
        if ((heatDing != null) && (wwwE != null))
        {
            heatDing.clip = wwwE.GetAudioClip(false);
            heatDing.volume = 1;
            heatDing.Stop();
        }
        #endregion*/
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;

        FlightCtrlState ctrl = vessel.ctrlState;

        Vector3 steeringInput = new Vector3(0, 0, 0);

        if (altInputModeEnabled)
        {            
            steeringInput.x = ctrl.yaw;
        }
        else
        {                        
            steeringInput.x = ctrl.roll;
        }
        
        steeringInput.z = -ctrl.pitch;

        bool inputReceived = (steeringInput != new Vector3(0, 0, 0));

        // if Alt is pressed, trim instead of steer:

        /*if (Input.GetKey(KeyCode.LeftAlt) && trimWithAlt && inputReceived)
        {
            if (!keyHeldDown)
            {
                trimPart(trimAmount, steeringInput);
                steerPart(0, steeringInput);
                //Debug.Log("Trimming " + trimAmount);
            }
            else
            {
                //Debug.Log("Trim denied");
            }
        }
        else
        {*/
            if (steeringEnabled && inputReceived)
            {
                steerPart(steerAmount, new Vector3(steeringInput.x, steeringInput.y, steeringInput.z));
            }
            else steerPart(0, steeringInput);
        //}

        /*if (inputReceived)
        {
            keyHeldDown = true;
        }
        else keyHeldDown = false;*/

        if (Input.GetKey(hoverKey)) //Auto hover
        {
            autoHover();
            engine.heatProduction = defaultHeatProduction * hoverHeatModifier;
        }
        else
        {
            setPartRotation();
            engine.heatProduction = defaultHeatProduction;
        }

        //if (part.temperature > part.maxTemp * warningSound)
        //{
        //    if (timeCounter == 0)
        //    {
        //        heatDing.Play();
        //    }
        //    timeCounter++;
        //    if (timeCounter >= 100)
        //    {
        //        timeCounter = 0;
        //    }
        //}
        //else
        //{
        //    timeCounter = 0;
        //}
    }
}
