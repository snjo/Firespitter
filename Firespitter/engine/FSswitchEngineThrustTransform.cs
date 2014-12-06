using Firespitter.info;
using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSswitchEngineThrustTransform : PartModule
    {
        [KSPField]
        public string defaultTTName = "thrustTransform"; // the normal position for the thrustTransform
        [KSPField]
        public string alternateTTName = "alternateThrustTransform"; // Optional - the position the thrustTransform will assume when the reverse is active. Otherwise, it gets set to the same as default, but flipped 180 degrees
        [KSPField]
        public int useNamedAlternate = 0; // if 1, use the alternateTTName to define the reverse tranform instead of creating a 180 deg flip of thrustTransform

        private Transform thrustTransform;
        private Transform defaultTT;
        private Transform alternateTT;
        private FSengineWrapper engine;

        private int animateThrottleMode = 1;
        private FSanimateThrottle animateThrottle;
        [KSPField]
        public Vector2 animateThrottleRange = new Vector2(0.5f, 0f);

        [KSPField(isPersistant = true)]
        public bool isReversed = false;
        public bool valid = true;

        [KSPField]
        public int moduleID = 0;

        [KSPField]
        public bool debugMode = false;

        private FSdebugMessages debug = new FSdebugMessages(false, "FSswitchEngineThrustTransform");

        //private bool showMenu = false;
        //private Rect windowRect = new Rect(500f, 250f, 250f, 50f);
        //private FSGUIPopup popup;

        public override void OnStart(PartModule.StartState state)
        {
            debug.debugMode = debugMode;
            engine = new FSengineWrapper(part);
            if (engine.type != FSengineWrapper.EngineType.NONE)
            {
                //Debug.Log("FSswitchEngineThrustTransform: Engine module found");
                thrustTransform = part.FindModelTransform(defaultTTName);
                defaultTT = new GameObject().transform;
                defaultTT.localPosition = thrustTransform.localPosition;
                defaultTT.localRotation = thrustTransform.localRotation;
                //defaultTT = part.FindModelTransform(defaultTTName);
                if (useNamedAlternate == 1)
                {
                    debug.debugMessage("Finding alternate TT");
                    alternateTT = part.FindModelTransform(alternateTTName);
                    if (alternateTT == null) debug.debugMessage("Did not find alternate TT " + alternateTTName);
                }
                else
                {
                    debug.debugMessage("Using flipped default TT as reverse");
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
                else
                {
                    setTTReverseState(false);
                }
            }
            else
            {
                valid = false;
                Debug.Log("FSswitchEngineThrustTransform: no engine module found");
            }

            animateThrottle = part.Modules.OfType<FSanimateThrottle>().FirstOrDefault();
            if (animateThrottle != null)
            {
                animateThrottle.modeList.Add(new mode(animateThrottleRange.x, animateThrottleRange.y));
                animateThrottleMode = animateThrottle.modeList.Count - 1;
            }
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

        [KSPEvent(name = "reverseTT", active = true, guiActive = true, guiName = "Set Reverse Thrust", guiActiveEditor = true)]
        public void reverseTTEvent()
        {
            setTTReverseState(true);
        }

        [KSPEvent(name = "normalTT", active = true, guiActive = false, guiName = "Set Normal Thrust", guiActiveEditor = true)]
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
                    thrustTransform.localPosition = alternateTT.localPosition;
                    thrustTransform.localRotation = alternateTT.localRotation;
                }
                else
                {
                    thrustTransform.localPosition = defaultTT.localPosition;
                    thrustTransform.localRotation = defaultTT.localRotation;
                }
                Events["normalTTEvent"].guiActive = doReverse;
                Events["reverseTTEvent"].guiActive = !doReverse;
                Events["normalTTEvent"].guiActiveEditor = doReverse;
                Events["reverseTTEvent"].guiActiveEditor = !doReverse;

                if (animateThrottle != null)
                {
                    if (isReversed)
                        animateThrottle.engineMode = animateThrottleMode;
                    else
                        animateThrottle.engineMode = 0;
                }
            }
            else
            {
                Debug.Log("FSswitchEngineThrustTransform: invalid setup");
            }

        }

        public void toggleIsReversed()
        {
            isReversed = !isReversed;
            //popup.sections[0].elements[0].buttons[0].toggle(isReversed);

            foreach (Part p in part.symmetryCounterparts)
            {
                FSswitchEngineThrustTransform switcher = p.GetComponent<FSswitchEngineThrustTransform>();
                if (switcher != null)
                {
                    switcher.isReversed = isReversed;
                    //switcher.popup.sections[0].elements[0].buttons[0].toggle(isReversed);
                }
            }
        }

        //public void OnGUI()
        //{
        //    if (!HighLogic.LoadedSceneIsEditor)
        //        return;

        //    popup.popup();
        //}
    }
}
