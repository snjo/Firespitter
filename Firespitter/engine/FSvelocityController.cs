using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FSvelocityController : PartModule
{
    [KSPField]
    public string thrustTransformName = "thruster";
    [KSPField]
    public float maxThrust = 2f;
    [KSPField]
    public float resourceConsumption = 0.1f;
    [KSPField]
    public string resourceName = "MonoPropellant";
    [KSPField]
    public float lowerSpeedThreshold = 0.1f;
    [KSPField]
    public float upperSpeedThreshold = 2.0f;
    [KSPField]
    public bool useFX = true;
    [KSPField]
    public string particleTextureName = string.Empty;
    [KSPField]
    public Vector3 EmitterLocalVelocity = new Vector3(0f, 0f, 1f);
    [KSPField]
    public string transformThrustDirection = "forward";
    [KSPField]
    public string thrustKey = "delete";
    [KSPField]
    public float minVelocityToActivate = 0.1f;

    public bool controllerActive = false;

    Transform[] transformArray;
    private bool transformsFound = false;
    private Vector3 velocityDirection = new Vector3(0f, 0f, 0f);
    private Firespitter.FSparticleFX[] particleFX;
    private Texture2D particleTexture;
    private Vector3 finalThrust = new Vector3(0f, 0f, 0f);

    private float defaultEmitterMinEmission = 120f;
    private float defaultEmitterMaxEmission = 160f;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (!HighLogic.LoadedSceneIsFlight) return;

        transformArray = part.FindModelTransforms(thrustTransformName);
        if (transformArray.Length > 0)
        {
            transformsFound = true;
            particleFX = new Firespitter.FSparticleFX[transformArray.Length];
        }
        else
        {
            Debug.Log("KTvelocityController: Transforms not found named " + thrustTransformName + ", disabling the module");
            this.enabled = false;
        }

        if (this.enabled && particleTextureName != string.Empty)
        {
            // set up fx ---- TODO, assign each transform its own FX
            particleTexture = GameDatabase.Instance.GetTexture(particleTextureName, false);
            if (particleTexture != null)
            {
                for (int i = 0; i < particleFX.Length; i++)
                {
                    particleFX[i] = new Firespitter.FSparticleFX(transformArray[i].gameObject, particleTexture);
                    particleFX[i].EmitterLocalVelocity = EmitterLocalVelocity;
                    //Debug.Log("KTvelocityController: particle texture found: " + particleTextureName);
                    particleFX[i].setupFXValues();
                    particleFX[i].pEmitter.minEmission = 0f;
                    particleFX[i].pEmitter.maxEmission = 0f;
                    particleFX[i].pEmitter.useWorldSpace = false;
                }
            }
            else
            {
                useFX = false;
                Debug.Log("KTvelocityController: particle texture not found, disabling fx");
            }
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (transformsFound)
        {
            //Debug.Log("Entering fixed update");
            try
            {
                velocityDirection = part.gameObject.rigidbody.velocity;
            }
            catch
            {
                //Debug.Log("failed to find rigidbody velocity");
                return;
            }

            int i = 0;
                        
            bool doThrust = Input.GetKey(thrustKey);

            foreach (Transform t in transformArray)
            {
                float thrustUsed = updateThruster(i, doThrust, t);
                bool resourceReceived = consumeResource(thrustUsed * maxThrust);
                if (!resourceReceived)
                    thrustUsed = 0f;
                if (thrustUsed > 0f)
                    part.gameObject.rigidbody.AddForceAtPosition(finalThrust, t.transform.position);                
                if (useFX)
                {
                    particleFX[i].pEmitter.minEmission = defaultEmitterMinEmission * thrustUsed;
                    particleFX[i].pEmitter.maxEmission = defaultEmitterMaxEmission * thrustUsed;
                }
                i++;
            }
        }
    }

    private bool consumeResource(float modifier)
    {        
        float resourceRequested = resourceConsumption * modifier * TimeWarp.deltaTime;
        if (CheatOptions.InfiniteRCS)
            return true;
        if (modifier > 0f)
        {
            if (base.part.RequestResource(resourceName, resourceRequested) > 0f)
                return true;
        }
        return false;
    }

    private float updateThruster(int fxNumber, bool doThrust, Transform t)
    {
        Vector3 thrustDirection;        
        float thrustModifier = 0f;
        if (doThrust)
        {
            if (transformThrustDirection == "up")
                thrustDirection = t.transform.up;
            else
                thrustDirection = t.transform.forward;

            thrustModifier = Vector3.Dot(thrustDirection, velocityDirection.normalized);
            if (thrustModifier > 0f && velocityDirection.magnitude > minVelocityToActivate)
            {
                finalThrust = -thrustDirection * thrustModifier * maxThrust;
                //part.gameObject.rigidbody.AddForceAtPosition(-thrustDirection * thrustModifier * maxThrust, t.transform.position);
            }
            else
            {
                thrustModifier = 0f;
            }
        }
        
        return thrustModifier;
    }
}
