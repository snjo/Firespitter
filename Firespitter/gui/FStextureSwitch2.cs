using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.gui
{
    public class FStextureSwitch2 : PartModule
    {
        [KSPField]
        public string displayName = "Texture switcher";
        [KSPField]
        public string objectNames = string.Empty;
        [KSPField]
        public string textureNames = string.Empty;
        [KSPField]
        public string mapNames = string.Empty;
        [KSPField]
        public string textureDisplayNames = string.Empty;

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
        [KSPField]
        public Vector4 GUIposition = new Vector4(FSGUIwindowID.standardRect.x, FSGUIwindowID.standardRect.y, FSGUIwindowID.standardRect.width, FSGUIwindowID.standardRect.height);
        [KSPField]
        public bool showPreviousButton = true;

        private List<Transform> targetObjectTransforms = new List<Transform>();
        private List<List<Material>> targetMats = new List<List<Material>>();
        private List<String> texList = new List<string>();
        private List<String> mapList = new List<string>();
        private List<String> objectList = new List<string>();
        private List<String> textureDisplayList = new List<string>();

        FSdebugMessages debug = new FSdebugMessages(false, FSdebugMessages.OutputMode.both, 2f); //set to true for debug   

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
            if (selectedTexture >= texList.Count)
                selectedTexture = 0;
            useTextureAll();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Previous Texture")]
        public void previousTextureEvent()
        {
            selectedTexture--;
            if (selectedTexture < 0)
                selectedTexture = texList.Count - 1;
            useTextureAll();
        }

        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 5f, guiActive = false, guiActiveEditor = false, guiName = "Repaint")]
        public void nextTextureEVAEvent()
        {
            nextTextureEvent();
        }

        public void useTextureAll()
        {
            foreach (List<Material> matList in targetMats)
            {
                foreach (Material mat in matList)
                {
                    useTexture(mat);
                }
            }
        }

        public void useTexture(Material targetMat)
        {
            if (targetMat != null && texList.Count > 0)
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

                    if (mapList.Count > selectedTexture)
                    {
                        targetMat.SetTexture(additionalMapType, GameDatabase.Instance.GetTexture(mapList[selectedTexture], mapIsNormal));
                        selectedMapURL = mapList[selectedTexture];
                    }
                }
                else
                {
                    debug.debugMessage("no such texture: " + texList[selectedTexture]);
                }
            }
            else
            {
                debug.debugMessage("FStextureSwitch: No target material in object.");
            }
        }

        public override string GetInfo()
        {
            texList = parseNames(textureNames);
            textureDisplayList = parseNames(textureDisplayNames);
            StringBuilder info = new StringBuilder();
            info.AppendLine("Alternate textures available:");
            if (texList.Count == 0)
            {
                if (texList.Count == 0)
                    info.AppendLine("None");
            }
            for (int i = 0; i < texList.Count; i++)
            {
                if (i > textureDisplayList.Count - 1)
                    info.AppendLine(getTextureDisplayName(texList[i]));
                else
                   info.AppendLine(textureDisplayList[i]);                
            }
            info.AppendLine("\nUse the Next Texture button on the right click menu.");
            return info.ToString();
        }

        private string getTextureDisplayName(string longName)
        {
            string[] splitString = longName.Split('/');
            return splitString[splitString.Length - 1];
        }

        public List<string> parseNames(string names)
        {            
            string[] nameArray = names.Split(';');
            return nameArray.ToList();
        }

        public override void OnStart(PartModule.StartState state)
        {
            debug.debugMode = debugMode;

            objectList = parseNames(objectNames);
            texList = parseNames(textureNames);
            mapList = parseNames(mapNames);
            textureDisplayList = parseNames(textureDisplayNames);

            debug.debugMessage("FStextureSwitch2 found " + texList.Count + " textures, using number " + selectedTexture + ", found " + objectList.Count + " objects, " + mapList.Count + " maps");

            foreach (String targetObjectName in objectList)
            {
                Transform[] targetObjectTransformArray = part.FindModelTransforms(targetObjectName);
                List<Material> matList = new List<Material>();
                foreach (Transform t in targetObjectTransformArray)
                {
                    if (t != null && t.gameObject.renderer != null) // check for if the object even has a mesh. otherwise part list loading crashes
                    {
                        Material targetMat = t.gameObject.renderer.material;
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



            useTextureAll();

            if (switchableInFlight) Events["nextTextureEvent"].guiActive = true;
            if (switchableInFlight && showPreviousButton) Events["previousTextureEvent"].guiActive = true;
            if (showListButton) Events["listAllObjects"].guiActiveEditor = true;
            if (!repaintableEVA) Events["nextTextureEVAEvent"].guiActiveUnfocused = false;
            if (!showPreviousButton)
            {
                Events["previousTextureEvent"].guiActive = false;
                Events["previousTextureEvent"].guiActiveEditor = false;
            }

        }
    }
}
