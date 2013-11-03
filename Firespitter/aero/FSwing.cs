using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

class FSwing : PartModule
{
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
    [KSPField]
    public float pitchResponse = 0f;
    [KSPField]
    public float rollResponse = 0f;
    [KSPField]
    public float yawResponse = 0f;
    [KSPField]
    public float flapResponse = 0f;

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
    //[KSPField]
    //public float ctrlSurfaceArea = 0.2f;

    public bool useMainSrf;
    public bool useCtrlSrf;
    public bool useFlap;
    public bool useLeadingEdge;

    private float leadingEdgeTarget = 0f;
    private float leadingEdgeCurrent = 0f;    
    private float flapCurrent = 0f;

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

    private Vector3 ctrllSrfDefRot = new Vector3();
    private Vector3 flapDefRot = new Vector3();

    private ControlSurface stockWingModule;
    private FSliftSurface mainLift;
    private float mainLiftAreaDefault;

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
        flap.localRotation = Quaternion.Euler(ctrllSrfDefRot + (controlSurfaceAxis * flapCurrent * invert));
    }

    public void updateDrag()
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
            }
            else if (leadingEdgeTop != null)
            {
                leadingEdgeTop.localPosition = Vector3.Lerp(leadingEdgeTopRetracted.localPosition, leadingEdgeTopExtended.localPosition, leadingEdgeCurrent);
            }
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
    	base.OnStart(state);

        if (HighLogic.LoadedSceneIsFlight)
        {

            #region find transforms
            //mainLiftSurface = part.FindModelTransform(mainLiftSurfaceName);
            //if (mainLiftSurface != null) useMainSrf = true;
            //else Debug.Log("FSwing: did not find mainLiftSurface " + mainLiftSurfaceName);

            controlSurface = part.FindModelTransform(controlSurfaceName);
            if (controlSurface != null) useCtrlSrf = true;
            else Debug.Log("FSwing: did not find controlSurface " + controlSurfaceName);

            flap = part.FindModelTransform(flapName);
            if (flap != null)
            {
                useFlap = true;
                flapDefRot = flap.localRotation.eulerAngles;
            }
            else Debug.Log("FSwing: did not find flap " + flapName);

            leadingEdgeTop = part.FindModelTransform(leadingEdgeTopName);
            if (leadingEdgeTop == null) Debug.Log("FSwing: did not find leadingEdgeTop " + leadingEdgeTopName);
            leadingEdgeBottom = part.FindModelTransform(leadingEdgeBottomName);
            if (leadingEdgeBottom == null) Debug.Log("FSwing: did not find leadingEdgeBottom " + leadingEdgeBottomName);
            if (leadingEdgeTop != null) useLeadingEdge = true;

            flapRetracted = part.FindModelTransform(flapRetractedName);
            if (flapRetracted == null) Debug.Log("FSwing: did not find flapRetracted " + flapRetractedName);
            flapExtended = part.FindModelTransform(flapExtendedName);
            if (flapExtended == null) Debug.Log("FSwing: did not find flapExtended " + flapExtendedName);

            leadingEdgeTopExtended = part.FindModelTransform(leadingEdgeTopExtendedName);
            if (leadingEdgeTopExtended == null) Debug.Log("FSwing: did not find leadingEdgeTopExtended " + leadingEdgeTopExtendedName);
            leadingEdgeTopRetracted = part.FindModelTransform(leadingEdgeTopRetractedName);
            if (leadingEdgeTopRetracted == null) Debug.Log("FSwing: did not find leadingEdgeTopRetracted " + leadingEdgeTopRetractedName);

            leadingEdgeBottomExtended = part.FindModelTransform(leadingEdgeBottomExtendedName);
            if (leadingEdgeBottomExtended == null) Debug.Log("FSwing: did not find leadingEdgeBottomExtended " + leadingEdgeBottomExtendedName);
            leadingEdgeBottomRetracted = part.FindModelTransform(leadingEdgeBottomRetractedName);
            if (leadingEdgeBottomRetracted == null) Debug.Log("FSwing: did not find leadingEdgeBottomRetracted " + leadingEdgeBottomRetractedName);

            controlSurface = part.FindModelTransform(controlSurfaceName);
            if (controlSurface == null)
            {
                Debug.Log("FSwing: did not find controlSurface " + controlSurfaceName);
            }
            else
            {
                useCtrlSrf = true;
                ctrllSrfDefRot = controlSurface.localRotation.eulerAngles;
            }

            #endregion

            // Check if a stock wing module is present, if not, manipulate FSliftSurface stuff instead.
            if (affectStockWingModule)
            {
                Debug.Log("FSwing: getting stock wing module");
                stockWingModule = part as ControlSurface;
                if (stockWingModule != null)
                {
                    Debug.Log("FSwing: success");
                }
                else
                {
                    Debug.Log("FSwing: failure");
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
                        Debug.Log("FSwing: Slat assigned main lift to: " + surface.liftTransformName);
                        break;
                    }
                }
                if (mainLift == null) Debug.Log("FSwing: mssing main FSliftSurface: " + leadingEdgeLiftSurface);
            }

            if (!useLeadingEdge)
            {
                Events["toggleLeadingEdgeEvent"].guiActive = false;
            }
                
        }
    }

    public override void OnUpdate()
    {
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
                    Debug.Log("FSwing: part is on the left: " + relativePosition);
                    wingIsOnLeft = true;
                }
                else
                {
                    Debug.Log("FSwing: part is on the right: " + relativePosition);
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
                float roll = ctrl.roll;
                if (wingIsOnLeft) roll *= -1;
                float amount = (ctrlSurfaceRange * ( (ctrl.pitch * pitchResponse) + (roll * rollResponse) + (ctrl.yaw * yawResponse) )) + (flapCurrent * flapResponse);
                if (wingIsOnLeft) amount *= -1;
                controlSurface.localRotation = Quaternion.Euler(ctrllSrfDefRot + (amount * controlSurfaceAxis));
            }

            if (useFlap)
            {
                updateFlap();
            }

        }
    }

    //public void OnGUI()
    //{
    //    GUI.Label(new Rect(100f, 100f, 400f, 50f), "kraken frameSpd: " + Krakensbane.GetFrameVelocity().magnitude);
    //    GUI.Label(new Rect(100f, 140f, 400f, 50f), "kraken partSpd : " + part.Rigidbody.velocity.magnitude);
    //    GUI.Label(new Rect(100f, 180f, 400f, 50f), "kraken frameVel: " + Krakensbane.GetFrameVelocity());
    //    GUI.Label(new Rect(100f, 220f, 400f, 50f), "kraken lastCorr: " + Krakensbane.GetLastCorrection());
    //}
}
