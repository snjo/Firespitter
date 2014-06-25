using UnityEngine;

class FSchangeControlDirection : PartModule
{
    [KSPAction("Control From Here")]
    public void refForward(KSPActionParam param)
    {
        Debug.Log("Setting vessel reference transform");
        vessel.SetReferenceTransform(part);
    }
}