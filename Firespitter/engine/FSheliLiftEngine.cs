using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSheliLiftEngine : PartModule
{
    [KSPField]
    public string rotorHubName = "rotor";
    [KSPField]
    public string bladeHubName = "blade";
    [KSPField]
    public string swashPlateName = "swashPlate";
    [KSPField]
    public string baseTransformName = "baseReference";
    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max RPM", isPersistant = true), UI_FloatRange(minValue = 50, maxValue = 1000f, stepIncrement = 1f)]
    public float maxRPM = 300f;
    //[KSPField]
    //public float fallingPowerProduction = 0.0005f;
    [KSPField(guiActive= true, guiName = "Vel thr. rotor")]
    public float airVelocity = 0f;
    [KSPField(guiActive = true, guiName = "collective")]
    public float collective = 0f;//-1f;
    [KSPField]
    public float collectivePitch = 15f;
    [KSPField]
    public float rollMultiplier = 0.5f;
    //[KSPField]
    //public float cyclicPitch = 4f;

    [KSPField]
    public float bladeLength = 4f;
    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;
    [KSPField]
    public float powerProduction = 0.15f;
    [KSPField]
    public float engineBrake = 0.05f;
    [KSPField]
    public float autoRotationGain = 0.005f;
    [KSPField(isPersistant = true, guiName = "Counter Rotating", guiActive = true, guiActiveEditor = true), UI_Toggle()]
    public bool counterRotating = false;

    [KSPField(guiActive = true, guiName="Input Options"), UI_Label()]
    public string labelText = string.Empty;
    [KSPField(isPersistant = true, guiName = "Dedicated Keys", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText= "", disabledText="")]
    public bool useDedicatedKeys = false;
    [KSPField(isPersistant = true, guiName = "Throttle Keys", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
    public bool useThrottleKeys = false;
    [KSPField(isPersistant = true, guiName = "Throttle State", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
    public bool useThrottleState = true;
    [KSPField(isPersistant = true, guiName = "Steering", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
    public bool steering = true;

    [KSPField]
    public bool tailRotor = false;
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Steering Response"), UI_FloatRange(minValue = 0f, maxValue = 15f, stepIncrement = 1f)]
    public float steeringResponse = 6f;
    [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Invert Yaw")]
    public bool invertYaw = false;

    [KSPField(guiActive = true, guiName = "correction"), UI_FloatRange(minValue = -1.1f, maxValue = 1.1f, stepIncrement = 0.01f)]
    public float liftSymmentryCorrection = 0.5f;
    [KSPField]
    public float correctionMaxSpeed = 80f;

    [KSPField]
    public bool debugMode = true;

    public bool flameOut = false;
    public bool initialized = false;
    //public bool staged = false;
    public List<Propellant> propellants;

    public bool hoverMode = false;
    private bool resetHoverHeight = false;

    private float circumeference = 25.13f;
    
    public bool inputProvided = false;
    public bool provideLift = false;
    private FSheliLiftSurface[] heliLiftSrf;
    private float rotationDirection = 1f;
    private Vector2 cyclic = Vector2.zero;
    private Vector3 partVelocity = Vector3.zero;
    private float airSpeedThroughRotor = 0f;
    private float hoverCollective = 0f;
    private float longTermHoverCollective = 0f;
    private double hoverHeight = 0f;
    private float partFacingUp = 1f;

    [KSPField(guiActive = true, guiName = "RPM")]
    private float RPM = 0f;

    [KSPField(isPersistant = true)]
    public bool fullSimulation = false;

    public float bladeSpeedLimitLow = 50f;
    public float bladeSpeedLimitHigh = 200f; //335f;

    private Transform rotorHubTransform;
    private Transform baseTransform;
    private FSinputVisualizer inputVisualizer = new FSinputVisualizer();

    //[KSPField(isPersistant = true)]
    //public bool autoThrottle = true;

    //[KSPEvent(guiName = "Normal Throttle", guiActive = true, guiActiveEditor = true)]
    //public void toggleThrottleMode()
    //{
    //    autoThrottle = !autoThrottle;
    //    if (autoThrottle)
    //        Events["toggleThrottleMode"].guiName = "Normal Throttle";
    //    else
    //        Events["toggleThrottleMode"].guiName = "Auto Throttle";
    //}

    [KSPAction("Activate Engine")]
    public void ActivateAction(KSPActionParam param)
    {
        Activate();
    }
    [KSPAction("Deactivate Engine")]
    public void DeactivateAction(KSPActionParam param)
    {
        Deactivate();
    }

    [KSPEvent(guiName = "Activate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void Activate()
    {
        engineIgnited = true;
        //staged = true;
        Debug.Log("igniting engine");
    }

    [KSPEvent(guiName = "Deactivate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void Deactivate()
    {
        engineIgnited = false;
    }

    [KSPEvent(guiName = "Hover", guiActive = true)]
    public void toggleHover()
    {
        hoverMode = !hoverMode;
        if (hoverMode)
        {
            resetHoverHeight = true;
            longTermHoverCollective = collective;
            Events["toggleHover"].guiName = "Disable Hover";
        }
        else
        {
            collective = longTermHoverCollective;
            hoverCollective = 0f;
            Events["toggleHover"].guiName = "Hover";
        }
    }

    [KSPAction("Toggle Hover")]
    public void toggleHoverAction(KSPActionParam param)
    {
        toggleHover();
    }

    [KSPEvent(guiName = "Activate Full Sim", guiActive = true, guiActiveEditor = true)]
    public void toggleFullSimulation()
    {
        fullSimulation = !fullSimulation;

        setFullSimulation(fullSimulation);        
    }

    private void setFullSimulation(bool newValue)
    {
        if (newValue)
            Events["toggleFullSimulation"].guiName = "Deactivate Full Sim";
        else
            Events["toggleFullSimulation"].guiName = "Activate Full Sim";

        for (int i = 0; i < heliLiftSrf.Length; i++)
        {
            heliLiftSrf[i].fullSimulation = newValue;
        }
    }

    public void FixedUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight || !initialized || rigidbody==null) return;

        partVelocity = GetVelocity(rigidbody, transform.position);
        float airDirection = Vector3.Dot(baseTransform.up, partVelocity.normalized);
        airSpeedThroughRotor = partVelocity.magnitude * airDirection;
        partFacingUp = Mathf.Sign(Vector3.Dot(vessel.upAxis, baseTransform.up));

        if (engineIgnited && !flameOut)
            RPM += powerProduction * (TimeWarp.deltaTime * 50f); // + (3f * powerProduction * (RPM / maxRPM));
        else
            RPM -= engineBrake * (TimeWarp.deltaTime * 50f);

        //else
        //{            
            //RPM -= engineBrake;

            if (airDirection < 0f) // && collective <= 0f)
            {
                RPM -= airSpeedThroughRotor * autoRotationGain * (TimeWarp.deltaTime * 50f);
                //Debug.Log("adding rpm: " + -airSpeedThroughRotor * autoRotationGain);
            }

            if (airDirection > 0f) 
            {
                for (int i = 0; i < heliLiftSrf.Length; i++)
                {
                    RPM -= heliLiftSrf[i].bladeDrag * 0.1f;
                }
            }
        //}

        RPM = Mathf.Clamp(RPM, 0f, maxRPM);
    }

    public override void OnUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight || !initialized) return;

        if (counterRotating)
            rotationDirection = -1f;
        else
            rotationDirection = 1f;

        //test
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            RPM = 200f;
        // ---

        animateRotor();

        getCollectiveInput();

        getSteeringInput();

        for (int i = 0; i < heliLiftSrf.Length; i++)
        {
            float bladePitchAligned = Vector3.Dot(heliLiftSrf[i].liftTransform.right, baseTransform.forward);
            float bladeRollAligned = -Vector3.Dot(heliLiftSrf[i].liftTransform.right, baseTransform.right);
            float bladeRotation = collective + (cyclic.x * bladePitchAligned * steeringResponse) + (cyclic.y * bladeRollAligned * steeringResponse * rollMultiplier);
            if (fullSimulation)
            {
                float correction = ((Vector3.Dot(heliLiftSrf[i].referenceTransform.forward, heliLiftSrf[i].partVelocity.normalized) * heliLiftSrf[i].partVelocity.magnitude) / correctionMaxSpeed)
                     * liftSymmentryCorrection;
                bladeRotation -= bladeRotation
                     * correction;

                if (heliLiftSrf[i].debugCubeTransform != null)
                    heliLiftSrf[i].debugCubeTransform.localPosition = -Vector3.up * correction;
            }

            //if (heliLiftSrf[i].debugCubeTransform != null)                
                //heliLiftSrf[i].debugCubeTransform.localPosition = -Vector3.up * heliLiftSrf[i].lift * 0.2f; //Vector3.up * correction;

            heliLiftSrf[i].liftTransform.localRotation = Quaternion.Euler((Vector3.right * -bladeRotation * rotationDirection));
            heliLiftSrf[i].pointVelocityMagnitude = (RPM * circumeference) / 60f * rotationDirection;
        }

    }

    private void getSteeringInput()
    {
        if (steering)
        {
            cyclic = new Vector2(vessel.ctrlState.pitch, vessel.ctrlState.roll);
            if (cyclic.magnitude > 1f)
                cyclic = cyclic.normalized;
        }
        else
        {
            cyclic = Vector3.zero;
        }
    }

    private void getCollectiveInput()
    {
        if (tailRotor)
        {
            collective = vessel.ctrlState.yaw * steeringResponse;
        }
        else
        {
            bool userInput = false;
            bool increaseCollective = false;
            bool decreaseCollective = false;
            bool maxCollective = false;
            bool minCollective = false;
            bool noCollective = false;

            if (useDedicatedKeys)
            {
                if (Input.GetKey(KeyCode.PageUp))
                {
                    increaseCollective = true;
                    userInput = true;
                }
                if (Input.GetKey(KeyCode.PageDown))
                {
                    decreaseCollective = true;
                    userInput = true;
                }
                if (Input.GetKey(KeyCode.End))
                {
                    minCollective = true;
                    userInput = true;                
                }
                if (Input.GetKey(KeyCode.Home))
                {
                    maxCollective = true;
                    userInput = true;
                }
                if (Input.GetKey(KeyCode.Backspace))
                {
                    noCollective = true;
                    userInput = true;
                }
            }
            if (useThrottleKeys || (useThrottleState && hoverMode))
            {                                
                if (Input.GetKey(GameSettings.THROTTLE_UP.primary) || Input.GetKey(GameSettings.THROTTLE_UP.secondary))
                {
                    increaseCollective = true;
                    userInput = true;
                }
                if (Input.GetKey(GameSettings.THROTTLE_DOWN.primary) || Input.GetKey(GameSettings.THROTTLE_DOWN.secondary))
                {
                    decreaseCollective = true;
                    userInput = true;
                }
            }

            //hover
            if (hoverMode && !userInput)
            {                
                if (resetHoverHeight)
                {
                    hoverHeight = vessel.altitude;
                    resetHoverHeight = false;
                }
                collective = 0f;

                float heightOffset = (float)(vessel.altitude - hoverHeight);
                float maxClimb = Mathf.Clamp(-heightOffset, -10f, 10f) * 0.3f;

                if (vessel.verticalSpeed * partFacingUp < maxClimb)
                {
                    //Debug.Log("go up");
                    hoverCollective = collectivePitch;
                    longTermHoverCollective = Mathf.Lerp(longTermHoverCollective, collectivePitch, 0.01f);
                }
                else if (vessel.verticalSpeed * partFacingUp > maxClimb)
                {
                    //Debug.Log("go down");
                    hoverCollective = -collectivePitch;
                    longTermHoverCollective = Mathf.Lerp(longTermHoverCollective, -collectivePitch, 0.01f);
                }
                else
                {
                    //Debug.Log("go nowhere");
                    hoverCollective = 0f;
                    longTermHoverCollective = Mathf.Lerp(longTermHoverCollective, 0f, 0.01f);
                }

                //hoverCollective = Mathf.Lerp(hoverCollective, Mathf.Sign(-airSpeedThroughRotor) * collectivePitch, 0.1f);
                //collective = Mathf.Sign(-airSpeedThroughRotor) * collectivePitch;                

                //Debug.Log(" as " + Math.Round(airSpeedThroughRotor, 2) + " maxClimb " +  Math.Round(maxClimb, 2) + " hoverHeight " +  (int)hoverHeight + " offset " + heightOffset);
            }
            else
            {
                //regular flight

                if (hoverMode && !resetHoverHeight) // will be triggered when a control override starts while in hover mode
                {
                    hoverCollective = 0f;
                    collective = longTermHoverCollective;
                    if (increaseCollective && collective < 0f) collective = 0f;
                    else if (decreaseCollective && collective > 0f) collective = 0f;
                }

                if (increaseCollective)
                    collective += 0.3f;
                if (decreaseCollective)
                    collective -= 0.3f;
                if (maxCollective)
                    collective = collectivePitch;
                if (minCollective)
                    collective = -collectivePitch;
                if (noCollective)
                    collective = 0f;

                if (useThrottleState && !hoverMode)
                {
                    //collective = (vessel.ctrlState.mainThrottle - 0.5f) * 2f * collectivePitch;
                    collective = vessel.ctrlState.mainThrottle * collectivePitch;
                }

                resetHoverHeight = true;

                //if (hoverMode)
                //    collective = longTermHoverCollective;
            }
            
        }        

        collective += hoverCollective;
        collective = Mathf.Clamp(collective, -collectivePitch, collectivePitch);
    }

    public override void OnStart(PartModule.StartState state)
    {
        initialized = true;

        part.stagingIcon = "LIQUID_ENGINE";

        rotorHubTransform = part.FindModelTransform(rotorHubName);
        baseTransform = part.FindModelTransform(baseTransformName);
        heliLiftSrf = part.GetComponents<FSheliLiftSurface>();
        circumeference = bladeLength * Mathf.PI * 2f;

        setFullSimulation(fullSimulation);

        if (tailRotor)
        {
            steering = false;
            useDedicatedKeys = false;
            useThrottleKeys = false;
            useThrottleState = false;
            
            Fields["steering"].guiActive = false;
            Fields["steering"].guiActiveEditor = false;
            Fields["useDedicatedKeys"].guiActive = false;
            Fields["useDedicatedKeys"].guiActiveEditor = false;
            Fields["useThrottleKeys"].guiActive = false;
            Fields["useThrottleKeys"].guiActiveEditor = false;
            Fields["useThrottleState"].guiActive = false;
            Fields["useThrottleState"].guiActiveEditor = false;
            Fields["invertYaw"].guiActive = true;
            Fields["invertYaw"].guiActiveEditor = true;
            Events["toggleHover"].guiActive = false;
        }        
    }

    private void animateRotor()
    {
        //normal
        rotorHubTransform.Rotate(Vector3.forward, RPM * 20f * TimeWarp.deltaTime * rotationDirection);

        //slow test
        //rotorHubTransform.Rotate(Vector3.forward, RPM * 0.1f * TimeWarp.deltaTime * rotationDirection);
    }

    public Vector3 GetVelocity(Rigidbody rigidbody, Vector3 refPoint) // from Ferram
    {
        Vector3 newVelocity = Vector3.zero;        
        newVelocity += rigidbody.GetPointVelocity(refPoint);
        newVelocity += Krakensbane.GetFrameVelocityV3f() - Krakensbane.GetLastCorrection() * TimeWarp.fixedDeltaTime;
        return newVelocity;
    }

    public override void OnActive()
    {
        Activate();
    }

    //public void OnGUI()
    //{
    //    if (HighLogic.LoadedSceneIsFlight)
    //    {
    //        //inputVisualizer.OnGUI();
    //        GUI.Label(FSGUIwindowID.standardRect, "DSoL: " + heliLiftSrf[0].lift / heliLiftSrf[2].lift);
    //        GUI.Label(new Rect(FSGUIwindowID.standardRect.x, FSGUIwindowID.standardRect.y + 25f, FSGUIwindowID.standardRect.width, FSGUIwindowID.standardRect.height), "DSoL: " + heliLiftSrf[1].lift / heliLiftSrf[3].lift);

    //        GUI.Label(new Rect(FSGUIwindowID.standardRect.x, FSGUIwindowID.standardRect.y + 60f, FSGUIwindowID.standardRect.width, FSGUIwindowID.standardRect.height), "0 spd: " + (int)heliLiftSrf[0].bladeVelocity.magnitude + " dir: " + Vector3.Dot(vessel.transform.up, heliLiftSrf[0].bladeVelocity.normalized));
    //        GUI.Label(new Rect(FSGUIwindowID.standardRect.x, FSGUIwindowID.standardRect.y + 85f, FSGUIwindowID.standardRect.width, FSGUIwindowID.standardRect.height), "2 spd: " + (int)heliLiftSrf[2].bladeVelocity.magnitude + " dir: " + Vector3.Dot(vessel.transform.up, heliLiftSrf[2].bladeVelocity.normalized));
    //    }
    //}
}



class FSheliLiftSurface : PartModule
{
    [KSPField]
    public string liftTransformName = "bladePoint";
    [KSPField]
    public string referenceTransformName = "bladeRef";
    [KSPField]
    public string debugCubeName = "Cube";
    [KSPField]
    public float power = 0.0008f;
    [KSPField]
    public float wingArea = 1f;
    [KSPField]
    public float span = 4f;    
    [KSPField]
    public string displayName = "Rotor blade";
    [KSPField]
    public float efficiency = 1f;   //Wright's plane 0.7f
    //Ideal: 1.0
    //Boeing 247D: 0.75
    //DC-3: 0.785   
    //P-51D: 0.69
    //L1049G: 0.75
    //Piper Cherokee: 0.76
    [KSPField]
    public float dragMultiplier = 1f;
    [KSPField]
    public float zeroLiftDrag = 0.0161f; // p-51 Mustang: 0.0161. sopwith camel: 0.0378
    [KSPField]
    public int moduleID = 0;

    public bool FARActive = false;

    public Transform liftTransform;
    public Transform referenceTransform;
    public Transform debugCubeTransform;
    private Rigidbody commonRigidBody;
    public Quaternion originalRotation;
    //private Vector3 rigidBodyVelocity;
    private float airDensity = 1f;
    public float lift = 0f;
    public float discDrag = 0f;
    public float bladeDrag = 0f;
    public bool fullSimulation = true;    

    public float pointVelocityMagnitude = 0f;

    [KSPField]
    public bool debugMode = true;
    private bool initialized = false;
    private Vector2 liftAndDrag = new Vector2(0f, 0f);
    private float speed = 0f;
    public float realSpeed = 0f;
    public Vector3 bladeVelocity = Vector3.zero;
    public Vector3 partVelocity = Vector3.zero;
    private List<FSliftSurface> liftSurfaces = new List<FSliftSurface>();

    public Vector3 GetVelocity(Rigidbody rigidbody, Vector3 refPoint) // from Ferram
    {
        Vector3 newVelocity = Vector3.zero;
        //newVelocity = commonRigidBody.velocity + Krakensbane.GetFrameVelocity() + Vector3.Cross(commonRigidBody.angularVelocity, liftTransform.position - commonRigidBody.position);
        newVelocity += rigidbody.GetPointVelocity(refPoint);
        newVelocity += Krakensbane.GetFrameVelocityV3f() - Krakensbane.GetLastCorrection() * TimeWarp.fixedDeltaTime;
        return newVelocity;
    }

    public float AngleOfAttack
    {
        get
        {
            return CalculateAoA(liftTransform, bladeVelocity) * Mathf.Rad2Deg;
        }
    }

    private float CalculateAoA(Transform wingOrientation, Vector3 inVelocity) // from Ferram
    {
        float PerpVelocity = Vector3.Dot(wingOrientation.up, inVelocity.normalized);
        float AoA = Mathf.Asin(Mathf.Clamp(PerpVelocity, -1, 1));
        return AoA;
    }

    private Vector2 getLiftAndDrag()
    {
        Vector3 pointVelocity = -referenceTransform.forward.normalized * pointVelocityMagnitude;
        commonRigidBody = part.Rigidbody;
        if (commonRigidBody != null)
        {
            partVelocity = GetVelocity(commonRigidBody, liftTransform.position);
            //bladeVelocity = partVelocity + pointVelocity;
            bladeVelocity = pointVelocity; // test
            realSpeed = (bladeVelocity + partVelocity).magnitude;
            if (fullSimulation) bladeVelocity += partVelocity;
            //velocity = pointVelocity; //+ (GetVelocity(commonRigidBody, liftTransform.position).magnitude * -liftTransform.up);

            speed = bladeVelocity.magnitude;
            float angleOfAttackRad = CalculateAoA(liftTransform, bladeVelocity);
            float liftCoeff = 2f * Mathf.PI * angleOfAttackRad;
            lift = 0.5f * liftCoeff * airDensity * (speed * speed) * wingArea;
            float aspectRatio = (span * span) / wingArea;
            float dragCoeff = zeroLiftDrag + (liftCoeff * liftCoeff) / (Mathf.PI * aspectRatio * efficiency);
            bladeDrag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;
            
            discDrag = 0.5f * dragCoeff * airDensity * (partVelocity.magnitude * partVelocity.magnitude) * wingArea;

            lift *= power; // modified by too low blade speed //;
            discDrag *= power;
            bladeDrag *= power;
        }
        return new Vector2(lift, discDrag);
        //return new Vector2(lift, bladeDrag);
    }

    private Vector3 getLiftVector() // from Ferram
    {
        Vector3 ParallelInPlane = Vector3.Exclude(liftTransform.up, bladeVelocity).normalized;  //Projection of velocity vector onto the plane of the wing
        Vector3 perp = Vector3.Cross(liftTransform.up, ParallelInPlane).normalized;       //This just gives the vector to cross with the velocity vector
        Vector3 liftDirection = Vector3.Cross(perp, bladeVelocity).normalized;
        Vector3 liftVector = liftDirection * liftAndDrag.x;
        return liftVector;
    }

    public override string GetInfo()
    {
        string info = string.Empty;
        info = String.Concat("Aerodynamic surface\nName: ",
            displayName, "\n",
            "Area: ", wingArea);

        return info;
    }

    public override void OnStart(PartModule.StartState state)
    {
        FARActive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
        // This line breaks the plugin :(
        if (FARActive)
        {
            this.enabled = false;
        }

        liftTransform = part.FindModelTransform(liftTransformName);
        referenceTransform = part.FindModelTransform(referenceTransformName);
        debugCubeTransform = part.FindModelTransform(debugCubeName);

        originalRotation = liftTransform.localRotation;

        if (liftTransform == null)
        {
            Debug.Log("FSliftSurface: Can't find lift transform " + liftTransformName);
        }
        //if (moduleID == 0)
        //{
        liftSurfaces = part.GetComponents<FSliftSurface>().ToList();
        //liftSurfaces.Add(this); // uhm, what?
        //}        
    }

    public void FixedUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight || !initialized) return;

        airDensity = (float)vessel.atmDensity;
        liftAndDrag = getLiftAndDrag();

        Vector3 liftVector = getLiftVector();
        
        commonRigidBody.AddForceAtPosition(liftVector, liftTransform.position);

        commonRigidBody.AddForceAtPosition(liftAndDrag.y * dragMultiplier * -commonRigidBody.GetPointVelocity(liftTransform.position).normalized, liftTransform.position);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        initialized = true;
    }

    public void OnCenterOfLiftQuery(CenterOfLiftQuery qry)
    {
        if (moduleID == 0)
        {
            CoLqueryData queryData = new CoLqueryData();
            queryData.refVector = qry.refVector;
            for (int i = 0; i < liftSurfaces.Count; i++)
            {
                CoLqueryData newQuery = liftSurfaces[i].liftQuery(queryData.refVector);
                float influence = new Vector2(queryData.dir.magnitude, newQuery.dir.magnitude).normalized.y;
                queryData.pos = Vector3.Lerp(queryData.pos, newQuery.pos, influence);
                queryData.lift += newQuery.lift;
                queryData.dir = Vector3.Lerp(queryData.dir, newQuery.dir, influence);
            }

            queryData.dir.Normalize();

            qry.dir = queryData.dir;
            qry.lift = queryData.lift;
            qry.pos = queryData.pos;
        }
    }

    public CoLqueryData liftQuery(Vector3 refVector)
    {
        CoLqueryData qry = new CoLqueryData();
        Vector3 testVelocity = refVector;
        speed = testVelocity.magnitude;
        float angleOfAttackRad = 0f;
        if (liftTransform != null)
            angleOfAttackRad = CalculateAoA(liftTransform, testVelocity);
        float liftCoeff = 2f * Mathf.PI * angleOfAttackRad;
        lift = 0.5f * liftCoeff * airDensity * (speed * speed) * wingArea;
        float aspectRatio = (span * span) / wingArea;
        float dragCoeff = zeroLiftDrag + (liftCoeff * liftCoeff) / (Mathf.PI * aspectRatio * efficiency);
        discDrag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;

        lift *= power;
        discDrag *= power;

        qry.pos += liftTransform.position;
        qry.dir += -liftTransform.up * lift;
        qry.lift += qry.dir.magnitude;
        //qry.dir.Normalize();

        return qry;
    }
}