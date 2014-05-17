using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.gui
{
    public class FSmeshSwitch : PartModule
    {
        [KSPField]
        public string buttonName = "Next part variant";
        [KSPField]
        public string previousButtonName = "Prev part variant";
        [KSPField]
        public bool showPreviousButton = true;

        [KSPField]
        public string objects = string.Empty;

        // in case of multiple instances of this module, on will be the master, the rest slaves.
        [KSPField]
        public bool isController = true;

        // in case of multiple sets of master/slaves, only affect ones on the same channel.
        [KSPField]
        public int channel = 0;

        [KSPField(isPersistant = true)]
        public int selectedObject = 0;

        private string[] objectNames;
        private List<Transform> objectTransforms = new List<Transform>();

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
            if (objectNumber >= objectTransforms.Count) return;

            for (int i = 0; i < objectTransforms.Count; i++)
            {                
                objectTransforms[i].gameObject.renderer.enabled = false;
            }

            // enable the selected one last because there might be several entries with the same object, and we don't want to disable it after it's been enabled.
            objectTransforms[objectNumber].gameObject.renderer.enabled = true;
        }

        public override void OnStart(PartModule.StartState state)
        {
            parseObjectNames();
            switchToObject(selectedObject);
            Events["nextObjectEvent"].guiName = buttonName;
            if (!showPreviousButton) Events["previousObjectEvent"].guiActiveEditor = false;
        }
    }
}
