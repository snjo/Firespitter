using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

class FSwing : PartModule
{
    #region kspfields
    [KSPField]
    public string mainLiftSurfaceName = "obj_main";
    [KSPField]
    public string controlSurfaceName = "obj_ctrlSrf";
    [KSPField]
    public string flapName = "obj_flap";
    [KSPField]
    public string leadingEdgeTopName;
    [KSPField]
    public string leadingEdgeBottomName;
    [KSPField]
    public string leadingEdgeTopRetractedName = "leadingEdgeRetracted";
    [KSPField]
    public string leadingEdgeTopExtendedName = "leadingEdgeExtended";
    [KSPField]
    public string leadingEdgeBottomRetractedName = "leadingEdgeRetracted";
    [KSPField]
    public string leadingEdgeBottomExtendedName = "leadingEdgeExtended";
    [KSPField]
    public bool leadingEdgeCanRotate = true; // if true, the edge will not only be moved between transforms, but also rotated.
    [KSPField]
    public float leadingEdgeSpeed = 0.01f;
    [KSPField]
    public float leadingEdgeDrag = 3f;
    [KSPField]
    public float leadingEdgeLift = 1f; // for stock wings, this increases lift coef, for FSliftSurface, it measures the increase in wingArea in meters
    [KSPField]
    public string leadingEdgeLiftSurface = "lift";
    [KSPField]
    public bool autoDeployLeadingEdge = false;
    [KSPField]
    public float autoLeadingEdgeSpeed = 90f;
    [KSPField]
    public float autoLeadingEdgeAoA = 15f;
    [KSPField(isPersistant = true)]
    public float flapTarget = 0f;
    [KSPField]
    public float flapIncrements = 10f;
    [KSPField]
    public float flapMax = 40f;
    [KSPField]
    public float flapMin = 0f;
    [KSPField]
    public float flapSpeed = 0.3f;
    [KSPField]
    public string flapRetractedName = "flapRetracted";
    [KSPField]
    public string flapExtendedName = "flapExtended";        
    [KSPField]
    public Vector3 controlSurfaceAxis = Vector3.right;
    [KSPField(isPersistant=true)]
    public float pitchResponse = 0f;
    [KSPField(isPersistant = true)]
    public float rollResponse = 0f;
    [KSPField(isPersistant = true)]
    public float yawResponse = 0f;
    [KSPField(isPersistant = true)]
    public float flapResponse = 0f;

    [KSPField]
    public bool allowInvertOnLeft = true;
    [KSPField(isPersistant = true)]
    public bool wingIsOnLeft = true; // used to determine leading edge to use, etc. Sniffed on first launch.
    [KSPField(isPersistant = true)]
    public bool isInFrontOfCoM;
    [KSPField(isPersistant = true)]
    public bool checkCoM = true;
    [KSPField(isPersistant = true)]
    public bool leadingEdgeExtended = false;
    [KSPField(isPersistant = true)]
    public bool positionOnVesselSet = false;

    [KSPField]
    public bool affectStockWingModule = true;
    [KSPField(isPersistant = true)]
    public float deflectionLiftCoeff = 0f;

    [KSPField]
    public float dragCoeff = 0.5f;
    [KSPField]
    public float currentDeflectionLiftCoeff = 1.5f;
    [KSPField]
    public float ctrlSurfaceRange = 20f;

    [KSPField]
    public int moduleID = 0;
    [KSPField]
    public string windowTitle = "Wing Settings";
    
    #endregion

    //public bool useMainSrf;
    public bool useCtrlSrf;
    public bool useFlap;
    public bool useLeadingEdge;

    private float leadingEdgeTarget = 0f;
    private float leadingEdgeCurrent = 0f;    
    private float flapCurrent = 0f;

    #region transforms
    public Transform mainLiftSurface;
    public Transform controlSurface;
    public Transform flap;
    public Transform flapRetracted;
    public Transform flapExtended;
    public Transform leadingEdgeTop;
    public Transform leadingEdgeBottom;
    public Transform leadingEdgeTopExtended;
    public Transform leadingEdgeTopRetracted;
    public Transform leadingEdgeBottomExtended;
    public Transform leadingEdgeBottomRetracted;
    #endregion

    private Vector3 ctrllSrfDefRot = new Vector3();
    private Vector3 flapDefRot = new Vector3();

    private ControlSurface stockWingModule;
    private FSliftSurface mainLift;
    private float mainLiftAreaDefault;

    #region gui variables

    private FSGUIPopup popup;
    //private PopupSection axisSection = new PopupSection();
    public WingAxisSection axisPitchSection;
    public WingAxisSection axisRollSection;
    public WingAxisSection axisYawSection;
    public WingAxisSection axisFlapSection;
    public PopupSection testAxisSection;
    private Rect windowRect = FSGUIwindowID.standardRect;
    public Vector4 testAxis = Vector4.zero;
    private bool editorTransformsFound = false;
    private bool doTestSymmetry = false;

    private float oldPitchResponse;
    private float oldRollResponse;
    private float oldYawResponse;
    private float oldFlapResponse;

    private bool invertAxisTest;

    private enum keys
    {
        pitchUp,
        pitchDown,
        rollLeft,
        rollRight,
        yawLeft,
        yawRight,
        flapPlus,
        flapMinus,
        reset,
    }
    private Vector4 oldTestAmount = Vector4.zero;

    #endregion

    #region events and actions

    [KSPEvent(guiActive = true, guiName = "Toggle Leading Edge")]
    public void toggleLeadingEdgeEvent()
    {
        setLeadingEdge(!leadingEdgeExtended);
    }

    [KSPAction("Toggle Leading Edge")]
    public void toggleLeadingEdgeAction(KSPActionParam param)
    {
        toggleLeadingEdgeEvent();
    }

    [KSPAction("Extend Flap")]
    public void extendFlapAction(KSPActionParam param)
    {
        flapTarget += flapIncrements;
        if (flapTarget > flapMax)
            flapTarget = flapMax;
    }

    [KSPAction("Retract Flap")]
    public void retractFlapAction(KSPActionParam param)
    {
        flapTarget -= flapIncrements;
        if (flapTarget < flapMin)
            flapTarget = flapMin;
    }

    #endregion

    private void updateFlap()
    {
        if (flapCurrent < flapTarget)
        {
            flapCurrent += flapSpeed;
            if (flapCurrent > flapMax)
                flapCurrent = flapMax;
        }
        else if (flapCurrent > flapTarget)
        {
            flapCurrent -= flapSpeed;
            if (flapCurrent < flapMin)
                flapCurrent = flapMin;
        }

        float invert = 1f;
        if (wingIsOnLeft) invert = -1f;

        if (useFlap)
            flap.localRotation = Quaternion.Euler(ctrllSrfDefRot + (controlSurfaceAxis * flapCurrent * invert));
    }

    public void updateDrag()  //only used if affectStockWingModule is true
    {
        if (leadingEdgeCurrent > 0f)
        {
            part.rigidbody.drag = leadingEdgeDrag * (float)vessel.atmDensity * leadingEdgeCurrent;
        }
        else
        {
            part.rigidbody.drag = 0f;
        }
    }

    public void setLeadingEdge(bool newState)
    {
        leadingEdgeExtended = newState;
        if (newState == true)
        {
            leadingEdgeTarget = 1f;
            //part.rigidbody.drag = leadingEdgeDrag * vessel.atmDensity * ;            
            if (affectStockWingModule)
            {
                currentDeflectionLiftCoeff = deflectionLiftCoeff + leadingEdgeLift;
                stockWingModule.deflectionLiftCoeff = currentDeflectionLiftCoeff;
            }
            else
            {
                mainLift.wingArea = mainLiftAreaDefault + leadingEdgeLift;
            }
        }
        else
        {
            leadingEdgeTarget = 0f;
            //part.rigidbody.drag = 0f;
            if (affectStockWingModule)
            {
                stockWingModule.deflectionLiftCoeff = deflectionLiftCoeff;
            }
            else
            {
                mainLift.wingArea = mainLiftAreaDefault;
            }
        }
    }

    public void updateLeadingEdgePosition()
    {
        if (leadingEdgeTarget != leadingEdgeCurrent)
        {
            if (wingIsOnLeft && leadingEdgeBottom != null)
            {
                leadingEdgeBottom.localPosition = Vector3.Lerp(leadingEdgeBottomRetracted.localPosition, leadingEdgeBottomExtended.localPosition, leadingEdgeCurrent);
                if (leadingEdgeCanRotate)
                    leadingEdgeBottom.localRotation = Quaternion.Lerp(leadingEdgeBottomRetracted.localRotation, leadingEdgeBottomExtended.localRotation, leadingEdgeCurrent);
            }
            else if (leadingEdgeTop != null)
            {
                leadingEdgeTop.localPosition = Vector3.Lerp(leadingEdgeTopRetracted.localPosition, leadingEdgeTopExtended.localPosition, leadingEdgeCurrent);
                if (leadingEdgeCanRotate)
                    leadingEdgeTop.localRotation = Quaternion.Lerp(leadingEdgeTopRetracted.localRotation, leadingEdgeTopExtended.localRotation, leadingEdgeCurrent);
            }
        }
    }

    private void updateValuesFromGUI()
    {        
        if (axisPitchSection == null) return;
        axisPitchSection.updateCollapseStatus();
        axisRollSection.updateCollapseStatus();
        axisYawSection.updateCollapseStatus();
        axisFlapSection.updateCollapseStatus();

        try
        {
            float.TryParse(axisPitchSection.responseElement.inputText, out pitchResponse);
        }
        catch{}

        try
        {
            float.TryParse(axisRollSection.responseElement.inputText, out rollResponse);
        }
        catch { }

        try
        {
            float.TryParse(axisYawSection.responseElement.inputText, out yawResponse);
        }
        catch { }

        try
        {
            float.TryParse(axisFlapSection.responseElement.inputText, out flapResponse);
        }
        catch { }

        if (oldPitchResponse != pitchResponse || oldRollResponse != rollResponse || oldYawResponse != yawResponse || oldFlapResponse != flapResponse)
        {
            List<Part> wings = new List<Part>(part.symmetryCounterparts);
            //wings.Add(part);
            foreach (Part p in wings)
            {
                FSwing wing = p.GetComponent<FSwing>();
                if (wing != null)
                {
                    wing.pitchResponse = pitchResponse;
                    wing.rollResponse = rollResponse;
                    wing.yawResponse = yawResponse;
                    wing.flapResponse = flapResponse;
                    wing.axisPitchSection.response = pitchResponse;
                    wing.axisRollSection.response = rollResponse;
                    wing.axisYawSection.response = yawResponse;
                    wing.axisFlapSection.response = flapResponse;                    
                }
            }
        }

        oldPitchResponse = pitchResponse;
        oldRollResponse = rollResponse;
        oldYawResponse = yawResponse;
        oldFlapResponse = flapResponse;
    }

    public PopupSection createTestSection()
    {
        float buttonWidth = 30f;
        PopupSection newSection = new PopupSection();

        PopupElement testElement = new PopupElement("Test", new PopupButton("Q", buttonWidth, testFunction, (int)keys.rollLeft));
        testElement.useTitle = false;
        testElement.buttons.Add(new PopupButton("W", buttonWidth, testFunction, (int)keys.pitchDown));
        testElement.buttons.Add(new PopupButton("E", buttonWidth, testFunction, (int)keys.rollRight));
        testElement.buttons.Add(new PopupButton("Fl.+", buttonWidth, testFunction, (int)keys.flapPlus));

        PopupElement testElement2 = new PopupElement("", new PopupButton("A", buttonWidth, testFunction, (int)keys.yawLeft));
        testElement2.useTitle = false;
        testElement2.buttons.Add(new PopupButton("S", buttonWidth, testFunction, (int)keys.pitchUp));
        testElement2.buttons.Add(new PopupButton("D", buttonWidth, testFunction, (int)keys.yawRight));
        testElement2.buttons.Add(new PopupButton("Fl.-", buttonWidth, testFunction, (int)keys.flapMinus));
        testElement2.buttons.Add(new PopupButton("Reset", buttonWidth*2, testFunction, (int)keys.reset));

        newSection.elements.Add(new PopupElement("Test settings"));
        newSection.elements.Add(testElement);
        newSection.elements.Add(testElement2);

        return newSection;
    }

    private void testFunction(int ID)
    {
        doTestSymmetry = true;
        testAxis = Vector4.zero;
        //invertAxisTest = true;

        if (ID > (int)keys.reset || ID < 0) ID = (int)keys.reset;
        keys IDasKey = (keys)ID;

        switch (IDasKey)
        {
            case keys.pitchUp: // Pitch ^
                testAxis.x = 1; 
                break;
            case keys.pitchDown: // Pitch v
                testAxis.x = -1;
                break;
            case keys.rollLeft:// Roll <
                testAxis.y = 1;
                //invertAxisTest = false;
                break;
            case keys.rollRight: // Roll >
                testAxis.y = -1;
                //invertAxisTest = false;
                break;
            case keys.yawLeft: // Yaw <
                testAxis.z = -1;
                break;
            case keys.yawRight: // Yaw >
                testAxis.z = 1;
                break;
            case keys.flapPlus: // Flap ^
                testAxis.w = 1;
                break;
            case keys.flapMinus: // Flap v
                testAxis.w = -1;
                break;
            case keys.reset: // Reset
                testAxis = Vector4.zero;
                break;
        }
        Debug.Log("test axis: " + testAxis);
    }

    private void findTransforms(bool verboseErrors)
    {
        #region find transforms
        controlSurface = part.FindModelTransform(controlSurfaceName);
        if (controlSurface != null) useCtrlSrf = true;
        else if (verboseErrors) Debug.Log("FSwing: did not find controlSurface " + controlSurfaceName);

        flap = part.FindModelTransform(flapName);
        if (flap != null)
        {
            useFlap = true;
            flapDefRot = flap.localRotation.eulerAngles;
        }
        else if (verboseErrors) Debug.Log("FSwing: did not find flap " + flapName);

        leadingEdgeTop = part.FindModelTransform(leadingEdgeTopName);
        if (leadingEdgeTop == null && verboseErrors) Debug.Log("FSwing: did not find leadingEdgeTop " + leadingEdgeTopName);
        leadingEdgeBottom = part.FindModelTransform(leadingEdgeBottomName);
        if (leadingEdgeBottom == null&& verboseErrors) Debug.Log("FSwing: did not find leadingEdgeBottom " + leadingEdgeBottomName);
        if (leadingEdgeTop != null) useLeadingEdge = true;

        flapRetracted = part.FindModelTransform(flapRetractedName);
        if (flapRetracted == null && verboseErrors) Debug.Log("FSwing: did not find flapRetracted " + flapRetractedName);
        flapExtended = part.FindModelTransform(flapExtendedName);
        if (flapExtended == null && verboseErrors) Debug.Log("FSwing: did not find flapExtended " + flapExtendedName);

        leadingEdgeTopExtended = part.FindModelTransform(leadingEdgeTopExtendedName);
        if (leadingEdgeTopExtended == null && verboseErrors) Debug.Log("FSwing: did not find leadingEdgeTopExtended " + leadingEdgeTopExtendedName);
        leadingEdgeTopRetracted = part.FindModelTransform(leadingEdgeTopRetractedName);
        if (leadingEdgeTopRetracted == null && verboseErrors) Debug.Log("FSwing: did not find leadingEdgeTopRetracted " + leadingEdgeTopRetractedName);

        leadingEdgeBottomExtended = part.FindModelTransform(leadingEdgeBottomExtendedName);
        if (leadingEdgeBottomExtended == null && verboseErrors) Debug.Log("FSwing: did not find leadingEdgeBottomExtended " + leadingEdgeBottomExtendedName);
        leadingEdgeBottomRetracted = part.FindModelTransform(leadingEdgeBottomRetractedName);
        if (leadingEdgeBottomRetracted == null && verboseErrors) Debug.Log("FSwing: did not find leadingEdgeBottomRetracted " + leadingEdgeBottomRetractedName);

        controlSurface = part.FindModelTransform(controlSurfaceName);
        if (controlSurface == null)
        {
            if (verboseErrors) Debug.Log("FSwing: did not find controlSurface " + controlSurfaceName);
        }
        else
        {
            useCtrlSrf = true;
            ctrllSrfDefRot = controlSurface.localRotation.eulerAngles;
        }

        #endregion
    }

    public void testAxisOnSymmetry(Vector4 inputAxis) //, bool allowInvert)
    {        
        try
        {

            //float invert = 1f;
            List<Part> wings = new List<Part>(part.symmetryCounterparts);
            wings.Add(part);            
            foreach (Part p in wings)
            {
                FSwing wing = p.GetComponent<FSwing>();
                if (wing != null)
                {
                    //wing.targetAngle = availableAnglesList[selectedListAngle];
                    float dot = Vector3.Dot(wing.part.transform.right, Vector3.right);
                    if (dot < 0f && allowInvertOnLeft) // check for orientation of the part, relative to world directions, since there is no vessel transfrom to compare to
                    {
                        inputAxis.x *= -1; //invert pitch, yaw and flap, but not roll.
                        inputAxis.z *= -1;
                        inputAxis.w *= -1;                        
                    }

                    float amount = (((inputAxis.x * pitchResponse) + (inputAxis.y * rollResponse) + (inputAxis.z * yawResponse)) * ctrlSurfaceRange) + (inputAxis.w * flapResponse * flapMax);                    
                    wing.controlSurface.localRotation = Quaternion.Euler(ctrllSrfDefRot + (amount * controlSurfaceAxis));
                }
            }
        }
        catch
        {
            //Debug.Log("FSwing testAxisSymmetryError");
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        #region fligth mode

        if (HighLogic.LoadedSceneIsFlight)
        {

            findTransforms(true);

            // Check if a stock wing module is present, if not, manipulate FSliftSurface stuff instead.
            if (affectStockWingModule)
            {
                //Debug.Log("FSwing: getting stock wing module");
                stockWingModule = part as ControlSurface;
                if (stockWingModule != null)
                {
                    //Debug.Log("FSwing: success");
                }
                else
                {
                    Debug.Log("FSwing: did not Find stock wing module");
                    affectStockWingModule = false;
                }
            }

            // get the main lift surface for the leading edge to manipulate
            if (!affectStockWingModule)
            {
                FSliftSurface[] surfaces = part.GetComponents<FSliftSurface>();
                foreach (FSliftSurface surface in surfaces)
                {
                    if (surface.liftTransformName == leadingEdgeLiftSurface)
                    {
                        mainLift = surface;
                        mainLiftAreaDefault = surface.wingArea;
                        //Debug.Log("FSwing: Slat assigned main lift to: " + surface.liftTransformName);
                        break;
                    }
                }
                if (mainLift == null) Debug.Log("FSwing: leading edge missing main FSliftSurface: " + leadingEdgeLiftSurface);
            }

            if (!useLeadingEdge || autoDeployLeadingEdge)
            {
                Events["toggleLeadingEdgeEvent"].guiActive = false;
            }

        }
        #endregion

        #region editor mode

        if (HighLogic.LoadedSceneIsEditor)
        {
            popup = new FSGUIPopup(part, "FSwing", moduleID, FSGUIwindowID.wing, windowRect, windowTitle);
            axisPitchSection = new WingAxisSection("Pitch Response", pitchResponse);
            axisRollSection = new WingAxisSection("Roll Response", rollResponse);
            axisYawSection = new WingAxisSection("Yaw Response", yawResponse);
            axisFlapSection = new WingAxisSection("Flap Response", flapResponse);
            popup.sections.Add(axisPitchSection);
            popup.sections.Add(axisRollSection);
            popup.sections.Add(axisYawSection);
            popup.sections.Add(axisFlapSection);

            testAxisSection = createTestSection();
            popup.sections.Add(testAxisSection);

            popup.showCloseButton = false;
            popup.useInActionEditor = true;
            popup.useInFlight = false;

            oldPitchResponse = pitchResponse;
            oldRollResponse = rollResponse;
            oldYawResponse = yawResponse;
            oldFlapResponse = flapResponse;

        }
        #endregion
    }

    public override void OnUpdate()
    {
        #region flight
        if (HighLogic.LoadedSceneIsFlight)
        {

            if (!positionOnVesselSet) //run only the first time the craft is loaded
            {
                // test whether the part is in the front or rear of the craft
                Transform CoMTransform = new GameObject().transform;
                CoMTransform.position = vessel.CoM;
                CoMTransform.rotation = vessel.transform.rotation;
                Vector3 relativePosition = CoMTransform.InverseTransformPoint(part.transform.position);
                if (relativePosition.y < 0)
                {
                    isInFrontOfCoM = false;
                }
                else
                {
                    isInFrontOfCoM = true;
                }

                if (relativePosition.x < 0)
                {
                    //Debug.Log("FSwing: part is on the left: " + relativePosition);
                    wingIsOnLeft = true;
                }
                else
                {
                    //Debug.Log("FSwing: part is on the right: " + relativePosition);
                    wingIsOnLeft = false;
                }

                positionOnVesselSet = true;
            }

            FlightCtrlState ctrl = vessel.ctrlState;

            if (useLeadingEdge)
            {
                if (autoDeployLeadingEdge)
                {
                    if (vessel.horizontalSrfSpeed < autoLeadingEdgeSpeed || Mathf.Abs(mainLift.AngleOfAttack) > autoLeadingEdgeAoA)
                        setLeadingEdge(true);
                    else
                        setLeadingEdge(false);
                }

                if (leadingEdgeCurrent < leadingEdgeTarget)
                {
                    leadingEdgeCurrent += leadingEdgeSpeed;
                    if (leadingEdgeCurrent > 1f)
                        leadingEdgeCurrent = 1f;
                }
                else if (leadingEdgeCurrent > leadingEdgeTarget)
                {
                    leadingEdgeCurrent -= leadingEdgeSpeed;
                    if (leadingEdgeTarget < 0f)
                        leadingEdgeCurrent = 0f;
                }
                updateLeadingEdgePosition();
                if (affectStockWingModule)
                    updateDrag();
            }

            if (useCtrlSrf)
            {
                Vector4 input = new Vector4(ctrl.pitch, -ctrl.roll, ctrl.yaw, flapCurrent);
                if (wingIsOnLeft && allowInvertOnLeft)
                {
                    input.x *= -1;
                    input.z *= -1;
                    input.w *= -1;
                }
                float amount = (ctrlSurfaceRange * ((input.x * pitchResponse) + (input.y * rollResponse) + (input.z * yawResponse))) + (input.w * flapResponse);                
                controlSurface.localRotation = Quaternion.Euler(ctrllSrfDefRot + (amount * controlSurfaceAxis));
            }

            //if (useFlap)
            //{
            updateFlap();
            //}

        }
        #endregion
    }

    public void Update()
    {
        #region editor

        if (popup == null) return;

        if (HighLogic.LoadedSceneIsEditor)
        {
            //Debug.Log("editor " + testAxis);
            if (!editorTransformsFound)
            {
                findTransforms(false);
                editorTransformsFound = true;
            }

            EditorLogic editor = EditorLogic.fetch;
            if (editor)
            {
                if (editor.editorScreen == EditorLogic.EditorScreen.Actions && popup.showMenu)
                {
                    Vector4 inputAxis = Vector4.zero;

                    if (Input.GetKey(KeyCode.S)) inputAxis.x += 1f; //GameSettings.PITCH_UP.primary
                    if (Input.GetKey(KeyCode.W)) inputAxis.x -= 1f;
                    if (Input.GetKey(KeyCode.Q)) inputAxis.y += 1f;
                    if (Input.GetKey(KeyCode.E)) inputAxis.y -= 1f;
                    if (Input.GetKey(KeyCode.A)) inputAxis.z -= 1f;
                    if (Input.GetKey(KeyCode.D)) inputAxis.z += 1f;
                    
                    inputAxis += testAxis;                    
                    // todo: limit the max
                    
                    Vector4 testAmount = Vector4.Lerp(oldTestAmount, inputAxis, 0.2f);
                    oldTestAmount = testAmount;
                    //controlSurface.localRotation = Quaternion.Euler(ctrllSrfDefRot + (amount * controlSurfaceAxis));

                    //if (doTestSymmetry)
                    testAxisOnSymmetry(testAmount); //, invertAxisTest);
                }
            }
        

        }

        #endregion
    }

    public void OnGUI()
    {
        if (HighLogic.LoadedSceneIsEditor && popup != null)
        {
            //testAxis = Vector4.zero;
            doTestSymmetry = false;
            popup.popup();
            updateValuesFromGUI();
        }
    }
}

public class WingAxisSection : PopupSection
{    
    private bool _collapseSection = true;
    public bool collapseSection
    {
        get
        {
            return _collapseSection;
        }
        set
        {
            _collapseSection = value;
            setCollapseState(value);
        }
    }

    public string displayName = "";
    public PopupElement collapseElement;
    public PopupElement responseElement;
    public PopupElement testElement;
    public PopupElement testElement2;
    private float _response;
    public float response
    {
        get
        {
            return _response;
        }
        set
        {
            _response = value;
            if (value != 0f)
                collapseSection = false;
            if  (responseElement != null)
                responseElement.inputText = value.ToString();
        }
    }
    
    public Firespitter.IntVector2 collapseRange = new Firespitter.IntVector2(1, 0);

    public void setCollapseState(bool newState)
    {
        Firespitter.IntVector2 range = collapseRange;
        if (range.y > elements.Count-1 || range.y < range.x)
        {
            range.y = elements.Count-1;
        }

        if (range.x < elements.Count)
        {
            for (int i = range.x; i <= range.y; i++)
            {
                elements[i].showElement = newState;
            }
        }

        if (newState == false)
        {
            //response = 0f;            
        }
    }

    public WingAxisSection(string title, float axisResponse)
    {
        displayName = title;
        response = axisResponse;
        collapseElement = new PopupElement(new PopupButton(title, 0f));
        collapseElement.buttons[0].isGUIToggle = true;
        elements.Add(collapseElement);
        if (axisResponse == 0f)
        {
            collapseSection = true;
            collapseElement.buttons[0].toggleState = false;
        }
        else
        {
            collapseSection = false;
            collapseElement.buttons[0].toggleState = true;
        }

        responseElement = new PopupElement("Amount/Direction", response.ToString());
        responseElement.titleSize = FSGUIwindowID.standardRect.width - 115f;
        responseElement.inputSize = 80f;
        elements.Add(responseElement);
    }

    public WingAxisSection()
    {     
    }

    public void updateCollapseStatus()
    {
        collapseSection = collapseElement.buttons[0].toggleState;
    }
}
