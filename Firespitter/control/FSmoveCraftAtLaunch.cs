using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Threading.Tasks;
using UnityEngine;


public class FSmoveCraftAtLaunch : PartModule
{
    //[KSPField(isPersistant=true)]
    //public bool moveAtLaunch = true; // for overriding with SPH option menu

    [KSPField(isPersistant = true)]
    public string selectedPositionName = string.Empty;
    [KSPField(guiActiveEditor=true, guiName="Launch position:", isPersistant=true)]
    public string positionDisplayName = "Default";

    //[KSPField]
    //public float addLongitude = 0.4f;

    //[KSPField]
    //public float altitude = 5.0f;

    //[KSPField]
    //public Vector3 launchPosition = new Vector3(-1199.2f, 66.2f, 4095.4f);    // new Vector3(-1199.2f, 66.2f, 4095.4f); 

    // Runway: lat -0.0485890418349364, long 285.276094692895, alt 71.9665353324963
    // Beach: lat -0.039751185006272, long 285.639486693549, alt 1.68487426708452
    // Beach by Island: lat -1.53556797173857, long 287.956960620886, alt 1.56112247915007

    [KSPField(isPersistant = true, guiActiveEditor=true)]
    public float latitude = -0.039751f;
    [KSPField(isPersistant = true, guiActiveEditor = true)]
    public float longitude = 285.639486f;
    [KSPField(isPersistant = true, guiActiveEditor = true)]
    public float altitude = 1.6f;
    [KSPField(isPersistant = true, guiActiveEditor = true), UI_FloatRange(minValue = -50f, maxValue = 50f, stepIncrement = 1f)]
    public float altitudeShift = 0f;


    [KSPField(isPersistant=true)]
    public bool hasLaunched = false;

    [KSPField(isPersistant=true), UI_FloatRange(minValue=3f, maxValue=60f, stepIncrement=1f)]
    public float timer = 12f;

    public bool doQuickLoad = false;
    private bool isFixed = false;

    private int windowID = 0;
    public FSGUIPopup popup;
    PopupElement fileNameElement;
    private string[] files;
    private int selectedPositionNumber = -1;
    //public FSGUIPopup popup;
    //private Transform boundsTransform;
    //private Transform partPosition;

    [KSPEvent(guiActive = true, guiName = "Log position")]
    public void logPositionEvent()
    {
        //Debug.Log("FSmoveCAL: part posistion is " + part.transform.position);
        Debug.Log("Coordinates: lat " + vessel.latitude + ", long " + vessel.longitude + ", alt " + vessel.altitude);
    }

    [KSPEvent(guiActive = true, guiName = "Save position")]
    public void savePositionEvent()
    {
        logPositionEvent();
        popup.showMenu = true;
    }

    //[KSPEvent(guiActiveEditor=true, guiName="Launch on Runway")]
    //public void toggleMoveAtLaunchEvent()
    //{
    //    moveAtLaunch = !moveAtLaunch;
    //    setLaunchEventText();
    //}

    [KSPEvent(guiActiveEditor = true, guiActive = false, guiName = "Next Position")]
    public void nextPositionEvent()
    {
        selectedPositionNumber++;
        if (selectedPositionNumber > files.Length - 1)
            selectedPositionNumber = -1;
        if (selectedPositionNumber == -1)
        {
            selectedPositionName = string.Empty;
            positionDisplayName = "Default";
            latitude = -0.048589f;
            longitude = 285.27609f;
            altitude = 71.966535f;
        }
        else
        {
            selectedPositionName = files[selectedPositionNumber];
            positionDisplayName = selectedPositionName.Split('/').Last().Split('.').First();
            readPositionFromFile(selectedPositionName);
        }
    }

    private void savePositionToFile()
    {
        latitude = (float)vessel.latitude;
        longitude = (float)vessel.longitude;
        altitude = (float)vessel.altitude;
        popup.showMenu = false;
        string positionName = fileNameElement.inputText;
        StreamWriter stream = new StreamWriter(Firespitter.Tools.PlugInDataPath + positionName + ".pos");
        stream.WriteLine(latitude);
        stream.WriteLine(longitude);
        stream.WriteLine(altitude);
        stream.WriteLine("[EOF]");
        stream.Close();        
    }

    private void readPositionFromFile(string fileName)
    {        
        StreamReader stream = new StreamReader(fileName); // exceptions handled by assembleCraft                              
        try
        {
            Debug.Log("Reading position file: " + fileName);                               
                float.TryParse(stream.ReadLine(), out latitude);
                float.TryParse(stream.ReadLine(), out longitude);
                float.TryParse(stream.ReadLine(), out altitude);
        }
        catch (Exception e)
        {            
            Debug.Log("Exception when reading position file: " + e.ToString());
        }        
    }

    //private void setLaunchEventText()
    //{
    //    if (moveAtLaunch)
    //    {
    //        Events["toggleMoveAtLaunchEvent"].guiName = "Launch on runway";
    //    }
    //    else
    //    {
    //        Events["toggleMoveAtLaunchEvent"].guiName = "Launch in water";
    //    }
    //}

    public override void OnStart(PartModule.StartState state)
    {
        //setLaunchEventText();
        if (HighLogic.LoadedSceneIsFlight)
        {
            windowID = FSGUIwindowID.getNextID();
            fileNameElement = new PopupElement("Name", "New");
            PopupElement saveButtonElement = new PopupElement(new PopupButton("Save", 0f, savePositionToFile));
            saveButtonElement.buttons.Add(new PopupButton("Cancel", 0f, cancelSave));
            popup = new FSGUIPopup(part, "FSmoveCraftAtLaunch", 0, windowID, FSGUIwindowID.standardRect, "Save Position", fileNameElement);
            popup.sections[0].elements.Add(saveButtonElement);
            popup.showCloseButton = true;            
            popup.useInEditor = false;
            popup.useInActionEditor = false;
            popup.useInFlight = true;
        }

        files = Directory.GetFiles(Firespitter.Tools.PlugInDataPath, "*.pos");
    }

    public void cancelSave()
    {
        popup.showMenu = false;
    }

    //public void moveCraft()
    //{
    //    vessel.longitude += addLongitude;
    //    vessel.altitude = altitude;
    //}

    public Vector3d calculateLaunchPosition()
    {
        return vessel.mainBody.GetWorldSurfacePosition((double)latitude, (double)longitude, (double)(altitude + altitudeShift));
    }

    public void tryMoveVessel()
    {
        if (vessel != null)
        {
            //Debug.Log("FSmoveCAL: moving vessel to: " + launchPosition);
            vessel.SetPosition(calculateLaunchPosition(), true);
            if (!vessel.rigidbody.isKinematic)
            {
                vessel.rigidbody.velocity = Vector3.zero;
                vessel.rigidbody.angularVelocity = Vector3.zero;
            }
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

    public void Update() //TODO, check if this is the active vessel
    {
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        if (selectedPositionName == string.Empty) return;


        
        
        if (!hasLaunched)
        {
            //Debug.Log("FSmoveCraftAtLaunch: Launching vessel at " + positionDisplayName + ", lat " + latitude + ", long " + longitude + ", alt " + altitude);
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
        if (HighLogic.LoadedSceneIsFlight && popup != null)
            popup.popup();
    }

    //public void OnGUI()
    //{
    //        popup.popup();
    //}
}
