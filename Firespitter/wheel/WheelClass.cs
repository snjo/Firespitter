using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class WheelClass
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;
    public Transform suspensionParent;
    public bool useRotation = false;
    public bool useSuspension = false;

    public float screechCountdown = 0f;
    public Firespitter.FSparticleFX smokeFX;
    public GameObject fxLocation = new GameObject();

    private float deltaRPM = 0f;
    private float oldRPM = 0f;
    public bool oldIsGrounded = true;

    public WheelClass(WheelCollider _wheelCollider, Transform _wheelMesh, Transform _suspensionParent)
    {
        wheelCollider = _wheelCollider;
        wheelMesh = _wheelMesh;
        suspensionParent = _suspensionParent;
        setupFxLocation();
    }

    public WheelClass(WheelCollider _wheelCollider)
    {
        wheelCollider = _wheelCollider;
        useRotation = false;
        useSuspension = false;
    }

    public void setupFxLocation()
    {
        fxLocation.transform.parent = wheelCollider.gameObject.transform;
        fxLocation.transform.localPosition = new Vector3(0f, -wheelCollider.radius * 0.8f, 0f); //
    }

    public float getDeltaRPM()
    {        
        deltaRPM = oldRPM - wheelCollider.rpm;
        oldRPM = wheelCollider.rpm;        
        return deltaRPM;
    }
}
