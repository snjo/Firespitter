using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Firespitter.info;

namespace Firespitter.customization
{
    public class FStextureSwitch2 : PartModule
    {
        [KSPField]
        public int moduleID = 0;
        [KSPField]
        public string textureRootFolder = string.Empty;
        [KSPField]
        public string objectNames = string.Empty;
        [KSPField]
        public string textureNames = string.Empty;
        [KSPField]
        public string mapNames = string.Empty;
        [KSPField]
        public string textureDisplayNames = "Default";

        [KSPField]
        public string nextButtonText = "Next Texture";
        [KSPField]
        public string prevButtonText = "Previous Texture";
        [KSPField]
        public string statusText = "Current Texture";

        [KSPField(isPersistant = true)]
        public int selectedTexture = 0;
        [KSPField(isPersistant = true)]
        public string selectedTextureURL = string.Empty;
        [KSPField(isPersistant = true)]
        public string selectedMapURL = string.Empty;
        [KSPField]
        public bool showListButton = false;
        [KSPField]
        public bool debugMode = false;
        [KSPField]
        public bool switchableInFlight = false;
        [KSPField]
        public string additionalMapType = "_BumpMap";
        [KSPField]
        public bool mapIsNormal = true;
        [KSPField]
        public bool repaintableEVA = true;
        //[KSPField]
        //public Vector4 GUIposition = new Vector4(FSGUIwindowID.standardRect.x, FSGUIwindowID.standardRect.y, FSGUIwindowID.standardRect.width, FSGUIwindowID.standardRect.height);
        [KSPField]
        public bool showPreviousButton = true;
        [KSPField]
        public bool useFuelSwitchModule = false;
        [KSPField]
        public string fuelTankSetups = "0";
        [KSPField]
        public bool showInfo = true;

        [KSPField]
        public bool updateSymmetry = true;

        private List<Transform> targetObjectTransforms = new List<Transform>();
        private List<List<Material>> targetMats = new List<List<Material>>();
        private List<String> texList = new List<string>();
        private List<String> mapList = new List<string>();
        private List<String> objectList = new List<string>();
        private List<String> textureDisplayList = new List<string>();
        private List<int> fuelTankSetupList = new List<int>();
        private FSfuelSwitch fuelSwitch;

        private bool initialized = false;

        FSdebugMessages debug;

        [KSPField(guiActiveEditor = true, guiName = "Current Texture")]
        public string currentTextureName = string.Empty;

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Debug: Log Objects")]
        public void listAllObjects()
        {
            List<Transform> childList = ListChildren(part.transform);
            foreach (Transform t in childList)
            {
                Debug.Log("object: " + t.name);
            }
        }


        List<Transform> ListChildren(Transform a)
        {
            List<Transform> childList = new List<Transform>();
            foreach (Transform b in a)
            {
                childList.Add(b);
                childList.AddRange(ListChildren(b));
            }
            return childList;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Next Texture")]
        public void nextTextureEvent()
        {
            selectedTexture++;
            if (selectedTexture >= texList.Count && selectedTexture >= mapList.Count)
                selectedTexture = 0;
            useTextureAll(true);
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Previous Texture")]
        public void previousTextureEvent()
        {
            selectedTexture--;
            if (selectedTexture < 0)
                selectedTexture = Mathf.Max(texList.Count - 1, mapList.Count-1);
            useTextureAll(true);
        }

        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 5f, guiActive = false, guiActiveEditor = false, guiName = "Repaint")]
        public void nextTextureEVAEvent()
        {
            nextTextureEvent();
        }

        public void useTextureAll(bool calledByPlayer)
        {
            applyTexToPart(calledByPlayer);

            if (updateSymmetry)
            {
                for (int i = 0; i < part.symmetryCounterparts.Count; i++)
                {
                    // check that the moduleID matches to make sure we don't target the wrong tex switcher
                    FStextureSwitch2[] symSwitch = part.symmetryCounterparts[i].GetComponents<FStextureSwitch2>();
                    for (int j = 0; j < symSwitch.Length; j++)
                    {
                        if (symSwitch[j].moduleID == moduleID)
                        {
                            symSwitch[j].selectedTexture = selectedTexture;
                            symSwitch[j].applyTexToPart(calledByPlayer);
                        }
                    }
                }
            }
        }

        private void applyTexToPart(bool calledByPlayer)
        {
            initializeData();
            foreach (List<Material> matList in targetMats)
            {
                foreach (Material mat in matList)
                {
                    useTextureOrMap(mat);
                }
            }
            if (useFuelSwitchModule)
            {
                debug.debugMessage("calling on FSfuelSwitch tank setup " + selectedTexture);
                if (selectedTexture < fuelTankSetupList.Count)
                    fuelSwitch.selectTankSetup(fuelTankSetupList[selectedTexture], calledByPlayer);
                else
                    debug.debugMessage("no such fuel tank setup");
                if (calledByPlayer && HighLogic.LoadedSceneIsFlight)
                {
                    //Do a screeen message of what we just swapped to!
                    var msg = string.Format("Converted Tank to {0}", textureDisplayList[selectedTexture]);
                    ScreenMessages.PostScreenMessage(msg, 5f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }

        public void useTextureOrMap(Material targetMat)
        {
            if (targetMat != null)
            {
                useTexture(targetMat);

                useMap(targetMat);
            }
            else
            {
                debug.debugMessage("No target material in object.");
            }
        }

        private void useMap(Material targetMat)
        {
            debug.debugMessage("maplist count: " + mapList.Count + ", selectedTexture: " + selectedTexture + ", texlist Count: " + texList.Count);
            if (mapList.Count > selectedTexture)
            {
                if (GameDatabase.Instance.ExistsTexture(mapList[selectedTexture]))
                {
                    debug.debugMessage("map " + mapList[selectedTexture] + " exists in db");
                    targetMat.SetTexture(additionalMapType, GameDatabase.Instance.GetTexture(mapList[selectedTexture], mapIsNormal));
                    selectedMapURL = mapList[selectedTexture];

                    if (selectedTexture < textureDisplayList.Count && texList.Count == 0)
                    {
                        currentTextureName = textureDisplayList[selectedTexture];
                        debug.debugMessage("setting currentTextureName to " + textureDisplayList[selectedTexture]);
                    }
                    else
                    {
                        debug.debugMessage("not setting currentTextureName. selectedTexture is " + selectedTexture + ", texDispList count is" + textureDisplayList.Count + ", texList count is " + texList.Count);
                    }
                }
                else
                {
                    debug.debugMessage("map " + mapList[selectedTexture] + " does not exist in db");
                }
            }
            else
            {
                if (mapList.Count > selectedTexture) // why is this check here? will never happen.
                    debug.debugMessage("no such map: " + mapList[selectedTexture]);
                else
                {
                    debug.debugMessage("useMap, index out of range error, maplist count: " + mapList.Count + ", selectedTexture: " + selectedTexture);
                    for (int i = 0; i < mapList.Count; i++)
                    {
                        debug.debugMessage("map " + i + ": " + mapList[i]);
                    }
                }
            }
        }

        private void useTexture(Material targetMat)
        {
            if (texList.Count > selectedTexture)
            {
                if (GameDatabase.Instance.ExistsTexture(texList[selectedTexture]))
                {
                    debug.debugMessage("assigning texture: " + texList[selectedTexture]);
                    targetMat.mainTexture = GameDatabase.Instance.GetTexture(texList[selectedTexture], false);
                    selectedTextureURL = texList[selectedTexture];

                    if (selectedTexture > textureDisplayList.Count - 1)
                        currentTextureName = getTextureDisplayName(texList[selectedTexture]);
                    else
                        currentTextureName = textureDisplayList[selectedTexture];
                }
                else
                {
                    debug.debugMessage("no such texture: " + texList[selectedTexture]);
                }
            }
        }

        public override string GetInfo()
        {
            if (showInfo)
            {
                List<string> variantList;
                if (textureNames.Length > 0)
                {
                    variantList = Tools.parseNames(textureNames);
                }
                else
                {
                    variantList = Tools.parseNames(mapNames);
                }
                textureDisplayList = Tools.parseNames(textureDisplayNames);
                StringBuilder info = new StringBuilder();
                info.AppendLine("Alternate textures available:");
                if (variantList.Count == 0)
                {
                    if (variantList.Count == 0)
                        info.AppendLine("None");
                }
                for (int i = 0; i < variantList.Count; i++)
                {
                    if (i > textureDisplayList.Count - 1)
                        info.AppendLine(getTextureDisplayName(variantList[i]));
                    else
                        info.AppendLine(textureDisplayList[i]);
                }
                info.AppendLine("\nUse the Next Texture button on the right click menu.");
                return info.ToString();
            }
            else
                return string.Empty;
        }

        private string getTextureDisplayName(string longName)
        {
            string[] splitString = longName.Split('/');
            return splitString[splitString.Length - 1];
        }

        public override void OnStart(PartModule.StartState state)
        {                        
            initializeData();

            useTextureAll(false);

            if (switchableInFlight) Events["nextTextureEvent"].guiActive = true;
            if (switchableInFlight && showPreviousButton) Events["previousTextureEvent"].guiActive = true;
            if (showListButton) Events["listAllObjects"].guiActiveEditor = true;
            if (!repaintableEVA) Events["nextTextureEVAEvent"].guiActiveUnfocused = false;
            if (!showPreviousButton)
            {
                Events["previousTextureEvent"].guiActive = false;
                Events["previousTextureEvent"].guiActiveEditor = false;
            }            

            Events["nextTextureEvent"].guiName = nextButtonText;
            Events["previousTextureEvent"].guiName = prevButtonText;
            Fields["currentTextureName"].guiName = statusText;

        }

        // runs the kind of commands that would normally be in OnStart, if they have not already been run. In case a method is called upon externally, but values have not been set up yet
        private void initializeData()
        {
            if (!initialized)
            {
                debug = new FSdebugMessages(debugMode, "FStextureSwitch2");
                // you can't have fuel switching without symmetry, it breaks the editor GUI.
                if (useFuelSwitchModule) updateSymmetry = true;

                objectList = Tools.parseNames(objectNames, true);
                texList = Tools.parseNames(textureNames, true, true, textureRootFolder);
                mapList = Tools.parseNames(mapNames, true, true, textureRootFolder);
                textureDisplayList = Tools.parseNames(textureDisplayNames);
                fuelTankSetupList = Tools.parseIntegers(fuelTankSetups);

                debug.debugMessage("found " + texList.Count + " textures, using number " + selectedTexture + ", found " + objectList.Count + " objects, " + mapList.Count + " maps");

                foreach (String targetObjectName in objectList)
                {
                    Transform[] targetObjectTransformArray = part.FindModelTransforms(targetObjectName);
                    List<Material> matList = new List<Material>();
                    foreach (Transform t in targetObjectTransformArray)
                    {
                        if (t != null && t.gameObject.GetComponent<Renderer>() != null) // check for if the object even has a mesh. otherwise part list loading crashes
                        {
                            Material targetMat = t.gameObject.GetComponent<Renderer>().material;
                            if (targetMat != null)
                            {
                                if (!matList.Contains(targetMat))
                                {
                                    matList.Add(targetMat);
                                }
                            }
                        }
                    }
                    targetMats.Add(matList);
                }

                if (useFuelSwitchModule)
                {
                    fuelSwitch = part.GetComponent<FSfuelSwitch>(); // only looking for first, not supporting multiple fuel switchers
                    if (fuelSwitch == null)
                    {
                        useFuelSwitchModule = false;
                        debug.debugMessage("no FSfuelSwitch module found, despite useFuelSwitchModule being true");
                    }
                }
                initialized = true;
            }
        }
    }
}
