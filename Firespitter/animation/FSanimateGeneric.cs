using System.Linq;
using UnityEngine;

public class FSanimateGeneric : PartModule
{
    [KSPField]
    public string animationName;
    [KSPField]
    public string startEventGUIName = "Deploy";
    [KSPField]
    public string endEventGUIName = "Retract";
    [KSPField]
    public string toggleActionName;
    //[KSPField]
    //public bool isOneShot = false;
    [KSPField(isPersistant = true)]
    public bool startDeployed = false;
    [KSPField]
    public float customAnimationSpeed = 1f;
    [KSPField]
    public bool availableInEVA = false;
    [KSPField]
    public bool availableInVessel = true;
    [KSPField]
    public float EVArange = 5f;
    [KSPField]
    public int layer = 1;
    [KSPField]
    public bool useActionEditorPopup = true;
    [KSPField]
    public int moduleID = 0;
    [KSPField]
    public bool playAnimationOnEditorSpawn = true;
    [KSPField]
    public Vector4 defaultWindowRect = new Vector4(550f, 300f, 150f, 100f);

    // animationRampspeed is how quickly it gets up to speed.  1 meaning it gets to full speed (as defined by 
    // the animSpeed and customAnimationSpeed) immediately, less than that will ramp up over time
    [KSPField]
    public float animationRampSpeed = 1f;
    // When the mod starts, what speed to set the initial animSpeed to
    [KSPField]
    public float startAnimSpeed = 1f;
    // Multiplier to be used when in reverse mode
    [KSPField]
    public float reverseAnimSpeed = 1f;

    [KSPField(isPersistant = true)]
    public bool isAnimating = false;
    [KSPField(isPersistant = true)]
    public float animTime;
    [KSPField(isPersistant = true)]
    public bool reverseAnimation;
    [KSPField(isPersistant = true)]
    public float animSpeed = 1f;
    //[KSPField(isPersistant = true)]
    //public bool locked = false;

    [KSPField(isPersistant = true)]
    public bool hasBeenInitialized = false;

    [KSPField]
    public string startDeployedString = "Start Deployed?";

    private Animation anim;
    private bool effectPlaybackReady = false;

    [KSPField]
    public string fullyRetractedEffect = string.Empty;
    [KSPField]
    public string fullyDeployedEffect = string.Empty;
    [KSPField]
    public string startDeployEffect = string.Empty;
    [KSPField]
    public string startRetractEffect = string.Empty;
    //private FSGUIPopup popup;

    //private bool showMenu = false;
    //private Rect windowRect; // = new Rect(550f, 300f, 150f, 100f);


    // The following are for the new functionality for the animation ramp ability

    private enum RampDirection { none, up, down };
    private RampDirection rampDirection = RampDirection.none;
    private float ramp = 0;
    private float unstartedSpeed;
    private bool animSpeediInited = false;

    [KSPAction("Toggle")]
    public void toggleAction(KSPActionParam param)
    {
        if (availableInVessel)
            toggleEvent();
    }

    [KSPEvent(name = "toggleEvent", guiName = "Deploy", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor=true)]
    public void toggleEvent()
    {        

        Events["toggleEvent"].active = false; //see if this removes the button when clicking
        isAnimating = true;

        //if (startDeployed && reverseAnimation && anim[animationName].normalizedTime == 0f)
        //    anim[animationName].normalizedTime = 0.999f;
        
        if (reverseAnimation)
        {
            Actions["toggleAction"].guiName = startEventGUIName;
            Events["toggleEvent"].guiName = startEventGUIName;
            if (toggleActionName != string.Empty)
            {
                Actions["toggleAction"].guiName = toggleActionName;
            }
            // Following two commented out lines (anim[animationName].speed =, and anim.Play) now taken care of in LateUpdate
            //anim[animationName].speed = -1f *  customAnimationSpeed;
            if (anim[animationName].normalizedTime == 0f || anim[animationName].normalizedTime == 1f)
                anim[animationName].normalizedTime = 1f;
            
            //anim.Play(animationName);
            startDeployed = false; // to get the hangar toggle button state right
            if (startRetractEffect != string.Empty)
            {
                part.Effect(startRetractEffect);
                Debug.Log("start retract effect");
            }
            rampDirection = RampDirection.down;
        }
        else
        {
            Actions["toggleAction"].guiName = endEventGUIName;
            Events["toggleEvent"].guiName = endEventGUIName;
            if (toggleActionName != string.Empty)
            {
                Actions["toggleAction"].guiName = toggleActionName;
            }
            // Following two commented out lines (anim[animationName].speed =, and anim.Play) now taken care of in LateUpdate
            //anim[animationName].speed = 1f * customAnimationSpeed;
            if (anim[animationName].normalizedTime == 0f || anim[animationName].normalizedTime == 1f)
                anim[animationName].normalizedTime = 0f;

            //anim.Play(animationName);
            startDeployed = true; // to get the hangar toggle button state right
            if (startDeployEffect != string.Empty)
            {
                part.Effect(startDeployEffect);
                Debug.Log("start deploy effect");
            }
            rampDirection = RampDirection.up;
        }

        reverseAnimation = !reverseAnimation;

        //if (isOneShot)
        //{
        //    locked = true;
        //    Events["toggleEvent"].active = false;
        //}
    }



    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        anim = part.FindModelAnimators(animationName).FirstOrDefault();
        if (anim != null)
        {
            // Run Only on first launch
            if (!hasBeenInitialized)
            {
                if (startDeployed)
                {
                    reverseAnimation = true;
                    animTime = 1f;
                    anim[animationName].normalizedTime = 1f;
                }
                else
                {
                    reverseAnimation = false;
                    animTime = 0f;                    
                    anim[animationName].normalizedTime = 0f;
                }
                hasBeenInitialized = true;
            }

            // make sure you stay in your place on launch, and don't start going in the wrong direction if deployed/retracted
            if (reverseAnimation)
            {
                animSpeed = 1f * customAnimationSpeed;
                // unity animations go from 0-0.99999 to 0 again, so check for that to correct deployed animation state.
                //if (anim[animationName].normalizedTime == 0f)
                //    animTime = 1f;         
                if (animTime == 0f)
                    animTime = 1f;
            }
            else
            {
                animSpeed = -1f * customAnimationSpeed * reverseAnimSpeed;
                //this fixes the animation running on entering the scene, but mayn not be good for paused animations
                //animTime = 0f;
            }

            // make the animation play in the editor to show off the part capabilities.
            if (HighLogic.LoadedSceneIsEditor && playAnimationOnEditorSpawn)
                animTime = 1f;

            //set up animation state according to persistent values
            anim[animationName].layer = layer;
            // Following two commented out lines now taken care of in LateUpdate
            //anim[animationName].speed = animSpeed * startAnimSpeed;

            //anim.Play(animationName);
            anim[animationName].normalizedTime = animTime;            

            if (reverseAnimation)
            {
                Actions["toggleAction"].guiName = endEventGUIName;
                Events["toggleEvent"].guiName = endEventGUIName;
            }
            else
            {
                Actions["toggleAction"].guiName = startEventGUIName;
                Events["toggleEvent"].guiName = startEventGUIName;
            }
            if (toggleActionName != string.Empty)
            {
                Actions["toggleAction"].guiName = toggleActionName;
            }

            Events["toggleEvent"].guiActiveUnfocused = availableInEVA;
            Events["toggleEvent"].guiActive = availableInVessel;
            Events["toggleEvent"].unfocusedRange = EVArange;
            if (!availableInVessel)
                Actions["toggleAction"].guiName = "Toggle (Disabled)";            
        }
        else
        {
            Debug.Log("FSanimateGeneric: No such animation: " + animationName);
            Events["toggleEvent"].guiActive = false;
            Events["toggleEvent"].guiActiveUnfocused = false;
        }

        //windowRect.y += (moduleID * windowRect.height) + 20;

        //windowRect = new Rect(defaultWindowRect.x, defaultWindowRect.y, defaultWindowRect.z, defaultWindowRect.w);
        //popup = new FSGUIPopup(part, "FSanimateGeneric", moduleID, FSGUIwindowID.animateGeneric + moduleID, windowRect, startDeployedString, new PopupElement(new PopupButton("Yes", "No", 0f, guiToggleEvent)));
        //popup.sections[0].elements[0].useTitle = false;
        //popup.sections[0].elements[0].buttons[0].toggle(reverseAnimation);
    }

    //
    // Process the ramping in LateUpdate
    //
    public void LateUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) return;

        if (anim != null)
        {
            switch (rampDirection)
            {
                case RampDirection.up:
                    if (ramp < 1f)
                    {
                        if (ramp < 0)
                        {
                            if (reverseAnimSpeed == 0)
                                ramp = 0;
                            else
                                ramp += animationRampSpeed / reverseAnimSpeed;
                        }
                        else
                            ramp += animationRampSpeed;
                    }
                    if (ramp > 1f)
                        ramp = 1f;
                    break;
                case RampDirection.down:
                    if (ramp > -1f)
                    {
                        if (ramp < 0)
                        {
                            if (reverseAnimSpeed != 0)                                
                                ramp -= animationRampSpeed / reverseAnimSpeed;
                        }
                        else
                            ramp -= animationRampSpeed;
                    }
                    if (ramp < -1f)
                    {
                        ramp = -1f;
                        //rampDirection = RampDirection.up;
                    }
                    break;
            }
            if (/* reverseAnimation && */ ramp < 0)
            {                
                anim[animationName].speed = 1f * customAnimationSpeed * reverseAnimSpeed * ramp;
            }
            else
            {
                anim[animationName].speed = 1f * customAnimationSpeed * ramp;
            }
            if (rampDirection == RampDirection.none)
            {
                if (!animSpeediInited)
                {
                    anim[animationName].speed = animSpeed * startAnimSpeed;
                    animSpeediInited = true;
                    unstartedSpeed = anim[animationName].speed;
                    ramp = startAnimSpeed;
                }
                else
                    anim[animationName].speed = unstartedSpeed;
            }
            // Debug.Log("rampDirection: " + rampDirection.ToString() + "   animSpeed: " + animSpeed.ToString() + "   startAnimSpeed: " + startAnimSpeed.ToString() + "   anim[animationName].speed: " + anim[animationName].speed.ToString());
            // I don't think that this causes a problem, to call it every time here even if the speed didn't change.  Also catches any other
            // changes
            anim.Play(animationName);
        }
    }

    //public void guiToggleEvent()
    //{
    //    toggleEvent();
    //    popup.sections[0].elements[0].buttons[0].toggle(startDeployed);
    //}

    public void Update()
    {
        if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) return;               

        if (!Events["toggleEvent"].active) // see if this works as a way of hiding and showing the button when clicked.
        {            
                Events["toggleEvent"].active = true;
        }

        if (anim != null)
        {

            if (!anim.isPlaying)
            {
                if (reverseAnimation)
                {
                    animTime = 1f; // 0
                }
                else
                {
                    animTime = 0f; // 1
                }
                anim[animationName].normalizedTime = animTime;
                isAnimating = false;

                if (effectPlaybackReady)
                {
                    if (animTime == 0 && fullyRetractedEffect != string.Empty)
                    {
                        part.Effect(fullyRetractedEffect);                        
                    }
                    else if (animTime == 1 && fullyDeployedEffect != string.Empty)
                    {
                        part.Effect(fullyDeployedEffect);                        
                    }
                }
                effectPlaybackReady = false;
            }
            else
            {
                animTime = anim[animationName].normalizedTime;
                animSpeed =  anim[animationName].speed;
                isAnimating = true; // this was false... hmmm
                effectPlaybackReady = true;
            }
        }
    }

    //public void OnGUI()
    //{
    //    if (!HighLogic.LoadedSceneIsEditor)
    //       return;

    //    popup.popup();
    //}
}
