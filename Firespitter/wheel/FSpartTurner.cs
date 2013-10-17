using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using UnityEngine;

//namespace Firespitter


   public class FSpartTurner : PartModule
    {
        [KSPField]
        public string targetPartObject = "turnEmpty";
        [KSPField]
        public string targetPartObject2;
        [KSPField]
        public string targetPartObject3;
        [KSPField]
        public string targetPartObject4;
        [KSPField]
        public string targetPartObject5;
        [KSPField]
        public string targetPartObject6;
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
        [KSPField(guiActive = true, guiName = "Steering Multiplier", isPersistant = true)]
        public float steerMultiplier = 10f;
        [KSPField]
        public float steerMaxSpeed = 15f;
        [KSPField]
        public bool speedAdjustedSteering = true;
        [KSPField]
        public float speedAdjustedSteeringMinimumMultiplier = 0.1f;

        [KSPField(guiActive=true, guiName="Steering", isPersistant=true)]
        public bool steeringEnabled = false;
        [KSPField(guiActive=true, guiName="use QE, not AD", isPersistant=true)]
        public bool altInputModeEnabled = false;
        [KSPField(guiActive = true, guiName = "Reverse Steering", isPersistant = true)]
        public bool reversedInput = false;
        [KSPField(guiActive = true, guiName = "Ignore Trim", isPersistant = true)]
        public bool ignoreTrim = true;
        [KSPField]
        public bool useWheelSteeringAxis = true; // use the new separate axis for wheels instead of roll/yaw. will also disable ignore trim and alternate input buttons

        [KSPField]
        public int moduleID = 0;

        private Vector3 currentTrim = new Vector3(0,0,0);
        private Vector3 currentRotation = new Vector3(0, 0, 0);
        private Vector3 steeringInput;
        private List<Transform> partTransforms = new List<Transform>();

        public FSGUIPopup popup;
        public PopupElement elementSteeringEnabled;
        public PopupElement elementInvertSteering;
        public PopupElement elementSteeringRange;

        private float oldSteerMultiplier = 0f;

        [KSPAction("Toggle Steering")]
        public void toggleSteeringAction(KSPActionParam param)
        {
            toggleSteering();
        }

        [KSPAction("Invert Steering")]
        public void toggleInvertAction(KSPActionParam param)
        {
            reversedInput = !reversedInput;
        }

        [KSPEvent(name = "toggleSteering", active = true, guiActive = true, guiName = "Toggle Steering")]
        public void toggleSteering()
        {
            steeringEnabled = !steeringEnabled;
        }

        [KSPEvent(name = "toggleAltInputMode", active = true, guiActive = true, guiName = "QE or AD to steer")]
        public void toggleAltInputMode()
        {
            altInputModeEnabled = !altInputModeEnabled;
        }

        [KSPEvent(name = "toggleIgnoreTrim", active = true, guiActive = true, guiName = "Toggle Ignore trim")]
        public void toggleIgnoreTrim()
        {
            ignoreTrim = !ignoreTrim;
        }

        [KSPEvent(name = "toggleReverseInput", active = true, guiActive = true, guiName = "Toggle Reverse Steering")]
        public void toggleReverseInput()
        {
            reversedInput = !reversedInput;
        }

        [KSPEvent(name = "increaseSteering", active = true, guiActive = true, guiName = "Increase Steering")]
        public void increaseSteering()
        {
            steerMultiplier += 1f;
            if (steerMultiplier > 90f) steerMultiplier = 90f;
        }

        [KSPEvent(name = "decreaseSteering", active = true, guiActive = true, guiName = "Decrease Steering")]
        public void decreaseSteering()
        {
            steerMultiplier -= 1;
            if (steerMultiplier < 1f) steerMultiplier = 1f;
        }

        [KSPEvent(name = "ToggleSpeedAdjustedSteering", active = true, guiActive = true, guiName = "Dynamic Steering")]
        public void toggleSpeedAdjustedSteeringEvent()
        {
            speedAdjustedSteering = !speedAdjustedSteering;
        }

        public void steerPart(float direction)
        {
            float steerModifier = 1f;
            if (speedAdjustedSteering)
            {
                steerModifier = Mathf.Max(speedAdjustedSteeringMinimumMultiplier, -(((float)vessel.horizontalSrfSpeed - steerMaxSpeed) / steerMaxSpeed));
            }            

            currentRotation.x = (steerMultiplier * rotationDirectionX * direction * steerModifier) + defaultRotationX;
            currentRotation.y = (steerMultiplier * rotationDirectionY * direction * steerModifier) + defaultRotationY;
            currentRotation.z = (steerMultiplier * rotationDirectionZ * direction * steerModifier) + defaultRotationZ;
        }
                
        private void setPartRotation()
        {
            foreach (Transform t in partTransforms)
            {
                t.localRotation = Quaternion.Euler(currentRotation);
            }
        }

        private void popupToggleSteering()
        {
            toggleSteering();
            elementSteeringEnabled.buttons[0].toggle(steeringEnabled);

            foreach (Part p in part.symmetryCounterparts)
            {
                FSpartTurner wheel = p.GetComponent<FSpartTurner>();
                if (wheel != null)
                {
                    wheel.steeringEnabled = steeringEnabled;
                    wheel.elementSteeringEnabled.buttons[0].toggle(steeringEnabled);
                }
            }
        }

        private void popupToggleReverseInput()
        {
            toggleReverseInput();
            elementInvertSteering.buttons[0].toggle(reversedInput);

            foreach (Part p in part.symmetryCounterparts)
            {
                FSpartTurner wheel = p.GetComponent<FSpartTurner>();
                if (wheel != null)
                {
                    wheel.reversedInput = reversedInput;
                    wheel.elementInvertSteering.buttons[0].toggle(reversedInput);
                }
            }
        }

        private void addTransformToList(string targetName)
        {
            if (targetName != string.Empty)
            {
                Transform newTransform = part.FindModelTransform(targetName);
                if (newTransform != null)
                {
                    partTransforms.Add(newTransform);
                }
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {
                addTransformToList(targetPartObject);
                addTransformToList(targetPartObject2);
                addTransformToList(targetPartObject3);
                addTransformToList(targetPartObject4);
                addTransformToList(targetPartObject5);
                addTransformToList(targetPartObject6);

                if (useWheelSteeringAxis)
                {
                    Events["toggleIgnoreTrim"].guiActive = false;
                    Events["toggleAltInputMode"].guiActive = false;
                    Fields["altInputModeEnabled"].guiActive = false;
                    Fields["ignoreTrim"].guiActive = false;
                }                
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                #region GUI popup

                elementSteeringEnabled = new PopupElement("Enabled", new PopupButton("Yes", "No", 0f, popupToggleSteering));
                elementSteeringEnabled.buttons[0].toggle(steeringEnabled);
                elementInvertSteering = new PopupElement("Steer Inverted", new PopupButton("Yes", "No", 0f, popupToggleReverseInput));
                elementInvertSteering.buttons[0].toggle(reversedInput);
                elementSteeringRange = new PopupElement("Range", steerMultiplier.ToString());
                popup = new FSGUIPopup(part, "FSpartTurner", moduleID, FSGUIwindowID.partTurner, new Rect(753f, 300f, 250f, 100f), "Steering", elementSteeringEnabled);
                //popup.elementList.Add(elementSteeringEnabled);
                popup.elementList.Add(elementInvertSteering);
                popup.elementList.Add(elementSteeringRange);                

                #endregion
            }

            oldSteerMultiplier = steerMultiplier;
        }

        public override void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
            FlightCtrlState ctrl = vessel.ctrlState;

            int reverseModifier = 1;

            if (useWheelSteeringAxis)
            {
                steeringInput.x = -(vessel.ctrlState.wheelSteer + vessel.ctrlState.wheelSteerTrim);
            }
            else
            {
                if (altInputModeEnabled)
                {
                    steeringInput.x = ctrl.roll;
                    if (ignoreTrim) steeringInput.x -= ctrl.rollTrim;
                }
                else
                {
                    steeringInput.x = ctrl.yaw;
                    if (ignoreTrim) steeringInput.x -= ctrl.yawTrim;
                }
            }

            if (reversedInput)
            {
                reverseModifier = -1;
            }

            if (steeringEnabled)
            {                
                steerPart(steeringInput.x * reverseModifier);
            }
            else steerPart(0);


            setPartRotation();
        }

        public void OnGUI()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (popup != null)
                {                    
                    popup.popup();
                    steerMultiplier = float.Parse(elementSteeringRange.inputText);
                    if (popup.showMenu)
                    {                        
                        if (oldSteerMultiplier != steerMultiplier)
                        {
                            foreach (Part p in part.symmetryCounterparts)
                            {
                                FSpartTurner wheel = p.GetComponent<FSpartTurner>();
                                if (wheel != null)
                                {
                                    wheel.elementSteeringRange.inputText = steerMultiplier.ToString();
                                    wheel.steerMultiplier = steerMultiplier;
                                }
                            }
                        }
                        oldSteerMultiplier = steerMultiplier;
                    }
                }
            }
        }
    }

