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
    public float power = 0.0008f;
    [KSPField]
    public float wingArea = 12f;
    [KSPField]
    public float span = 6f;
    [KSPField]
    public string displayName = "Wing"; 
    [KSPField]
    public float efficiency = 1f;   //Wright's plane 0.7f
                                    //Ideal: 1.0
                                    //Boeing 247D: 0.75
                                    //DC-3: 0.785   
                                    //P-51D: 0.69
                                    //L1049G: 0.75
                                    //Piper Cherokee: 0.76
    [KSPField]
    public float dragMultiplier = 1f;
    [KSPField]
    public float zeroLiftDrag = 0.0161f; // p-51 Mustang: 0.0161. sopwith camel: 0.0378
    [KSPField]
    public int moduleID = 0;

    public bool FARActive = false;

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
    private List<FSliftSurface> liftSurfaces = new List<FSliftSurface>();

    public Vector3 GetVelocity(Rigidbody rigidbody, Vector3 refPoint) // from Ferram
    {
        Vector3 newVelocity = Vector3.zero;
        //newVelocity = commonRigidBody.velocity + Krakensbane.GetFrameVelocity() + Vector3.Cross(commonRigidBody.angularVelocity, liftTransform.position - commonRigidBody.position);
        newVelocity += rigidbody.GetPointVelocity(refPoint);
        newVelocity += Krakensbane.GetFrameVelocityV3f() - Krakensbane.GetLastCorrection() * TimeWarp.fixedDeltaTime;
        return newVelocity;
    }

    public float AngleOfAttack
    {
        get
        {
            return CalculateAoA(liftTransform, velocity) * Mathf.Rad2Deg;
        }
    }

    private float CalculateAoA(Transform wingOrientation, Vector3 inVelocity) // from Ferram
    {
        float PerpVelocity = Vector3.Dot(wingOrientation.up, inVelocity.normalized);
        float AoA = Mathf.Asin(Mathf.Clamp(PerpVelocity, -1, 1));
        return AoA;
    }

    private Vector2 getLiftAndDrag()
    {
        commonRigidBody = part.Rigidbody;
        if (commonRigidBody != null)
        {
            velocity = GetVelocity(commonRigidBody, liftTransform.position);            
            speed = velocity.magnitude;
            float angleOfAttackRad = CalculateAoA(liftTransform, velocity);
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

    private Vector3 getLiftVector() // from Ferram
    {
        Vector3 ParallelInPlane = Vector3.Exclude(liftTransform.up, velocity).normalized;  //Projection of velocity vector onto the plane of the wing
        Vector3 perp = Vector3.Cross(liftTransform.up, ParallelInPlane).normalized;       //This just gives the vector to cross with the velocity vector
        Vector3 liftDirection = Vector3.Cross(perp, velocity).normalized;
        Vector3 liftVector = liftDirection * liftAndDrag.x;
        return liftVector;
    } 

    public override string GetInfo()
    {
        string info = string.Empty;
        info = String.Concat("Aerodynamic surface\nName: ",
            displayName, "\n",
            "Area: ", wingArea);

        return info;
    }

    public override void OnStart(PartModule.StartState state)
    {
        FARActive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
        // This line breaks the plugin :(
        if (FARActive)
        {            
            this.enabled = false;
        }
    
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
        //if (moduleID == 0)
        //{
            liftSurfaces = part.GetComponents<FSliftSurface>().ToList();
            //liftSurfaces.Add(this); // uhm, what?
        //}        
    }

    public void FixedUpdate()
    {       
        if (!HighLogic.LoadedSceneIsFlight || !initialized) return;

        airDensity = (float)vessel.atmDensity;
        liftAndDrag = getLiftAndDrag();

        Vector3 liftVector = getLiftVector();

        //Vector3 liftVector = liftAndDrag.x * -liftTransform.up;
        commonRigidBody.AddForceAtPosition(liftVector, liftTransform.position);

        commonRigidBody.AddForceAtPosition(liftAndDrag.y * dragMultiplier * -commonRigidBody.GetPointVelocity(liftTransform.position).normalized, liftTransform.position);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        initialized = true;
    }

    public void OnCenterOfLiftQuery(CenterOfLiftQuery qry)
    {        
        if (moduleID == 0)
        {
            CoLqueryData queryData = new CoLqueryData();
            queryData.refVector = qry.refVector;
            for (int i = 0; i < liftSurfaces.Count; i++)
            {
                CoLqueryData newQuery = liftSurfaces[i].liftQuery(queryData.refVector);
                float influence = new Vector2(queryData.dir.magnitude, newQuery.dir.magnitude).normalized.y;
                queryData.pos = Vector3.Lerp(queryData.pos, newQuery.pos, influence);
                queryData.lift += newQuery.lift;
                queryData.dir = Vector3.Lerp(queryData.dir, newQuery.dir, influence);
            }

            queryData.dir.Normalize();

            qry.dir = queryData.dir;
            qry.lift = queryData.lift;
            qry.pos = queryData.pos;
        }
    }

    public CoLqueryData liftQuery(Vector3 refVector)
    {
        CoLqueryData qry = new CoLqueryData();
        Vector3 testVelocity = refVector;
        speed = testVelocity.magnitude;
        float angleOfAttackRad = 0f;
        if (liftTransform != null)
            angleOfAttackRad = CalculateAoA(liftTransform, testVelocity);
        float liftCoeff = 2f * Mathf.PI * angleOfAttackRad;
        lift = 0.5f * liftCoeff * airDensity * (speed * speed) * wingArea;
        float aspectRatio = (span * span) / wingArea;
        float dragCoeff = zeroLiftDrag + (liftCoeff * liftCoeff) / (Mathf.PI * aspectRatio * efficiency);
        drag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;

        lift *= power;
        drag *= power;

        qry.pos += liftTransform.position;
        qry.dir += -liftTransform.up * lift;
        qry.lift += qry.dir.magnitude;
        //qry.dir.Normalize();

        return qry;
    }
}

public class CoLqueryData
{
    public Vector3 refVector = Vector3.zero;
    public Vector3 pos = Vector3.zero;
    public Vector3 dir = Vector3.zero;
    public float lift = 0f;
}