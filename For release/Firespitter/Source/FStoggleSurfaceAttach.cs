using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class FStoggleSurfaceAttach : PartModule // doesn't work when you leave the action editor...
{
    [KSPField]
    public string colliderGameObjectName = "All";
    [KSPField]
    public int moduleID = 0;
    private Rect windowRect = new Rect(550f, 220f, 200f, 50f);
    private bool colliderEnabled = true;
    private List<Collider> colliderList = new List<Collider>();
    public bool showMenu = false;
    Shader defaultShader;
    Material defaultMat;
    Material transparentMat;


    private void setColliderState(bool newState)
    {
        for (int i = 0; i < colliderList.Count; i++)
        {
            if (colliderList[i] != null)
            {
                colliderList[i].enabled = newState;
                //if (newState && colliderList[i].gameObject.renderer != null)
                //{
                //    colliderList[i].gameObject.renderer.material = defaultMat;
                //}
                //else
                //{
                //    colliderList[i].gameObject.renderer.material = transparentMat;
                //}
            }
        }
    }

    public override void OnStart(PartModule.StartState state)
    {        
        base.OnStart(state);
        if (!HighLogic.LoadedSceneIsEditor)
            return;

        if (colliderGameObjectName == "All")
        {

        }
        else if (colliderGameObjectName != string.Empty)
        {
            Debug.Log("FStSA: assigning transform: " + colliderGameObjectName);
            Transform newTransfrom;
            newTransfrom = part.FindModelTransform(colliderGameObjectName);

            if (newTransfrom != null)
            {
                Debug.Log("FStSA: looking for collider on: " + colliderGameObjectName);
                if (newTransfrom.gameObject.collider != null)
                {
                    Debug.Log("FStSA: adding collider to the list: " + colliderGameObjectName);
                    colliderList.Add(part.FindModelTransform(colliderGameObjectName).gameObject.collider);
                    // get the deafult mesh material
                    //defaultMat = newTransfrom.gameObject.renderer.material;
                }
                else
                {
                    Debug.Log("FStSA: no collider on: " + colliderGameObjectName);
                }
            }
            else
            {
                Debug.Log("FStSA: no such object: " + colliderGameObjectName);
            }
        }

        //Debug.Log("FStSA: creating new material");
        //Color color = defaultMat.GetColor("_Color");
        //Debug.Log("FStSA: setting mat alpha");
        //color.a = 0.5f;
        //Debug.Log("FStSA: find shader");
        //transparentMat.shader = Shader.Find("Transparent/Diffuse");
        //Debug.Log("FStSA: transMAt tex = defMat tex");
        //transparentMat.mainTexture = defaultMat.mainTexture;
        //Debug.Log("FStSA: set color");
        //transparentMat.color = color;        
    }

    public void Update()
    {
        if (!HighLogic.LoadedSceneIsEditor)
            return;
        //Debug.Log("update");
        //setColliderState(false);        
    }

    private void drawWindow(int windowID)
    {
        string enabledString;
        if (colliderEnabled)
        {
            enabledString = "On";
        }
        else
        {
            enabledString = "Off";
        }
        enabledString += colliderList[0].enabled;

        GUI.Label(new Rect(8f, 25f, 100f, 22f), "Collider");
        if (GUI.Button(new Rect(115f, 25f, 60f, 22f), enabledString))
        {
            colliderEnabled = !colliderEnabled;
            setColliderState(colliderEnabled);
        }
        GUI.DragWindow();
    }

    public void OnGUI()
    {
        if (!HighLogic.LoadedSceneIsEditor)
            return;
        if (showMenu)
        {
            windowRect = GUI.Window(FSGUIwindowID.toggleSurfaceAttach + moduleID, windowRect, drawWindow, "Srf Attach disabler");
        }

        if(colliderEnabled)
            showMenu = false;        

        EditorLogic editor = EditorLogic.fetch;
        if (editor)
        {
            if (editor.editorScreen == EditorLogic.EditorScreen.Actions)
            {
                List<Part> partlist = EditorActionGroups.Instance.GetSelectedParts();
                if (partlist.Count > 0)
                {
                    if (partlist[0] == part)
                    {
                        if (partlist[0].Modules.Contains("FStoggleSurfaceAttach"))
                        {
                            showMenu = true;
                        }
                    }
                }
            }
            if (!colliderEnabled)
                showMenu = true;
        }
    }
}

