using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSresourceDial : InternalModule
{
    [KSPField]
    public string dialType = "cosmetic";
    [KSPField]
    public string needle = "needle";
    [KSPField]
    public string spinner = "spinner";
    [KSPField]
    public int spinnerDigits = 5;
    [KSPField]
    public float spinnerMultiplier = 1;

    public bool isController = false;

    private bool hasIntitialized = false;
    private GameObject needleObject;
    private GameObject[] spinnerObjects;

    private void getResourcesFromVessel()
    {

    }

    private void getRourcesFromController()
    {

    }

    private void setNeedleRotation(float rotation)
    {

    }

    private void calculateNeedleRotation(string resource, float minAngle, float maxAngle)
    {

    }

    public override void OnAwake()
    {

    }

    public override void OnFixedUpdate()
    {
        base.OnUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA
            || CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
        {

            if (!hasIntitialized) //run once - check to see if this part should be the dial controller or slave for all IVA dials in this cockpit
            {

            }

            if (isController) getResourcesFromVessel();
            else getRourcesFromController();
        }

    }
}