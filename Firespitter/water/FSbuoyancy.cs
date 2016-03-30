using System;
using UnityEngine;

public class FSbuoyancy : PartModule
{
    [KSPField(isPersistant=true, guiName="Buoyancy", guiActive = false, guiActiveEditor = true), UI_FloatRange(minValue=0f, maxValue=50f, stepIncrement=1f)]
    public float buoyancyForce = 12f; // the force applied to lift the part, scaled by depth according to buoyancyRange
    [KSPField]
    public double buoyancyRange = 1f; // the max depth at which the buoyancy will be scaled up. at this depth, the force applied is equal to buiyoancyForce. At 0 depth, the force is 0
    [KSPField]
    public float buoyancyVerticalOffset = 0.1f; // how high the part rides on the water in meters. Not a position offset inside the part. This will be applied in the global axis regardless of part rotation. Think iceberg/styrofoam.
    [KSPField]
    public float maxVerticalSpeed = 0.2f; // the max speed vertical speed at which there will be a lifitng force applied. Reduces bobbing.
    [KSPField]
    public float dragInWater = 1.5f; // when in water, apply drag to slow the craft down. Stock drag is 3.
    [KSPField]
    public bool debugMode = true;
    [KSPField]
    public float waterImpactTolerance = 125f;
    [KSPField]
    public string forcePointName; // if defined, this is the point that's checked for height, and where the force is applied. allows for several modules on one part through use of many named forecePoints. If undefined, uses part.transform
    [KSPField(isPersistant=true, guiName = "Splash FX", guiActive = false, guiActiveEditor = true), UI_Toggle(enabledText="On", disabledText="Off")]
    public bool splashFXEnabled = true;

    public Transform forcePoint;
    public float buoyancyIncrements = 1f; // using the events, increase or decrease buoyancyForce by this amount
    //private float defaultMinDrag;
    //private float defaultMaxDrag;
    public bool splashed;
    private float splashTimer = 0f;
    public float splashCooldown = 0.5f;

    //[KSPEvent(guiActive = false, guiName = "increase buoyancy")]
    //public void increaseBuoyancyEvent()
    //{
    //    buoyancyForce += buoyancyIncrements;
    //    Debug.Log("buoyancy: " + buoyancyForce);
    //}

    //[KSPEvent(guiActive = false, guiName = "decrease buoyancy")]
    //public void decreaseBuoyancyEvent()
    //{
    //    buoyancyForce -= buoyancyIncrements;
    //    Debug.Log("buoyancy: " + buoyancyForce);
    //}

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        //defaultMinDrag = part.minimum_drag;
        //defaultMaxDrag = part.maximum_drag;
        if (forcePointName != string.Empty)
        {
            forcePoint = part.FindModelTransform(forcePointName);
        }
        if (forcePointName == string.Empty || forcePoint == null)
        {
            forcePoint = part.transform;
        }
        if (debugMode)
        {
            //Events["increaseBuoyancyEvent"].guiActive = true;
            //Events["decreaseBuoyancyEvent"].guiActive = true;
            Fields["buoyancyForce"].guiActive = true;
            if (forcePointName != string.Empty)
            {
                //Events["increaseBuoyancyEvent"].guiName = "increase buoy " + forcePointName;
                //Events["decreaseBuoyancyEvent"].guiName = "decrease buoy " + forcePointName;
                Fields["buoyancyForce"].guiName = forcePointName + "Buoyancy";
            }
        }
    }

    public void FixedUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight)
            return;
        if (vessel.mainBody.ocean && part.GetComponent<Rigidbody>() != null)
        {
            if (part.partBuoyancy != null)
            {
                Destroy(part.partBuoyancy);
            }
            
            double partAltitude = Vector3d.Distance(forcePoint.position, vessel.mainBody.position) - vessel.mainBody.Radius - buoyancyVerticalOffset;
            if (partAltitude < 0d)
            {
                // float code

                double floatMultiplier = Math.Max(0d, -Math.Max(partAltitude, -buoyancyRange)) / buoyancyRange;

                if (floatMultiplier > 0f)
                {
                    Vector3d up = (this.vessel.GetComponent<Rigidbody>().position - this.vessel.mainBody.position).normalized;
                    Vector3d uplift = up * buoyancyForce * floatMultiplier;

                    //float relativeDirection = Vector3.Dot(vessel.GetComponent<Rigidbody>().velocity.normalized, up);                        

                    if (vessel.verticalSpeed < maxVerticalSpeed) // || relativeDirection < 0f) // if you are going down, apply force regardless, of going up, limit up speed
                    {
                        this.part.GetComponent<Rigidbody>().AddForceAtPosition(uplift, forcePoint.position);
                    }
                }

                // set water drag

                part.GetComponent<Rigidbody>().drag = dragInWater;

                // splashed status

                splashed = true;
                part.WaterContact = true;
                part.vessel.Splashed = true;

                // part destruction

                if (base.GetComponent<Rigidbody>().velocity.magnitude > waterImpactTolerance)
							{								
								GameEvents.onCrashSplashdown.Fire(new EventReport(FlightEvents.SPLASHDOWN_CRASH, this.part, this.part.partInfo.title, "ocean", 0, "FSbuoyancy: Hit the water too hard"));
								this.part.Die();
								return;
							}

                //FX                

                if (splashFXEnabled)
                {
                    splashTimer -= Time.deltaTime;
                    if (splashTimer <= 0f)
                    {
                        splashTimer = splashCooldown;
                        if (base.GetComponent<Rigidbody>().velocity.magnitude > 6f && partAltitude > -buoyancyRange) // don't splash if you are deep in the water or going slow
                        {
                            if (Vector3.Distance(base.transform.position, FlightGlobals.camera_position) < 500f)
                            {
                                FXMonger.Splash(base.transform.position, base.GetComponent<Rigidbody>().velocity.magnitude / 50f);
                            }
                        }
                    }
                }
                                
            }
            else
            {
                if (splashed)
                {
                    splashed = false;

                    // set air drag
                    part.GetComponent<Rigidbody>().drag = 0f;

                    part.WaterContact = false;
                    part.vessel.checkSplashed();
                }
            }
        }
    }
}

