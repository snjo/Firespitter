using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

    public class FSswitchEngineThrustTransform : PartModule
    {
        [KSPField]
        public string defaultTTName = "thrustTransform";
        [KSPField]
        public string alternateTTName = "alternateThrustTransform";
        [KSPField]
        public int useNamedAlternate = 0;

        private Transform thrustTransform;
        private Transform defaultTT;
        private Transform alternateTT;
        private ModuleEngines engine;

        [KSPField(isPersistant = true)]
        public bool isReversed = false;
        public bool valid = true;

        [KSPField]
        public int moduleID = 0;

        //private bool showMenu = false;
        private Rect windowRect = new Rect(500f, 300f, 125f, 50f);
        private FSGUIPopup popup;

        public override void OnStart(PartModule.StartState state)
        {            

            engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
            if (engine != null)
            {
                Debug.Log("FSswitchEngineThrustTransform: Engine module found");
                thrustTransform = part.FindModelTransform(defaultTTName);                
                defaultTT = new GameObject().transform;
                defaultTT.localPosition = thrustTransform.localPosition;
                defaultTT.localRotation = thrustTransform.localRotation;
                //defaultTT = part.FindModelTransform(defaultTTName);
                if (useNamedAlternate == 1)
                {
                    alternateTT = part.FindModelTransform(alternateTTName);
                }
                else
                {
                    alternateTT = new GameObject().transform;
                    alternateTT.localPosition = defaultTT.localPosition;
                    alternateTT.localRotation = defaultTT.localRotation;
                    alternateTT.Rotate(alternateTT.right, 180f);
                }
                if (defaultTT == null || alternateTT == null) valid = false;

                if (isReversed)
                {
                    setTTReverseState(true);
                }
            }
            else
            {
                valid = false;
                Debug.Log("FSswitchEngineThrustTransform: no engine module found");
            }

            popup = new FSGUIPopup(part, "FSswitchEngineThrustTransform", moduleID, FSGUIwindowID.switchEngineThrustTransform, windowRect, "Start Reversed?", new PopupElement(new PopupButton("Yes","No",0f,toggleIsReversed)));
            popup.elementList[0].useTitle = false;
        }

        [KSPAction("Toggle Thrust Reverser")]
        public void switchTTAction(KSPActionParam param)
        {
            setTTReverseState(!isReversed);
        }

        [KSPAction("Set Reverse Thrust")]
        public void reverseTTAction(KSPActionParam param)
        {
            setTTReverseState(true);
        }

        [KSPAction("Set Normal Thrust")]
        public void normalTTAction(KSPActionParam param)
        {
            setTTReverseState(false);
        }

        [KSPEvent(name = "reverseTT", active = true, guiActive = true, guiName = "Set Reverse Thrust")]
        public void reverseTTEvent()
        {
            setTTReverseState(true);            
        }

        [KSPEvent(name = "normalTT", active = true, guiActive = false, guiName = "Set Normal Thrust")]
        public void normalTTEvent()
        {
            setTTReverseState(false);
        }

        [KSPEvent(name = "debug", active = true, guiActive = false, guiName = "debug")]
        public void debugEvent()
        {
            try
            {
                Debug.Log("TT: " + thrustTransform.name);
            }
            catch
            {
                Debug.Log("TT: error");
            }
            try
            {
                Debug.Log("defaultTT: " + defaultTT.name);
            }
            catch
            {
                Debug.Log("defaultTT: error");
            }
            try
            {
                Debug.Log("altTT: " + alternateTT.name);
            }
            catch
            {
                Debug.Log("altTT: error");
            }
        }

        public void setTTReverseState(bool doReverse)
        {
            if (valid)
            {
                isReversed = doReverse;
                if (doReverse)
                {
                    thrustTransform.localRotation = alternateTT.localRotation;                    
                }
                else
                {
                    thrustTransform.localRotation = defaultTT.localRotation;
                }
                Events["normalTTEvent"].guiActive = doReverse;
                Events["reverseTTEvent"].guiActive = !doReverse;
            }
            else
            {
                Debug.Log("FSswitchEngineThrustTransform: invalid setup");
            }

        }

        public void toggleIsReversed()
        {
            isReversed = !isReversed;
            popup.elementList[0].buttons[0].toggle(isReversed);
        }

        //private void drawWindow(int windowID)
        //{
        //    string startReversedString;
        //    if (isReversed)
        //    {
        //        startReversedString = "Yes";
        //    }
        //    else
        //    {
        //        startReversedString = "No";
        //    }
        //    if (GUI.Button(new Rect(25f, 25f, 65f, 22f), startReversedString))
        //    {
        //        isReversed = !isReversed;
        //    }
        //    GUI.DragWindow();
        //}

        public void OnGUI()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;

            popup.popup();

            //if (showMenu)
            //{
            //    windowRect = GUI.Window(FSGUIwindowID.switchEngineThrustTransform, windowRect, drawWindow, "Start Reversed?");
            //}

            //showMenu = false;

            //EditorLogic editor = EditorLogic.fetch;
            //if (editor.editorScreen == EditorLogic.EditorScreen.Actions)
            //{
            //    List<Part> partlist = EditorActionGroups.Instance.GetSelectedParts();
            //    if (partlist.Count > 0)
            //    {
            //        if (partlist[0] == part)
            //        {
            //            if (partlist[0].Modules.Contains("FSswitchEngineThrustTransform"))
            //            {
            //                showMenu = true;
            //            }
            //        }
            //    }
            //}
        }
    }
