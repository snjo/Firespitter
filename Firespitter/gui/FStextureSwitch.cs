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
    [KSPField(isPersistant = true)]
    public string selectedTextureURL = string.Empty;
    [KSPField]
    public bool showListButton = false;
    [KSPField]
    public bool debugMode = false;
    [KSPField]
    public bool switchableInFlight = false;
    [KSPField]
    public Vector4 GUIposition = new Vector4(FSGUIwindowID.standardRect.x, FSGUIwindowID.standardRect.y, FSGUIwindowID.standardRect.width, FSGUIwindowID.standardRect.height);

    List<Transform> targetObjectTransforms = new List<Transform>();
    List<Material> targetMats = new List<Material>();
    FSnodeLoader textureNode;
    private string textureNodeName = "textures";
    private string textureValueName = "name";
    private List<String> texList = new List<string>();
    FSnodeLoader objectNode;
    private string objectNodeName = "objects";
    private string objectValueName = "name";
    private List<String> objectList = new List<string>();
    FSdebugMessages debug = new FSdebugMessages(false, FSdebugMessages.OutputMode.both, 2f); //set to true for debug

    [KSPEvent(guiActive=false, guiActiveEditor=false, guiName="Debug: Log Objects")]
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

    [KSPEvent(guiActive = false, guiActiveEditor=true, guiName = "Next Texture")]
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
                selectedTextureURL = texList[selectedTexture];
            }
            else
            {
                debug.debugMessage("no such texture: " + texList[selectedTexture]);
            }
        }
    }

    public override string GetInfo()
    {
        return "Alternate textures are available. Use the Next Texture button on the right click menu.";
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

        if (switchableInFlight) Events["nextTextureEvent"].guiActive = true;
        if (showListButton) Events["listAllObjects"].guiActiveEditor = true;
    }
}
