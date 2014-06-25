using System.Collections.Generic;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSheliLiftEngine : PartModule
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
        [KSPField(guiActive = true, guiName = "Vel thr. rotor")]
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

        [KSPField(guiActive = true, guiName = "Input Options"), UI_Label()]
        public string labelText = string.Empty;
        [KSPField(isPersistant = true, guiName = "Dedicated Keys", guiActive = true, guiActiveEditor = true), UI_Toggle(enabledText = "", disabledText = "")]
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
        private aero.FSbladeLiftSurface[] heliLiftSrf;
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
        private info.FSinputVisualizer inputVisualizer = new Firespitter.info.FSinputVisualizer();

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
            if (!HighLogic.LoadedSceneIsFlight || !initialized || rigidbody == null) return;

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
            heliLiftSrf = part.GetComponents<aero.FSbladeLiftSurface>();
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
}