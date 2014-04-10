using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;


namespace Firespitter.engine
{
    public class FSgroundParticles : PartModule
    {        
        //TODO: look for engine and engine thrust

        // The URL to the bitmap, no file extension, for the dust particle
        [KSPField]
        public string particleTextureName = "Firespitter/textures/propwash";

        // A ray is cast from this transform, to check for engine thrust impacting the ground
        [KSPField]
        public string thrustTransformName = "thrustTransform";
        private Transform thrustTransform;
        private RaycastHit[] hit;

        // a disc mesh will be created at runtime. Particles spawn inside this mesh on the ground
        private GameObject washDisc = new GameObject();        

        // how far off the ground you can be and still spawn dust particles
        [KSPField]
        public float maxDistance = 10f;
        private float currentDistance = 5f;

        // the max amount of particles to spawn
        [KSPField]
        public float emission = 200f;
        private float currentEmission = 0f;

        // the radius of the disc in which particles spawn on the ground
        [KSPField]
        public float emissionDiscSize = 3f;        

        private MeshFilter meshFilter;

        // a class which contains mesh emitter, animator and renderer, assigned to a GameObject
        private FSparticleFX particleFX;
        private Texture2D particleTexture;

        void Start()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;


            // Create the mesh disc. Particles spawn inside this mesh on the ground
            washDisc.transform.parent = transform;
            meshFilter = washDisc.AddComponent<MeshFilter>();            
            meshFilter.mesh = tools.MeshCreator.createDisc(emissionDiscSize, 100);

            // fetch the particle texture from KSP's Game Database
            particleTexture = GameDatabase.Instance.GetTexture(particleTextureName, false);            
                                      
            if (particleTexture == null)
            {
                Debug.Log("FSgroundParticles: particle texture loading error");
                // it should use the default particle in this case, or just some pink crap maybe
            }
            else
            {
                //Setting the values for the particle system. the animator is never doing anything exciting, all particle motion is handled in the late update code
                particleFX = new FSparticleFX(washDisc, particleTexture);  
           
                // particles change color and alpha over time.
                particleFX.AnimatorColor0 = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                particleFX.AnimatorColor1 = new Color(1.0f, 1.0f, 1.0f, 0.1f);
                particleFX.AnimatorColor2 = new Color(1.0f, 1.0f, 1.0f, 0.15f);
                particleFX.AnimatorColor3 = new Color(1.0f, 1.0f, 1.0f, 0.2f);
                particleFX.AnimatorColor4 = new Color(1.0f, 1.0f, 1.0f, 0.05f);

                particleFX.EmitterMinSize = 0.5f;
                particleFX.EmitterMaxSize = 0.5f;
                particleFX.EmitterMinEnergy = 3f;
                particleFX.EmitterMaxEnergy = 3f;
                particleFX.EmitterMinEmission = 0f;
                particleFX.EmitterMaxEmission = 0f;
                particleFX.AnimatorSizeGrow = 1f;

                particleFX.EmitterLocalVelocity = new Vector3(0f, 0f, 0f);
                particleFX.EmitterRndVelocity = new Vector3(0f, 0f, 0f);                
                // creates the emitters etc and assigns the above values
                particleFX.setupFXValues();

                //particleFX.pEmitter.rndRotation = true;

                // Can't turn on Interpolate Triangles on the emitter, casue it's not exposed to code. REALLY?!? WHY?
            }

            thrustTransform = part.FindModelTransform(thrustTransformName);
        }

        void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            float distanceFromGround = maxDistance;

            // shoot a ray from the thrustTransform, along the direction of thrust. If it hits the ground, the distance value will be less than maxDistance, 
            // moving the disc to that location and causing particles to appear.
            Ray ray = new Ray(thrustTransform.position, thrustTransform.forward);//-vessel.upAxis);
            hit = Physics.RaycastAll(ray, 15f);
            for (int i = 0; i < hit.Length; i++)
            {
                // layer 15 is the landscap/buildine layer. parts are layer 10, ignore those. the runway should also be layer 15, but it's not registering properly...
                if (hit[i].collider.gameObject.layer == 15)
                {
                    washDisc.transform.position = hit[i].point + Vector3.up * 0.1f;                    
                    distanceFromGround = hit[i].distance;
                    break;
                }
            }

            // TODO: make prop wash appear on the ocean surface

            //float seaAltitude = Vector3.Distance(washDisc.transform.position, vessel.mainBody.position) - (float)vessel.mainBody.Radius;
            //if (seaAltitude < 0f)
            //{
            //    washDisc.transform.Translate(vessel.upAxis * seaAltitude);
            //    distanceFromGround = Vector3.Distance(thrustTransform.position, washDisc.transform.position);
            //}

            //Debug.Log("seaAltitude: " + seaAltitude);

            // rotate the disc so it's horizontal (does not follow the terrain slope though. Maybe there is a terrain normal to look at, but it looks OK on hills as is)
            washDisc.transform.LookAt(transform.position + vessel.upAxis, Vector3.forward);                 

            // scale the emission amount based on distance from ground
            currentDistance = Mathf.Clamp(distanceFromGround, 1f, maxDistance);

            currentEmission = ((maxDistance / currentDistance) * emission) - emission;
            currentEmission = Mathf.Clamp(currentEmission, 0f, emission);

            particleFX.pEmitter.minEmission = currentEmission;
            particleFX.pEmitter.maxEmission = currentEmission;
        }

        void LateUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            // to change particles you first have to get the array, modify it, then feed the whole thing back to the emitter
            Particle[] particles = particleFX.pEmitter.particles;

            for (int i = 0; i < particles.Length; i++)
            {
                // Oh hey, you can't access Interpolate Triangles on mesh emitters, so I have to this junk! Fuck you, whoever made the old Unity particle system.
                // if a new particle has a very high energy, it means it's a newly created one. Move it!
                if (particles[i].energy > particles[i].startEnergy - (Time.deltaTime * 1.1f))
                {
                    //particles spawn on the outer points of the disc. move it a random amount towrds the center to distribute the spawning. a high number of outer points makes it look OK without exra sideways randomness.
                    particles[i].position = Vector3.Lerp(particles[i].position, washDisc.transform.position, UnityEngine.Random.value);
                }
                
                // The position of the current particle relative to the disc center
                Vector3 offset = washDisc.transform.position - particles[i].position;
                // Repel the particles. The closer a particle is to the disc center, the faster it moves away from it.
                particles[i].position -= offset.normalized * 0.01f * Mathf.Clamp((maxDistance - currentDistance) - offset.magnitude, 1f, 15f);
            }

            // assign the array back to the emitter
            particleFX.pEmitter.particles = particles;
        }
    }
}