using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSaltimeterCounter : InternalModule
{
    [KSPField]
    public Vector3 defaultRotation = new Vector3(15f, -90, 90);
    [KSPField]
    public Vector3 rotationAxis = new Vector3(-1f, 0f, 0f);
    [KSPField]
    public string wheel0 = string.Empty;
    [KSPField]
    public string wheel1 = string.Empty;
    [KSPField]
    public string wheel2 = string.Empty;
    [KSPField]
    public string wheel3 = string.Empty;
    [KSPField]
    public string wheel4 = string.Empty;
    [KSPField]
    public string wheel5 = string.Empty;
    [KSPField]
    public string wheel6 = string.Empty;
    [KSPField]
    public string wheel7 = string.Empty;
    [KSPField]
    public string wheel8 = string.Empty;
    [KSPField]
    public string wheel9 = string.Empty;
    [KSPField]
    public string infoWheel = string.Empty;

    private Firespitter.cockpit.AnalogCounter analogCounter = new Firespitter.cockpit.AnalogCounter();    

    public void addWheel(string wheelName)
    {
        if (wheelName != string.Empty)
        {
            Transform newWheel = base.internalProp.FindModelTransform(wheelName);
            if (newWheel != null)
            {
                analogCounter.wheels.Add(newWheel);
            }
            else
            {
                Debug.Log("FSaltimeterCounter: Could not find dial wheel " + wheelName);
            }
        }        
    }

    public void Start()
    {
        addWheel(wheel0);
        addWheel(wheel1);
        addWheel(wheel2);
        addWheel(wheel3);
        addWheel(wheel4);
        addWheel(wheel5);
        addWheel(wheel6);
        addWheel(wheel7);
        addWheel(wheel8);
        addWheel(wheel9);

        analogCounter.rotationAxis = rotationAxis;
        analogCounter.defaultRotation = defaultRotation;
        //Debug.Log("altimeter Counter list: " + analogCounter.wheels.Count);
    }

    public override void OnUpdate()
    {                
        analogCounter.updateNumber((float)vessel.altitude);
    }
}
