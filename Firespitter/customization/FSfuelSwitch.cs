using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.customization
{
    public class FSfuelSwitch : PartModule
    {
        [KSPField]
        public string resourceNames = "ElectricCharge;LiquidFuel,Oxidizer;MonoPropellant";
        [KSPField]
        public string resourceAmounts = "100;75,25;200";
        [KSPField]
        public float basePartMass = 0.25f;
        [KSPField]
        public string tankMass = "0;0;0;0";
        [KSPField]
        public bool hasGUI = true;
        [KSPField]
        public bool availableInFlight = false;
        [KSPField]
        public bool availableInEditor = true;

        [KSPField(isPersistant = true)]
        public Vector4 currentAmounts = Vector4.zero;
        [KSPField(isPersistant = true)]
        public int selectedTankSetup = 0;
        [KSPField(isPersistant = true)]
        public bool hasLaunched = false;
        [KSPField]
        public bool showInfo = true; // if false, does not feed info to the part list pop up info menu

        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "Dry mass")]
        public float dryMassInfo = 0f;
        private List<FSmodularTank> tankList = new List<FSmodularTank>();
        private List<float> weightList = new List<float>();
        private bool initialized = false;
        [KSPField (isPersistant = true)]
        private bool brandNewPart = true;
        

        UIPartActionWindow tweakableUI;

        public override void OnStart(PartModule.StartState state)
        {            
            initializeData();
            assignResourcesToPart(false);
            brandNewPart = false;
        }

        private void initializeData()
        {
            if (!initialized)
            {
                setupTankList(false);
                this.weightList = Tools.parseFloats(this.tankMass);
                if (HighLogic.LoadedSceneIsFlight) hasLaunched = true;
                if (hasGUI)
                {
                    Events["nextTankSetupEvent"].guiActive = availableInFlight;
                    Events["nextTankSetupEvent"].guiActiveEditor = availableInEditor;
                }
                else
                {
                    Events["nextTankSetupEvent"].guiActive = false;
                    Events["nextTankSetupEvent"].guiActiveEditor = false;
                }
                initialized = true;
            }
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Next tank setup")]
        public void nextTankSetupEvent()
        {
            selectedTankSetup++;
            if (selectedTankSetup >= tankList.Count)
            {
                selectedTankSetup = 0;
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                currentAmounts = Vector4.zero;
            }
            assignResourcesToPart(true);            
        }

        public void selectTankSetup(int i, bool calledByPlayer)
        {            
            initializeData();
            selectedTankSetup = i;
            assignResourcesToPart(calledByPlayer);            
        }

        private void assignResourcesToPart(bool calledByPlayer)
        {            
            // destroying a resource messes up the gui in editor, but not in flight.
            setupTankInPart(part, calledByPlayer);
            if (HighLogic.LoadedSceneIsEditor)
            {
                for (int s = 0; s < part.symmetryCounterparts.Count; s++)
                {
                    setupTankInPart(part.symmetryCounterparts[s], calledByPlayer);
                    FSfuelSwitch symSwitch = part.symmetryCounterparts[s].GetComponent<FSfuelSwitch>();
                    if (symSwitch != null)
                    {
                        symSwitch.selectedTankSetup = selectedTankSetup;
                    }
                }
            }

            Debug.Log("refreshing UI");

            if (tweakableUI == null)
            {
                tweakableUI = Tools.FindActionWindow(part);
            }
            if (tweakableUI != null)
            {
                tweakableUI.displayDirty = true;
            }
            else
            {
                Debug.Log("no UI");
            }
        }

        private void setupTankInPart(Part currentPart, bool calledByPlayer)
        {
            currentPart.Resources.list.Clear();
            PartResource[] partResources = currentPart.GetComponents<PartResource>();
            for (int i = 0; i < partResources.Length; i++)
            {
                DestroyImmediate(partResources[i]);
            }            

            for (int i = 0; i < tankList.Count; i++)
            {
                if (selectedTankSetup == i)
                {
                    for (int j = 0; j < tankList[i].resources.Count; j++)
                    {
                        if (tankList[i].resources[j].name != "Structural")
                        {
                            //Debug.Log("new node: " + tankList[i].resources[j].name);
                            ConfigNode newResourceNode = new ConfigNode("RESOURCE");
                            newResourceNode.AddValue("name", tankList[i].resources[j].name);
                            if (calledByPlayer || brandNewPart)
                            {
                                newResourceNode.AddValue("amount", tankList[i].resources[j].maxAmount);
                                setResource(j, tankList[i].resources[j].amount);
                            }
                            else
                            {
                                newResourceNode.AddValue("amount", getResource(j));
                            }
                            newResourceNode.AddValue("maxAmount", tankList[i].resources[j].maxAmount);

                            //Debug.Log("add node to part");
                            currentPart.AddResource(newResourceNode);                          
                        }
                        else
                        {
                            //Debug.Log("Skipping structural fuel type");
                        }
                    }
                }
            }
            currentPart.Resources.UpdateList();
            updateWeight(currentPart, selectedTankSetup);

            
        }

        private void updateWeight(Part currentPart, int newTankSetup)
        {
            if (newTankSetup < weightList.Count)
            {
                currentPart.mass = basePartMass + weightList[newTankSetup];
            }
        }

        public override void OnUpdate()
        {
            updateResourcePersistence();

            
        }

        private void updateResourcePersistence()
        {
            if (selectedTankSetup < tankList.Count)
            {
                if (tankList[selectedTankSetup] != null)
                {
                    for (int i = 0; i < tankList[selectedTankSetup].resources.Count; i++)
                    {
                        if (tankList[selectedTankSetup].resources[i].name == "Structural")
                        {

                        }
                        else
                        {
                            setResource(i, (float)part.Resources[tankList[selectedTankSetup].resources[i].name].amount);
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                dryMassInfo = part.mass;
                updateResourcePersistence();
            }
        }

        private float getResource(int number)
        {
            switch (number)
            {
                case 0:
                    return currentAmounts.x;
                case 1:
                    return currentAmounts.y;
                case 2:
                    return currentAmounts.z;
                case 3:
                    return currentAmounts.w;
                default:
                    return 0f;
            }
        }

        private void setResource(int number, float amount)
        {
            switch (number)
            {
                case 0:
                    currentAmounts.x = amount;
                    break;
                case 1:
                    currentAmounts.y = amount;
                    break;
                case 2:
                    currentAmounts.z = amount;
                    break;
                case 3:
                    currentAmounts.w = amount;
                    break;
            }
        }

        private void setupTankList(bool calledByPlayer)
        {
            tankList.Clear();

            List<List<float>> resourceList = new List<List<float>>();
            string[] resourceTankArray = resourceAmounts.Split(';');
            for (int i = 0; i < resourceTankArray.Length; i++)
            {
                resourceList.Add(new List<float>());
                string[] resourceAmountArray = resourceTankArray[i].Split(',');
                for (int j = 0; j < resourceAmountArray.Length; j++)
                {
                    try
                    {
                        resourceList[i].Add(float.Parse(resourceAmountArray[j]));
                    }
                    catch
                    {
                        Debug.Log("FSfuelSwitch: error parsing resource amount " + i + "/" +j + ": " + resourceTankArray[j]);
                    }
                }
            }

            string[] tankArray = resourceNames.Split(';');
            for (int i = 0; i < tankArray.Length; i++)
            {
                FSmodularTank newTank = new FSmodularTank();
                tankList.Add(newTank);
                string[] resourceNameArray = tankArray[i].Split(',');
                for (int j = 0; j < resourceNameArray.Length; j++)
                {
                    engine.FSresource newResource = new engine.FSresource(resourceNameArray[j]);
                    if (resourceList[i] != null)
                    {
                        if (j < resourceList[i].Count)
                        {
                            newResource.maxAmount = resourceList[i][j];
                            if (calledByPlayer)
                            {
                                newResource.amount = resourceList[i][j];
                            }
                            else
                            {                                
                                newResource.amount = getResource(j);
                            }
                        }
                    }
                    newTank.resources.Add(newResource);
                }
            }            
        }

        public override string GetInfo()
        {
            if (showInfo)
            {
                List<string> resourceList = Tools.parseNames(resourceNames);
                StringBuilder info = new StringBuilder();
                info.AppendLine("Fuel tank setups available:");
                for (int i = 0; i < resourceList.Count; i++)
                {
                    info.AppendLine(resourceList[i].Replace(",", ", "));
                }
                return info.ToString();
            }
            else
                return string.Empty;
        }
    }    
}
