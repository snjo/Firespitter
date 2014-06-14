using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSengine : PartModule
    {
        /// <summary>
        /// The total force applied when at full RPM, and full throttle, scaled by the various curves
        /// </summary>
        [KSPField(guiName = "Max Thrust", guiActiveEditor = true)]
        public float maxThrust = 100f;
        /// <summary>
        /// Current Thrust is scaled by the (current RPM / maxRPM) value. RPM is built up by the powerProduction value, and taken away by powerDrain and engineBrake
        /// </summary>
        [KSPField(guiName = "Max RPM", guiActive = false, guiActiveEditor = false), UI_FloatRange(minValue = 100, maxValue = 1000, stepIncrement = 1)]
        public float maxRPM = 600f;
        /// <summary>
        /// Sets how much thrust you get at different atmospheric densities. Fuel consumption remains constant.
        /// Fed to a floatCurve in the format "key,value;key,value" or "key,value,inTangent,outTangent;key,value,inTangent,outTangent"
        /// </summary>
        [KSPField]
        public string atmosphericThrust = "0,0;1,1";
        /// <summary>
        /// Basically the same as the velocityCurve in stock, tells you how much thrust to give at different speeds.
        /// Fed to a floatCurve in the format "key,value;key,value" or "key,value,inTangent,outTangent;key,value,inTangent,outTangent"
        /// </summary>
        [KSPField]
        public string velocityLimit = "0,1;200,1;500,0";
        /// <summary>
        /// Sets how much fuel is spent at the curent throttle setting. Allows for fuel use when idling, and fuel use ramping up as the throttle is higher.
        /// Fed to a floatCurve in the format "key,value;key,value" or "key,value,inTangent,outTangent;key,value,inTangent,outTangent"
        /// </summary>
        [KSPField]
        public string fuelConsumption = "0,0.0001;1,0.01";
        /// <summary>
        /// Sets the thrust at various throttle settings. This means built in afterburner support. In tandem with fuelConsumption, you could have a steep thrust and fuel use ramp up at 70%-100%.
        /// Fed to a floatCurve in the format "key,value;key,value" or "key,value,inTangent,outTangent;key,value,inTangent,outTangent"
        /// </summary>
        [KSPField]
        public string throttleThrust = "0,0;1,1";
        /// <summary>
        /// Fuel values in the format "resourceName,ratio;resourceName,ratio". I prefer normalized values over integers, but that's optional.
        /// </summary>
        [KSPField]
        public string resources = "LiquidFuel,1;IntakeAir,15";
        [KSPField]
        public string thrustTransformName = "thrustTransform";
        /// <summary>
        /// The teakable slider value for max throttle value seen in flight and in the hangar.
        /// </summary>
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max throttle", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.01f)]
        public float maxThrottle = 100f;
        /// <summary>
        /// If the engine receives less than this amount of fuel (normalized), it flames out. a low threshold makes sense for electric engines that could run on partial power.
        /// Should be less than 1f to account for math issues with the supplied fuel amount
        /// </summary>
        [KSPField]
        public float flameoutThreshold = 0.7f;
        /// <summary>
        /// Is the engine powered up? (It might still be flamed out)
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool EngineIgnited = false;
        /// <summary>
        /// If the engine is ignited but gets insufficient fuel, it will flame out.
        /// </summary>
        public bool flameout = false;
        public bool staged = false;                
        public Transform[] thrustTransforms;

        public float finalThrust = 0f;
        public float finalThrustNormalized = 0f;
        public float thrustPerTransform = 0f;

        [KSPField(guiActive = false, guiName = "Requested throttle")]
        public float requestedThrottle = 0f;
        public float smoothFxThrust = 0f;
        [KSPField]
        public float smoothFXSpeed = 0.1f;

        [KSPField(guiActive = false, guiName = "Power Production")]
        public float powerProduction = 20f; // RPM gain when ignited and fuelled
        [KSPField]
        public float engineBrake = 15f; // engine RPM slowdown when not ignited
        [KSPField(guiActive = true, guiName = "Current RPM")]
        public float RPM = 0f;
        [KSPField(guiActive = false, guiName = "Power Drain")]
        public float powerDrain = 10f; // blade drag for instance. scales with throttle setting

        [KSPField(guiName = "Current Thrust", guiActive = true)]
        public float thrustInfo = 0f;
        [KSPField(guiName = "Status", guiActive = true)]
        public string status = "Inactive";
        [KSPField(guiName = "Cause", guiActive = false)]
        public string cause = "";

        [KSPField]
        public bool debugMode = false;

        public enum FSengineType
        {
            normal,
            bladed
        }

        public FSengineType type = FSengineType.normal;

        protected FloatCurve atmosphericThrustCurve = new FloatCurve();
        protected FloatCurve velocityCurve = new FloatCurve();
        protected FloatCurve fuelConsumptionCurve = new FloatCurve();
        protected FloatCurve throttleThrustCurve = new FloatCurve();
        protected List<FSresource> resourceList = new List<FSresource>();        

        [KSPEvent(guiName = "Activate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void Activate()
        {
            EngineIgnited = true;
            staged = true;            
        }

        [KSPEvent(guiName = "Deactivate Engine", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void Deactivate()
        {
            EngineIgnited = false;
        }

        protected void updateStatus()
        {
            if (EngineIgnited)
            {
                if (flameout)
                {
                    status = "Flameout!";                    
                    Fields["cause"].guiActive = true;                    
                                        
                    //toadicus: UIPartActionWindow::displayDirty = true /does/ make the tweakable window redraw next frame                    
                }
                else
                {
                    status = "Running";
                    Fields["cause"].guiActive = false;
                }
            }
            else
            {
                status = "Inactive";
                Fields["cause"].guiActive = false;
            }
        }

        public void updateFX()
        {
            if (EngineIgnited && !flameout)
            {
                part.Effect("running", Mathf.Clamp(smoothFxThrust, 0.01f, 1f));
            }
            else
                part.Effect("running", 0f);
        }

        public float maxThrottleNormalized
        {
            get
            {
                return maxThrottle / 100f;
            }
        }

        public float RPMnormalized
        {
            get
            {
                return Mathf.Clamp(RPM / maxRPM, 0f, 1f);
            }
        }

        protected void fillResourceList(string resourceString)
        {
            string[] keyString = resourceString.Split(';');
            for (int i = 0; i < keyString.Length; i++)
            {
                string[] valueString = keyString[i].Split(',');
                if (valueString.Length > 0)
                {
                    try
                    {
                        resourceList.Add(new FSresource(valueString[0].Trim(' '), float.Parse(valueString[1])));
                        //Debug.Log("FSengine: Added resource " + valueString[0] + ", ratio " + valueString[1]);
                    }
                    catch
                    {
                        //Debug.Log("FSengine: could not add resource to list: " + valueString[0]);
                    }
                }
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            //part.stackIcon.SetIcon(DefaultIcons.LIQUID_ENGINE);
            part.stagingIcon = "LIQUID_ENGINE";
            thrustTransforms = part.FindModelTransforms(thrustTransformName);

            velocityCurve = Firespitter.Tools.stringToFloatCurve(velocityLimit);
            atmosphericThrustCurve = Firespitter.Tools.stringToFloatCurve(atmosphericThrust);
            fuelConsumptionCurve = Firespitter.Tools.stringToFloatCurve(fuelConsumption);
            throttleThrustCurve = Firespitter.Tools.stringToFloatCurve(throttleThrust);
            fillResourceList(resources);
        }

        public virtual void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            calculateFinalThrust();

            //burn fuel        
            float fuelReceivedNormalized = consumeResources();

            if (EngineIgnited && !flameout)
            {
                RPM += powerProduction * TimeWarp.deltaTime * fuelReceivedNormalized;
            }
            else
            {
                RPM -= (engineBrake + (Mathf.Abs(requestedThrottle) * powerDrain)) * TimeWarp.deltaTime; // for reducing engine power when it's no longer ignited                
            }

            RPM = Mathf.Clamp(RPM, 0f, maxRPM);

            float thrustTransformRelativeSpeed = getForwardSpeed();

            float applyThrust = thrustPerTransform * RPMnormalized * atmosphericThrustCurve.Evaluate((float)vessel.atmDensity) * velocityCurve.Evaluate(thrustTransformRelativeSpeed);
            thrustInfo = applyThrust * thrustTransforms.Length;

            for (int i = 0; i < thrustTransforms.Length; i++)
            {
                rigidbody.AddForceAtPosition(-thrustTransforms[i].forward * applyThrust, thrustTransforms[i].position);
            }
            smoothFxThrust = Mathf.Lerp(smoothFxThrust, finalThrustNormalized, smoothFXSpeed);

            updateStatus();
        }

        private float getForwardSpeed()
        {
            Vector3 vel = GetVelocity(part.rigidbody, thrustTransforms[0].position);
            float thrustTransformRelativeSpeed = Vector3.Dot(-vel.normalized, thrustTransforms[0].forward) * vel.magnitude;
            thrustTransformRelativeSpeed = Mathf.Max(0f, thrustTransformRelativeSpeed);            
            return thrustTransformRelativeSpeed;
        }

        public Vector3 GetVelocity(Rigidbody rigidbody, Vector3 refPoint) // from Ferram
        {
            Vector3 newVelocity = Vector3.zero;
            try
            {
                newVelocity += rigidbody.GetPointVelocity(refPoint);
                newVelocity += Krakensbane.GetFrameVelocityV3f() - Krakensbane.GetLastCorrection() * TimeWarp.fixedDeltaTime;
            }
            catch (Exception e)
            {
                if (debugMode)
                    Debug.Log("FSengineBladed GetVelocity Exception " + e.GetType().ToString());
            }
            return newVelocity;
        }

        protected virtual void calculateFinalThrust()
        {
            finalThrust = maxThrust * Mathf.Clamp(requestedThrottle, -maxThrottleNormalized, maxThrottleNormalized) * throttleThrustCurve.Evaluate(requestedThrottle);
            thrustPerTransform = finalThrust / thrustTransforms.Length;
            finalThrustNormalized = finalThrust / maxThrust;
        }

        /// <summary>
        /// In regaler engines, returns finalThrustNormalized, in bladed engines etc, can return a normalized float representing the amount of work the engine had to do to keep RPM up
        /// </summary>        
        protected virtual float getWorkDone()
        {
            return finalThrustNormalized;
        }

        protected virtual float consumeResources()
        {
            float fuelReceivedNormalized = 0f;
            float lowestResourceSupply = 1f;
            if (EngineIgnited)
            {
                for (int i = 0; i < resourceList.Count; i++)
                {                                       
                    float requestFuelAmount = fuelConsumptionCurve.Evaluate(getWorkDone()) * maxThrust * resourceList[i].ratio * TimeWarp.deltaTime;

                    //Debug.Log("reqf: fcc " + fuelConsumptionCurve.Evaluate(finalThrustNormalized) + " maxTh " + maxThrust + " resLr " + resourceList[i].ratio);

                    if (requestFuelAmount > 0f)
                    {
                        float fuelReceived = part.RequestResource(resourceList[i].name, requestFuelAmount);
                        resourceList[i].currentSupply = Mathf.Clamp(fuelReceived / requestFuelAmount, 0f, 1f);
                        if (resourceList[i].currentSupply < 0.1f) cause = resourceList[i].name + " deprived";
                    }
                    //Debug.Log("resource " + i + " : " + requestFuelAmount + ", " + resourceList[i].name);

                    lowestResourceSupply = Mathf.Min(lowestResourceSupply, resourceList[i].currentSupply);
                }

                fuelReceivedNormalized = lowestResourceSupply;

                if (fuelReceivedNormalized < flameoutThreshold
                    && fuelConsumptionCurve.Evaluate(finalThrustNormalized) > 0f
                    && throttleThrustCurve.Evaluate(requestedThrottle) > 0f)
                {
                    flameout = true;
                    //Debug.Log("flameout: " + fuelReceivedNormalized.ToString() + " / fcc " + fuelConsumptionCurve.Evaluate(finalThrustNormalized));
                }
                else
                {
                    flameout = false;
                }
            }
            return fuelReceivedNormalized;
        }

        /// <summary>
        /// this can be overriden to make other modules control the throttle instead of reading the game's central throttle.
        /// </summary>
        /// <returns>Returns a normalized float</returns>
        public virtual float getThrottle()
        {
            return vessel.ctrlState.mainThrottle;
        }

        public override void OnUpdate()
        {

            maxThrottle = Mathf.Round(maxThrottle);

            if (HighLogic.LoadedSceneIsFlight)
            {
                requestedThrottle = getThrottle();
                updateFX();
            }
        }

        public override void OnActive()
        {
            Activate();
        }

        public void OnCenterOfThrustQuery(CenterOfThrustQuery CoTquery)
        {
            Vector3 newPos = Vector3.zero;
            Vector3 newDir = Vector3.zero;
            for (int i = 0; i < thrustTransforms.Length; i++)
            {
                newPos += thrustTransforms[i].position - part.transform.position;
                newDir += thrustTransforms[i].forward;
            }            
            CoTquery.pos = part.transform.position + (newPos / thrustTransforms.Length);
            CoTquery.dir = newDir.normalized;
            CoTquery.thrust = maxThrust * (maxThrottle / 100f);
        }        
    }
}