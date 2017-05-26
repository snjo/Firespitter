using System;
using UnityEngine;

namespace Firespitter.engine
{
    /// <summary>
    /// Helicopter steering module
    /// </summary>
    public class FSrotorTrim : PartModule
    {
        [KSPField]
        public string targetPartObject = "thrustTrimObject";
        [KSPField]
        public string hoverKey = "f";
        [KSPField]
        public float rotationDirectionX = 0f;
        [KSPField]
        public float rotationDirectionY = 0f;
        [KSPField]
        public float rotationDirectionZ = 1f;
        [KSPField]
        public float defaultRotationX = 0f;
        [KSPField]
        public float defaultRotationY = 0f;
        [KSPField]
        public float defaultRotationZ = 0f;
        [KSPField]
        public float steerAmount = 20f;
        [KSPField]
        public float hoverHeatModifier = 5f;
        [KSPField]
        public bool useTransformTranslation = false;
        [KSPField]
        public float translationDistance = 0.5f;
        //[KSPField]
        //public string rootPart = "copterEngineMain";

        [KSPField(guiActive = true, guiName = "Steering", isPersistant = true)]
        public bool steeringEnabled = false;
        [KSPField(guiActive = true, guiName = "use AD, not QE", isPersistant = true)]
        public bool altInputModeEnabled = false;

        private Vector3 currentRotation = new Vector3(0, 0, 0);

        private Transform partTransform;
        private Vector3 thrustTransformDefaultPosition = Vector3.zero;
        private Transform modifiedUp;

        private bool initialized = false;

        [KSPAction("Toggle Steering")]
        public void toggleSteeringAction(KSPActionParam param)
        {
            toggleSteering();
        }

        [KSPEvent(name = "toggleSteering", active = true, guiActive = true, guiName = "Toggle Steering")]
        public void toggleSteering()
        {
            steeringEnabled = !steeringEnabled;
        }

        [KSPEvent(name = "toggleAltInputMode", active = true, guiActive = true, guiName = "QE or AD to rotate")]
        public void toggleAltInputMode()
        {
            altInputModeEnabled = !altInputModeEnabled;
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private void resetTrim()
        {
            vessel.ctrlState.pitchTrim = 0f;
            if (altInputModeEnabled)
            {
                vessel.ctrlState.yawTrim = 0f;
            }
            else
            {
                vessel.ctrlState.rollTrim = 0f;
            }
        }

        public void steerPart(float steerDegrees, Vector3 axis)
        {
            float steerThrustModifier = vessel.ctrlState.mainThrottle / 1.7f; // engine.currentThrottle / 1.7f;
            currentRotation = steerDegrees * axis * (1 - steerThrustModifier);
        }

        public void translateThrustTransform(Vector3 steeringInput)
        {
            float steerThrustModifier = vessel.ctrlState.mainThrottle / 1.7f;
            partTransform.localPosition = thrustTransformDefaultPosition;
            partTransform.position -= vessel.ReferenceTransform.up.normalized * translationDistance * steeringInput.z * steerThrustModifier;
            partTransform.position -= vessel.ReferenceTransform.right.normalized * translationDistance * steeringInput.x * steerThrustModifier;
        }


        private void setPartRotation()
        {
            partTransform.localRotation = Quaternion.Euler(currentRotation + new Vector3(defaultRotationX, defaultRotationY, defaultRotationZ));
        }

        private void autoHover()
        {
            {
                Vector3 heading = (Vector3d)this.vessel.transform.up;
                Vector3d up = (this.vessel.GetComponent<Rigidbody>().position - this.vessel.mainBody.position).normalized;

                modifiedUp.rotation = Quaternion.LookRotation(up, heading);
                modifiedUp.Rotate(new Vector3(-90, 0, 180));

                partTransform.localRotation = Quaternion.Euler(currentRotation + new Vector3(defaultRotationX, defaultRotationY, defaultRotationZ));
                partTransform.rotation = Quaternion.RotateTowards(partTransform.rotation, modifiedUp.rotation, steerAmount * 4);
            }

        }

        public override void OnStart(PartModule.StartState state)
        {
            partTransform = part.FindModelTransform(targetPartObject);
            if (partTransform != null)
            {
                thrustTransformDefaultPosition = partTransform.localPosition;
                initialized = true;
            }
            else
            {
                Debug.Log("FSrotorTrim: Could not find partTransform '" + targetPartObject + "', disabling module");
            }
            modifiedUp = new GameObject("ModifiedUpTransform").transform;
            modifiedUp.parent = part.transform;
            modifiedUp.localPosition = Vector3.zero;
        }

        public void FixedUpdate()
        {
            if (initialized)
            {
                if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;

                FlightCtrlState ctrl = vessel.ctrlState;

                Vector3 steeringInput = new Vector3(0, 0, 0);

                if (altInputModeEnabled)
                {
                    steeringInput.x = ctrl.yaw;
                }
                else
                {
                    steeringInput.x = ctrl.roll;
                }

                steeringInput.z = -ctrl.pitch;

                bool inputReceived = (steeringInput != new Vector3(0, 0, 0));

                if (steeringEnabled) // && inputReceived)
                {
                    if (useTransformTranslation)
                    {
                        translateThrustTransform(steeringInput);
                    }
                    else
                    {
                        steerPart(steerAmount, new Vector3(steeringInput.x, steeringInput.y, steeringInput.z));
                    }
                }
                else steerPart(0, steeringInput);

                if (!useTransformTranslation)
                {
                    if (Input.GetKey(hoverKey)) //Auto hover
                    {
                        autoHover();
                    }
                    else
                    {
                        setPartRotation();
                    }
                }
            }
        }
    }
}
