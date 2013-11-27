using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

// for wheel angle to ground adjuster:
/** * Determines the angle of a straight line drawn between point one and two. The number returned, which is a float in degrees, tells us how much we have to rotate a horizontal line clockwise for it to match the line between the two points.
 * If you prefer to deal with angles using radians instead of degrees, just change the last line to: "return Math.Atan2(yDiff, xDiff);" */ 
//public static float GetAngleOfLineBetweenTwoPoints(PointF p1, PointF p2) { float xDiff = p2.X - p1.X; float yDiff = p2.Y - p1.Y; return Math.Atan2(yDiff, xDiff) * (180 / Math.PI); }
//See more at: http://wikicode.wikidot.com/get-angle-of-line-between-two-points#sthash.zAEKYkPK.dpuf

class FSwheel : PartModule
{
    #region variables
    [KSPField]
    public string wheelColliderName = "wheelCollider";
    [KSPField]
    public string boundsCollider = "Bounds";
    [KSPField]
    public string wheelMeshName = "Wheel";
    [KSPField]
    public string suspensionParentName = "suspensionParent";
    [KSPField]
    public int numberOfWheels = 1;

    [KSPField]
    public bool disableColliderWhenRetracted = false;

    [KSPField]
    public bool disableColliderWhenRetracting = false;
    [KSPField]
    public bool disableColliderTimeInverted = false;
    [KSPField]
    public float disableColliderAtAnimTime = 0.5f;
   
    [KSPField]
    public string animationName = "Retract";
    [KSPField]
    public int animationLayer = 1;
    [KSPField]
    public float deploymentCooldown = 0.5f;

    [KSPField(isPersistant = true)]
    public string deploymentState = "Deployed"; // Deployed, Deploying, Retracted, Retracting, Stopped

    [KSPField]
    public float brakeTorque = 15f;
    [KSPField]
    public float brakeSpeed = 0.5f;
    [KSPField(isPersistant=true)]
    public bool brakesEngaged = false;

    [KSPField]
    public bool hasMotor = true;
    [KSPField(isPersistant = true, guiActive = true, guiName="Motor Enabled")]
    public bool motorEnabled = true;
    [KSPField]
    public float motorTorque = 2f;
    [KSPField]
    public float nerfNegativeTorque = 1.0f;
    [KSPField]
    public float maxSpeed = 30f;
    [KSPField]
    public string resourceName = "ElectricCharge";
    [KSPField]
    public float resourceConsumptionRate = 0.2f;
    [KSPField(isPersistant = true)]
    public bool reverseMotor = false;
    [KSPField(isPersistant = true)]
    public bool reverseMotorSet = false;
    [KSPField(isPersistant = true)]
    public bool motorStartsReversed = false;   
    [KSPField]
    public string brakeEmissiveObjectName; // for emissive textures to indicate brake status    
    [KSPField]
    public Vector3 onEmissiveColor = new Vector3(1f, 0.3f, 0f);
    [KSPField]
    public Vector3 offEmissiveColor = new Vector3(0f, 1f, 0f);
    [KSPField]
    public Vector3 deployingEmissiveColor = new Vector3(1f, 0f, 0f);
    [KSPField]
    public Vector3 disabledEmissiveColor = new Vector3(0f, 0f, 0f); 

    #region  wheel collider settings
    [KSPField]
    public bool overrideModelFrictionValues = false;
    [KSPField]
    public bool overrideModelSpringValues = false;
    [KSPField]
    public float forwardsStiffness = 10.0f; //for tire friction
    [KSPField]
    public float forwardsExtremumSlip = 1.0f;
    [KSPField]
    public float forwardsExtremumValue = 20000.0f;
    [KSPField]
    public float forwardsAsymptoteSlip = 2.0f;
    [KSPField]
    public float forwardsAsymptoteValue = 10000.0f;
    [KSPField]
    public float sidewaysStiffness = 1.0f; //for tire friction
    [KSPField]
    public float sidewaysExtremumSlip = 1.0f;
    [KSPField]
    public float sidewaysExtremumValue = 20000.0f;
    [KSPField]
    public float sidewaysAsymptoteSlip = 2.0f;
    [KSPField]
    public float sidewaysAsymptoteValue = 10000.0f;
    [KSPField]
    public float wheelColliderMass = -1f;
    [KSPField]
    public float wheelColliderRadius = -1f;
    [KSPField]
    public float wheelColliderSuspensionDistance = -1f;
    [KSPField]
    public float suspensionSpring = -1f;
    [KSPField]
    public float suspensionDamper = -1f;
    [KSPField]
    public float suspensionTargetPosition = -1f;
    #endregion

    [KSPField] // Adjust the roll speed of the wheel if it looks off
    public float rotationAdjustment = 1f;

    [KSPField]
    public float deployedDrag = 0f;
    [KSPField]
    public float retractedDrag = 0f;

    [KSPField]
    public bool guiActiveUnfocused = true;
    [KSPField]
    public float unfocusedRange = 5f;

    [KSPField]
    public int moduleID = 0;

    public bool hasAnimation = false;
    public float animTime = 0f;
    public float animSpeed = 0f;

    private Animation anim;    
    private WheelList wheelList = new WheelList();
    private bool boundsColliderRemoved = false;
    private float animNormalizedTime = 0f;

    FSGUIPopup popup;
    PopupElement motorToggleElement;
    PopupElement motorReverseElement;
    PopupElement suspensionSpringElement;
    PopupElement suspensionDamperElement;
    PopupElement suspensionTargetPositionElement;
    PopupElement suspensionUpdateElement;
    Light brakeLight;
    Transform brakeEmissiveObject;


    public enum BrakeStatus
    {
        on,
        off,
        deploying,
        disabled,
    }

    [KSPField]
    public bool debugMode = false;
    
    #endregion    

    #region Actions and Events

    [KSPAction("Raise Lower Gear", KSPActionGroup.Gear)]
    public void ToggleGearAction(KSPActionParam param)
    {
        if (param.type == KSPActionType.Activate)
        {
            LowerGear();
        }
        else
        {
            RaiseGear();
        }
        param.Cooldown = deploymentCooldown;
    }

    [KSPEvent(guiName = "Raise Gear", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void RaiseGear()
    {
        animate("Retract");        
    }

    [KSPEvent(guiName = "Lower Gear", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void LowerGear()
    {
        animate("Deploy");
    }

    [KSPAction("Reverse Motor")]
    public void ReverseMotorAction(KSPActionParam param)
    {
        reverseMotor = !reverseMotor;
    }

    [KSPEvent(guiName = "Enable Reverse Motor", guiActive = true)]
    public void ReverseMotorEvent()
    {
        reverseMotor = !reverseMotor;
        if (reverseMotor)
        {
            Events["ReverseMotorEvent"].guiName = "Disable Reverse Motor";
        }
        else
        {
            Events["ReverseMotorEvent"].guiName = "Enable Reverse Motor";
        }
    }

    [KSPAction("Toggle Motor")]
    public void ToggleMotorAction(KSPActionParam param)
    {
        if (param.type == KSPActionType.Activate)
        {
            EnableMotorEvent();
        }
        else
        {
            DisableMotorEvent();
        }        
    }

    [KSPEvent(guiName = "Enable Motor", guiActive = true)]
    public void EnableMotorEvent()
    {
        motorEnabled = true;
        Events["EnableMotorEvent"].guiActive = false;
        Events["DisableMotorEvent"].guiActive = true;
    }

    [KSPEvent(guiName = "Disable Motor", guiActive = false)]
    public void DisableMotorEvent()
    {
        motorEnabled = false;
        Events["EnableMotorEvent"].guiActive = true;
        Events["DisableMotorEvent"].guiActive = false;
    }

    private void animate(string mode)
    {
        if (anim != null)
        {            
            animTime = anim[animationName].normalizedTime;

            if (deploymentState == "Retracted") //fixes stupid unity animation timing (0 to 0.99 to 0)
                animTime = 1f;

            if (mode == "Deploy")
            {
                if (deploymentState == "Retracted")
                    animTime = 1f;
                animSpeed = -1f;
                deploymentState = "Deploying";
            }
            else if (mode == "Retract")
            {
                if (deploymentState == "Deployed")
                    animTime = 0f;
                animSpeed = 1f;
                deploymentState = "Retracting";
            }
            else if (mode == "Stop")
            {
                animSpeed = 0f;
                deploymentState = "Stopped";
            }

            anim[animationName].normalizedTime = animTime;
            anim[animationName].speed = animSpeed;
            if (animSpeed != 0f)
            {                
                anim.Play(animationName);
                setBrakeLight(BrakeStatus.deploying);
            }
            else
            {
                anim.Stop();
            }
        }
    }

    [KSPAction("Brakes", KSPActionGroup.Brakes)]
    public void BrakesAction(KSPActionParam param)
    {        
        if (param.type == KSPActionType.Activate)
        {
            brakesEngaged = true;
            setBrakeLight(true);
        }
        else
        {
            brakesEngaged = false;
            setBrakeLight(false);
        }
    }

    [KSPEvent(name = "brakesOn", guiActive = true, active = true, guiName = "Brakes On", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void brakesOnEvent()
    {
        brakesEngaged = true;
        setBrakeLight(BrakeStatus.on);
        Events["brakesOnEvent"].guiActive = false;
        Events["brakesOffEvent"].guiActive = true;
    }

    [KSPEvent(name = "brakesOff", guiActive = true, active = true, guiName = "Brakes Off", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void brakesOffEvent()
    {
        brakesEngaged = false;
        setBrakeLight(BrakeStatus.off);
        Events["brakesOnEvent"].guiActive = true;
        Events["brakesOffEvent"].guiActive = false;
    }

    [KSPEvent(guiName = "increase friction (d)", guiActive = false)]
    public void increaseFrictionEvent()
    {
        wheelList.forwardStiffness += 1f;
    }
    [KSPEvent(guiName = "decrease friction (d)", guiActive = false)]
    public void decreaseFrictionEvent()
    {
        wheelList.forwardStiffness -= 1f;
    }
    [KSPEvent(guiName = "adjust suspension (d)", guiActive = false)]
    public void suspensionGUIEvent()
    {
        popup.showMenu = !popup.showMenu;
    }

    #endregion

    #region GUI popup functions

    private void popupToggleMotor()
    {
        motorEnabled = !motorEnabled;
        motorToggleElement.buttons[0].toggle(motorEnabled);

        foreach (Part p in part.symmetryCounterparts)
        {
            FSwheel wheel = p.GetComponent<FSwheel>();
            if (wheel != null)
            {
                wheel.motorEnabled = motorEnabled;
                wheel.motorToggleElement.buttons[0].toggle(motorEnabled);
            }
        }
    }

    private void popupToggleReverseMotor()
    {
        motorStartsReversed = !motorStartsReversed;
        motorReverseElement.buttons[0].toggle(motorStartsReversed);

        foreach (Part p in part.symmetryCounterparts)
        {
            FSwheel wheel = p.GetComponent<FSwheel>();
            if (wheel != null)
            {
                wheel.motorStartsReversed = motorStartsReversed;
                wheel.motorReverseElement.buttons[0].toggle(motorStartsReversed);
            }
        }
    }

    private void popupUpdateSuspension()
    {
        suspensionSpring = float.Parse(suspensionSpringElement.inputText);
        suspensionDamper = float.Parse(suspensionDamperElement.inputText);
        suspensionTargetPosition = float.Parse(suspensionTargetPositionElement.inputText);
        wheelList.updateSpring(suspensionSpring, suspensionDamper, suspensionTargetPosition);
    }

    #endregion

    public void setBrakeLight(bool _brakesEngaged)
    {
        if (disableColliderWhenRetracted && deploymentState == "Retracted")
        {
            setBrakeLight(BrakeStatus.disabled);
        }
        else
        {
            if (_brakesEngaged)
                setBrakeLight(BrakeStatus.on);
            else
                setBrakeLight(BrakeStatus.off);            
        }        
    }

    public void setBrakeLight(BrakeStatus status)
    {
        if (brakeEmissiveObject != null)
        {
            switch (status)
            {
                case BrakeStatus.on:
                    brakeEmissiveObject.renderer.material.SetColor("_EmissiveColor", new Color(onEmissiveColor.x, onEmissiveColor.y, onEmissiveColor.z));
                    break;
                case BrakeStatus.off:
                    brakeEmissiveObject.renderer.material.SetColor("_EmissiveColor", new Color(offEmissiveColor.x, offEmissiveColor.y, offEmissiveColor.z));
                    break;
                case BrakeStatus.deploying:
                    brakeEmissiveObject.renderer.material.SetColor("_EmissiveColor", new Color(deployingEmissiveColor.x, deployingEmissiveColor.y, deployingEmissiveColor.z));
                    break;
                case BrakeStatus.disabled:
                    brakeEmissiveObject.renderer.material.SetColor("_EmissiveColor", new Color(disabledEmissiveColor.x, disabledEmissiveColor.y, disabledEmissiveColor.z));
                    break;
            }
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        #region In flight
        if (HighLogic.LoadedSceneIsFlight)
        {

            #region create wheel setup
            List<WheelCollider> colliderList = new List<WheelCollider>();
            List<Transform> wheelMeshList = new List<Transform>();
            List<Transform> suspensionList = new List<Transform>();

            for (int i = 0; i < numberOfWheels; i++)
            {
                string suffix = (i + 1).ToString(); // the names used are e.g. "Wheel", "Wheel2", "Wheel3", to remain compatible with stock wheels
                if (i == 0)
                    suffix = "";
                Transform colliderTransform = part.FindModelTransform(wheelColliderName + suffix);
                if (colliderTransform != null)
                {
                    WheelCollider collider = colliderTransform.GetComponent<WheelCollider>();
                    if (collider != null)
                    {
                        colliderList.Add(collider);

                        Transform wheelMeshTransform = part.FindModelTransform(wheelMeshName + suffix);
                        if (wheelMeshTransform != null)
                        {
                            wheelMeshList.Add(wheelMeshTransform);
                        }
                        else
                        {
                            Debug.Log("FSwheel: missing wheel mesh " + wheelMeshName + suffix);
                        }
                        Transform suspensionTransform = part.FindModelTransform(suspensionParentName + suffix);
                        if (suspensionTransform != null)
                        {
                            suspensionList.Add(suspensionTransform);
                        }
                        else
                        {
                            Debug.Log("FSwheel: missing suspensionParent " + suspensionParentName + suffix);
                        }
                    }
                }
                else
                {
                    Debug.Log("FSwheel: missing wheel collider " + wheelColliderName + suffix);
                }
            }

            wheelList.Create(colliderList, wheelMeshList, suspensionList);
            if (wheelList != null)
            {
                if (!wheelList.enabled)
                {
                    wheelList.enabled = true;
                }
            }

            // set the motor direction based on the first found wheelColliders orientation
            if (wheelList.wheels.Count > 0)
            {
                if (!reverseMotorSet) //run only the first time the craft is loaded
                {
                    float dot = Vector3.Dot(wheelList.wheels[0].wheelCollider.transform.forward, vessel.ReferenceTransform.forward);
                    if (dot < 0) // below 0 means the engine is on the left side of the craft
                    {
                        reverseMotor = true;
                        //Debug.Log("FSwheel: Reversing motor, dot: " + dot);
                    }
                    else
                    {
                        //Debug.Log("FSwheel: Motor reversing skipped, dot: " + dot);
                    }
                    if (motorStartsReversed)
                        reverseMotor = !reverseMotor;
                    reverseMotorSet = true;
                }                
            }

            if (disableColliderWhenRetracted)
            {
                if (deploymentState == "Retracted")
                {
                    wheelList.enabled = false;
                }
            }            

            //friction override
            if (overrideModelFrictionValues)
            {
                wheelList.forwardStiffness = forwardsStiffness;
                wheelList.forwardsAsymptoteSlip = forwardsAsymptoteSlip;
                wheelList.forwardsAsymptoteValue = forwardsAsymptoteValue;
                wheelList.forwardsExtremumSlip = forwardsExtremumSlip;
                wheelList.forwardsExtremumValue = forwardsExtremumValue;
                wheelList.sidewaysStiffness = sidewaysStiffness;
                wheelList.forwardsAsymptoteSlip = sidewaysAsymptoteSlip;
                wheelList.sidewaysAsymptoteValue = sidewaysAsymptoteValue;
                wheelList.sidewaysExtremumSlip = sidewaysExtremumSlip;
                wheelList.sidewaysExtremumValue = sidewaysExtremumValue;
                wheelList.updateWheelFriction();
            }

            //optionally set collider and spring values
            if ((suspensionSpring >= 0f && suspensionDamper >= 0f && suspensionTargetPosition >= 0f) || overrideModelSpringValues)
            {
                wheelList.updateSpring(suspensionSpring, suspensionDamper, suspensionTargetPosition);
            }
            if (wheelColliderRadius >= 0f) wheelList.radius = wheelColliderRadius;
            if (wheelColliderMass >= 0f) wheelList.mass = wheelColliderMass;
            if (wheelColliderSuspensionDistance >= 0f) wheelList.suspensionDistance = wheelColliderSuspensionDistance;

            #endregion

            #region animation

            anim = part.FindModelAnimators(animationName).FirstOrDefault();
            if (anim != null)
            {
                hasAnimation = true;
                anim[animationName].layer = animationLayer;
                float startAnimTime = 0f;
                if (deploymentState == "Retracted")
                {
                    startAnimTime = 1f;
                    animSpeed = 1f;
                }
                else
                {
                    animSpeed = -1f;
                }
                anim[animationName].normalizedTime = startAnimTime;
                anim[animationName].speed = animSpeed;
                anim.Play(animationName);
            }
            #endregion           

            #region GUI popup

            popup = new FSGUIPopup(part, "FSwheel", moduleID, FSGUIwindowID.wheel, new Rect(500f, 300f, 250f, 100f), "Wheel settings", new PopupElement("Suspension Settings:"));
            popup.useInFlight = true;
            suspensionSpringElement = new PopupElement("Spring", suspensionSpring.ToString());
            suspensionDamperElement = new PopupElement("Damper", suspensionDamper.ToString());
            suspensionTargetPositionElement = new PopupElement("Target pos", suspensionTargetPosition.ToString());
            popup.sections[0].elements.Add(suspensionSpringElement);
            popup.sections[0].elements.Add(suspensionDamperElement);
            popup.sections[0].elements.Add(suspensionTargetPositionElement);
            
            suspensionUpdateElement = new PopupElement(new PopupButton("Update", 0f, popupUpdateSuspension));
            popup.sections[0].elements.Add(suspensionUpdateElement);

            #endregion

            #region GUI element changes
            Events["RaiseGear"].guiActiveUnfocused = guiActiveUnfocused;
            Events["RaiseGear"].unfocusedRange = unfocusedRange;
            Events["LowerGear"].guiActiveUnfocused = guiActiveUnfocused;
            Events["LowerGear"].unfocusedRange = unfocusedRange;
            Events["EnableMotorEvent"].guiActive = !motorEnabled;
            Events["DisableMotorEvent"].guiActive = motorEnabled;
            Events["brakesOnEvent"].guiActive = !brakesEngaged;
            Events["brakesOffEvent"].guiActive = brakesEngaged;
            Events["brakesOnEvent"].guiActiveUnfocused = guiActiveUnfocused;
            Events["brakesOffEvent"].guiActiveUnfocused = guiActiveUnfocused;
            if (!hasMotor)
            {
                //Events["EnableMotorEvent"].guiActive = false;
                //Events["DisableMotorEvent"].guiActive = false;
                Events["EnableMotorEvent"].active = false;
                Events["DisableMotorEvent"].active = false;
                Events["ReverseMotorEvent"].active = false;
            }
            if (!hasAnimation)
            {
                Events["RaiseGear"].active = false;
                Events["LowerGear"].active = false;
            }
            if (debugMode)
            {
                Events["increaseFrictionEvent"].guiActive = true;
                Events["decreaseFrictionEvent"].guiActive = true;
                Events["suspensionGUIEvent"].guiActive = true;
            }

            if (reverseMotor)
            {
                Events["ReverseMotorEvent"].guiName = "Disable Reverse Motor";
            }
            else
            {
                Events["ReverseMotorEvent"].guiName = "Enable Reverse Motor";
            }
            #endregion

            if (brakeEmissiveObjectName != string.Empty)
            {
                brakeEmissiveObject = part.FindModelTransform(brakeEmissiveObjectName);
            }
            setBrakeLight(brakesEngaged);
            
        }
        #endregion
        #region In Editor
        else if (HighLogic.LoadedSceneIsEditor)
        {
            #region GUI popup

            motorToggleElement = new PopupElement("Motor", new PopupButton("On", "Off", 0f, popupToggleMotor));            
            popup = new FSGUIPopup(part, "FSwheel", moduleID, FSGUIwindowID.wheel, new Rect(500f, 300f, 250f, 100f), "Wheel settings", new PopupElement("Settings affect symmetry group"));
            popup.sections[0].elements.Add(motorToggleElement);
            motorReverseElement = new PopupElement("Reverse Motor", new PopupButton("On", "Off", 0f, popupToggleReverseMotor));
            popup.sections[0].elements.Add(motorReverseElement);

            motorToggleElement.buttons[0].toggle(motorEnabled);
            motorReverseElement.buttons[0].toggle(motorStartsReversed);

            #endregion

            setBrakeLight(BrakeStatus.off);
        }
        #endregion
    }    

    public void FixedUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight)
            return;        

        #region destroy bounds collider
        if (!boundsColliderRemoved)
        {            
            if (boundsCollider != string.Empty)
            {
                Transform boundsTransform = part.FindModelTransform(boundsCollider);
                if (boundsTransform != null)
                {
                    GameObject.Destroy(boundsTransform.gameObject);
                }
            }
            boundsColliderRemoved = true;
        }
        #endregion

        #region update deployment state        
        if (anim != null)
        {
            
            if (!anim.isPlaying)
            {
                if (deploymentState == "Deploying")
                {
                    deploymentState = "Deployed";
                    setBrakeLight(brakesEngaged);
                }
                else if (deploymentState == "Retracting")
                {
                    deploymentState = "Retracted";
                    setBrakeLight(brakesEngaged);
                }                
            }
        }
        #endregion

        #region update brake torque        
        if (brakesEngaged)
        {
            wheelList.brakeTorque = Mathf.Lerp(wheelList.brakeTorque, brakeTorque, TimeWarp.deltaTime * brakeSpeed);
        }
        else
        {
            wheelList.brakeTorque = 0f;
        }
        #endregion

        #region rotate wheel meshes        
        for (int i = 0; i < wheelList.wheels.Count; i++)
        {
            if (wheelList.wheels[i].useRotation)
            {                
                float rotation = wheelList.wheels[i].wheelCollider.rpm * rotationAdjustment * Time.deltaTime;
                wheelList.wheels[i].wheelMesh.Rotate(new Vector3(0f, 0f, rotation));
                //Debug.Log("rotating wheel " + i + " : " + rotation);
            }
        }
        #endregion

        #region update suspension        
        for (int i = 0; i < wheelList.wheels.Count; i++)
        {
            if (wheelList.wheels[i].useSuspension)
            {                
                RaycastHit raycastHit;
                WheelCollider wheelCollider = wheelList.wheels[i].wheelCollider;
                Transform suspensionParent = wheelList.wheels[i].suspensionParent;
                /*WheelHit wheelHit;
                float suspensionTravel = 1.0f;
                if(wheelCollider.GetGroundHit(out wheelHit))
                {
                    suspensionTravel = (-wheelCollider.transform.InverseTransformPoint(wheelHit.point).x - wheelCollider.radius) / wheelCollider.suspensionDistance;
                }
                suspensionParent.position = wheelCollider.transform.position - (wheelCollider.transform.up * (suspensionTravel)); //  * part.rescaleFactor
                */

                if (Physics.Raycast(wheelCollider.transform.position, -wheelCollider.transform.up, out raycastHit, (wheelCollider.suspensionDistance + wheelCollider.radius) * part.rescaleFactor))
                {                 
                   suspensionParent.position = raycastHit.point + wheelCollider.transform.up * wheelCollider.radius * part.rescaleFactor;
                }
                else
                {
                    suspensionParent.position = wheelCollider.transform.position - wheelCollider.transform.up * (wheelCollider.suspensionDistance * part.rescaleFactor);
                }
            }
        }
        #endregion

        #region Active vessel code        

        if (vessel.isActiveVessel && base.vessel.IsControllable)
        {
            #region collider disabling

            if (anim != null)
            {
                animNormalizedTime = anim[animationName].normalizedTime;

                if (disableColliderWhenRetracted || disableColliderWhenRetracting) // runs OnStart too, so no need to run it in fixed update on non active vessels
                {
                    switch (deploymentState)
                    {
                        case "Retracted":
                            if (disableColliderWhenRetracted)
                                wheelList.enabled = false;
                            else
                                wheelList.enabled = true;
                            break;
                        case "Retracting":
                            if (disableColliderWhenRetracting)
                            {
                                if (animNormalizedTime > disableColliderAtAnimTime)
                                    wheelList.enabled = false;
                                else wheelList.enabled = true;
                            }
                            break;
                        case "Deploying":
                            if (disableColliderWhenRetracting)
                            {
                                if (animNormalizedTime > disableColliderAtAnimTime)
                                    wheelList.enabled = false;
                                else wheelList.enabled = true;
                            }
                            else
                            {
                                wheelList.enabled = true;
                            }
                            break;
                        default:
                            wheelList.enabled = true;
                            break;
                    }
                }
            }
            
            #endregion

            #region update motors            

            if (hasMotor && motorEnabled && deploymentState == "Deployed")
            {
                float speedModifier = Mathf.Max(0f, -(((float)vessel.horizontalSrfSpeed - maxSpeed) / maxSpeed));
                float throttleInput = vessel.ctrlState.wheelThrottle * speedModifier;
                if (reverseMotor)
                    throttleInput *= -1;
                double resourceConsumed = (double)Mathf.Abs(resourceConsumptionRate * throttleInput) * (double)TimeWarp.deltaTime;
                if (!CheatOptions.InfiniteFuel)
                {
                    double receivedResource = base.part.RequestResource(resourceName, resourceConsumed);                    
                    if (resourceConsumed > 0f)
                    {
                        double resouceReceivedNormalized = receivedResource / resourceConsumed;
                        //Debug.Log("got " + resouceReceivedNormalized + " of requested resource");
                        throttleInput *= Mathf.Clamp((float)resouceReceivedNormalized, 0f, 1f);
                    }
                }
                if (throttleInput < 0f)
                    throttleInput *= nerfNegativeTorque; // fixes negative values being overpowered
                wheelList.motorTorque = throttleInput * motorTorque;
                //Debug.Log("FSwheel: applying torque: " + throttleInput);
            }
            else
            {
                wheelList.motorTorque = 0f;
            }
            #endregion

            #region update drag            

            if (deploymentState == "Deployed")
            {
                part.minimum_drag = deployedDrag;
                part.maximum_drag = deployedDrag;
            }
            else
            {
                part.minimum_drag = 0f;
                part.maximum_drag = 0f;
            }

            #endregion

        #endregion

        }
    }

    public void OnGUI()
    {
        if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
        {
            if (popup != null)
                popup.popup();
        }
    }

}

class WheelClass
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;
    public Transform suspensionParent;
    public bool useRotation = false;
    public bool useSuspension = false;
    public WheelClass(WheelCollider _wheelCollider, Transform _wheelMesh, Transform _suspensionParent)
    {
        wheelCollider = _wheelCollider;
        wheelMesh = _wheelMesh;
        suspensionParent = _suspensionParent;
    }

    public WheelClass(WheelCollider _wheelCollider)
    {
        wheelCollider = _wheelCollider;
        useRotation = false;
        useSuspension = false;
    }
}

class WheelList
{
    public List<WheelClass> wheels;
    //public List<WheelCollider> wheelColliders = new List<WheelCollider>();
    //public List<Transform> wheelMeshes;
    //public List<Transform> suspensionParents;
    private bool _enabled = false;
    private float _brakeTorque = 0f;
    private float _motorTorque = 0f;
    private float _radius = 0.25f;
    private float _suspensionDistance = 0.025f;
    private float _mass = 0.1f;
    public float forwardStiffness = 10f;    
    public float forwardsExtremumSlip = 1.0f;
    public float forwardsExtremumValue = 20000.0f;
    public float forwardsAsymptoteSlip = 2.0f;
    public float forwardsAsymptoteValue = 10000.0f;
    public float sidewaysStiffness = 1.0f;
    public float sidewaysExtremumSlip = 1.0f;    
    public float sidewaysExtremumValue = 20000.0f;    
    public float sidewaysAsymptoteSlip = 2.0f;    
    public float sidewaysAsymptoteValue = 10000.0f;

    public void Create(List<WheelCollider> colliders, List<Transform> wheelMeshes, List<Transform> suspensionParents)
    {
        wheels = new List<WheelClass>();
        for (int i = 0; i < colliders.Count; i++)
        {            
            wheels.Add(new WheelClass(colliders[i]));
            if (i < wheelMeshes.Count)
            {
                wheels[i].wheelMesh = wheelMeshes[i];
                wheels[i].useRotation = true;
            }
            if (i < suspensionParents.Count)
            {
                wheels[i].suspensionParent = suspensionParents[i];
                wheels[i].useSuspension = true;
            }
        }        
    }

    public void Create(WheelCollider collider, Transform wheelMesh, Transform suspensionParent)
    {
        wheels = new List<WheelClass>();
        wheels.Add(new WheelClass(collider, wheelMesh, suspensionParent));           
    }

    public bool enabled
    {
        get
        {
            return _enabled;
        }

        set
        {
            _enabled = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.enabled = value;
            }
        }
    }

    public float brakeTorque
    {
        get
        {
            return _brakeTorque;
        }
        set
        {
            _brakeTorque = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.brakeTorque = value;
            }
        }
    }

    public float motorTorque
    {
        get
        {
            return _motorTorque;
        }
        set
        {
            _motorTorque = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.motorTorque = value;
                //Debug.Log("torque: " + value);
            }
        }
    }

    public void updateSpring(float spring, float damper, float targetPosition)
    {
        JointSpring jointSpring = new JointSpring();
        jointSpring.spring = spring;
        jointSpring.damper = damper;
        jointSpring.targetPosition = targetPosition;
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].wheelCollider.suspensionSpring = jointSpring;            
        }
    }

    public float suspensionDistance
    {
        get
        {
            return _suspensionDistance;
        }
        set
        {
            _suspensionDistance = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.suspensionDistance = value;
            }            
        }
    }

    public float mass
    {
        get
        {
            return _mass;
        }
        set
        {
            _mass = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.mass = value;
            }
        }
    }

    public float radius
    {
        get
        {
            return _radius;
        }
        set
        {
            _radius = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.radius = value;
            }
        }
    }

    public void updateWheelFriction()
    {
        
        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = forwardsExtremumSlip;
        forwardFriction.extremumValue = forwardsExtremumValue;
        forwardFriction.asymptoteSlip = forwardsAsymptoteSlip;
        forwardFriction.asymptoteValue = forwardsAsymptoteValue;
        forwardFriction.stiffness = forwardStiffness;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = sidewaysExtremumSlip;
        sidewaysFriction.extremumValue = sidewaysExtremumValue;
        sidewaysFriction.asymptoteSlip = sidewaysAsymptoteSlip;
        sidewaysFriction.asymptoteValue = sidewaysAsymptoteValue;
        sidewaysFriction.stiffness = sidewaysStiffness;

        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
        }        
    }
}
