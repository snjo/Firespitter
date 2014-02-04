using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSremoteControl : PartModule //This is just a testing class for various ideas right now
{

    //public bool isEnabled;
    public bool passActiongGroups;
    public float fogDensity = 0.005f;

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (FlightGlobals.fetch.vesselTargetMode != FlightGlobals.VesselTargetModes.None)
        {
            Vessel target = (Vessel)FlightGlobals.fetch.VesselTarget;
            //target.ctrlState.mainThrottle = 1f;

            foreach (Part part in target.Parts)
            {
                ControlSurface ctrlsurf = part.Modules.OfType<ControlSurface>().FirstOrDefault();
                if (ctrlsurf != null)
                {
                    ctrlsurf.ActivatesEvenIfDisconnected = true;
                    ctrlsurf.inputVector = new Vector3(vessel.ctrlState.X, vessel.ctrlState.Y, vessel.ctrlState.Z);
                }
            }

            target.ctrlState.mainThrottle = vessel.ctrlState.Z;
            target.ctrlState.pitch = vessel.ctrlState.X;
            target.ctrlState.roll = vessel.ctrlState.Z;
            //Vessel targetVessel = (Vessel)target;
            //Debug.Log(target);
            BaseFieldList test = this.part.Fields;
            foreach (BaseField field in test)
            {
                field.guiActive = false;
            }

            fogDensity -= 0.0001f;
            if (fogDensity <= 0f)
            {
                fogDensity = 0.01f;
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.white;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = -50f;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            //Camera.main.clearFlags = CameraClearFlags.SolidColor;
            //Camera.main.backgroundColor = new Color(1f,1f,1f,0.1f);
        }
        else
        {
            
            RenderSettings.fog = false;
            RenderSettings.fogColor = new Color(0.439f, 0.859f, 1.000f, 0.0f); ;
            RenderSettings.fogDensity = 1.5E-05f;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            Camera.main.clearFlags = CameraClearFlags.Depth;
            Camera.main.backgroundColor = new Color(0f,0f,0f,0.02f);  
            
            /*Debug.Log("fs: rsFog " + RenderSettings.fog);
            Debug.Log("fs: fogC " + RenderSettings.fogColor);
            Debug.Log("fs: fDens " + RenderSettings.fogDensity);
            Debug.Log("fs: fMode" + RenderSettings.fogMode);
            Debug.Log("fs: camCF" + Camera.main.clearFlags);
            Debug.Log("fs: camBGc" + Camera.main.backgroundColor);*/
        }
    }
}