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
    public float customAnimationSpeed = 1f;
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
    public bool brakesLockedOn = false;

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
    public string brakeEmissiveObjectName = string.Empty; // for emissive textures to indicate brake status    
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
    public bool useDragUpdate = true;
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

    [KSPField]
    public float animTime = 0f;
    public float animSpeed = 0f;

    [KSPField]
    public float wheelScreechThreshold = 10f; // RPM difference between expected value and current wheel RPM at curretn velocity    
    [KSPField]
    public bool useCustomParticleFX = false;
    //Firespitter.FSparticleFX smokeFX;
    Texture2D smokeFXtexture;
    [KSPField]
    public string smokeFXtextureName = "Firespitter/textures/particle";
    [KSPField]
    public float particleEmissionRate = 1200f;
    private float fxLevel = 0f;
    public float screechMindeltaRPM = 30f;

    [KSPField]
    public string startDeployEffect = string.Empty;
    [KSPField]
    public string startRetractEffect = string.Empty;

    private float finalBrakeTorque = 0f;

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
    PopupElement suspensionDistanceElement;
    PopupElement wheelRadiusElement;
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

    [KSPEvent(guiName = "Raise Gear", guiActive = true, guiActiveEditor=true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void RaiseGear()
    {
        animate("Retract");
    }

    [KSPEvent(guiName = "Lower Gear", guiActive = true, guiActiveEditor=true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void LowerGear()
    {
        animate("Deploy");
    }

    [KSPAction("Reverse Motor")]
    public void ReverseMotorAction(KSPActionParam param)
    {
        reverseMotor = !reverseMotor;
    }

    [KSPEvent(guiName = "Enable Reverse Motor", guiActive = true, guiActiveEditor=true)]
    public void EnableReverseMotorEvent()
    {
        reverseMotor = true;
        Events["EnableReverseMotorEvent"].guiActive = false;
        Events["EnableReverseMotorEvent"].guiActiveEditor = false;
        Events["DisableReverseMotorEvent"].guiActive = true;
        Events["DisableReverseMotorEvent"].guiActiveEditor = true;
                
    }

    [KSPEvent(guiName = "Disable Reverse Motor", guiActive = true, guiActiveEditor = true)]
    public void DisableReverseMotorEvent()
    {
        reverseMotor = false;
        Events["DisableReverseMotorEvent"].guiActive = false;
        Events["DisableReverseMotorEvent"].guiActiveEditor = false;
        Events["EnableReverseMotorEvent"].guiActive = true;
        Events["EnableReverseMotorEvent"].guiActiveEditor = true;
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

    [KSPEvent(guiName = "Enable Motor", guiActive = true, guiActiveEditor=true)]
    public void EnableMotorEvent()
    {
        motorEnabled = true;
        Events["EnableMotorEvent"].guiActive = false;
        Events["DisableMotorEvent"].guiActive = true;
        Events["EnableMotorEvent"].guiActiveEditor = false;
        Events["DisableMotorEvent"].guiActiveEditor = true;
    }

    [KSPEvent(guiName = "Disable Motor", guiActive = false, guiActiveEditor=true)]
    public void DisableMotorEvent()
    {
        motorEnabled = false;
        Events["EnableMotorEvent"].guiActive = true;
        Events["DisableMotorEvent"].guiActive = false;
        Events["EnableMotorEvent"].guiActiveEditor = true;
        Events["DisableMotorEvent"].guiActiveEditor = false;
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
                animSpeed = -1f * customAnimationSpeed;
                deploymentState = "Deploying";
                if (startDeployEffect != string.Empty)
                {
                    part.Effect(startDeployEffect);
                }
            }
            else if (mode == "Retract")
            {
                if (deploymentState == "Deployed")
                    animTime = 0f;
                animSpeed = 1f * customAnimationSpeed;
                deploymentState = "Retracting";
                if (startRetractEffect != string.Empty)
                {
                    part.Effect(startRetractEffect);
                }
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
        Events["brakesOnEvent"].guiActiveEditor = false;
        Events["brakesOffEvent"].guiActiveEditor = true;
    }

    [KSPEvent(name = "brakesOff", guiActive = true, active = true, guiName = "Brakes Off", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
    public void brakesOffEvent()
    {
        brakesEngaged = false;
        setBrakeLight(BrakeStatus.off);
        Events["brakesOnEvent"].guiActive = true;
        Events["brakesOffEvent"].guiActive = false;
        Events["brakesOnEvent"].guiActiveEditor = true;
        Events["brakesOffEvent"].guiActiveEditor = false;
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
        overrideModelSpringValues = true;
        wheelColliderRadius = float.Parse(wheelRadiusElement.inputText);
        wheelColliderSuspensionDistance = float.Parse(suspensionDistanceElement.inputText);
        suspensionSpring = float.Parse(suspensionSpringElement.inputText);
        suspensionDamper = float.Parse(suspensionDamperElement.inputText);
        suspensionTargetPosition = float.Parse(suspensionTargetPositionElement.inputText);
        wheelList.updateSpring(suspensionSpring, suspensionDamper, suspensionTargetPosition);
        wheelList.radius = wheelColliderRadius;
        wheelList.suspensionDistance = wheelColliderSuspensionDistance;
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

    public override string GetInfo()
    {
        StringBuilder info = new StringBuilder();
        if (hasMotor) info.Append("Motor Torque: ").AppendLine(motorTorque.ToString());
        else info.AppendLine("No motor.");
        info.Append("Brake Torque: ").AppendLine(brakeTorque.ToString());
        if (brakesLockedOn) info.AppendLine("Brakes are locked on");

        return info.ToString();
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

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
                animSpeed = 1f * customAnimationSpeed;
            }
            else
            {
                animSpeed = -1f * customAnimationSpeed;
            }
            anim[animationName].normalizedTime = startAnimTime;
            anim[animationName].speed = animSpeed;
            anim.Play(animationName);
        }
        #endregion  

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
            //Debug.Log("FSwheel: wheelist count is " + wheelList.wheels.Count);
            if (wheelList.wheels.Count > 0)
            {
                Debug.Log("FSwheel: reversemotorset: " + reverseMotorSet);
                if (!reverseMotorSet) //run only the first time the craft is loaded
                {
                    float dot = Vector3.Dot(wheelList.wheels[0].wheelCollider.transform.forward, vessel.ReferenceTransform.up); // up is forward
                    if (dot < 0) // below 0 means the engine is on the left side of the craft
                    {
                        reverseMotor = true;
                        //Debug.Log("FSwheel: Reversing motor, dot: " + dot);
                    }
                    else
                    {
                        reverseMotor = false;
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
            else
                wheelColliderRadius = wheelList.radius;
            if (wheelColliderMass >= 0f) wheelList.mass = wheelColliderMass;
            if (wheelColliderSuspensionDistance >= 0f) wheelList.suspensionDistance = wheelColliderSuspensionDistance;
            else
                wheelColliderSuspensionDistance = wheelList.suspensionDistance;

            #endregion                     

            #region GUI popup

            popup = new FSGUIPopup(part, "FSwheel", moduleID, FSGUIwindowID.wheel, new Rect(500f, 300f, 250f, 100f), "Wheel settings", new PopupElement("Suspension Settings:"));
            popup.useInFlight = true;
            wheelRadiusElement = new PopupElement("Radius", wheelColliderRadius.ToString());
            suspensionDistanceElement = new PopupElement("Distance", wheelColliderSuspensionDistance.ToString());
            suspensionSpringElement = new PopupElement("Spring", suspensionSpring.ToString());
            suspensionDamperElement = new PopupElement("Damper", suspensionDamper.ToString());
            suspensionTargetPositionElement = new PopupElement("Target pos", suspensionTargetPosition.ToString());
            popup.sections[0].elements.Add(wheelRadiusElement);
            popup.sections[0].elements.Add(suspensionDistanceElement);
            popup.sections[0].elements.Add(suspensionSpringElement);
            popup.sections[0].elements.Add(suspensionDamperElement);
            popup.sections[0].elements.Add(suspensionTargetPositionElement);
            
            suspensionUpdateElement = new PopupElement(new PopupButton("Update", 0f, popupUpdateSuspension));
            popup.sections[0].elements.Add(suspensionUpdateElement);

            #endregion

            if (brakeEmissiveObjectName != string.Empty)
            {
                brakeEmissiveObject = part.FindModelTransform(brakeEmissiveObjectName);
            }
            setBrakeLight(brakesEngaged);

            #region set up fx
            if (useCustomParticleFX)
            {                
                smokeFXtexture = GameDatabase.Instance.GetTexture(smokeFXtextureName, false);
                if (smokeFXtexture == null)
                {
                    useCustomParticleFX = false;                    
                }
                else
                {
                    for (int i = 0; i < wheelList.wheels.Count; i++)
                    {
                        wheelList.wheels[i].smokeFX = new Firespitter.FSparticleFX(wheelList.wheels[i].fxLocation, smokeFXtexture);
                        wheelList.wheels[i].smokeFX.AnimatorColor0 = new Color(1.0f, 1.0f, 1.0f, 0.8f);
                        wheelList.wheels[i].smokeFX.AnimatorColor1 = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                        wheelList.wheels[i].smokeFX.AnimatorColor2 = new Color(1.0f, 1.0f, 1.0f, 0.2f);
                        wheelList.wheels[i].smokeFX.AnimatorColor3 = new Color(1.0f, 1.0f, 1.0f, 0.1f);
                        wheelList.wheels[i].smokeFX.AnimatorColor4 = new Color(1.0f, 1.0f, 1.0f, 0.0f);

                        wheelList.wheels[i].smokeFX.EmitterMinSize = 0.3f;
                        wheelList.wheels[i].smokeFX.EmitterMaxSize = 0.5f;
                        wheelList.wheels[i].smokeFX.EmitterMinEnergy = 0.1f;
                        wheelList.wheels[i].smokeFX.EmitterMaxEnergy = 0.3f;
                        wheelList.wheels[i].smokeFX.EmitterMinEmission = 0f;
                        wheelList.wheels[i].smokeFX.EmitterMaxEmission = 0f;
                        wheelList.wheels[i].smokeFX.AnimatorSizeGrow = 0.1f;
                        
                        wheelList.wheels[i].smokeFX.setupFXValues();
                    }
                }
            }

            #endregion

        }        

        #endregion

        #region GUI element changes
        Events["RaiseGear"].guiActiveUnfocused = guiActiveUnfocused;
        Events["RaiseGear"].unfocusedRange = unfocusedRange;

        Events["LowerGear"].guiActiveUnfocused = guiActiveUnfocused;
        Events["LowerGear"].unfocusedRange = unfocusedRange;

        Events["EnableMotorEvent"].guiActive = !motorEnabled;
        Events["DisableMotorEvent"].guiActive = motorEnabled;
        Events["EnableMotorEvent"].guiActiveEditor = !motorEnabled;
        Events["DisableMotorEvent"].guiActiveEditor = motorEnabled;

        Events["brakesOnEvent"].guiActive = !brakesEngaged;
        Events["brakesOffEvent"].guiActive = brakesEngaged;
        Events["brakesOnEvent"].guiActiveEditor = !brakesEngaged;
        Events["brakesOffEvent"].guiActiveEditor = brakesEngaged;

        Events["EnableReverseMotorEvent"].guiActive = !reverseMotor;
        Events["DisableReverseMotorEvent"].guiActive = reverseMotor;
        Events["EnableReverseMotorEvent"].guiActiveEditor = !reverseMotor;
        Events["DisableReverseMotorEvent"].guiActiveEditor = reverseMotor;

        Events["brakesOnEvent"].guiActiveUnfocused = guiActiveUnfocused;
        Events["brakesOffEvent"].guiActiveUnfocused = guiActiveUnfocused;

        if (!hasMotor)
        {
            //Events["EnableMotorEvent"].guiActive = false;
            //Events["DisableMotorEvent"].guiActive = false;
            Events["EnableMotorEvent"].active = false;
            Events["DisableMotorEvent"].active = false;
            Events["EnableReverseMotorEvent"].active = false;
            Events["DisableReverseMotorEvent"].active = false;
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

        if (brakesLockedOn)
        {
            Events["brakesOnEvent"].guiActive = false;
            Events["brakesOffEvent"].guiActive = false;
            Events["brakesOnEvent"].guiActiveUnfocused = false;
            Events["brakesOffEvent"].guiActiveUnfocused = false;
        }
        
        #endregion
    }    

    public void FixedUpdate()
    {
        updateDeploymentState();        

        if (!HighLogic.LoadedSceneIsFlight)
            return;        
        
        destroyBoundsCollider();                
              
        updateBrakeTorque();        
             
        rotateWheelMeshes();        
        
        updateSuspension();        

        #region Active vessel code        

        if (vessel.isActiveVessel && base.vessel.IsControllable)
        {
            disableColliders();

            updateMotors();

            updateDrag();            
        }
        #endregion        
    }

    public override void OnUpdate()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            checkSounds();
        }
    }

    private void updateSuspension()
    {
        for (int i = 0; i < wheelList.wheels.Count; i++)
        {
            if (wheelList.wheels[i].useSuspension)
            {
                RaycastHit raycastHit;
                WheelCollider wheelCollider = wheelList.wheels[i].wheelCollider;
                Transform suspensionParent = wheelList.wheels[i].suspensionParent;                

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
    }

    private void rotateWheelMeshes()
    {
        for (int i = 0; i < wheelList.wheels.Count; i++)
        {
            if (wheelList.wheels[i].useRotation)
            {                
                float rotation = wheelList.wheels[i].wheelCollider.rpm * TimeWarp.deltaTime * rotationAdjustment;

                wheelList.wheels[i].wheelMesh.Rotate(new Vector3(0f, 0f, rotation));

                float deltaRPM = Mathf.Max(0f, Mathf.Abs(wheelList.wheels[i].getDeltaRPM()) - screechMindeltaRPM);
                if (deltaRPM > 0f && wheelList.wheels[i].wheelCollider.isGrounded)
                {
                    fireScreechEffect(i, deltaRPM);
                }

                updateScreechEffect(i);
            }
        }
    }

    private void updateBrakeTorque()
    {
        if (brakesEngaged || brakesLockedOn)
        {
            finalBrakeTorque = Mathf.Lerp(wheelList.brakeTorque, brakeTorque, TimeWarp.deltaTime * brakeSpeed);
            if (brakesLockedOn)
                if (vessel.ctrlState.mainThrottle > 0.1f)
                    finalBrakeTorque = 0f;
            wheelList.brakeTorque = finalBrakeTorque;            
        }
        else
        {
            wheelList.brakeTorque = 0f;
        }
    }

    private void destroyBoundsCollider()
    {
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
    }

    private void updateDeploymentState()
    {
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
    }

    private void disableColliders()
    {
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
    }

    private void updateMotors()
    {
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
                    throttleInput *= Mathf.Clamp((float)resouceReceivedNormalized, 0f, 1f);
                }
            }
            if (throttleInput < 0f)
                throttleInput *= nerfNegativeTorque; // fixes negative values being overpowered
            wheelList.motorTorque = throttleInput * motorTorque;            
        }
        else
        {
            wheelList.motorTorque = 0f;
        }
    }

    private void updateDrag()
    {
        if (useDragUpdate)
        {
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
        }
    }
        
    private void checkSounds()
    {
        for (int i = 0; i < wheelList.wheels.Count; i++)
        {
            bool isGrounded = wheelList.wheels[i].wheelCollider.isGrounded;
            if (isGrounded && !wheelList.wheels[i].oldIsGrounded)
            {
                if (vessel.verticalSpeed < -1f)
                    fireTouchdownThud();
            }
            wheelList.wheels[i].oldIsGrounded = isGrounded;

            if (isGrounded && vessel.horizontalSrfSpeed > 2f)
            {
                fireRollSound();
                fireBrakeSound(i);
            }
            else
            {
                part.Effect("wheelRoll", 0f);
                part.Effect("brakes", 0f);
            }
        }
    }

    private void fireBrakeSound(int wheelNumber)
    {
        part.Effect("brakes", (wheelList.wheels[wheelNumber].wheelCollider.brakeTorque / brakeTorque) * Mathf.Clamp((float)vessel.horizontalSrfSpeed, 0f, 15f) / 15f);
        
    }

    private void fireRollSound()
    {
        float rollLevel = Mathf.Clamp((float)vessel.horizontalSrfSpeed, 0f, 40f) / 40f;
        part.Effect("wheelRoll", rollLevel);                
    }

    private void fireTouchdownThud()
    {
        float thudLevel = Mathf.Clamp(-(float)vessel.verticalSpeed, 0f, 15f) / 15f;
        part.Effect("touchdownThud", thudLevel);
    }

    private void fireScreechEffect(int wheelNumber, float deltaRPM)
    {
        fxLevel = Mathf.Clamp((float)vessel.horizontalSrfSpeed, 0f, 40f) / 40f;
        fxLevel *= deltaRPM / 200f;
        part.Effect("touchdown", fxLevel);
        //Debug.Log("wheels: " + wheelList.wheels.Count + ", current: " + wheelNumber);
        wheelList.wheels[wheelNumber].screechCountdown = 0.5f;
        //Debug.Log(Vector3.Distance(vessel.ReferenceTransform.position, wheelList.wheels[wheelNumber].smokeFX.gameObject.transform.position));
        // play one shot audio                
    }

    private void updateScreechEffect(int wheelNumber)
    {
        if (wheelList.wheels[wheelNumber].screechCountdown > 0f)
        {
            // emit particles
            if (wheelList.wheels[wheelNumber].wheelCollider.isGrounded)
            {
                if (useCustomParticleFX)
                {
                    wheelList.wheels[wheelNumber].smokeFX.pEmitter.minEmission = particleEmissionRate * fxLevel;
                    wheelList.wheels[wheelNumber].smokeFX.pEmitter.maxEmission = particleEmissionRate * fxLevel;
                }
                else
                {
                    part.Effect("tireSmoke", fxLevel);
                }
            }
            else
            {
                if (useCustomParticleFX)
                {
                    wheelList.wheels[wheelNumber].smokeFX.pEmitter.minEmission = 0f;
                    wheelList.wheels[wheelNumber].smokeFX.pEmitter.maxEmission = 0f;
                }
            }
            //smokeFX
            wheelList.wheels[wheelNumber].screechCountdown -= TimeWarp.deltaTime;
        }
        else
        {
            if (useCustomParticleFX)
            {
                wheelList.wheels[wheelNumber].smokeFX.pEmitter.minEmission = 0f;
                wheelList.wheels[wheelNumber].smokeFX.pEmitter.maxEmission = 0f;
            }
        }
    }

    public void OnGUI()
    {
        if (debugMode)
        {
            //float rpmOverSpeed = currentRPM / (float)vessel.srf_velocity.magnitude;
            //GUI.Label(new Rect(300f, 300f, 400f, 100f), "Speed: " + Mathf.Round((float)vessel.srf_velocity.magnitude).ToString() + ", rpm: " + currentRPM + " speed / RP: " + rpmOverSpeed);
            
            //if (screechCountdown > 0f)
            //{                
            //    GUI.Label(new Rect(300f, 350f, 400f, 100f), "Screeech! grounded: " + wheelList.wheels[0].wheelCollider.isGrounded);
            //}            
            //wheelList.wheels[0].wheelCollider.isGrounded
            popup.popup();
        }
    }

}
