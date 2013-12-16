using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FSmaintenanceCounter : InternalModule
{
    [KSPField]
    public Vector3 defaultRotation = new Vector3(15f, -90, 90);
    [KSPField]
    public Vector3 rotationAxis = new Vector3(-1f, 0f, 0f);
    [KSPField]
    public string hour0 = string.Empty;
    [KSPField]
    public string hour1 = string.Empty;
    [KSPField]
    public string hour2 = string.Empty;
    [KSPField]
    public string hour3 = string.Empty;
    [KSPField]
    public string hour4 = string.Empty;
    [KSPField]
    public string cycle0 = string.Empty;
    [KSPField]
    public string cycle1 = string.Empty;
    [KSPField]
    public string cycle2 = string.Empty;
    [KSPField]
    public string cycleButtonName = "button";

    GameObject cycleButton;
    FSgenericButtonHandler cycleButtonHandler;
    private FSmaintenanceInfo maintenanceInfo;
    private Firespitter.cockpit.AnalogCounter flightTimeCounterHour = new Firespitter.cockpit.AnalogCounter();
    private Firespitter.cockpit.AnalogCounter flightTimeCounterMin = new Firespitter.cockpit.AnalogCounter();
    private Firespitter.cockpit.AnalogCounter cycleCounter = new Firespitter.cockpit.AnalogCounter();
    private float flightTime = 0f;
    private int cycles = 0;
    private float transition = 0f;
    private float useTime = 0f;

    private void addWheel(Firespitter.cockpit.AnalogCounter counter, string wheelName)
    {
        if (wheelName != string.Empty)
        {
            Transform newWheel = base.internalProp.FindModelTransform(wheelName);
            if (newWheel != null)
            {
                counter.wheels.Add(newWheel);
            }
        }
    }

    public void addCycle()
    {
        cycles++;
    }

    public void Start()
    {
        addWheel(flightTimeCounterMin, hour0);
        addWheel(flightTimeCounterMin, hour1);
        addWheel(flightTimeCounterHour, hour2);
        addWheel(flightTimeCounterHour, hour3);
        addWheel(flightTimeCounterHour, hour4);
        addWheel(cycleCounter, cycle0);
        addWheel(cycleCounter, cycle1);
        addWheel(cycleCounter, cycle2);

        flightTimeCounterHour.rotationAxis = rotationAxis;
        flightTimeCounterHour.defaultRotation = defaultRotation;
        flightTimeCounterMin.rotationAxis = rotationAxis;
        flightTimeCounterMin.defaultRotation = defaultRotation;
        cycleCounter.rotationAxis = rotationAxis;
        cycleCounter.defaultRotation = defaultRotation;
        //Debug.Log("altimeter Counter list: " + analogCounter.wheels.Count);

        maintenanceInfo = part.Modules.OfType<FSmaintenanceInfo>().FirstOrDefault();
        if (maintenanceInfo != null)
        {
            flightTime = maintenanceInfo.flightTime;
            cycles = maintenanceInfo.cycles;
        }

        cycleButton = base.internalProp.FindModelTransform(cycleButtonName).gameObject;
        cycleButtonHandler = cycleButton.AddComponent<FSgenericButtonHandler>();
        cycleButtonHandler.mouseDownFunction = addCycle;
    }

    public override void OnUpdate()
    {
        useTime = (flightTime / 60f) % 60f;
        flightTimeCounterMin.updateNumber(useTime); //60
        if (useTime > 59f)
            transition = useTime % 1f;
        else
            transition = 0f;
        flightTimeCounterHour.updateNumber(flightTime / 3600f, transition); //3600        
        cycleCounter.updateNumber(cycles);
        if (maintenanceInfo != null)
        {
            maintenanceInfo.flightTime = flightTime;
            maintenanceInfo.cycles = cycles;
        }
    }

    public override void OnFixedUpdate()
    {
        if (vessel.srfSpeed > 5f)
            flightTime += TimeWarp.deltaTime;        
    }
}

public class FSmaintenanceInfo : PartModule
{
    [KSPField(isPersistant=true)]
    public float flightTime = 0f;
    [KSPField(isPersistant = true)]
    public int cycles = 0;
}