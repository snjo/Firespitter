using System;
using UnityEngine;

namespace Firespitter.aero
{
    class FSbladeLiftSurface : MonoBehaviour
    {
        [KSPField]
        public string liftTransformName = "bladePoint";
        [KSPField]
        public string referenceTransformName = "bladeRef";
        //[KSPField]
        //public string debugCubeName = "Cube";
        [KSPField]
        public float power = 0.0008f;
        [KSPField]
        public float wingArea = 1f;
        [KSPField]
        public float span = 4f;
        [KSPField]
        public string displayName = "Rotor blade";
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

        public Part part;
        public GameObject thisGameObject;
        //public bool FARActive = false;

        public Transform liftTransform;
        public Transform referenceTransform;
        public Transform debugCubeTransform;
        private Rigidbody commonRigidBody;
        public Quaternion originalRotation;
        //private Vector3 rigidBodyVelocity;
        private float airDensity = 1f;
        public float lift = 0f;
        public float discDrag = 0f;
        public float bladeDrag = 0f;
        public bool fullSimulation = false;

        public float pointVelocityMagnitude = 0f;

        [KSPField]
        public bool debugMode = true;
        private bool flightStarted = false;
        private Vector2 liftAndDrag = new Vector2(0f, 0f);
        private float speed = 0f;
        public float realSpeed = 0f;
        public Vector3 bladeVelocity = Vector3.zero;
        public Vector3 partVelocity = Vector3.zero;

        // info field used for debugging
        public float bladePitch = 0f;

        //private List<FSbladeLiftSurface> liftSurfaces = new List<FSbladeLiftSurface>();

        private bool initialized = false;
        private info.FSdebugMessages debug;

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
                return CalculateAoA(liftTransform, bladeVelocity) * Mathf.Rad2Deg;
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
            
            Vector3 pointVelocity = -referenceTransform.forward.normalized * pointVelocityMagnitude;
            
            if (part == null)
            {
                debug.debugMessage("FSbladeLiftSurface: part is null");
                return Vector2.zero;
            }
            
            commonRigidBody = part.Rigidbody;
            if (commonRigidBody != null)
            {
                
                partVelocity = GetVelocity(commonRigidBody, liftTransform.position);
                //bladeVelocity = partVelocity + pointVelocity;
                bladeVelocity = pointVelocity; // test
                realSpeed = (bladeVelocity + partVelocity).magnitude;
                if (fullSimulation) bladeVelocity += partVelocity;
                //velocity = pointVelocity; //+ (GetVelocity(commonRigidBody, liftTransform.position).magnitude * -liftTransform.up);

                
                speed = bladeVelocity.magnitude;
                float angleOfAttackRad = CalculateAoA(liftTransform, bladeVelocity);
                float liftCoeff = 2f * Mathf.PI * angleOfAttackRad;
                lift = 0.5f * liftCoeff * airDensity * (speed * speed) * wingArea;
                float aspectRatio = (span * span) / wingArea;
                float dragCoeff = zeroLiftDrag + (liftCoeff * liftCoeff) / (Mathf.PI * aspectRatio * efficiency);
                bladeDrag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;

                discDrag = 0.5f * dragCoeff * airDensity * (partVelocity.magnitude * partVelocity.magnitude) * wingArea;

                
                lift *= power; // modified by too low blade speed //;
                discDrag *= power;
                bladeDrag *= power;
            }
            return new Vector2(lift, discDrag);
            //return new Vector2(lift, bladeDrag);
        }

        private Vector3 getLiftVector() // from Ferram
        {
            Vector3 ParallelInPlane = Vector3.Exclude(liftTransform.up, bladeVelocity).normalized;  //Projection of velocity vector onto the plane of the wing
            Vector3 perp = Vector3.Cross(liftTransform.up, ParallelInPlane).normalized;       //This just gives the vector to cross with the velocity vector
            Vector3 liftDirection = Vector3.Cross(perp, bladeVelocity).normalized;
            Vector3 liftVector = liftDirection * liftAndDrag.x;
            return liftVector;
        }

        //public override string GetInfo()
        //{
        //    string info = string.Empty;
        //    info = String.Concat("Aerodynamic surface\nName: ",
        //        displayName, "\n",
        //        "Area: ", wingArea);

        //    return info;
        //}

        //public override void OnStart(PartModule.StartState state)
        //{
        //    if (!initialized)
        //    {
        //        initialize();      
        //    }
        //}

        private Transform findTransform(string transformName)
        {
            Transform result = null;

            foreach (Transform t in thisGameObject.GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name == transformName)
                {
                    result = t;
                    debug.debugMessage("Found transform " + transformName);
                    break;
                }
            }
            return result;
        }

        public void initialize()
        {
            debug = new info.FSdebugMessages(debugMode, "FSbladeLiftSurface");
            //liftTransform = part.FindModelTransform(liftTransformName);            
            //referenceTransform = part.FindModelTransform(referenceTransformName);
            //debugCubeTransform = part.FindModelTransform(debugCubeName);

            liftTransform = findTransform(liftTransformName);
            referenceTransform = findTransform(referenceTransformName);            

            if (liftTransform == null)
            {
                debug.debugMessage("FSliftSurface: Can't find lift transform " + liftTransformName);
            }
            else
            {
                originalRotation = liftTransform.localRotation;
            }

            if (referenceTransform == null)
            {
                debug.debugMessage("FSliftSurface: Can't find lift transform " + liftTransformName);
            }            

            //liftSurfaces = part.GetComponents<FSbladeLiftSurface>().ToList();

            initialized = true;
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !initialized) return;

            try
            {
                airDensity = (float)part.vessel.atmDensity;
                liftAndDrag = getLiftAndDrag();

                Vector3 liftVector = getLiftVector();

                commonRigidBody.AddForceAtPosition(liftVector, liftTransform.position);

                commonRigidBody.AddForceAtPosition(liftAndDrag.y * dragMultiplier * -commonRigidBody.GetPointVelocity(liftTransform.position).normalized, liftTransform.position);
            }
            catch (Exception e)
            {
                if (debugMode)
                    debug.debugMessage("FSbladeLiftSurface FixedUpdate Exception " + e.GetType().ToString());
            }
        }

        //public override void OnUpdate()
        //{            
        //    flightStarted = true;
        //}

        //public void OnCenterOfLiftQuery(CenterOfLiftQuery qry)
        //{
        //    if (moduleID == 0)
        //    {
        //        CoLqueryData queryData = new CoLqueryData();
        //        queryData.refVector = qry.refVector;
        //        for (int i = 0; i < liftSurfaces.Count; i++)
        //        {
        //            CoLqueryData newQuery = liftSurfaces[i].liftQuery(queryData.refVector);
        //            float influence = new Vector2(queryData.dir.magnitude, newQuery.dir.magnitude).normalized.y;
        //            queryData.pos = Vector3.Lerp(queryData.pos, newQuery.pos, influence);
        //            queryData.lift += newQuery.lift;
        //            queryData.dir = Vector3.Lerp(queryData.dir, newQuery.dir, influence);
        //        }

        //        queryData.dir.Normalize();

        //        qry.dir = queryData.dir;
        //        qry.lift = queryData.lift;
        //        qry.pos = queryData.pos;
        //    }
        //}

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
            discDrag = 0.5f * dragCoeff * airDensity * (speed * speed) * wingArea;

            lift *= power;
            discDrag *= power;

            qry.pos += liftTransform.position;
            qry.dir += -liftTransform.up * lift;
            qry.lift += qry.dir.magnitude;
            //qry.dir.Normalize();

            return qry;
        }
    }
}
