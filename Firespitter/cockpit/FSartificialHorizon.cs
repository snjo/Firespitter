using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSartificialHorizon :InternalModule
{
    public Transform needle;
    public Transform refTransform; // should correspond to vessel.referencetransform in KSP	
    //private Transform rollLevel;
    //private Transform pitchLevel;
    //public Transform angleJoint;
    //private Vector3 worldUp = Vector3.up;
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
    //[KSPField]
    //public string refDirectioName = "refDirection";
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

    //public float getRoll()
    //{
    //    rollLevel.rotation = Quaternion.LookRotation(refDirection.forward, worldUp);
    //    float result = rollLevel.localRotation.eulerAngles.z;
    //    if (result > 180f) result -= 360f;
    //    return result;
    //}

    //public float getPitch()
    //{
    //    //pitchLevel.position = refDirection.position;
    //    pitchLevel.rotation = Quaternion.LookRotation(refDirection.right, worldUp);
    //    float result = pitchLevel.localRotation.eulerAngles.z;
    //    if (result > 180f) result -= 360f;
    //    return result;
    //}

    // Use this for initialization
    void Start()
    {
        //rollLevel = new GameObject().transform;
        //rollLevel.transform.parent = refDirection;
        //rollLevel.localPosition = Vector3.zero;
        //pitchLevel = new GameObject().transform;
        //pitchLevel.transform.parent = refDirection;
        //pitchLevel.localPosition = Vector3.zero;
        refTransform = new GameObject().transform;
        refTransform.parent = part.transform;
        refTransform.rotation = Quaternion.LookRotation(vessel.ReferenceTransform.up, -vessel.ReferenceTransform.forward);
        shipHeading = new Firespitter.ShipHeading(refTransform);

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

            discMat = innerDisc.renderer.material;
        }

        needle = base.internalProp.FindModelTransform(outerRingName);        

        innerDisc = base.internalProp.FindModelTransform(innerDiscName).gameObject;
        if (discMat == null)
        {
            useOffset = false;
            //Debug.Log("FSartificialHorizon: Couldn't find disc material");
        }
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        //.LookAt(refDirection.forward, new Vector3(0f, 1f, 0f));
        //needle.rotation = Quaternion.LookRotation(refDirection.forward, Firespitter.Tools.worldUp(vessel));
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

        //if (angleJoint != null)
        //	angleJoint.localRotation = Quaternion.Euler(new Vector3(Mathf.Clamp(getPitch()*-0.2f, -5f, 5f), 0f, 0f));

        //Debug.Log(getRoll());
    }
}
