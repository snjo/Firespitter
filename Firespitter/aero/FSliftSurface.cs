using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSliftSurface : PartModule
{
    [KSPField]
    public string liftTransformName = "lift";
    [KSPField]
    public float power = 0.0002f;
    [KSPField]
    public float wingArea = 12f;
    [KSPField]
    public float span = 6f;
    [KSPField]
    public string displayName = "Aileron"; 
    [KSPField]
    public float efficiency = 1f; // wright's plane 0.7f
    [KSPField]
    public float dragMultiplier = 1f;
    //[KSPField]
    //public float glideEfficiency = 0.015f; // used by cheatyLiftVector 0.002 in the unity test.

    float zeroLiftDrag = 0.0161f; // p-51 Mustang: 0.0161. sopwith camel: 0.0378
    private Transform liftTransform;
    private Rigidbody commonRigidBody;
    //private Vector3 rigidBodyVelocity;
    private float airDensity = 1f;
    public float lift = 0f;
    public float drag = 0f;
        
    public bool debugMode = true;
    private bool initialized = false;
    private Vector2 liftAndDrag = new Vector2(0f, 0f);
    private float speed = 0f;
    private Vector3 velocity = Vector3.zero;

    public Vector3 GetVelocity(Rigidbody rigidbody, Vector3 refPoint) //from Ferram
    {
        Vector3 newVelocity = Vector3.zero;
        //newVelocity = commonRigidBody.velocity + Krakensbane.GetFrameVelocity() + Vector3.Cross(commonRigidBody.angularVelocity, liftTransform.position - commonRigidBody.position);
        newVelocity += rigidbody.GetPointVelocity(refPoint);
        newVelocity += Krakensbane.GetFrameVelocityV3f() - Krakensbane.GetLastCorrection() * TimeWarp.fixedDeltaTime; // OR +?
        return newVelocity;
    }

    public float AngleOfAttack
    {
        get
        {
            return CalculateAoA(liftTransform) * Mathf.Rad2Deg;
        }
    }

    private float CalculateAoA(Transform wingOrientation)
    {
        float PerpVelocity = Vector3.Dot(wingOrientation.up, velocity.normalized);
        float AoA = Mathf.Asin(Mathf.Clamp(PerpVelocity, -1, 1));
        return AoA;
    }

    private Vector2 getLiftAndDrag()
    {
        commonRigidBody = part.Rigidbody;
        if (commonRigidBody != null)
        {
            velocity = GetVelocity(commonRigidBody, liftTransform.position);
            //velocity = commonRigidBody.GetPointVelocity(liftTransform.position);
            speed = velocity.magnitude;
            float angleOfAttackRad = CalculateAoA(liftTransform);
            float liftCoeff = 2f * Mathf.PI * angleOfAttackRad;
            lift = 0.5f * liftCoeff * airDensity * (speed * speed) * wingArea;
            float aspectRatio = (span * span) / wingArea;
            float dragCoeff = zeroLiftDrag + (liftCoeff * liftCoeff) / (Mathf.PI * aspectRatio * efficiency);
            drag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;

            lift *= power;
            drag *= power;
        }
        return new Vector2(lift, drag);
    }

    private Vector3 getLiftVector()
    {
        Vector3 ParallelInPlane = Vector3.Exclude(liftTransform.up, velocity).normalized;  //Projection of velocity vector onto the plane of the wing
        Vector3 perp = Vector3.Cross(liftTransform.up, ParallelInPlane).normalized;       //This just gives the vector to cross with the velocity vector
        Vector3 liftDirection = Vector3.Cross(perp, velocity).normalized;
        Vector3 liftVector = liftDirection * liftAndDrag.x;
        return liftVector;
    } 

    public override void OnStart(PartModule.StartState state)
    {
 	     base.OnStart(state);
        initialized = true;
    
        if (liftTransformName == string.Empty)
        {
            liftTransform = part.transform;
        }
        else
        {
            liftTransform = part.FindModelTransform(liftTransformName);
        }

        if (liftTransform == null)
        {
            Debug.Log("FSliftSurface: Can't find lift transform " + liftTransformName);
        }                
    }

    public void FixedUpdate()
    {
       // base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !initialized) return;

        airDensity = (float)vessel.atmDensity;
        liftAndDrag = getLiftAndDrag();

        Vector3 liftVector = getLiftVector();

        //Vector3 liftVector = liftAndDrag.x * -liftTransform.up;
        commonRigidBody.AddForceAtPosition(liftVector, liftTransform.position);

        commonRigidBody.AddForceAtPosition(liftAndDrag.y * dragMultiplier * -commonRigidBody.GetPointVelocity(liftTransform.position).normalized, liftTransform.position);
    }   
}

