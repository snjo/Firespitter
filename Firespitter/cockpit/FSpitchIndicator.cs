using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSpitchIndicator : InternalModule
{
    public Transform needle;
    public Transform refTransform;    
    [KSPField]
    public bool debugMode = false;
    private float pitch = 0f;
    
    [KSPField]
    public string needleName = "needleHolder";    
    [KSPField]
    public float pitchLimit = 30f;    
    [KSPField]
    public float rollDirection = 1f;        
    private Firespitter.ShipHeading shipHeading;    

    // Use this for initialization
    void Start()
    {        
        refTransform = new GameObject().transform;
        refTransform.parent = part.transform;
        refTransform.rotation = Quaternion.LookRotation(vessel.ReferenceTransform.up, -vessel.ReferenceTransform.forward);
        shipHeading = new Firespitter.ShipHeading(refTransform);

        needle = base.internalProp.FindModelTransform(needleName);
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        pitch = shipHeading.getPitch(refTransform, Firespitter.Tools.WorldUp(vessel));
        if (pitch > 90f) { pitch -= 180f; pitch *= -1f; }
        if (pitch < -90f) { pitch += 180f; pitch *= -1f; }
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
        needle.localRotation = Quaternion.Euler(0f, 0f, pitch);        
    }
}

