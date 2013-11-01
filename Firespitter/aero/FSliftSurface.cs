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

    float zeroLiftDrag = 0.0161f; // p-51 Mustang: 0.0161. sopwith camel: 0.0378
    private Transform liftTransform;
    private Rigidbody commonRigidBody;
    private Vector3 rigidBodyVelocity;
    private float airDensity = 1f;
    public float lift = 0f;
    public float drag = 0f;
        
    public bool debugMode = false;
    private Vector2 liftAndDrag = new Vector2(0f, 0f);

    public float AngleOfAttack
    {
        get
        {
            return CalculateAoA(liftTransform) * Mathf.Rad2Deg;
        }
    }

    private float CalculateAoA(Transform wingOrientation)
    {
        float PerpVelocity = Vector3.Dot(wingOrientation.up, commonRigidBody.velocity.normalized);
        float AoA = Mathf.Asin(Mathf.Clamp(PerpVelocity, -1, 1));
        return AoA;
    }

    private Vector2 getLiftAndDrag()
    {
        commonRigidBody = part.Rigidbody;
        if (commonRigidBody != null)
        {
            float speed = commonRigidBody.GetPointVelocity(liftTransform.position).magnitude;
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

    public override void OnStart(PartModule.StartState state)
    {
 	     base.OnStart(state);
    
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

    // Update is called once per frame
    public void FixedUpdate()
    {
       // base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight) return;

        airDensity = (float)vessel.atmDensity;
        liftAndDrag = getLiftAndDrag();
        commonRigidBody.AddForceAtPosition(liftAndDrag.x * -liftTransform.up, liftTransform.position);
        commonRigidBody.AddForceAtPosition(liftAndDrag.y * dragMultiplier * -commonRigidBody.GetPointVelocity(liftTransform.position).normalized, liftTransform.position);
    }
}

