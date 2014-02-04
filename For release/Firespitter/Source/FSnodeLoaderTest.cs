using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class FSnodeLoaderTest : PartModule
{
    FSnodeLoader nodeLoader;
    [KSPField]
    public string moduleID = "0";
    private List<String> trimList = new List<string>();

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        if (nodeLoader == null)
        {
            FSdebugMessages.Post("FSnodeLoaderTest is null, creating new one (OnLoad)", true, 0f);
            nodeLoader = new FSnodeLoader(part, moduleName, moduleID, "trim", "amount");
            nodeLoader.OnLoad(node);
        }
        else
            FSdebugMessages.Post("FSnodeLoaderTest OnLoad: nodeLoader not null", true, 0f);
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        if (nodeLoader == null)
        {
            FSdebugMessages.Post("FSnodeLoaderTest is null, creating new one (OnStart)", true, 0f);
            nodeLoader = new FSnodeLoader(part, moduleName, moduleID, "trim", "amount");
            nodeLoader.OnStart();            
        }
        else
        {
            FSdebugMessages.Post("FSnodeLoaderTest OnStart: nodeLoader not null", true, 0f);
            trimList = nodeLoader.OnStart();
            if (trimList.Count > 0)
            {
                for (int i = 0; i < trimList.Count; i++)
                {
                    FSdebugMessages.Post("FSnodeLoaderTest: trim " + i + ": " + trimList[i], true, 5f);
                }
            }
            else
            {
                FSdebugMessages.Post("FSnodeLoaderTest: trimList is empty", true, 5f);
            }
        }
    }

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);
        node = nodeLoader.OnSave(node);
    }
}
