using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using KSP.IO;

class FStextureSwitch : PartModule
{
    [KSPField]
    public string displayName = "Texture switcher";
    [KSPField]
    public int moduleID = 0;
    [KSPField(isPersistant = true)]
    public int selectedTexture = 0;
    [KSPField]
    public bool showListButton = false;
    [KSPField]
    public bool debugMode = false;
    [KSPField]
    public bool switchableInFlight = false;
    //[KSPField]
    //public string targetObjectName = "mountCasing";
    
    //Transform targetObjectTransform;
    List<Transform> targetObjectTransforms = new List<Transform>();
    List<Material> targetMats = new List<Material>();
    //Material targetMat;
    FSnodeLoader textureNode;
    private string textureNodeName = "textures";
    private string textureValueName = "name";
    private List<String> texList = new List<string>();
    FSnodeLoader objectNode;
    private string objectNodeName = "objects";
    private string objectValueName = "name";
    private List<String> objectList = new List<string>();
    FSdebugMessages debug = new FSdebugMessages(false, FSdebugMessages.OutputMode.both, 2f); //set to true for debug
    FSGUIPopup popup;

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

    [KSPEvent(guiActive = false, guiName = "Next Texture")]
    public void nextTextureEvent()
    {
        selectedTexture++;
        if (selectedTexture >= texList.Count)
            selectedTexture = 0;
        useTextureAll();
    }

    public void useTextureAll()
    {
        foreach (Material mat in targetMats)
        {
            useTexture(mat);
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
            }
            else
            {
                debug.debugMessage("no such texture: " + texList[selectedTexture]);
            }
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        debug.debugMode = debugMode;
        textureNode = new FSnodeLoader(part, moduleName, moduleID.ToString(), textureNodeName, textureValueName);
        textureNode.debugMode = debugMode;
        texList = textureNode.OnStart();

        objectNode = new FSnodeLoader(part, moduleName, moduleID.ToString(), objectNodeName, objectValueName);
        objectNode.debugMode = debugMode;
        objectList = objectNode.OnStart();
        debug.debugMessage("FStextureSwitch found " + texList.Count + " textures, using number " + selectedTexture + ", found " + objectList.Count + " objects");

        foreach (String targetObjectName in objectList)
        {
            Transform targetObjectTransform = part.FindModelTransform(targetObjectName);
            if (targetObjectTransform != null && targetObjectTransform.gameObject.renderer != null) // check for if the object even has a mesh. otherwise part list loading crashes
            {                
                Material targetMat = targetObjectTransform.gameObject.renderer.material;
                if (targetMat != null)
                {
                    if (!targetMats.Contains(targetMat))
                    {
                        targetMats.Add(targetMat);
                    }
                }
            }
        }        

        useTextureAll();

        popup = new FSGUIPopup(part, "FStextureSwitch", moduleID, FSGUIwindowID.textureSwitch + moduleID, new Rect(500f, 500f, 200f, 100f), displayName, new PopupElement(new PopupButton("Next texture", 0f, nextTextureEvent)));
        if (showListButton)
        {
            popup.sections[0].elements.Add(new PopupElement(new PopupButton("List objects", 0f, listAllObjects)));
        }

        if (switchableInFlight) Events["nextTextureEvent"].guiActive = true;
    }

    public void OnGUI()
    {
        if (!HighLogic.LoadedSceneIsEditor)
            return;

        if (popup != null)
            popup.popup();
    }

}
