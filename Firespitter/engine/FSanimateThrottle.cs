using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSanimateThrottle : PartModule
    {
        [KSPField]
        public string animationName = "throttle";
        /// <summary>
        /// If false, the animation reverts to the start position if the engine state is off/flameout
        /// </summary>
        [KSPField]
        public bool dependOnEngineState = false;
        /// <summary>
        /// how quickly it goes from the last animation time to the target time. 1 is instant.
        /// </summary>
        [KSPField]
        public float responseSpeed = 0.5f;
        /// <summary>
        /// to avoid conflict with other animations, choose a different layer for each animation on the part if there's a problem
        /// </summary>
        [KSPField]
        public int animationLayer = 1;
        /// <summary>
        /// moves from anim times of different modes smoothly if true, if false it snaps to correct the time when the mode changes
        /// </summary>
        [KSPField]
        public bool smoothModeTransition = true;
        /// <summary>
        /// the start and end time of the default engine modes throttle animation
        /// </summary>
        [KSPField]
        public Vector2 primaryModeRange = new Vector2(0f, 1f);

        public List<mode> modeList = new List<mode>();
        public float animTime = 0f;

        private Firespitter.engine.FSengineWrapper engine;
        private int _engineMode = 0;
        private Animation anim;
        private float targetTime = 0f;
        private int oldEngineMode = 0;

        public int engineMode
        {
            get
            {
                return _engineMode;
            }

            set
            {
                if (modeList.Count > value)
                    _engineMode = value;
                else
                    _engineMode = 0;
            }
        }

        private void updateAnim(float time, float currentResponseSpeed)
        {
            animTime = Mathf.Lerp(anim[animationName].normalizedTime, targetTime, currentResponseSpeed);
            anim[animationName].normalizedTime = animTime;
            //Debug.Log("animTime = " + animTime + ", normTime = " + anim[animationName].normalizedTime);
        }

        private float calculateTargetTime()
        {
            float lerpTime = Mathf.Clamp(vessel.ctrlState.mainThrottle, 0f, 1f);
            return Mathf.Lerp(modeList[engineMode].startTime, modeList[engineMode].endTime, lerpTime);
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            modeList.Add(new mode(primaryModeRange.x, primaryModeRange.y));

            engine = new Firespitter.engine.FSengineWrapper(part);

            anim = part.FindModelAnimators(animationName).FirstOrDefault();
            if (anim != null)
            {
                animTime = 0f;
                anim[animationName].layer = animationLayer;
                anim[animationName].speed = 0f;
                anim[animationName].normalizedTime = 0f;
                anim[animationName].wrapMode = WrapMode.ClampForever;
            }
            else
            {
                Debug.Log("FSanimateThrottle: no animation found called " + animationName);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (engine != null && anim != null)
            {
                if (!anim.IsPlaying(animationName))
                    anim.Play(animationName);

                if (dependOnEngineState)
                {
                    if (!engine.EngineIgnited || engine.flameout)
                    {
                        targetTime = modeList[engineMode].startTime;
                    }
                    else
                    {
                        targetTime = calculateTargetTime();
                    }

                }
                else
                {
                    targetTime = calculateTargetTime();
                }

                if (engineMode == oldEngineMode || smoothModeTransition)
                {
                    updateAnim(targetTime, responseSpeed);
                }
                else
                {
                    updateAnim(targetTime, 1f);
                }

            }
        }
    }

    public class mode
    {
        public float startTime = 0f;
        public float endTime = 1f;

        public mode(float start, float end)
        {
            startTime = start;
            endTime = end;
        }
    }
}