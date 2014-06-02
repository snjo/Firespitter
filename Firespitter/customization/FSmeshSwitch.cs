using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.customization
{
    public class FSmeshSwitch : PartModule
    {
        [KSPField]
        public int moduleID = 0;
        [KSPField]
        public string buttonName = "Next part variant";
        [KSPField]
        public string previousButtonName = "Prev part variant";
        [KSPField]
        public string objectDisplayNames = "Default";
        [KSPField]
        public bool showPreviousButton = true;
        [KSPField]
        public bool useFuelSwitchModule = false;
        [KSPField]
        public string fuelTankSetups = "0";

        [KSPField]
        public string objects = string.Empty;

        //// in case of multiple instances of this module, on will be the master, the rest slaves.
        //[KSPField]
        //public bool isController = true;

        //// in case of multiple sets of master/slaves, only affect ones on the same channel.
        //[KSPField]
        //public int channel = 0;

        [KSPField(isPersistant = true)]
        public int selectedObject = 0;

        private string[] objectNames;
        private List<Transform> objectTransforms = new List<Transform>();
        private List<int> fuelTankSetupList = new List<int>();
        private List<string> objectDisplayList = new List<string>();

        private FSfuelSwitch fuelSwitch;

        [KSPField(guiActiveEditor = true, guiName = "Current Variant")]
        public string currentObjectName = string.Empty;

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Next part variant")]
        public void nextObjectEvent()
        {
            selectedObject++;
            if (selectedObject >= objectTransforms.Count)
            {
                selectedObject = 0;
            }
            switchToObject(selectedObject);
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Prev part variant")]
        public void previousObjectEvent()
        {
            selectedObject--;
            if (selectedObject < 0)
            {
                selectedObject = objectTransforms.Count - 1;
            }
            switchToObject(selectedObject);
        }

        private void parseObjectNames()
        {
            objectNames = objects.Split(';');
            if (objectNames.Length < 1)
                Debug.Log("FSmeshSwitch: Found no object names in the object list");
            else
            {
                objectTransforms.Clear();
                for (int i = 0; i < objectNames.Length; i++)
                {
                    Transform newTransform = part.FindModelTransform(objectNames[i]);
                    if (newTransform != null)
                    {
                        objectTransforms.Add(newTransform);
                        //Debug.Log("FSmeshSwitch: added object to list: " + objectNames[i]);
                    }
                    else
                    {
                        Debug.Log("FSmeshSwitch: could not find object " + objectNames[i]);
                    }
                }
            }
        }

        private void switchToObject(int objectNumber)
        {
            setObject(objectNumber);

            for (int i = 0; i < part.symmetryCounterparts.Count; i++)
            {
                FSmeshSwitch[] symSwitch = part.symmetryCounterparts[i].GetComponents<FSmeshSwitch>();
                for (int j = 0; j < symSwitch.Length; j++)
                {
                    if (symSwitch[j].moduleID == moduleID)
                    {
                        symSwitch[j].selectedObject = selectedObject;
                        symSwitch[j].setObject(objectNumber);
                    }
                }
            }
        }

        private void setObject(int objectNumber)
        {
            //if (objectNumber >= objectTransforms.Count) return;

            for (int i = 0; i < objectTransforms.Count; i++)
            {
                objectTransforms[i].gameObject.renderer.enabled = false;
            }

            // enable the selected one last because there might be several entries with the same object, and we don't want to disable it after it's been enabled.
            objectTransforms[objectNumber].gameObject.renderer.enabled = true;

            if (useFuelSwitchModule)
            {
                //Debug.Log("FStextureSwitch2 calling on FSfuelSwitch tank setup " + objectNumber);
                if (objectNumber < fuelTankSetupList.Count)
                    fuelSwitch.selectTankSetup(fuelTankSetupList[objectNumber]);
                else
                    Debug.Log("FStextureSwitch2: no such fuel tank setup");
            }

            if (selectedObject > objectDisplayList.Count - 1)
            {
                currentObjectName = objectNames[selectedObject];
            }
            else
            {
                currentObjectName = objectDisplayList[selectedObject];
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            parseObjectNames();
            fuelTankSetupList = Tools.parseIntegers(fuelTankSetups);
            objectDisplayList = Tools.parseNames(objectDisplayNames);

            if (useFuelSwitchModule)
            {
                fuelSwitch = part.GetComponent<FSfuelSwitch>();
                if (fuelSwitch == null)
                {
                    useFuelSwitchModule = false;
                    Debug.Log("FStextureSwitch2: no FSfuelSwitch module found, despite useFuelSwitchModule being true");
                }
            }

            switchToObject(selectedObject);
            Events["nextObjectEvent"].guiName = buttonName;
            if (!showPreviousButton) Events["previousObjectEvent"].guiActiveEditor = false;
        }
    }
}
