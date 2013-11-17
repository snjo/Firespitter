using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter
{
    public class ShipHeading
    {
        // --- attempt 3, works perfectly in unity test ---
        
        //public Transform refDirection; // should correspond to vessel.referencetransform in KSP	
        private Transform rollLevel;
        private Transform pitchLevel;
        //private Vector3 worldUp = Vector3.up;

        public Vector3 getRollRaw(Transform refDirection, Vector3 worldUp)
        {
            //rollLevel.position = refDirection.position;
            rollLevel.rotation = Quaternion.LookRotation(refDirection.forward, worldUp);
            return rollLevel.localRotation.eulerAngles;
        }
        public Vector3 getPitchRaw(Transform refDirection, Vector3 worldUp)
        {
            //rollLevel.position = refDirection.position;
            pitchLevel.rotation = Quaternion.LookRotation(refDirection.right, worldUp);
            return pitchLevel.localRotation.eulerAngles;
        }

        public float getRoll(Transform refDirection, Vector3 worldUp)
        {            
            rollLevel.rotation = Quaternion.LookRotation(refDirection.forward, worldUp);
            //rollLevel.rotation = Quaternion.LookRotation(refDirection.up, worldUp);
            float result = rollLevel.localRotation.eulerAngles.z;
            if (result > 180f) result -= 360f;
            return result;
        }

        public float getPitch(Transform refDirection, Vector3 worldUp)
        {            
            //pitchLevel.rotation = Quaternion.LookRotation(refDirection.right, worldUp);
            pitchLevel.rotation = Quaternion.LookRotation(refDirection.right, worldUp);
            float result = pitchLevel.localRotation.eulerAngles.z;
            if (result > 180f) result -= 360f;
            return result;
        }

        // Use this for initialization
        public ShipHeading(Transform parentTransform)
        {
            rollLevel = new GameObject().transform;            
            rollLevel.transform.parent = parentTransform;
            rollLevel.localPosition = Vector3.zero;

            pitchLevel = new GameObject().transform;
            pitchLevel.transform.parent = parentTransform;
            pitchLevel.localPosition = Vector3.zero;
        }

        // --- attempt 2 ---

        //private Transform rollHelper = new GameObject().transform;
        //private Transform pitchHelper = new GameObject().transform;
        //private Transform mainHelper = new GameObject().transform;

        //// code from: http://answers.unity3d.com/questions/187249/attitude-indicator-for-flight-sim.html?sort=oldest

        //public Vector3 AngleHeadings(Vessel vessel)
        //{
        //    Vector3 result = Vector3.zero;
        //    Vector3 worldUp = Firespitter.Tools.WorldUp(vessel); //WorldUp(vessel);
        //    mainHelper.rotation = Quaternion.LookRotation(worldUp, vessel.transform.forward);
        //    mainHelper.rotation = Quaternion.LookRotation(mainHelper.up, mainHelper.forward);

        //    //rollHelper.forward = vessel.transform.forward;
        //    rollHelper.rotation = mainHelper.rotation;
        //    rollHelper.forward = mainHelper.forward;
        //    if (Vector3.Dot(rollHelper.up, vessel.transform.right) > 0f)
        //    {
        //        result.z = -Vector3.Angle(rollHelper.right, vessel.transform.right);
        //    }
        //    else
        //    {
        //        result.z = Vector3.Angle(rollHelper.right, vessel.transform.right);
        //    }

        //    //pitchHelper.forward = vessel.transform.forward;
        //    pitchHelper.rotation = mainHelper.rotation;
        //    //pitchHelper.forward = mainHelper.forward;
        //    pitchHelper.forward = new Vector3(mainHelper.forward.x, 0f, mainHelper.forward.z);
        //    if (Vector3.Dot(pitchHelper.up, vessel.transform.forward) > 0f)
        //    {
        //        result.x = Vector3.Angle(pitchHelper.forward, vessel.transform.forward);
        //    }
        //    else
        //    {
        //        result.x = -Vector3.Angle(pitchHelper.forward, vessel.transform.forward);
        //    }

        //    return result;
        //}

        // --- attempt 1 ---

        //private static Transform target;
        //private static Quaternion offsetGimbal;
        //private static Quaternion attitudeGimbal;
        //private static Quaternion relativeGimbal;
        //public static Transform navBall = new GameObject().transform;
        //public static Vector3 rotationOffset = Vector3.zero;
        //private static Vector3 vesselMainBodyPosition;

        //public static float heading(Vessel vessel)
        //{
        //    return orientation(vessel).y;
        //}

        //public static float pitch(Vessel vessel)
        //{
        //    return orientation(vessel).x;
        //}

        //public static float roll(Vessel vessel)
        //{
        //    return orientation(vessel).z;
        //}

        //public static Vector3 orientation(Vessel vessel)
        //{
        //    Transform target;
        //    Quaternion offsetGimbal;
        //    Quaternion attitudeGimbal;
        //    Quaternion relativeGimbal;
        //    Transform navBall = new GameObject().transform;
        //    Vector3 rotationOffset = Vector3.zero;
        //    Vector3 vesselMainBodyPosition;

        //    Vector3 result = Vector3.zero;

        //    target = vessel.ReferenceTransform;
        //    offsetGimbal = Quaternion.Euler(rotationOffset);
        //    attitudeGimbal = offsetGimbal * Quaternion.Inverse(target.rotation);
        //    vesselMainBodyPosition = vessel.mainBody.position;
        //    relativeGimbal = attitudeGimbal * Quaternion.LookRotation(Vector3.Exclude((target.position - vesselMainBodyPosition).normalized, vesselMainBodyPosition + vessel.mainBody.transform.up * (float)vessel.mainBody.Radius - target.position).normalized, (target.position - vesselMainBodyPosition).normalized);
        //    navBall.rotation = relativeGimbal;


        //    Quaternion vesselRot = Quaternion.Inverse(relativeGimbal);

        //    //Heading:
        //    result.y = vesselRot.eulerAngles.y;

        //    //Pitch
        //    result.x = (vesselRot.eulerAngles.x > 180) ? (360 - vesselRot.eulerAngles.x) : -vesselRot.eulerAngles.x;

        //    //Roll
        //    result.z = (vesselRot.eulerAngles.z > 180) ? (360 - vesselRot.eulerAngles.z) : -vesselRot.eulerAngles.z;

        //    //return result;
        //    return vesselRot.eulerAngles;
        //}

        
    }
}
