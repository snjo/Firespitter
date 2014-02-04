using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.IO;

public class FSnodeTest : PartModule
{
    //List<ConfigNode> moduleNodes = new List<ConfigNode>();
    List<float> trimList = new List<float>();
    float[] trimArray = new float[1];
    public bool testOnloadMemory = false;
    public bool foundExistingNodes = false;
    string[] values = new string[1];
    //string[] 
    //List<string> moduleIDList = new List<string>(); // seems lists aren't copied from OnLoad to a new part, do filling a list with just the ID will test for whether this is a new part
    [KSPField]
    public string moduleID = "0"; // only needed in the orginal onLoad from the part.cfg to differentiate similar modules. After that, they will opnly check their internal nodes.
    public bool debugMode = true;
    

    public void debugMessage(string input)
    {
        if (debugMode)
        {
            FSdebugMessages.Post(input, true, 5f);
        }
    }

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        testOnloadMemory = true;
        
        // ("OnLoad testOnLoadMemory set to " + testOnloadMemory);

        ConfigNode[] existingNodes = node.GetNodes("trim");
        if (existingNodes.Length > 0)
        {
            debugMessage("OnLoad: Found " + existingNodes.Length + " trim nodes");            
            for (int i = 0; i < existingNodes.Length; i++)
            {
                

                values = existingNodes[i].GetValues("amount");
                for (int j = 0; j < values.Length; j++)
                {
                    trimList.Add(float.Parse(values[j]));
                    debugMessage("OnLoad: adding to list: " + values[j]);
                }
                //if (values.Length > 0)
                if (trimList.Count > 0)
                {
                    foundExistingNodes = true;
                    //trimArray = new float[trimList.Count];
                    //for (int ta = 0; ta < trimList.Count; ta++)
                    //{
                    //    trimArray[ta] = trimList[ta];
                    //}
                }
                else
                {
                    foundExistingNodes = false;
                }
            }
        }
        else
        {
            debugMessage("OnLoad: Found no existing trim nodes");
            foundExistingNodes = false;
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
 	    base.OnStart(state);        
        ConfigNode[] nodes;

        //debugMessage("OnStart testOnLoadMemory == " + testOnloadMemory);

        //if (!foundExistingNodes)
        if (trimList.Count == 0)
        {
            debugMessage("OnStart: no existing nodes, filling values from part.cfg");
            if (part.partInfo != null)
            {
                // fill trimList from part.cfg module
                debugMessage("OnStart moduleName is " + moduleName);
                debugMessage("OnStart partName is " + part.partName);
                debugMessage("OnStart partInfor.name is " + part.partInfo.name);
                debugMessage("getting configs");
                UrlDir.UrlConfig[] cfg = GameDatabase.Instance.GetConfigs("PART");
                debugMessage("looping through " + cfg.Length);
                for (int i = 0; i < cfg.Length; i++)
                {
                    if (part.partInfo.name == cfg[i].name)
                    {
                        debugMessage("found this part");
                        nodes = cfg[i].config.GetNodes("MODULE");
                        debugMessage("nodes: " + nodes.Length);
                        for (int j = 0; j < nodes.Length; j++)
                        {
                            debugMessage("node loop: " + nodes[j].GetValue("name"));
                            if (nodes[j].GetValue("name") == moduleName)
                            {
                                debugMessage("found this type of module");

                                bool correctModuleFound = false;
                                string[] IDArray = nodes[j].GetValues("moduleID");
                                if (IDArray.Length > 0)
                                {
                                    if (IDArray[0] == moduleID)
                                        correctModuleFound = true;
                                    else
                                        correctModuleFound = false;
                                }
                                else
                                {
                                    moduleID = "0";
                                    correctModuleFound = true;
                                }

                                if (correctModuleFound)
                                {
                                    debugMessage("Found module with matching or blank ID, proceeding");
                                    ConfigNode[] moduleNodeArray = nodes[j].GetNodes("trim");
                                    debugMessage("moduleNodeArray.length " + moduleNodeArray.Length);
                                    for (int k = 0; k < moduleNodeArray.Length; k++)
                                    {
                                        debugMessage("found trim node");
                                        string[] trimArray = moduleNodeArray[k].GetValues("amount");
                                        debugMessage("found " + trimArray.Length + " amounts");
                                        for (int l = 0; l < trimArray.Length; l++)
                                        {
                                            debugMessage("Adding trim to List " + trimArray[l]);
                                            trimList.Add(float.Parse(trimArray[l]));
                                        }
                                    }
                                }
                                else
                                {
                                    debugMessage("Found module with wrong ID, skipping");
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            debugMessage("OnStart: found " + values.Length + " existing values, trimList.Count is " + trimList.Count);           
        }
    }

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);        
        debugMessage("OnSave testOnLoadMemory == " + testOnloadMemory);
        ConfigNode trimNode = new ConfigNode("trim");
        debugMessage("Trim List count: " + trimList.Count);
        for (int i = 0; i < trimList.Count; i++)
        {
            debugMessage("Add " + trimList[i] + " to the node");
            trimNode.AddValue("amount", trimList[i]);
        }
        node.AddNode(trimNode);
    }

    //public void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        debugMessage("adding to trimlist 0, new value " + trimList[0]);
    //        trimList[0] += 1f;
    //    }
    //}
}



/* ------------- from majiir :
if (part.partInfo != null)
{
    node = GameDatabase.Instance.GetConfigs("PART").
        Single(c => part.partInfo.name == c.name.Replace('_', '.')).config.
        GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
}
*/