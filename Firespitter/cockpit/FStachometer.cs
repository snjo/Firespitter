using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FStachometer : InternalModule
{
    [KSPField]
    public string RPMneedleName = "needleRPM";
    [KSPField]
    public string heatNeedleName = "needleHeat";
    [KSPField]
    public string thrustLimitNeedleName = "needleThrustLimit";
    [KSPField]
    public string buttonName = "button";
    [KSPField]
    public string engineSelectionDialName = "engineSelection";
    [KSPField]
    public float minAngle = 0f;
    [KSPField]
    public float maxAngle = -320f;
    //[KSPField]
    //public float rotationDirection = 1f;
    [KSPField]
    public Vector3 rotationAxis = Vector3.forward;
    [KSPField]
    public Vector3 selectionDefaultRotation = new Vector3(75f, 0f, -90f);
    [KSPField]
    public Vector3 selectionRotationAxis = Vector3.right;

    public int selectedEngineNumber = 0;

    private List<ModuleEngines> engines = new List<ModuleEngines>();

    private GameObject button;
    private FSgenericButtonHandler buttonHandler;

    private Transform RPMneedle;
    private Transform heatNeedle;
    private Transform thrustLimitNeedle;

    private Transform engineSelectionDialTransform;
    private Firespitter.cockpit.AnalogCounter engineSelectionDial;

    //private bool useEngineSelection = true;
    private bool Initialized = false;
    private bool hasEngines = false;

    private int oldPartCount = 0;

    private void nextEngine()
    {
        //Debug.Log("next button clicked");
        if (oldPartCount != vessel.parts.Count || engines.Count < 1 || !hasEngines)
            updateEngineList();
        if (hasEngines)
        {
            selectedEngineNumber++;
            if (selectedEngineNumber >= engines.Count)
            {
                selectedEngineNumber = 0;
            }
            engineSelectionDial.updateNumber((float)selectedEngineNumber + 1);
            //Debug.Log("selected engine: " + selectedEngineNumber + ", engines: " + engines.Count);
        }
    }

    public void Start()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            try
            {                
                button = internalProp.FindModelTransform(buttonName).gameObject;                
                buttonHandler = button.AddComponent<FSgenericButtonHandler>();                
                buttonHandler.mouseDownFunction = nextEngine;
                engineSelectionDialTransform = internalModel.FindModelTransform(engineSelectionDialName);                
                engineSelectionDial = new Firespitter.cockpit.AnalogCounter(engineSelectionDialTransform, selectionDefaultRotation, selectionRotationAxis);
            }
            catch
            {
                //useEngineSelection = false;
                Debug.Log("FStachometer: No button or dial, disabling engine selection");
            }

            RPMneedle = internalProp.FindModelTransform(RPMneedleName);
            heatNeedle = internalProp.FindModelTransform(heatNeedleName);
            thrustLimitNeedle = internalProp.FindModelTransform(thrustLimitNeedleName);
        }
    }

    private void updateNeedles(ModuleEngines engine)
    {
        if (thrustLimitNeedle != null)
        {
            thrustLimitNeedle.localRotation = Quaternion.Euler(rotationAxis * Mathf.Lerp(minAngle, maxAngle, engine.thrustPercentage / 100f));
        }
        if (RPMneedle != null)
        {
            RPMneedle.localRotation = Quaternion.Euler(rotationAxis * Mathf.Lerp(minAngle, maxAngle, engine.normalizedThrustOutput));
        }
        if (heatNeedle != null)
        {
            heatNeedle.localRotation = Quaternion.Euler(rotationAxis * Mathf.Lerp(minAngle, maxAngle, engine.part.temperature / engine.part.maxTemp));
        }        
        //debug
        //float limiter = engine.thrustPercentage / 100f;
        //float rpm = engine.normalizedThrustOutput;
        //float heat = engine.part.maxTemp / engine.part.temperature;
        //Debug.Log("tLimit: " + limiter + " (" + engine.thrustPercentage + ") rpm: " + rpm + " heat: " + heat);
    }

    public override void OnUpdate()
    {
        if (!Initialized)
        {
            updateEngineList();
            if (hasEngines)
                engineSelectionDial.updateNumber(selectedEngineNumber + 1);
            Initialized = true;
        }

        //if (engines.Count < 1) hasEngines = false; // you can still update the engine list by using the next engine button
        if (hasEngines)
        {
            if (selectedEngineNumber >= engines.Count)
                updateEngineList();
            if (engines[selectedEngineNumber] != null)
            {
                updateNeedles(engines[selectedEngineNumber]);
            }
            else
            {
                updateEngineList();
            }
        }
    }

    private void updateEngineList()
    {
        //Debug.Log("updating engine list");
        engines.Clear();
        foreach (Part p in vessel.parts)
        {
            ModuleEngines engine = p.GetComponent<ModuleEngines>();
            if (engine != null)
                engines.Add(engine);
        }
        oldPartCount = vessel.parts.Count;
        if (selectedEngineNumber >= engines.Count)
        {
            selectedEngineNumber = 0;
            engineSelectionDial.updateNumber(0);
        }

        if (engines.Count > 0)
            hasEngines = true;
        else
            hasEngines = false;
    }
}

