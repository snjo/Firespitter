using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSanimateManually : PartModule
{
    [KSPField]
    public string targetObject;
    [KSPField]
    public int allowInvert = 0;
    [KSPField]
    public string animationName = "Retract";

    [KSPField(guiActive = false, guiName = "invertMotion", isPersistant = true)]    
    private bool invertMotion = false;
    [KSPField(guiActive = false, guiName = "invertSet", isPersistant = true)]
    private bool invertSet = false;

    [KSPField]
    public Vector2 startAndEndTime = new Vector2(0f, 1f);

    private Transform objectTransform;
    private Transform originalTransform;
    private Transform endTransform;
    private Transform endTransformInverted;
    Animation anim;
    private bool animationExists = false;
    private bool objectExists = false;    
    private bool animatingForward = false;
    private float currentTime;
    private float oldTime = -1; // default to -1 so that the position update is forced the firste time it's run

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        //get the starting rotations           
        objectTransform = part.FindModelTransform(targetObject);
        originalTransform = new GameObject().transform;
        endTransform = part.FindModelTransform(targetObject + "End");
        endTransformInverted = part.FindModelTransform(targetObject + "EndInverted");
        if (!objectTransform)
        {
            Debug.LogWarning("FSanimateManually: No such object, " + targetObject);
            objectExists = false;
        }
        else
        {
            //Debug.Log("FSanimateManually: Found object " + targetObject);
            originalTransform.localPosition = objectTransform.localPosition;
            originalTransform.localRotation = objectTransform.localRotation;
            //originalTransform = objectTransform;
            objectExists = true;
        }

        //Debug.Log("FSanimateManually: looking for anim");
        anim = part.GetComponentInChildren<Animation>();
        if (anim != null)
        {            
            //Debug.Log("FSanimateManually: Found anim ");// + animationName + " / " + anim.name);
            animationExists = true;
        }
        else
        {
            Debug.LogWarning("FSanimateManually: no animation, ");
        }
        //set the rotation based on flip when in the VAB/SPH, or at launch  
        // NOPE, can't run the update code in the sph, because there is no vessel object yet, so no finding left/right
        // maybe some world coords?
        
    }

    public override void OnUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight) return;
        if (!invertSet) //run only the first time the craft is loaded
        {
            //check if the part is on the left or right side of the ship
            if (Vector3.Dot(objectTransform.position.normalized, vessel.ReferenceTransform.right) < 0) // below 0 means the engine is on the left side of the craft
            {
                invertMotion = true;
               // Debug.Log("FSanimateManually: Inverting left side Gear");
            }
           invertSet = true;
        }

        if (animationExists && objectExists)
        {            
            //check the animation time, normalize
            currentTime = anim[animationName].normalizedTime;
            float useTime = currentTime;
            if (currentTime != 0f)
            {
                animatingForward = (currentTime > oldTime);
            }
            if (animatingForward && currentTime == 0f)
                useTime = 1f;                 

            //set the rotation if the animation has progressed
            //if (useTime != oldTime)
            //{
                if (invertMotion && allowInvert == 1)
                {
                    //add translation when needed
                    objectTransform.localRotation = Quaternion.Lerp(endTransformInverted.localRotation, originalTransform.localRotation, useTime);                    
                }
                else
                {
                    //add translation when needed
                    objectTransform.localRotation = Quaternion.Slerp(endTransform.localRotation, originalTransform.localRotation, useTime);
                }
            //}  

                oldTime = currentTime;
        }
        else
        {
            Debug.LogWarning("FSanimateManually: Error, missing object " + targetObject + ": " + objectExists + " / missing animation " + animationName + ": " + animationExists);
        }
    }
}
