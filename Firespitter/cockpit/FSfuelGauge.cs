using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FSfuelGauge : InternalModule
{
    [KSPField]
    public bool checkFuelInAllParts = false;
    [KSPField]
    public string resourceName = "LiquidFuel";
    [KSPField]
    public string gaugeName = "fuelgauge_liquid";
    [KSPField]
    public float updateFrequency = 3f;
    public double fuelLevel = 0f;
    private Transform gaugeObject;
    private List<PartResource> resourceList = new List<PartResource>();
    public float currentFuel = 0f;
    public float maxFuel = 0f;

    private float resourceUpdateCountdown = 0;    
    int vesselNumParts = 0; // initial value

    private void updateFuel()
    {
        // do resource update if enought ime has passed or the number of parts in the vessel has changed
        if (vesselNumParts != vessel.Parts.Count)
        {            
            getResourceList();
        }

        if (resourceUpdateCountdown <=0)
        {
            currentFuel = 0f;
            maxFuel = 0f;
            foreach (PartResource resource in resourceList)
            {
                try
                {
                    currentFuel += (float)resource.amount;
                    maxFuel += (float)resource.maxAmount;
                }
                catch
                {
                    getResourceList();
                }
            }
            fuelLevel = currentFuel / maxFuel;
            gaugeObject.localScale = new Vector3(1f, 1f, (float)fuelLevel);
            resourceUpdateCountdown = updateFrequency;
            vesselNumParts = vessel.Parts.Count;            
        }
        else
        {
            resourceUpdateCountdown-= Time.deltaTime;
        }
    }
        
    private void getResourceList()
    {
        resourceList.Clear();
        vesselNumParts = vessel.Parts.Count;        
        foreach (Part part in vessel.parts)
        {            
            foreach (PartResource resource in part.Resources)
            {                
                if (resource.resourceName == resourceName)
                {
                    resourceList.Add(resource);
                }
            }
        }        
    }     

    public void Start()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            gaugeObject = base.internalProp.FindModelTransform(gaugeName);
        }
    }

    public override void OnUpdate()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {            
            updateFuel();                        
        }
    }
}
