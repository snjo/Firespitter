using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.animation
{
    public class FSanimateLooping : PartModule
    {
        [KSPField]
        public string animationName;
        [KSPField]
        public bool goToBeginningWhenStopped = true;
        //[KSPField]
        //public string startEventGUIName = "Deploy";
        //[KSPField]
        //public string endEventGUIName = "Retract";
        [KSPField]
        public string toggleActionName = "Toggle";
        [KSPField(isPersistant = true)]
        public bool isAnimating = false;
        [KSPField]
        public float customAnimationSpeed = 1f;
        [KSPField]
        public bool availableInEVA = true;
        [KSPField]
        public bool availableInVessel = true;
        [KSPField]
        public float EVArange = 5f;
        [KSPField]
        public int layer = 1;

        private Animation anim;

        [KSPAction("Toggle")]
        public void toggleAction(KSPActionParam param)
        {
            if (availableInVessel)
                toggleEvent();
        }

        [KSPEvent(name = "toggleEvent", guiName = "Deploy", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = true)]
        public void toggleEvent()
        {
            isAnimating = !isAnimating;
            setPlayMode(isAnimating);
        }

        private void setPlayMode(bool newState)
        {
            if (newState)
            {
                anim.Play(animationName);
                anim[animationName].speed = customAnimationSpeed;
            }
            else
            {                
                if (goToBeginningWhenStopped)
                    anim[animationName].normalizedTime = 0f;
                anim[animationName].speed = 0f;
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            anim = part.FindModelAnimators(animationName).FirstOrDefault();
            if (anim != null)
            {
                anim[animationName].layer = layer;
                anim[animationName].speed = customAnimationSpeed;
                anim.wrapMode = WrapMode.Loop;
                setPlayMode(isAnimating);
            }
            else
                Debug.Log("Could not find anim " + animationName);

            Events["toggleEvent"].guiName = toggleActionName;
            Actions["toggleAction"].guiName = toggleActionName;
        }
    }
}
