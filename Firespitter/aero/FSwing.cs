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
    public string displayName = "Wing";
    //[KSPField]
    //public string windowTitle = "Wing Settings";
    //[KSPField]
    //public string mainLiftSurfaceName = "obj_main";
    [KSPField]
    public string controlSurfaceName = "obj_ctrlSrf";
    [KSPField]
    public string flapName = string.Empty;
    [KSPField]
    public string leadingEdgeTopName = string.Empty;
    [KSPField]
    public string leadingEdgeBottomName = string.Empty;
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
    //[KSPField]
    //public string flapRetractedName = "flapRetracted";
    //[KSPField]
    //public string flapExtendedName = "flapExtended";        
    [KSPField]
    public Vector3 controlSurfaceAxis = Vector3.right;
    [KSPField(isPersistant=true, guiName="Pitch Response", guiActiveEditor=true, guiActive=true), UI_FloatRange(minValue=-2f, maxValue=2f, stepIncrement=0.025f)]
    public float pitchResponse = 0f;
    [KSPField(isPersistant = true, guiName="Roll Response", guiActiveEditor=true, guiActive=true), UI_FloatRange(minValue=-2f, maxValue=2f, stepIncrement=0.025f)]
    public float rollResponse = 0f;
    [KSPField(isPersistant = true, guiName="Yaw Response", guiActiveEditor=true, guiActive=true), UI_FloatRange(minValue=-2f, maxValue=2f, stepIncrement=0.025f)]
    public float yawResponse = 0f;
    [KSPField(isPersistant = true, guiName="Flap Response", guiActiveEditor=true, guiActive=true), UI_FloatRange(minValue=-2f, maxValue=2f, stepIncrement=0.025f)]
    public float flapResponse = 0f;
    [KSPField]
    public float controlRotationSpeed = 0.2f;

    [KSPField]
    public bool allowInvertOnLeft = true;
    [KSPField(isPersistant = true)]
    public bool wingIsPointingLeft = true; // used to determine leading edge to use, etc. Sniffed on first launch.
    [KSPField(isPersistant = true)]
    public bool wingIsPointingUp = true; // used to determine leading edge to use, etc. Sniffed on first launch.
    [KSPField(isPersistant = true)]
    public bool isInFrontOfCoM;
    [KSPField(isPersistant = true)]
    public bool checkCoM = true;
    [KSPField(isPersistant = true)]
    public bool leadingEdgeExtended = false;
    [KSPField(isPersistant = true)]
    public bool positionOnVesselSet = false;

    [KSPField]
    public bool affectStockWingModule = false;
    [KSPField(isPersistant = true)]
    public float deflectionLiftCoeff = 0f;

    [KSPField]
    public float currentDeflectionLiftCoeff = 1.5f;
    [KSPField]
    public float ctrlSurfaceRange = 20f;
    [KSPField]
    public float controlLimiterMaxSpeed = 400f;
    [KSPField]
    public float controlLimiterMultiplier = 0.3f;
    
    private float limiterMultiplier = 1f;

    [KSPField]
    public bool showTweakables = true;
    [KSPField]
    public bool showHelp = true;
    [KSPField]
    public int moduleID = 0;    
    [KSPField]
    public bool debugMode = false;
    
    #endregion

    FSdebugMessages debug = new FSdebugMessages(false, FSdebugMessages.OutputMode.log, 2f);
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

    public Vector4 testAxis = Vector4.zero;
    private bool editorTransformsFound = false;

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

    [KSPField]
    public string helpTextString = string.Empty;
    public static string defaultHelpText = "Each wing has three control axis, and the flap axis. The sliders in the tweakable menu set their response to the Pitch, Roll and Yaw, taking input from the keyboard/joystick.\n\n<color=#99ff00ff>Testing</color>\nWhile in the Action Group editor you can test the control surface movement by pressing the W/A, Q/E, A/D and F keys.\n\n<color=#99ff00ff>Axis response</color>\nA setting of 1 in an axis will give the default control surface response. a 0 will give no response on that axis, and -1 will give the normal amount, but in the opposite direction.\nEach part has a default control response. Wings respond top roll only, rudders to yaw, and elevators to pitch. You can override these if you want to use an elevator as a rudder for instance.\n\n<color=#99ff00ff>Tweaks</color>\nYou can set up a bit or pitch on the main wings, or roll response on the rudder by setting a low number to an axis that is 0 by default.\nSome special settings are easier to do in flight mode because of SPH tweakable symmetry constraints.\n\n<color=#99ff00ff>Flaps</color>\nThe flap axis responds to Action Group inputs set up in the Action Group editor. Some wings also have a separate flap surface controlled by the action group. This can not be tested in the editor.";

    private Firespitter.gui.HelpPopup helpPopup;

    //private FSGUIPopup helpPopup;
    //private Rect windowRect = FSGUIwindowID.standardRect;
    //private PopupSection helpSection;
    //private int windowID = 0;
    private string helpTextInternal
    {
        get
        {
            if (helpTextString == string.Empty)
                return defaultHelpText;
            else
                return helpTextString;
        }
    }

    #endregion

    #region events and actions

    [KSPEvent(guiName = "Help", guiActiveEditor = true, guiActive = true)]
    public void showHelpEvent()
    {
        helpPopup.showMenu = true;
    }

    [KSPEvent(guiActive = false, guiName = "Toggle Leading Edge")]
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
        if (wingIsPointingLeft) invert = -1f;

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
            if (wingIsPointingLeft && leadingEdgeBottom != null)
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

    private void findTransforms(bool verboseErrors)
    {
        #region find transforms

        flap = part.FindModelTransform(flapName);
        if (flap != null)
        {
            useFlap = true;
            flapDefRot = flap.localRotation.eulerAngles;
        }
        else if (verboseErrors) debug.debugMessage("FSwing: did not find flap " + flapName);

        leadingEdgeTop = part.FindModelTransform(leadingEdgeTopName);
        if (leadingEdgeTop == null && verboseErrors) debug.debugMessage("FSwing: did not find leadingEdgeTop " + leadingEdgeTopName);
        leadingEdgeBottom = part.FindModelTransform(leadingEdgeBottomName);
        if (leadingEdgeBottom == null && verboseErrors) debug.debugMessage("FSwing: did not find leadingEdgeBottom " + leadingEdgeBottomName);
        if (leadingEdgeTop != null) useLeadingEdge = true;

        //flapRetracted = part.FindModelTransform(flapRetractedName);
        //if (flapRetracted == null && verboseErrors) debug.debugMessage("FSwing: did not find flapRetracted " + flapRetractedName);
        //flapExtended = part.FindModelTransform(flapExtendedName);
        //if (flapExtended == null && verboseErrors) debug.debugMessage("FSwing: did not find flapExtended " + flapExtendedName);

        leadingEdgeTopExtended = part.FindModelTransform(leadingEdgeTopExtendedName);
        if (leadingEdgeTopExtended == null && verboseErrors) debug.debugMessage("FSwing: did not find leadingEdgeTopExtended " + leadingEdgeTopExtendedName);
        leadingEdgeTopRetracted = part.FindModelTransform(leadingEdgeTopRetractedName);
        if (leadingEdgeTopRetracted == null && verboseErrors) debug.debugMessage("FSwing: did not find leadingEdgeTopRetracted " + leadingEdgeTopRetractedName);

        leadingEdgeBottomExtended = part.FindModelTransform(leadingEdgeBottomExtendedName);
        if (leadingEdgeBottomExtended == null && verboseErrors) debug.debugMessage("FSwing: did not find leadingEdgeBottomExtended " + leadingEdgeBottomExtendedName);
        leadingEdgeBottomRetracted = part.FindModelTransform(leadingEdgeBottomRetractedName);
        if (leadingEdgeBottomRetracted == null && verboseErrors) debug.debugMessage("FSwing: did not find leadingEdgeBottomRetracted " + leadingEdgeBottomRetractedName);

        controlSurface = part.FindModelTransform(controlSurfaceName);
        if (controlSurface == null)
        {
            if (verboseErrors) debug.debugMessage("FSwing: did not find controlSurface " + controlSurfaceName);
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
            List<Part> wings = new List<Part>(part.symmetryCounterparts);
            wings.Add(part);

            foreach (Part p in wings)
            {
                FSwing wing = p.GetComponent<FSwing>();
                if (wing != null)
                {
                    Vector4 applyAxis = new Vector4(inputAxis.x, inputAxis.y, inputAxis.z, inputAxis.w);                    
                    float dotRight = Vector3.Dot(p.transform.right, Vector3.right);
                    float dotUp = Vector3.Dot(p.transform.right, Vector3.up);                    

                    if (dotRight < -0.01f && allowInvertOnLeft) // check for orientation of the part, relative to world directions, since there is no vessel transfrom to compare to
                    {
                        applyAxis.x *= -1; //invert pitch, yaw and flap, but not roll.                        
                        applyAxis.w *= -1;                        
                    }
                    if (dotUp > 0f)
                    {
                        applyAxis.z *= -1;
                    }

                    float amount = (((applyAxis.x * pitchResponse) + (applyAxis.y * rollResponse) + (applyAxis.z * yawResponse)) * ctrlSurfaceRange) + (applyAxis.w * flapResponse * flapMax);                    
                    wing.controlSurface.localRotation = Quaternion.Euler(ctrllSrfDefRot + (amount * controlSurfaceAxis));                    
                }
            }
        }
        catch
        {
            //Debug.Log("FSwing testAxisSymmetryError");
        }
    }

    public override string GetInfo()
    {
        StringBuilder info = new StringBuilder();
        info.AppendLine("Advanced reconfigurable wing");
        info.Append("Default control range: ").AppendLine(ctrlSurfaceRange.ToString());
        if (leadingEdgeBottomName != string.Empty || leadingEdgeTopName != string.Empty)
        {
            if (autoDeployLeadingEdge)
                info.AppendLine("Leading Edge auto deploys");
            else
                info.AppendLine("Manually deployed leading edge");
        }
        if (flapName != string.Empty)
            info.AppendLine("Separate Flap surface");
        info.AppendLine("<color=#99ff00ff>Axis Response</color>");
        info.Append("Pitch: ").AppendLine(pitchResponse.ToString());
        info.Append("Roll: ").AppendLine(rollResponse.ToString());
        info.Append("Yaw: ").AppendLine(yawResponse.ToString());
        info.Append("Flap: ").AppendLine(flapResponse.ToString());
        return info.ToString();
    }

    public override void OnStart(PartModule.StartState state)
    {
        debug.debugMode = debugMode;

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
                    debug.debugMessage("FSwing: did not Find stock wing module");
                    affectStockWingModule = false;
                }
            }

            // get the main lift surface for the leading edge to manipulate
            if (affectStockWingModule)
            {
                useCtrlSrf = false;
            }
            else
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
                if (mainLift == null) debug.debugMessage("FSwing: leading edge missing main FSliftSurface: " + leadingEdgeLiftSurface);
            }            
        }
        #endregion        
        
        #region help popup

        helpPopup = new Firespitter.gui.HelpPopup("Wing setup help", helpTextInternal);

        /*helpSection = new PopupSection();
        PopupElement helpText = new PopupElement(helpTextnternal, true);
        helpSection.AddElement(helpText, 300f);
        if (windowID == 0)
            windowID = FSGUIwindowID.getNextID();
        helpPopup = new FSGUIPopup(part, "FSwing", 0, windowID, windowRect, "Wing setup help");
        helpPopup.sections.Add(helpSection);
        helpPopup.useInEditor = true;
        helpPopup.useInFlight = true;*/

        #endregion

        if (affectStockWingModule || !showTweakables)
        {
            Fields["pitchResponse"].guiActive = false;
            Fields["pitchResponse"].guiActiveEditor = false;
            Fields["rollResponse"].guiActive = false;
            Fields["rollResponse"].guiActiveEditor = false;
            Fields["yawResponse"].guiActive = false;
            Fields["yawResponse"].guiActiveEditor = false;
            Fields["flapResponse"].guiActive = false;
            Fields["flapResponse"].guiActiveEditor = false;
        }

        if (!useLeadingEdge || autoDeployLeadingEdge)
        {
            Events["toggleLeadingEdgeEvent"].guiActive = false;
        }
        
        if (affectStockWingModule || !showHelp)
        {
            Events["showHelpEvent"].guiActive = false;
            Events["showHelpEvent"].guiActiveEditor = false;
        }
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

                float dotRight = Vector3.Dot(part.transform.right, vessel.ReferenceTransform.right);                
                if (dotRight < -0.01f && allowInvertOnLeft)                
                {
                    //Debug.Log("FSwing: part is on the left: " + relativePosition);
                    wingIsPointingLeft = true;
                }
                else
                {
                    //Debug.Log("FSwing: part is on the right: " + relativePosition);
                    wingIsPointingLeft = false;
                }

                float dotUp = Vector3.Dot(part.transform.right, vessel.ReferenceTransform.forward); //forward is up! ugh!
                if (dotUp > 0f)
                {
                    //Debug.Log("FSwing: part is on the left: " + relativePosition);
                    wingIsPointingUp = true;
                }
                else
                {
                    //Debug.Log("FSwing: part is on the right: " + relativePosition);
                    wingIsPointingUp = false;
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
                if (wingIsPointingLeft && allowInvertOnLeft)
                {
                    input.x *= -1;
                    //input.z *= -1;
                    input.w *= -1;
                }
                if (!wingIsPointingUp)
                    input.z *= -1;

                float amount = (limiterMultiplier * ctrlSurfaceRange * ((input.x * pitchResponse) + (input.y * rollResponse) + (input.z * yawResponse))) + (input.w * flapResponse);                
                controlSurface.localRotation = Quaternion.Lerp(controlSurface.localRotation, Quaternion.Euler(ctrllSrfDefRot + (amount * controlSurfaceAxis)), controlRotationSpeed);
            }

            //if (useFlap)
            //{
            updateFlap();
            //}

        }
        #endregion
    }

    public override void OnFixedUpdate()
    {
        limiterMultiplier = Mathf.Max(controlLimiterMultiplier, -(((float)vessel.srf_velocity.magnitude - controlLimiterMaxSpeed) / controlLimiterMaxSpeed));        
    }

    public void Update()
    {
        #region editor

        

        //if (popup == null) return;

        if (HighLogic.LoadedSceneIsEditor)
        {
            if (!editorTransformsFound)
            {
                findTransforms(false);
                editorTransformsFound = true;
            }

            EditorLogic editor = EditorLogic.fetch;
            if (editor)
            {
                if (editor.editorScreen == EditorLogic.EditorScreen.Actions)// && popup.showMenu)
                {
                    Vector4 inputAxis = Vector4.zero;

                    if (Input.GetKey(KeyCode.S)) inputAxis.x += 1f; //GameSettings.PITCH_UP.primary
                    if (Input.GetKey(KeyCode.W)) inputAxis.x -= 1f;
                    if (Input.GetKey(KeyCode.Q)) inputAxis.y += 1f;
                    if (Input.GetKey(KeyCode.E)) inputAxis.y -= 1f;
                    if (Input.GetKey(KeyCode.A)) inputAxis.z -= 1f;
                    if (Input.GetKey(KeyCode.D)) inputAxis.z += 1f;
                    if (Input.GetKey(KeyCode.F)) inputAxis.w += 1f;
                    
                    inputAxis += testAxis;                    
                    // todo: limit the max
                    
                    Vector4 testAmount = Vector4.Lerp(oldTestAmount, inputAxis, 0.2f);
                    oldTestAmount = testAmount;                    
                    
                    //if (popup.partSelected)
                        testAxisOnSymmetry(testAmount); //, invertAxisTest);
                }
            }
        

        }

        #endregion
    }

    public void OnGUI()
    {
        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        {
            helpPopup.draw();
        }
    //    if (HighLogic.LoadedSceneIsEditor && popup != null)
    //    {
    //        //testAxis = Vector4.zero;
    //        //doTestSymmetry = false;

    //        //popup.popup();
    //        //updateValuesFromGUI();
    //    }
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
        //collapseElement = new PopupElement(new PopupButton(title, 0f));
        //collapseElement.buttons[0].isGUIToggle = true;
        //elements.Add(collapseElement);
        //if (axisResponse == 0f)
        //{
        //    collapseSection = true;
        //    collapseElement.buttons[0].toggleState = false;
        //}
        //else
        //{
        //    collapseSection = false;
        //    collapseElement.buttons[0].toggleState = true;
        //}

        responseElement = new PopupElement(title, response.ToString());
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
