﻿using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    public class FStailRotorThrust : PartModule
    {
        ModuleEngines engine = new ModuleEngines();
        Transform partTransform;
        Transform rotorParentTransform;

        private int timeCount = 0;
        private float idleThrust = 0.001f;
        //private Boolean keyHeldDown = false;

        [KSPField]
        public string rotorparent = "rotor";
        [KSPField]
        public string thrustTransform = "thrustTransform";
        [KSPField]
        public int spinUpTime = 5;
        [KSPField]
        public float rotationSpeed = -700f; // in RPM
        
        private Quaternion defaultRotation = Quaternion.identity;
        //[KSPField]
        //public float trimAmount = 0.1f;

        //[KSPField(guiActive = true, guiName = "Trim", isPersistant = true)]
        //public float trim = 0f;

        [KSPField(guiActive = true, guiName = "max Thrust", isPersistant = true)]
        public float maxThrust = 1;

        [KSPField(guiActive = true, guiName = "use QE, not AD", isPersistant = true)]
        public bool altInputModeEnabled = false;

        [KSPField(guiActive = true, guiName = "Inverted Left/Right", isPersistant = true)]
        public bool invertInput = false;

        private bool initialized = false;

        //[KSPField(guiActive = true, guiName = "Trim with Alt Key", isPersistant = true)]
        //public bool trimWithAlt = true;

        [KSPEvent(name = "toggleAltInputMode", active = true, guiActive = true, guiActiveEditor = true, guiName = "QE or AD to rotate")]
        public void toggleAltInputMode()
        {
            altInputModeEnabled = !altInputModeEnabled;
        }

        [KSPEvent(name = "toggleInvertInput", active = true, guiActive = true, guiActiveEditor = true, guiName = "Invert Left/Right")]
        public void toggleInvertInput()
        {
            invertInput = !invertInput;
        }

        [KSPEvent(name = "increaseThrust", active = true, guiActive = true, guiName = "Increase Thrust")]
        public void increaseThrust()
        {
            setThrust(1);
        }

        [KSPEvent(name = "reduceThrust", active = true, guiActive = true, guiName = "Reduce Thrust")]
        public void reduceThrust()
        {
            setThrust(-1);
        }

        //[KSPEvent(name = "toggleTrimWithAlt", active = true, guiActive = true, guiName = "Toggle trim with Alt")]
        //public void toggleTrimWithAlt()
        //{
        //    trimWithAlt = !trimWithAlt;
        //}

        //[KSPEvent(name = "Reset trim", active = true, guiActive = true, guiName = "Reset Trim")]
        //public void resetTrimEvent(KSPActionParam param)
        //{
        //    resetTrim();
        //}

        //[KSPAction("Reset trim")]
        //public void resetTrimAction(KSPActionParam param)
        //{
        //    resetTrim();
        //}

        private void resetTrim()
        {
            if (altInputModeEnabled)
            {
                vessel.ctrlState.rollTrim = 0f;
            }
            else
            {
                vessel.ctrlState.yawTrim = 0f;
            }
        }

        public void setThrust(int modifier)
        {
            if (modifier == 1)
            {
                maxThrust *= 2;
            }
            else if (modifier == -1)
            {
                maxThrust /= 2;
            }
            if (maxThrust < 0.25f) maxThrust = 0.25f;
            if (maxThrust > 32f) maxThrust = 32f;
        }

        public override void OnStart(PartModule.StartState state)
        {            
            engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
            partTransform = part.FindModelTransform(thrustTransform);
            rotorParentTransform = part.FindModelTransform(rotorparent);
            if (partTransform != null)
            {
                defaultRotation = partTransform.localRotation;
                initialized = true;
            }
            else
            {
                Debug.Log("FSrotorTrim: Could not find partTransform '" + thrustTransform + "', disabling module");
            }
        }

        public void FixedUpdate()
        {
            if (initialized)
            {
                if (!HighLogic.LoadedSceneIsFlight) return;
                FlightCtrlState ctrl = vessel.ctrlState;
                Vector3 steeringInput = new Vector3(0, 0, 0);

                if (altInputModeEnabled)
                {
                    steeringInput.y = ctrl.roll;
                }
                else
                {
                    steeringInput.y = ctrl.yaw;
                }

                //bool inputReceived = steeringInput.y != 0f;
                //Debug.Log("Force: " + steeringInput.y);

                if (invertInput) steeringInput *= -1; // if the part is upside down, you can toggle inverse controls for it.            

                engine.throttleLocked = true;

                if (steeringInput.y > 0)
                {
                    partTransform.localRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(0, -1, 0)) * defaultRotation;
                }
                else
                {
                    partTransform.localRotation = defaultRotation;
                }

                if (steeringInput.y == 0)
                {
                    engine.maxThrust = idleThrust;
                }
                else
                {
                    engine.maxThrust = maxThrust * Mathf.Abs(steeringInput.y);
                }

                // blade rotation

                bool engineActive = engine && engine.getIgnitionState && !engine.getFlameoutState;

                if (engineActive && timeCount < 1000)
                {
                    timeCount += spinUpTime;
                }
                else if (!engineActive && timeCount > 0)
                {
                    timeCount -= spinUpTime;
                }

                if (timeCount < 0) timeCount = 0; // in case people give the spinUpTime in an unexpected way

                float currentSpeed = ((rotationSpeed * 6) * TimeWarp.deltaTime * ((float)timeCount / 1000));
                if (rotorParentTransform != null)
                {
                    rotorParentTransform.transform.Rotate(Vector3.forward * currentSpeed);
                }
            }
        }
    }
}