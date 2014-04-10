using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter
{
    public class FSparticleFX
    {                

        public GameObject gameObject;
        public GameObject parentObject;
        public Texture particleTexture;
        public bool useLocalSpaceVelocityHack = false;

        public ParticleEmitter pEmitter;
        public ParticleRenderer pRenderer;
        public ParticleAnimator pAnimator;

        private bool componentsAdded = false;

        #region component variables

        public string RendererShader = "Particles/Alpha Blended";

        public float EmitterMinSize = 0.1f;
        public float EmitterMaxSize = 0.2f;
        public float EmitterMinEnergy = 0.1f;
        public float EmitterMaxEnergy = 0.8f;
        public float EmitterMinEmission = 120f;
        public float EmitterMaxEmission = 160f;
        public Vector3 EmitterLocalVelocity = new Vector3(0f, 0f, 1f);
        public Vector3 EmitterRndVelocity = new Vector3(0f, 0.1f, 0.1f);
        public bool EmitterUseWorldSpace = true;
        public bool EmitterRandomRotation = true;

        public bool AnimatorDoesAnimateColor = true;
            
        public Color AnimatorColor0 = new Color(0.2f, 0.2f, 0.2f, 0.1f);
        public Color AnimatorColor1 = new Color(0.3f, 0.3f, 0.3f, 0.1f);
        public Color AnimatorColor2 = new Color(0.5f, 0.5f, 0.5f, 0.09f);
        public Color AnimatorColor3 = new Color(0.8f, 0.8f, 0.8f, 0.07f);
        public Color AnimatorColor4 = new Color(1f, 1f, 1f, 0.05f);            

        public float AnimatorSizeGrow = 0.5f;

        #endregion

        public FSparticleFX(GameObject _gameObject, Texture2D _particleTexture)
        {
            gameObject = _gameObject;
            particleTexture = _particleTexture;
        }

        private void addComponents()
        {
            pEmitter = (ParticleEmitter)gameObject.AddComponent("MeshParticleEmitter");
            pRenderer = gameObject.AddComponent<ParticleRenderer>();
            pAnimator = gameObject.AddComponent<ParticleAnimator>();
        }
        
        public void setupFXValues()
        {
            if (!componentsAdded)
            {
                addComponents();
                componentsAdded = true;
            }            
            pRenderer.materials = new Material[1];
            pRenderer.materials[0].shader = Shader.Find(RendererShader);
            pRenderer.materials[0].mainTexture = particleTexture;

            pEmitter.minSize = EmitterMinSize;
            pEmitter.maxSize = EmitterMaxSize;
            pEmitter.minEnergy = EmitterMinEnergy;
            pEmitter.maxEnergy = EmitterMaxEnergy;
            pEmitter.minEmission = EmitterMinEmission;
            pEmitter.maxEmission = EmitterMaxEmission;
            pEmitter.localVelocity = EmitterLocalVelocity;
            pEmitter.rndVelocity = EmitterRndVelocity;
            pEmitter.useWorldSpace = EmitterUseWorldSpace;

            pAnimator.doesAnimateColor = AnimatorDoesAnimateColor;

            Color[] colorAnimation = pAnimator.colorAnimation;
            colorAnimation[0] = AnimatorColor0;
            colorAnimation[1] = AnimatorColor1;
            colorAnimation[2] = AnimatorColor2;
            colorAnimation[3] = AnimatorColor3;
            colorAnimation[4] = AnimatorColor4;
            pAnimator.colorAnimation = colorAnimation;

            pAnimator.sizeGrow = AnimatorSizeGrow;            
        }
        
        public void updateFX()
        {
            
            if (useLocalSpaceVelocityHack)
            {
                //float velMagnitude = parentObject.rigidbody.velocity.magnitude;
                float fxSpeed = Vector3.Dot(gameObject.transform.forward, parentObject.rigidbody.velocity);
                if (fxSpeed > 0f)
                    fxSpeed = 0;
                pEmitter.localVelocity = new Vector3(0f, 0f, 1f + (-fxSpeed * 0.1f));
                //pEmitter.maxEmission = pEmitter.minEmission + (velMagnitude * 3);
            }            
        }

    }
}
