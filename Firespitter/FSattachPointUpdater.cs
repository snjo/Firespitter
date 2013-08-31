using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class FSattachPointUpdater : PartModule
{
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        part.attachNodes[1].originalPosition += new Vector3(0.01f, 0.0f, 0.0f);
        part.attachNodes[1].orientation += new Vector3(0.01f, 0.0f, 0.0f);
        //if (this.vessel != null)
        //{
        //    part.UpdateOrgPosAndRot(this.vessel.rootPart);
        //    Part[] array = part.FindChildParts<Part>(true);
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        Part childPart = array[i];
        //        childPart.UpdateOrgPosAndRot(this.vessel.rootPart);
        //    }
        //}
    }
}
