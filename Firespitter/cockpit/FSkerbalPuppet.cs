using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FSkerbalPuppet : PartModule
{
    [KSPField]
    public bool showInIVACam = false;
    [KSPField]
    public bool showInInternalCam = false;
    [KSPField]
    public bool showInExternalCam = true;
    [KSPField]
    public bool showInFlightCam = true;
    [KSPField]
    public bool showInEditor = true;
    [KSPField]
    public string objectName = "kerbalPuppet"; //multiple objects can have the same name to be hidden together.
    [KSPField]
    public bool checkSeat = true;
    [KSPField]
    public int seatNumber = 0;
    [KSPField(isPersistant=true)]
    public bool lastOccupiedState = false;

    public bool showPuppet = true;
    private Transform[] puppetTransforms;    
    private CameraManager.CameraMode cameraMode;
    private bool doUpdate = true;
    private bool seatStatus = false;
    private bool oldSeatStatus = true;

    private CameraManager.CameraMode oldCameraMode;

    private bool checkCamMode()
    {                
        cameraMode = CameraManager.Instance.currentCameraMode;

        if (cameraMode != oldCameraMode)
        {            
            doUpdate = true;
            oldCameraMode = cameraMode;  

            switch (cameraMode)
            {
                case CameraManager.CameraMode.Flight:
                    if (showInFlightCam) return true;
                    else return false;
                case CameraManager.CameraMode.IVA:
                    if (showInIVACam) return true;
                    else return false;
                case CameraManager.CameraMode.Internal:
                    if (showInInternalCam) return true;
                    else return false;
                case CameraManager.CameraMode.External:
                    if (showInExternalCam) return true;
                    else return false;
                default:
                    return true;
            }
        }
        return true;          
    }

    private bool checkSeatOccupied()
    {
        if (part.internalModel != null)
            lastOccupiedState = part.internalModel.seats[seatNumber].taken;
        return lastOccupiedState;
    }

    private void updatePuppetActive()
    {
        if (doUpdate)
        {            
            foreach (Transform t in puppetTransforms)
            {
                t.gameObject.renderer.enabled = showPuppet;                
            }
            doUpdate = false;
        }
    }    

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        puppetTransforms = part.FindModelTransforms(objectName);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();        
        if (HighLogic.LoadedSceneIsFlight)
        {            
            showPuppet = checkCamMode();
            if (showPuppet && checkSeat)
            {
                seatStatus = checkSeatOccupied();
                showPuppet = seatStatus;
                if (seatStatus != oldSeatStatus)
                    doUpdate = true;
                oldSeatStatus = seatStatus;
                //Debug.Log("seat occupied: " + checkSeatOccupied());
            }            
            updatePuppetActive();
        }
    }

    public void Update()
    {
        if (HighLogic.LoadedSceneIsEditor)
        {
            if (showInEditor) showPuppet = true;
            else showPuppet = false;
            updatePuppetActive();
        }
    }
}

