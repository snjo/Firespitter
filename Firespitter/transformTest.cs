using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class transformTest : PartModule
{
    [KSPField]
    public string particleTextureName = "Firespitter/textures/particle";
    [KSPField]
    public Vector3 EmitterLocalVelocity = new Vector3(0f, 0f, 1f);
    Texture2D particleTexture;
    private Firespitter.FSparticleFX particleFX;
    private Transform refTransform;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        particleTexture = GameDatabase.Instance.GetTexture(particleTextureName, false);
        if (particleTexture != null)
        {            
                particleFX = new Firespitter.FSparticleFX(part.gameObject, particleTexture);
                particleFX.EmitterLocalVelocity = EmitterLocalVelocity;
                //Debug.Log("KTvelocityController: particle texture found: " + particleTextureName);
                particleFX.setupFXValues();
                //particleFX.pEmitter.minEmission = 0f;
                //particleFX.pEmitter.maxEmission = 0f;
                particleFX.pEmitter.localVelocity = Vector3.zero;
                particleFX.pEmitter.useWorldSpace = true;
            
        }
        refTransform = new GameObject().transform;
        refTransform.parent = part.transform;
    }

    public void Update()
    {        
        if (!HighLogic.LoadedSceneIsFlight) return;
        if (vessel == null) return;        
        
        //particleFX.pEmitter.worldVelocity = vessel.ReferenceTransform.up * 5f;
        
        //Debug.Log("vessel forward: " + vessel.ReferenceTransform.up); //forward is down, up is forward

        Vector3 worldUp = Firespitter.Tools.WorldUp(vessel);
        refTransform.position = vessel.ReferenceTransform.position; // part.transform.position;
        refTransform.rotation = Quaternion.LookRotation(vessel.ReferenceTransform.up, -vessel.ReferenceTransform.forward);
        //Debug.Log("vessel up dot worldUp: " + Vector3.Dot(refTransform.up, worldUp));

        particleFX.pEmitter.worldVelocity = worldUp * 5f;
    }

}
