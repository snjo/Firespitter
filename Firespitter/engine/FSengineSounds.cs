using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using KSP.IO;
using UnityEngine;

/// <summary>
/// Engine sound handler. Allows for engines that have power sound even at minimum throttle, and engage sound that fires more consistently.
/// Will play a warning sound when overheating.
/// Any unnamed sound will be disabled, so just comment out the lines for the sounds you don't use in the cfg.
/// </summary>
public class FSengineSounds : PartModule
{
    /// <summary>
    /// the engine start sound
    /// </summary>
    [KSPField]
    public string engage;
    /// <summary>
    /// the constant thrust independent background hum of an engine
    /// </summary>
    [KSPField]
    public string running;
    /// <summary>
    /// the thrust based pitched main engine sound
    /// </summary>
    [KSPField]
    public string power;
    /// <summary>
    /// the engine shut down sound
    /// </summary>
    [KSPField]
    public string disengage;
    /// <summary>
    /// loss of fuel flameout sound
    /// </summary>
    [KSPField]
    public string flameout;
    /// <summary>
    /// engine overheat warning sound
    /// </summary>
    [KSPField]
    public string warning;
    /// <summary>
    /// how quickly the engine power sound volume ramps up from silent when started. set to 1 for immediate full volume. This value is added to the volume every 0.2 seconds until it reaches 1.
    /// </summary>
    [KSPField]
    public float powerFadeInSpeed = 0.02f;
    /// <summary>
    /// how long the volume is off after starting the engine, to allow the engage sound to be heard
    /// </summary>
    [KSPField]
    public float powerFadeInDelay = 0.0f;
    /// <summary>
    /// thrust values below this level will yield a silent engine sound
    /// </summary>
    [KSPField]
    public float powerLowerThreshold = 0.0f;
    [KSPField]
    public float runningVolume = 1.0f;
	[KSPField]
    public float powerVolume = 1.0f;
    [KSPField]
    public float engageVolume = 1.0f;
    [KSPField]
    public float disengageVolume = 1.0f;
    [KSPField]
    public float flameoutVolume = 1.0f;
    [KSPField]
    public float warningVolume = 1.0f;
    /// <summary>
    /// random start delay for the power sound, to make several engines not play in sync. makes for a better soundscape.
    /// </summary>
    [KSPField]
    public float randomStartDelay = 0.0f;

    /// <summary>
    /// normalized engine heat value at which the warning sound chimes. set above 1 to disable (or don't name the sound of course)
    /// </summary>
    [KSPField]
    public float warningSoundThreshold = 0.8f;
    /// <summary>
    /// how often the warning sound can play in seconds
    /// </summary>
    [KSPField]
    public float warningCooldownTime = 2f;

    private float warningCountDown = 0f;

    /// <summary>
    /// the default pitch of the engine power sound. 1 is the same pitch as the original sound file.
    /// </summary>
    [KSPField]
    public float powerPitchBase = 0.8f;
    /// <summary>
    /// how much above the default pitch the power goes based on thrust. final pitch = powerPitchBase + (thrust * thrustAddedToPitch). Based on maxThrust at launch.
    /// </summary>
    [KSPField]
    public float thrustAddedToPitch = 0.3f;

    [KSPField]
    public bool useDebug = false;
    [KSPField]
    public bool showVolumeDebug = false;

    private ModuleEngines engine;
    private bool oldIgnitionState;
    private bool oldFlameOutState;
    private float currentPowerFadeIn;
    private float currentPowerDelay;

    private float smoothedPowerPitch = 0f;
    private float maxThrust;

    private bool paused = false;
    private System.Random rand = new System.Random();

    private string doesExist = "...";

    public FXGroup engageGroup;
    public bool engageAssigned;
    public FXGroup runningGroup;
    public bool runningAssigned;
    public FXGroup powerGroup;
    public bool powerAssigned;
    public FXGroup disengageGroup;
    public bool disengageAssigned;
    public FXGroup flameoutGroup;
    public bool flameoutAssigned;
    public FXGroup warningGroup;
    public bool warningAssigned;

    //private AudioHighPassFilter highPassFilter;
    //private float highPassTest = 100.0f;

    /// <summary>
    /// Fills an FXGroup with sound values
    /// </summary>
    /// <param name="group">The group that will receive a new sound</param>
    /// <param name="name">The name of the sound in the game database. No file extensions. e.g. Firespitter\Sounds\sound_fspropidle</param>
    /// <param name="loop">Does the sound loop by default?</param>
    /// <returns></returns>
    public bool createGroup(FXGroup group, string name, bool loop)
    {
        if (name != string.Empty)
        {
            if (!GameDatabase.Instance.ExistsAudioClip(name))
                return false;
            group.audio = gameObject.AddComponent<AudioSource>();
            group.audio.volume = GameSettings.SHIP_VOLUME;            
            //group.audio.rolloffMode = AudioRolloffMode.Logarithmic;            
            group.audio.rolloffMode = AudioRolloffMode.Linear;            
            group.audio.dopplerLevel = 0f;
            group.audio.panLevel = 1f;
            group.audio.clip = GameDatabase.Instance.GetAudioClip(name);
            group.audio.loop = loop;
            group.audio.playOnAwake = false;
            

            //highPassFilter = group.audio.GetComponent<AudioHighPassFilter>();            
            return true;
        }
        return false;
    }

    //GameEvents.onGamePause.Add(new EventVoid.OnEvent(OnPause));
    //GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(OnResume)); 

    public void OnPause()
    {
        if (engageAssigned) engageGroup.audio.volume = 0f;
        if (runningAssigned) runningGroup.audio.volume = 0f;
        if (powerAssigned) powerGroup.audio.volume = 0f;        
        if (disengageAssigned) disengageGroup.audio.volume = 0f;
        if (flameoutAssigned) flameoutGroup.audio.volume = 0f;
        if (warningAssigned) warningGroup.audio.volume = 0f;
        paused = true;
    }

    public void OnResume()
    {        
        if (engageAssigned) engageGroup.audio.volume = GameSettings.SHIP_VOLUME * engageVolume;
        if (runningAssigned) runningGroup.audio.volume = GameSettings.SHIP_VOLUME * runningVolume;
        if (powerAssigned) powerGroup.audio.volume = GameSettings.SHIP_VOLUME * powerVolume;
        if (disengageAssigned) disengageGroup.audio.volume = GameSettings.SHIP_VOLUME * disengageVolume;
        if (flameoutAssigned) flameoutGroup.audio.volume = GameSettings.SHIP_VOLUME * flameoutVolume;
        if (warningAssigned) warningGroup.audio.volume = GameSettings.SHIP_VOLUME * warningVolume;
        paused = false;
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        engageAssigned = createGroup(engageGroup, engage, false);
        runningAssigned = createGroup(runningGroup, running, true);
        powerAssigned = createGroup(powerGroup, power, true);
        disengageAssigned = createGroup(disengageGroup, disengage, false);
        flameoutAssigned = createGroup(flameoutGroup, flameout, false);
        warningAssigned = createGroup(warningGroup, warning, false);

        engine = part.GetComponent<ModuleEngines>();

        if (engine != null)
        {
            maxThrust = engine.maxThrust;
        }
        //GameEvents.onGamePause.Add(new EventVoid.OnEvent(OnPause));
        //GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(OnResume));              
    }

    void Update()
    {
        if (FlightDriver.Pause != paused)
        {
            if (FlightDriver.Pause)
                OnPause();
            else
                OnResume();
        }
        //if (highPassFilter != null)
        //{
        //    highPassTest += 10f;
        //    highPassFilter.enabled = true;
        //    highPassFilter.cutoffFrequency = highPassTest;
        //}
    }

    public void FixedUpdate()
    {
 	    base.OnFixedUpdate();
        if (!HighLogic.LoadedSceneIsFlight) return;

        if (engine != null)
        {
            if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRate > 1.1f)
            {
                if (powerAssigned)
                    powerGroup.audio.volume = 0f;
            }
            else
            {                     
                // power/running sound                  
                if (engine.getIgnitionState && !engine.getFlameoutState)
                {
                    if (powerAssigned)
                    {                        
                        if (currentPowerDelay > 0f)
                            currentPowerDelay -= Time.deltaTime;

                        float adjustedThrustPitch = ((engine.finalThrust / maxThrust) * thrustAddedToPitch) + powerPitchBase;
                        smoothedPowerPitch = Mathf.Lerp(smoothedPowerPitch, adjustedThrustPitch, 0.1f);
                        if (currentPowerDelay <= 0f)
                        {
                            if (currentPowerFadeIn < 1f)
                            {
                                currentPowerFadeIn += powerFadeInSpeed;
                                if (currentPowerFadeIn > 1f)
                                    currentPowerFadeIn = 1f;
                            }
                            
                            if ((engine.finalThrust / maxThrust) > powerLowerThreshold || powerLowerThreshold <= 0f)
                            {
                                powerGroup.audio.volume = GameSettings.SHIP_VOLUME * currentPowerFadeIn * powerVolume;                                
                            }
                            else
                            {
                                powerGroup.audio.volume = Mathf.Lerp(powerGroup.audio.volume, 0f, 0.06f);                                
                            }                            

                            if (!powerGroup.audio.isPlaying)
                                powerGroup.audio.Play();
                            powerGroup.audio.pitch = smoothedPowerPitch;
                        }
                    }
                    if (runningAssigned)
                    {
                        if (!runningGroup.audio.isPlaying)
                        {
                            runningGroup.audio.volume = GameSettings.SHIP_VOLUME * runningVolume;
                            runningGroup.audio.Play();
                        }
                    }
                }
                else
                {
                    if (powerAssigned)
                    {
                        if (powerGroup.audio.isPlaying)
                            powerGroup.audio.Stop();
                        if (!engine.getIgnitionState)
                        {
                            currentPowerFadeIn = 0f;
                            currentPowerDelay = powerFadeInDelay + ((float)(rand.NextDouble()) * randomStartDelay);
                        }
                    }
                    if (runningAssigned)
                    {
                        if (runningGroup.audio.isPlaying)
                            runningGroup.audio.Stop();
                    }
                }

                // engage sound
                if (engageAssigned)
                {
                    if (engine.getIgnitionState && !oldIgnitionState)
                    {
                        if (!engine.getFlameoutState)
                        {
                            engageGroup.audio.volume = GameSettings.SHIP_VOLUME * engageVolume;
                            engageGroup.audio.Play();
                            if (disengageAssigned)
                                if (disengageGroup.audio.isPlaying)
                                    disengageGroup.audio.Stop();
                            if (flameoutAssigned)
                                if (flameoutGroup.audio.isPlaying)
                                    flameoutGroup.audio.Stop();
                        }
                    }                    
                }

                //disangage sound
                if (disengageAssigned)
                {
                    if (!engine.getIgnitionState && oldIgnitionState)
                    {
                        if (!engine.getFlameoutState)
                        {
                            disengageGroup.audio.volume = GameSettings.SHIP_VOLUME * disengageVolume;
                            disengageGroup.audio.Play();
                            if (engageAssigned)
                                if (engageGroup.audio.isPlaying)
                                    engageGroup.audio.Stop();
                        }
                    }                    
                }

                //flameout
                if (flameoutAssigned)
                {
                    if (engine.getFlameoutState && !oldFlameOutState)
                    {
                        flameoutGroup.audio.volume = GameSettings.SHIP_VOLUME * flameoutVolume;
                        flameoutGroup.audio.Play();
                        if (engageAssigned)
                            if (engageGroup.audio.isPlaying)
                                engageGroup.audio.Stop();
                    }
                }

                oldIgnitionState = engine.getIgnitionState;
                oldFlameOutState = engine.getFlameoutState;

            }

            //overheat warning
            if (warningAssigned)
            {
                if (warningCountDown > 0f)
                    warningCountDown -= Time.deltaTime;

                if (part.temperature > part.maxTemp * warningSoundThreshold)
                {
                    if (warningCountDown <= 0)
                    {
                        warningGroup.audio.volume = GameSettings.SHIP_VOLUME * warningVolume;
                        warningGroup.audio.Play();
                        warningCountDown = warningCooldownTime;
                    }
                }
            }
        }
    }    

    public void OnGUI()
    {
        if (showVolumeDebug)
            GUI.Label(new Rect(250f, 300f, 200f, 30f), "engine volume: " + currentPowerFadeIn);
        if (useDebug)
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            Rect menuItemRect = new Rect(250f, 200f, 300f, 150f);
            Vector2 buttonSize = new Vector2(30f, 30f);
            Vector2 menuItemSize = new Vector2(200f, 35f);
            if (GUI.Button(new Rect(menuItemRect.x, menuItemRect.y, buttonSize.x, buttonSize.y), "OK"))
            {
                if (GameDatabase.Instance.ExistsAudioClip(warning))
                {
                    doesExist = "Yes";
                    createGroup(warningGroup, warning, false);
                }
                else
                {
                    doesExist = "No";
                }
            }

            if (GUI.Button(new Rect(menuItemRect.x + buttonSize.x + 10f, menuItemRect.y, buttonSize.x, buttonSize.y), "C"))
            {
                doesExist = "clear";
            }

            if (GUI.Button(new Rect(menuItemRect.x + ((buttonSize.x + 10f) * 2), menuItemRect.y, buttonSize.x, buttonSize.y), ">"))
            {
                if (warningAssigned)
                    warningGroup.audio.Play();
            }

            GUI.Label(new Rect(menuItemRect.x + ((buttonSize.x + 10f) * 3), menuItemRect.y, menuItemSize.x - buttonSize.x - 10f, buttonSize.y),
                    "exists: " + doesExist);
            menuItemRect.y += 30;
            warning = GUI.TextField(new Rect(menuItemRect.x + buttonSize.x + 10f, menuItemRect.y, menuItemSize.x - buttonSize.x - 10f, buttonSize.y), warning);
        }
    }
}
