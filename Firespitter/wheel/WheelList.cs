using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class WheelList
{
    public List<WheelClass> wheels;
    //public List<WheelCollider> wheelColliders = new List<WheelCollider>();
    //public List<Transform> wheelMeshes;
    //public List<Transform> suspensionParents;
    private bool _enabled = false;
    private float _brakeTorque = 0f;
    private float _motorTorque = 0f;
    private float _radius = 0.25f;
    private float _suspensionDistance = 0.025f;
    private float _mass = 0.1f;
    public float forwardStiffness = 10f;
    public float forwardsExtremumSlip = 1.0f;
    public float forwardsExtremumValue = 20000.0f;
    public float forwardsAsymptoteSlip = 2.0f;
    public float forwardsAsymptoteValue = 10000.0f;
    public float sidewaysStiffness = 1.0f;
    public float sidewaysExtremumSlip = 1.0f;
    public float sidewaysExtremumValue = 20000.0f;
    public float sidewaysAsymptoteSlip = 2.0f;
    public float sidewaysAsymptoteValue = 10000.0f;

    public void Create(List<WheelCollider> colliders, List<Transform> wheelMeshes, List<Transform> suspensionParents)
    {
        wheels = new List<WheelClass>();
        for (int i = 0; i < colliders.Count; i++)
        {
            wheels.Add(new WheelClass(colliders[i]));
            if (i < wheelMeshes.Count)
            {
                wheels[i].wheelMesh = wheelMeshes[i];
                wheels[i].useRotation = true;
                wheels[i].setupFxLocation();
            }
            if (i < suspensionParents.Count)
            {
                wheels[i].suspensionParent = suspensionParents[i];
                wheels[i].useSuspension = true;
            }            
        }        
    }

    public void Create(WheelCollider collider, Transform wheelMesh, Transform suspensionParent)
    {
        wheels = new List<WheelClass>();
        wheels.Add(new WheelClass(collider, wheelMesh, suspensionParent));
    }

    public bool enabled
    {
        get
        {
            return _enabled;
        }

        set
        {
            _enabled = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.enabled = value;
            }
        }
    }

    public float brakeTorque
    {
        get
        {
            return _brakeTorque;
        }
        set
        {
            _brakeTorque = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.brakeTorque = value;
            }
        }
    }

    public float motorTorque
    {
        get
        {
            return _motorTorque;
        }
        set
        {
            _motorTorque = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.motorTorque = value;
                //Debug.Log("torque: " + value);
            }
        }
    }

    public void updateSpring(float spring, float damper, float targetPosition)
    {
        JointSpring jointSpring = new JointSpring();
        jointSpring.spring = spring;
        jointSpring.damper = damper;
        jointSpring.targetPosition = targetPosition;
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].wheelCollider.suspensionSpring = jointSpring;
        }
    }

    public float suspensionDistance
    {
        get
        {
            return _suspensionDistance;
        }
        set
        {
            _suspensionDistance = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.suspensionDistance = value;
            }
        }
    }

    public float mass
    {
        get
        {
            return _mass;
        }
        set
        {
            _mass = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.mass = value;
            }
        }
    }

    public float radius
    {
        get
        {
            return _radius;
        }
        set
        {
            _radius = value;
            for (int i = 0; i < wheels.Count; i++)
            {
                wheels[i].wheelCollider.radius = value;
            }
        }
    }

    public void updateWheelFriction()
    {

        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = forwardsExtremumSlip;
        forwardFriction.extremumValue = forwardsExtremumValue;
        forwardFriction.asymptoteSlip = forwardsAsymptoteSlip;
        forwardFriction.asymptoteValue = forwardsAsymptoteValue;
        forwardFriction.stiffness = forwardStiffness;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = sidewaysExtremumSlip;
        sidewaysFriction.extremumValue = sidewaysExtremumValue;
        sidewaysFriction.asymptoteSlip = sidewaysAsymptoteSlip;
        sidewaysFriction.asymptoteValue = sidewaysAsymptoteValue;
        sidewaysFriction.stiffness = sidewaysStiffness;

        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }
}