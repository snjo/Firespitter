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
    public float powerLowerThreshold = 0.0f; //add functionality
	[KSPField]
    public float powerVolume = 1.0f;
    [KSPField]
    public float engageVolume = 1.0f;
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

    private AudioHighPassFilter highPassFilter;
    private float highPassTest = 100.0f;

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
            

            highPassFilter = group.audio.GetComponent<AudioHighPassFilter>();            
            return true;
        }
        return false;
    }

    //private void OnDestroy()
    //{
    //    GameEvents.onGamePause.Remove(new EventVoid.OnEvent(OnPause));
    //    GameEvents.onGameUnpause.Remove(new EventVoid.OnEvent(OnResume));        
    //}

    public void OnPause()
    {
        powerGroup.audio.volume = 0f;
        paused = true;
    }

    public void OnResume()
    {
        powerGroup.audio.volume = GameSettings.SHIP_VOLUME;
        paused = false;
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);

        engageAssigned = createGroup(engageGroup, engage, false);
        runningAssigned = createGroup(runningGroup, running, true);
        powerAssigned = createGroup(powerGroup, power, true);
        disengageAssigned = createGroup(disengageGroup, disengage, true);
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
        if (highPassFilter != null)
        {
            highPassTest += 10f;
            highPassFilter.enabled = true;
            highPassFilter.cutoffFrequency = highPassTest;
        }
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
                            powerGroup.audio.volume = GameSettings.SHIP_VOLUME * currentPowerFadeIn * powerVolume;
                            if (!powerGroup.audio.isPlaying)
                                powerGroup.audio.Play();
                            powerGroup.audio.pitch = smoothedPowerPitch;
                        }
                    }
                }
                else
                {
                    if (powerAssigned)
                    {
                        if (powerGroup.audio.isPlaying)
                            powerGroup.audio.Stop();
                        currentPowerFadeIn = 0f;
                        currentPowerDelay = powerFadeInDelay + ((float)(rand.NextDouble()) * randomStartDelay);
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
                        }
                    }
                    oldIgnitionState = engine.getIgnitionState;
                }

            }

            if (warningAssigned)
            {
                if (warningCountDown > 0f)
                    warningCountDown -= Time.deltaTime;

                if (part.temperature > part.maxTemp * warningSoundThreshold)
                {
                    if (warningCountDown <= 0)
                    {
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
