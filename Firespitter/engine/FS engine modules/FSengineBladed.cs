using System.Collections.Generic;
using UnityEngine;
using Firespitter.aero;
using Firespitter.info;

namespace Firespitter.engine
{
    public class FSengineBladed : FSengine
    {
        [KSPField]
        public string liftTransformName = "bladePoint";
        [KSPField]
        public string referenceTransformName = "bladeRef";
        [KSPField]
        public string rotorHubName = "mainrotor";
        [KSPField]
        public string bladeHubName = "blade";
        [KSPField]
        public string swashPlateName = "swashPlate";
        [KSPField]
        public string baseTransformName = "baseReference";
        [KSPField]
        public float power = 0.0008f;
        [KSPField]
        public float wingArea = 1f;
        [KSPField]
        public float span = 2f;
        [KSPField]
        public float efficiency = 0.5f; // normal is 1... trying to get more blade drag.
        [KSPField]
        public float dragMultiplier = 1f;
        [KSPField]
        public float zeroLiftDrag = 0.0161f;
        [KSPField]
        public float autoRotationGain = 0.005f;

        [KSPField(isPersistant = true, guiName = "Steering", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
        public bool steering = true;
        [KSPField(guiActive = true, guiName = "collective")]
        public float collective = 0f;
        //[KSPField(isPersistant = true, guiName = "Dedicated Keys", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
        //public bool useDedicatedKeys = false;
        [KSPField(isPersistant = true, guiName = "Thr Keys", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
        public bool useThrottleKeys = false;
        [KSPField(isPersistant = true, guiName = "Thr State", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
        public bool useThrottleState = true;        
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Steering Response"), UI_FloatRange(minValue = 0f, maxValue = 15f, stepIncrement = 0.1f)]
        public float steeringResponse = 1f;
        [KSPField]
        public bool tailRotor = false;
        [KSPField(isPersistant = true)]
        public bool fullSimulation = false;

        [KSPField(guiActive = true, guiName = "Vel thr. rotor")]
        public float airVelocity = 0f;
        [KSPField]
        public float maxCollectivePitch = 15f;
        [KSPField]
        public float rollMultiplier = 0.5f;

        private List<FSbladeLiftSurface> bladeLifts = new List<FSbladeLiftSurface>();
        private engine.FSpropellerTweak propTweak;

        [KSPField(guiActive=true, guiName="cyclic")]
        private Vector2 cyclic = Vector2.zero;

        public new FSengineType type = FSengineType.bladed; // overrides the normal value
        private Vector3 partVelocity = Vector3.zero;
        private float airSpeedThroughRotor = 0f;
        private float hoverCollective = 0f;
        private float longTermHoverCollective = 0f;
        private double hoverHeight = 0f;
        private float partFacingUp = 1f;
        private float circumeference = 25.13f;
        private float rotationDirection = 1f;
        public bool hoverMode = false;
        private bool resetHoverHeight = false;
        private float RPMtoRadperSec = 0.104719755f;
        private float bladeMidPoint = 0f;
        /// <summary>
        /// the amount of work done by the engine to keep the RPM up. Used by bladed engines for consuming fuel. Static in regular engines.
        /// </summary>
        protected float workDone = 1f;

        private bool flightStarted = false;

        private Transform rotorHubTransform;
        private Transform baseTransform;

        private info.FSdebugMessages debugB;

        //private LineRenderer debugLine;        

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
                Debug.Log("disabled hover, lthc: " + longTermHoverCollective);
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

        //[KSPEvent(guiName = "Activate Full Sim", guiActive = true, guiActiveEditor = true)]
        //public void toggleFullSimulation()
        //{
        //    fullSimulation = !fullSimulation;

        //    setFullSimulation(fullSimulation);
        //}

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            getThrottleDelegate = getThrottle;

            debugB = new FSdebugMessages(debugMode, "FSengineBladed");

            rotorHubTransform = part.FindModelTransform(rotorHubName);
            if (rotorHubTransform == null) debugB.debugMessage("rotorHubTransform is null");
            baseTransform = part.FindModelTransform(baseTransformName);
            if (baseTransform == null) debugB.debugMessage("baseTransform is null");

            if (HighLogic.LoadedSceneIsFlight)
            {
                setupBlades();
            }

            circumeference = span * Mathf.PI * 2f;
            bladeMidPoint = span;
            if (propTweak != null)
            {
                circumeference *= propTweak.bladeLengthSlider;
                bladeMidPoint *= propTweak.bladeLengthSlider;
            }

            if (HighLogic.LoadedSceneIsFlight)
                flightStarted = true;

            Fields["maxRPM"].guiActive = true;
            Fields["maxRPM"].guiActiveEditor = true;
            Fields["maxThrust"].guiActiveEditor = false;
            Fields["maxThrottle"].guiActive = false;
            Fields["maxThrottle"].guiActiveEditor = false;

            if (tailRotor)
            {
                Fields["useThrottleKeys"].guiActive = false;
                Fields["useThrottleKeys"].guiActiveEditor = false;
                Fields["useThrottleState"].guiActive = false;
                Fields["useThrottleState"].guiActiveEditor = false;
            }
        }

        private void setupBlades()
        {
            List<GameObject> newBlades = new List<GameObject>();
            propTweak = part.GetComponent<engine.FSpropellerTweak>();
            if (propTweak != null)
            {                
                propTweak.initialize();
                
                newBlades = propTweak.blades;
            }
            else
            {
                Transform[] newBladeTransforms = part.FindModelTransforms(bladeHubName);
                for (int i = 0; i < newBladeTransforms.Length; i++)
                {
                    newBlades.Add(newBladeTransforms[i].gameObject);
                }
            }
            bladeLifts.Clear();

            foreach (GameObject blade in newBlades)
            {
                FSbladeLiftSurface bladeLift = blade.AddComponent<FSbladeLiftSurface>();
                if (bladeLift != null)
                {
                    bladeLift.thisGameObject = blade;
                    bladeLift.liftTransformName = liftTransformName;
                    bladeLift.referenceTransformName = referenceTransformName;
                    bladeLift.power = power;
                    if (propTweak == null)
                    {
                        bladeLift.span = span;
                        bladeLift.wingArea = wingArea;
                    }
                    else
                    {
                        bladeLift.span = span * propTweak.bladeLengthSlider;
                        bladeLift.wingArea = wingArea * propTweak.bladeLengthSlider;
                    }
                    bladeLift.efficiency = efficiency;
                    bladeLift.dragMultiplier = dragMultiplier;
                    bladeLift.zeroLiftDrag = zeroLiftDrag;
                    bladeLift.part = part;
                    bladeLift.debugMode = debugMode;

                    bladeLift.initialize();
                    bladeLifts.Add(bladeLift);
                }
                debugB.debugMessage(bladeLifts.Count.ToString() + " blades added to bladeLifts");
            }            
        }

        public override void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !flightStarted || vessel != FlightGlobals.ActiveVessel) return;

            float airDirection = getAirSpeed();

            finalThrustNormalized = Mathf.Abs(collective) / maxCollectivePitch;
            finalThrust = finalThrustNormalized * maxThrust;

            double fuelReceivedNormalized = consumeResources();

            updateRPM(airDirection, fuelReceivedNormalized);

            setBladePitch();

            updateStatus();
        }

        private float getAirSpeed()
        {
            partVelocity = GetVelocity(GetComponent<Rigidbody>(), transform.position);
            float airDirection = Vector3.Dot(baseTransform.up, partVelocity.normalized);
            airSpeedThroughRotor = partVelocity.magnitude * airDirection;
            partFacingUp = Mathf.Sign(Vector3.Dot(vessel.upAxis, baseTransform.up));
            return airDirection;
        }

        private void updateRPM(float airDirection, double fuelReceivedNormalized)
        {               
            if (airDirection < 0f) // && collective <= 0f)
            {
                RPM -= airSpeedThroughRotor * autoRotationGain * (TimeWarp.deltaTime * 50f);
            }

            for (int i = 0; i < bladeLifts.Count; i++)
            {
                RPM -= bladeLifts[i].bladeDrag * 0.1f;
            }

            if (EngineIgnited && !flameout)
            {
                //float RPMgain = Mathf.Lerp(powerProduction, maxPowerProduction, propTweak.engineLengthSlider) * TimeWarp.deltaTime * fuelReceivedNormalized;
                float RPMgain = powerProduction * TimeWarp.deltaTime * (float)fuelReceivedNormalized;
                workDone = Mathf.Min(maxRPM - RPM, RPMgain) / (powerProduction * TimeWarp.deltaTime); // normalized                
                RPM += RPMgain;
            }
            else
            {
                RPM -= (engineBrake + (Mathf.Abs(requestedThrottle) * powerDrain)) * TimeWarp.deltaTime; // for reducing engine power when it's no longer ignited
                workDone = 0f;
            }                       
            
            RPM = Mathf.Clamp(RPM, 0f, maxRPM);
        }

        protected override float getWorkDone()
        {
            return workDone;
        }

        private float getThrottle()
        {
            if (tailRotor)
            {
                return 1f;
            }
            else
            {
                return vessel.ctrlState.mainThrottle;
            }
        }

        private void setBladePitch()
        {
            for (int i = 0; i < bladeLifts.Count; i++)
            {
                float bladePitchAligned = Vector3.Dot(bladeLifts[i].liftTransform.right, baseTransform.forward);
                float bladeRollAligned = -Vector3.Dot(bladeLifts[i].liftTransform.right, baseTransform.right);
                float bladeRotation = collective + (cyclic.x * bladePitchAligned * steeringResponse) + (cyclic.y * bladeRollAligned * steeringResponse * rollMultiplier);

                bladeLifts[i].bladePitch = bladeRotation;

                bladeLifts[i].liftTransform.localRotation = Quaternion.Euler((Vector3.right * -bladeRotation * rotationDirection));
                //bladeLifts[i].pointVelocityMagnitude = Mathf.Clamp((RPM * circumeference) / 60f, 0, 340f) * rotationDirection; // clamping to supersonic 340 m/s

                float tangentialSpeed = RPM * RPMtoRadperSec * bladeMidPoint;
                bladeLifts[i].pointVelocityMagnitude = Mathf.Clamp(tangentialSpeed, 0f, 340f) * rotationDirection;                

            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!HighLogic.LoadedSceneIsFlight || !flightStarted || vessel != FlightGlobals.ActiveVessel) return;

            getCollectiveInput();

            getSteeringInput();            
        }

        //private void drawDebugLine()
        //{
        //    if (debugMode)
        //    {
        //        debugLine.SetVertexCount(bladeLifts.Count + 1);
        //        for (int i = 0; i < bladeLifts.Count; i++)
        //        {
        //            Vector3 pos = bladeLifts[i].liftTransform.position + -bladeLifts[i].liftTransform.up * (bladeLifts[i].bladePitch / 6f);
        //            debugLine.SetPosition(i, pos);
        //            if (i == 0)
        //                debugLine.SetPosition(bladeLifts.Count, pos);
        //        }
        //    }
        //}

        public override void OnActive()
        {
            base.OnActive();
            //Debug.Log("Activated engine");
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

                //if (useDedicatedKeys)
                //{
                //    if (Input.GetKey(KeyCode.PageUp))
                //    {
                //        increaseCollective = true;
                //        userInput = true;
                //    }
                //    if (Input.GetKey(KeyCode.PageDown))
                //    {
                //        decreaseCollective = true;
                //        userInput = true;
                //    }
                //    if (Input.GetKey(KeyCode.End))
                //    {
                //        minCollective = true;
                //        userInput = true;
                //    }
                //    if (Input.GetKey(KeyCode.Home))
                //    {
                //        maxCollective = true;
                //        userInput = true;
                //    }
                //    if (Input.GetKey(KeyCode.Backspace))
                //    {
                //        noCollective = true;
                //        userInput = true;
                //    }
                //}
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

                    double heightOffset = vessel.altitude - hoverHeight;
                    float maxClimb = Mathf.Clamp(-(float)heightOffset, -10f, 10f) * 0.3f;

                    if (vessel.verticalSpeed * partFacingUp < maxClimb)
                    {
                        //Debug.Log("go up");
                        hoverCollective = maxCollectivePitch;
                        longTermHoverCollective = Mathf.Lerp(longTermHoverCollective, maxCollectivePitch, 0.01f);
                    }
                    else if (vessel.verticalSpeed * partFacingUp > maxClimb)
                    {
                        //Debug.Log("go down");
                        hoverCollective = -maxCollectivePitch;
                        longTermHoverCollective = Mathf.Lerp(longTermHoverCollective, -maxCollectivePitch, 0.01f);
                    }
                    else
                    {
                        //Debug.Log("go nowhere");
                        hoverCollective = 0f;
                        longTermHoverCollective = Mathf.Lerp(longTermHoverCollective, 0f, 0.01f);
                    }

                    //FSdebugMessages.Post("lthc: " + longTermHoverCollective.ToString(), false, 0.1f);

                    //hoverCollective = Mathf.Lerp(hoverCollective, Mathf.Sign(-airSpeedThroughRotor) * collectivePitch, 0.1f);
                    //collective = Mathf.Sign(-airSpeedThroughRotor) * collectivePitch;                

                    //Debug.Log(" as " + Math.Round(airSpeedThroughRotor, 2) + " maxClimb " +  Math.Round(maxClimb, 2) + " hoverHeight " +  (int)hoverHeight + " offset " + heightOffset);
                }
                else
                {
                    //regular flight

                    //FSdebugMessages.Post("lthc: " + longTermHoverCollective.ToString(), false, 0.1f);

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
                        collective = maxCollectivePitch;
                    if (minCollective)
                        collective = -maxCollectivePitch;
                    if (noCollective)
                        collective = 0f;

                    if (useThrottleState && !hoverMode)
                    {
                        //collective = (vessel.ctrlState.mainThrottle - 0.5f) * 2f * collectivePitch;
                        collective = vessel.ctrlState.mainThrottle * maxCollectivePitch;
                    }

                    resetHoverHeight = true;

                    //if (hoverMode)
                    //    collective = longTermHoverCollective;
                }

            }

            collective += hoverCollective;
            collective = Mathf.Clamp(collective, -maxCollectivePitch, maxCollectivePitch);
        }

        public override void OnCenterOfThrustQuery(CenterOfThrustQuery CoTquery)
        {
            if (tailRotor)
            {
                // do nothing
            }
            else
            {
                base.OnCenterOfThrustQuery(CoTquery);
            }
        }

    }
}
