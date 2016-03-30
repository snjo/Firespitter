using UnityEngine;

class FSartificialHorizon :InternalModule
{
    public Transform needle;
    public Transform refTransform; // should correspond to vessel.referencetransform in KSP	
    public Material discMat;
    public GameObject innerDisc;
    [KSPField]
    public bool debugMode = false;
    private float pitch = 0f;
    public float cageOffset = 0f;
    public float cageIncrements = 0.0025f;
    public bool testCage = false;
    [KSPField]
    public string testButtonName = "buttonTest";
    [KSPField]
    public string plusButtonName = "buttonPlus";
    [KSPField]
    public string minusButtonName = "buttonMinus";
    [KSPField]
    public string outerRingName = "needleHolder";
    [KSPField]
    public string innerDiscName = "gauge_rear_disc";
    [KSPField]
    public float pitchOffsetMultiplier = 0.0028f;
    [KSPField]
    public float pitchOffsetLimit = 0.09f;
    [KSPField]
    public bool useOffset = true;
    [KSPField]
    public float rollDirection = 1f;
    private GameObject testButton;
    private GameObject plusButton;
    private GameObject minusButton;
    private FSgenericButtonHandler testButtonHandler;
    private FSgenericButtonHandler plusButtonHandler;
    private FSgenericButtonHandler minusButtonHandler;
    private Firespitter.ShipHeading shipHeading;

    public void cagePlus()
    {
        cageOffset += cageIncrements;
    }
    public void cageMinus()
    {
        cageOffset -= cageIncrements;
    }
    public void testCageToggle()
    {
        testCage = !testCage;
    }

    void Start()
    {
        refTransform = new GameObject().transform;
        refTransform.parent = part.transform;
        refTransform.rotation = Quaternion.LookRotation(vessel.ReferenceTransform.up, -vessel.ReferenceTransform.forward);
        shipHeading = new Firespitter.ShipHeading(refTransform);

        try // overdoing the error protection a bit just because I can't be sure the renderer will be valid
        {
            Transform innerDiscTransform = base.internalProp.FindModelTransform(innerDiscName);
            if (innerDiscTransform != null)
            {
                innerDisc = innerDiscTransform.gameObject;
                discMat = innerDisc.GetComponent<Renderer>().material;
            }
        }
        catch
        {
            Debug.Log("FSartificialHorizon: Can't find object, or its material: " + innerDiscName);
        }

        if (discMat == null)
        {
            useOffset = false;
            Debug.Log("FSartificialHorizon: Couldn't find disc material");
        }

        if (useOffset)
        {
            testButton = base.internalProp.FindModelTransform(testButtonName).gameObject;
            minusButton = base.internalProp.FindModelTransform(minusButtonName).gameObject;
            plusButton = base.internalProp.FindModelTransform(plusButtonName).gameObject;
            testButtonHandler = testButton.AddComponent<FSgenericButtonHandler>();
            minusButtonHandler = minusButton.AddComponent<FSgenericButtonHandler>();
            plusButtonHandler = plusButton.AddComponent<FSgenericButtonHandler>();
            testButtonHandler.mouseDownFunction = testCageToggle;
            minusButtonHandler.mouseDownFunction = cageMinus;
            plusButtonHandler.mouseDownFunction = cagePlus;            
        }

        needle = base.internalProp.FindModelTransform(outerRingName);                        
    }

    public override void OnUpdate()
    {
        needle.localRotation = Quaternion.Euler(0f, 0f, rollDirection * shipHeading.getRoll(refTransform, Firespitter.Tools.WorldUp(vessel)));

        if (useOffset)
        {
            pitch = shipHeading.getPitch(refTransform, Firespitter.Tools.WorldUp(vessel));
            if (pitch > 90f) { pitch -= 180f; pitch *= -1f; }
            if (pitch < -90f) { pitch += 180f; pitch *= -1f; }
            if (testCage)
            {
                discMat.SetTextureOffset("_MainTex", new Vector2(0f, cageOffset));
            }
            else
            {
                discMat.SetTextureOffset("_MainTex", new Vector2(0f, Mathf.Clamp(pitch * pitchOffsetMultiplier, -pitchOffsetLimit, pitchOffsetLimit) + cageOffset));
            }
        }
    }
}
