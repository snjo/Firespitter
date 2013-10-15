using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;


public class FSmoveCraftAtLaunch : PartModule
{
    [KSPField(isPersistant=true)]
    public bool moveAtLaunch = true; // for overriding with SPH option menu

    [KSPField]
    public float addLongitude = 0.4f;

    [KSPField]
    public float altitude = 5.0f;

    [KSPField]
    public Vector3 launchPosition = new Vector3(-1199.2f, 66.2f, 4095.4f);    // new Vector3(-1199.2f, 66.2f, 4095.4f);    

    [KSPField(isPersistant=true)]
    public bool hasLaunched = false;

    [KSPField]
    public float timer = 3f;
    public bool doQuickLoad = false;
    private bool isFixed = false;
    public FSGUIPopup popup;
    //private Transform boundsTransform;
    //private Transform partPosition;

    [KSPEvent(guiActive = true, guiName = "Log position")]
    public void logPositionEvent()
    {
        Debug.Log("FSmoveCAL: part posistion is " + part.transform.position);
    }


    public override void OnStart(PartModule.StartState state)
    {
        if (HighLogic.LoadedSceneIsFlight && moveAtLaunch)
        {
            Debug.Log("FSmoveCraftAtLaunch: moving craft");
            //if (!hasLaunched)
            //{
            //    moveCraft();
            //    timer = 3f;
            //}
        }
        popup = new FSGUIPopup(part, "FSmoveCraftAtLaunch", 0, FSGUIwindowID.moveCraftAtLaunch, new Rect(600f, 300f, 200f, 60f), "Move to the water at launch?", new PopupElement(new PopupButton("Yes", "No", 0f, toggleMoveAtLaunch)));
        popup.elementList[0].buttons[0].toggle(moveAtLaunch);

        //partPosition = new GameObject().transform;
        //partPosition.position = part.transform.position;
        //boundsTransform = part.FindModelTransform("Bounds");
        base.OnStart(state);
    }

    //public void moveCraft()
    //{
    //    vessel.longitude += addLongitude;
    //    vessel.altitude = altitude;
    //}

    public void tryMoveVessel()
    {
        if (vessel != null)
        {
            Debug.Log("FSmoveCAL: moving vessel to: " + launchPosition);
            vessel.SetPosition(launchPosition, true);
        }
    }

    public void fixCraftLock()
    {
        base.vessel.situation = Vessel.Situations.SPLASHED;
        vessel.state = Vessel.State.ACTIVE;
        base.vessel.Landed = false;
        base.vessel.Splashed = true;
        base.vessel.GoOnRails();
        base.vessel.rigidbody.WakeUp();
        base.vessel.ResumeStaging();
        vessel.landedAt = "";
        InputLockManager.ClearControlLocks();
        //ScreenMessages.PostScreenMessage(new ScreenMessage("Press F5 to Quicksave, then F9 to Quickload to get full control", 10f, ScreenMessageStyle.UPPER_CENTER));
    }

    public void toggleMoveAtLaunch()
    {
        moveAtLaunch = !moveAtLaunch;        
        popup.elementList[0].buttons[0].toggle(moveAtLaunch);        
    }

    public void Update() //TODO, check if this is the active vessel
    {
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        if (!moveAtLaunch) return;


        
        
        if (!hasLaunched)
        {
            // --------- TEMP DISABLING ----------
            //ScreenMessages.PostScreenMessage(new ScreenMessage("The water launch module is disabled in KSP 0.22 due to a bug", 10f, ScreenMessageStyle.UPPER_CENTER));
            //hasLaunched = true;
            // --------- -------------- ----------

            
            if (timer <= 0 && !isFixed)
            {
                //fixCraftLock();
                isFixed = true;
                hasLaunched = true;
            }
            else if (timer > 0 && !isFixed)
            {
                timer -= Time.deltaTime;
                tryMoveVessel();
                //moveBounds();
            }
             
        }
         
    }

    public void OnGUI()
    {
            popup.popup();
    }
}
