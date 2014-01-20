using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSheliLiftEngine : PartModule
{
    //[KSPField]
    //public float maxVerticalSpeed = 10f;
    //[KSPField]
    //public float minVerticalSpeed = -5f;
    //[KSPField]
    //public float positiveSpeedChangeRate = 0.1f;
    //[KSPField]
    //public float negativeSpeedChangeRate = 0.05f;
    //[KSPField(guiActive = true, guiName = "Req. Climb rate")]
    //public float requestedVerticalSpeed = 0f;
    //[KSPField(guiActive = true, guiName = "Allowed")]
    //public float allowedVerticalSpeed = 0f;
    [KSPField]
    public string rotorHubName = "rotor";
    [KSPField]
    public string bladeHubName = "blade";
    [KSPField]
    public string swashPlateName = "swashPlate";
    [KSPField]
    public float maxRPM = 300f;
    //[KSPField]
    //public float powerProduction = 0.01f;
    [KSPField]
    public float fallingPowerProduction = 0.0005f;
    [KSPField(guiActive= true, guiName = "Vel thr. rotor")]
    public float airVelocity = 0f;
    [KSPField(guiActive = true, guiName = "collective")]
    public float collective = 0f;//-1f;
    [KSPField]
    public float collectivePitch = 15f;
    [KSPField]
    public float cyclicPitch = 5f;

    [KSPField]
    public float bladeLength = 4f;
    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;
    [KSPField]
    public float powerProduction = 0.2f;
    [KSPField]
    public float engineBrake = 0.05f;
    [KSPField(isPersistant = true, guiName = "Counter Rotating", guiActive = true, guiActiveEditor = true), UI_Toggle()]
    public bool counterRotating = true;

    public bool flameOut = false;
    public bool staged = false;
    public List<Propellant> propellants;
    public FlightCtrlState ctrl;

    private float circumeference = 25.13f;
    //[KSPField(guiActive=true, guiName="Stored Momentum")]
    //public float momentum = 0f;

    private float upAlignment = 1f;
    
    public bool inputProvided = false;
    public bool provideLift = false;
    private float verticalSpeed = 0f;
    private float requestThrottleRaw = 0f;
    private FSheliLiftSurface[] heliLiftSrf;
    private float rotationDirection = 1f;
    private Vector2 cyclic = Vector2.zero;

    [KSPField(guiActive = true, guiName = "RPM")]
    private float RPM = 0f;

    private Transform rotorHubTransform;
    private FSinputVisualizer inputVisualizer = new FSinputVisualizer();

    [KSPField(isPersistant = true)]
    public bool autoThrottle = true;

    [KSPEvent(guiName = "Normal Throttle", guiActive = true, guiActiveEditor = true)]
    public void toggleThrottleMode()
    {
        autoThrottle = !autoThrottle;
        if (autoThrottle)
            Events["toggleThrottleMode"].guiName = "Normal Throttle";
        else
            Events["toggleThrottleMode"].guiName = "Auto Throttle";
    }

    [KSPEvent(guiName = "Activate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void Activate()
    {
        engineIgnited = true;
        staged = true;
        Debug.Log("igniting engine");
    }

    [KSPEvent(guiName = "Deactivate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
    public void Deactivate()
    {
        engineIgnited = false;
    }

    //public void updateFX()
    //{
    //    if (engineIgnited && !flameOut)
    //    {
    //        part.Effect("running", Mathf.Clamp(smoothFxThrust, 0.01f, 1f));
    //    }
    //    else
    //        part.Effect("running", 0f);
    //}

    public override void OnFixedUpdate()
    {
        if (engineIgnited && !flameOut)
            RPM += powerProduction + (3f * powerProduction * (RPM / maxRPM));
        else
        {
            RPM -= engineBrake;
            for (int i = 0; i < heliLiftSrf.Length; i++)
            {
                RPM -= heliLiftSrf[i].bladeDrag;
            }
        }

        RPM = Mathf.Clamp(RPM, 0f, maxRPM);
    }

    public override void OnUpdate()
    {
        if (counterRotating)
            rotationDirection = -1f;
        else
            rotationDirection = 1f;
        animateRotor();

        if (Input.GetKey(KeyCode.PageUp))
            collective += 0.1f;
        if (Input.GetKey(KeyCode.PageDown))
            collective -= 0.1f;
        if (Input.GetKey(KeyCode.End))
            RPM = 0f;

        collective = Mathf.Clamp(collective, -collectivePitch, collectivePitch);
        cyclic = new Vector2(vessel.ctrlState.pitch, vessel.ctrlState.roll);
        if (cyclic.magnitude > 1f)
            cyclic = cyclic.normalized;

        for (int i = 0; i < heliLiftSrf.Length; i++)
        {
            float bladePitchAligned = Vector3.Dot(heliLiftSrf[i].liftTransform.right, rotorHubTransform.right);
            float bladeRollAligned = Vector3.Dot(heliLiftSrf[i].liftTransform.right, rotorHubTransform.up);
            float bladeRotation = collective + (cyclic.x * bladePitchAligned * cyclicPitch) + (cyclic.y * bladeRollAligned * cyclicPitch);
            heliLiftSrf[i].liftTransform.localRotation = Quaternion.Euler(Vector3.right * -bladeRotation * rotationDirection);
            heliLiftSrf[i].pointVelocityMagnitude = (RPM * circumeference) / 60f * rotationDirection;
        }
    }

    //private void getThrottleInput()
    //{
    //    inputProvided = false;
    //    if (Input.GetKey(GameSettings.THROTTLE_UP.primary))
    //    {
    //        requestedVerticalSpeed += positiveSpeedChangeRate;
    //        provideLift = true;
    //        inputProvided = true;
    //    }
    //    if (Input.GetKey(GameSettings.THROTTLE_DOWN.primary))
    //    {
    //        requestedVerticalSpeed -= negativeSpeedChangeRate;
    //        provideLift = true;
    //        inputProvided = true;
    //    }
    //    if (Input.GetKey(GameSettings.THROTTLE_CUTOFF.primary))
    //    {
    //        provideLift = false;
    //        requestedVerticalSpeed = 0f;
    //        inputProvided = true;
    //    }

    //    if (!inputProvided || !vessel.IsControllable)
    //    {
    //        requestedVerticalSpeed = 0f;
    //    }

    //    requestedVerticalSpeed = Mathf.Clamp(requestedVerticalSpeed, minVerticalSpeed, maxVerticalSpeed);
    //}

    public override void OnStart(PartModule.StartState state)
    {
        rotorHubTransform = part.FindModelTransform(rotorHubName);
        heliLiftSrf = part.GetComponents<FSheliLiftSurface>();
        circumeference = bladeLength * Mathf.PI * 2f;
    }

    private void animateRotor()
    {
        rotorHubTransform.Rotate(Vector3.forward, RPM * 20f * TimeWarp.deltaTime * rotationDirection);
    }

    public Vector3 GetVelocity(Rigidbody rigidbody, Vector3 refPoint) // from Ferram
    {
        Vector3 newVelocity = Vector3.zero;
        //newVelocity = commonRigidBody.velocity + Krakensbane.GetFrameVelocity() + Vector3.Cross(commonRigidBody.angularVelocity, liftTransform.position - commonRigidBody.position);
        newVelocity += rigidbody.GetPointVelocity(refPoint);
        newVelocity += Krakensbane.GetFrameVelocityV3f() - Krakensbane.GetLastCorrection() * TimeWarp.fixedDeltaTime;
        return newVelocity;
    }

    public void OnGUI()
    {
        inputVisualizer.OnGUI();
    }
}



class FSheliLiftSurface : PartModule
{
    [KSPField]
    public string liftTransformName = "bladePoint";
    [KSPField]
    public string referenceTransformName = "bladeRef";
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
    private Transform referenceTransform;
    private Rigidbody commonRigidBody;
    public Quaternion originalRotation;
    //private Vector3 rigidBodyVelocity;
    private float airDensity = 1f;
    public float lift = 0f;
    public float discDrag = 0f;
    public float bladeDrag = 0f;

    public float pointVelocityMagnitude = 0f;

    public bool debugMode = true;
    private bool initialized = false;
    private Vector2 liftAndDrag = new Vector2(0f, 0f);
    private float speed = 0f;
    private Vector3 bladeVelocity = Vector3.zero;
    private Vector3 partVelocity = Vector3.zero;
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
            bladeVelocity = partVelocity + pointVelocity;
            //velocity = pointVelocity; //+ (GetVelocity(commonRigidBody, liftTransform.position).magnitude * -liftTransform.up);

            speed = bladeVelocity.magnitude;
            float angleOfAttackRad = CalculateAoA(liftTransform, bladeVelocity);
            float liftCoeff = 2f * Mathf.PI * angleOfAttackRad;
            lift = 0.5f * liftCoeff * airDensity * (speed * speed) * wingArea;
            float aspectRatio = (span * span) / wingArea;
            float dragCoeff = zeroLiftDrag + (liftCoeff * liftCoeff) / (Mathf.PI * aspectRatio * efficiency);
            bladeDrag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;
            
            discDrag = 0.5f * dragCoeff * airDensity * (partVelocity.magnitude * partVelocity.magnitude) * wingArea;

            lift *= power;
            discDrag *= power;
            bladeDrag *= power;
        }
        return new Vector2(lift, discDrag);
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

        //Vector3 liftVector = liftAndDrag.x * -liftTransform.up;
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