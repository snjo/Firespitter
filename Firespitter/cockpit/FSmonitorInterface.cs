using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSmonitorInterface : InternalModule
{

    public string targetMonitor = "scriptRunner";
    public string[] textArray;
    public int charPerLine = 23;
    public int linesPerPage = 17;
    //public GameObject button;
    public enum MenuState
    {
        splashScreen,
        mainMenu,
        flightData,
        settings,
        abort,
        gunner,
        info,
        fuel,
    }
    public enum ButtonNames
    {
        up,
        down,
        confirm,
        back,
    }

    public enum MainMenuItems
    {
        flightdata,
        settings,
        fuel,        
        gear,
        brakes,
        SAS,
        light,
        hover,
        abort,        
        reboot,
    }

    public enum SettingsMenuItems
    {
        unittype,
        debug,
        info,
    }

    private delegate void MenuStateHandler();

    private MenuStateHandler menuState;

    //ActionGroupList actionGroupList;

    //public MenuState menuState = MenuState.splashScreen;
    public MenuState startState = MenuState.splashScreen;    

    //private GameObject targetObject;
    private FSmonitorScript targetScript;
    private FSinfoPopup infoPopup;
    private string[] infoPopupStrings;
    private bool useInfoPopup = false;
        
    private bool[] buttonArray;
    public string[] buttonObjectNames;
    //private GameObject[] buttons;
    [KSPField]
    public int numButtons = 4;
    [KSPField]
    public string button1 = "buttonR2"; //up
    [KSPField]
    public string button2 = "buttonR3"; //down
    [KSPField]
    public string button3 = "buttonR1"; //confirm
    [KSPField]
    public string button4 = "buttonR4"; //back

    //menu vars
    int RAMcount = 0;
    int menuSelection = 0;
    int menuSelectionTop = 2;
    int menuSelectionBottom = 2;
    string unitType = "Metric";
    string speedMode = "Surface";
    //string monitorMode = "Pilot";
    string speedString = "";
    string altitudeString = "";
    string radarAltitudeString = "";
    string climbrateString = "";
    string hoverString = "";
    float hoverHeight = 0f;
    double displaySpeed = 0D;

    // action group number ints
    int gearGroupNumber;
    int brakeGroupNumber;
    int SASGroupNumber;
    int lightGroupNumber;
    bool gearState;
    bool brakeState;
    bool SASState;
    bool lightState;

    private int pauseCount = 0;
    private bool pauseInitialized = false;    

    private void pause(int delay)
    {
        pauseCount = delay;
        pauseInitialized = true;
    }

    private bool pause()
    {
        if (pauseCount <= 0)
        {
            return false;
        }
        else
        {
            pauseCount--;
            return true;
        }
    }

    public void buttonClick(int buttonNumber)
    {
        //Debug.Log("FS button " + buttonNumber + " clicked");
        buttonArray[buttonNumber] = true;
    }

    private void clearButtons()
    {
        for (int i = 0; i < numButtons; i++)
        {
            buttonArray[i] = false;
        }
    }

    public void toggleHover()
    {
        hoverHeight = 0f;
        foreach (Part part in vessel.Parts)
        {
            FScopterThrottle copterEngine = part.Modules.OfType<FScopterThrottle>().FirstOrDefault();
            if (copterEngine != null)
            {
                copterEngine.toggleHover();
                hoverHeight = copterEngine.hoverHeight;
            }
        }
    }

    public void getHoverHeight()
    {
        hoverHeight = 0f;
        foreach (Part part in vessel.Parts)
        {
            FScopterThrottle copterEngine = part.Modules.OfType<FScopterThrottle>().FirstOrDefault();
            if (copterEngine != null)
            {
                //copterEngine.toggleHover();
                hoverHeight = copterEngine.hoverHeight;
            }
        }
    }

    // get fuel amounts ----------------------

    int resourceUpdateCountdown = 0;
    private Dictionary<string, Vector2d> resourceDictionary = new Dictionary<string,Vector2d>();
    int vesselNumParts = 0; // initial value
    private void getResourceList()
    {        
        resourceDictionary = new Dictionary<string, Vector2d>();
        vesselNumParts = vessel.Parts.Count;
        foreach (Part part in vessel.parts)
        {
            foreach (PartResource resource in part.Resources)
            {
                if (!resourceDictionary.ContainsKey(resource.resourceName))
                {
                    resourceDictionary.Add(resource.resourceName, new Vector2d(resource.amount, resource.maxAmount));
                }
                else
                {
                    resourceDictionary[resource.resourceName] += new Vector2d(resource.amount, resource.maxAmount);
                }                    
            }
        }
    }

    private bool getInfoPopupObject()
    {        
        foreach (Part p in vessel.parts)
        {
            infoPopup = p.GetComponent<FSinfoPopup>();
            if (infoPopup != null)
            {
                getInfoPopupText();
                return true;
            }
        }
        return false;
    }

    private void getInfoPopupText()
    {        
        if (infoPopup != null)
        {
            infoPopupStrings = new string[12];
            infoPopupStrings[0] = infoPopup.textHeading;
            infoPopupStrings[1] = infoPopup.textBody1;
            infoPopupStrings[2] = infoPopup.textBody2;
            infoPopupStrings[3] = infoPopup.textBody3;
            infoPopupStrings[4] = infoPopup.textBody4;
            infoPopupStrings[5] = infoPopup.textBody5;
            infoPopupStrings[6] = infoPopup.textBody6;
            infoPopupStrings[7] = infoPopup.textBody7;
            infoPopupStrings[8] = infoPopup.textBody8;
            infoPopupStrings[9] = infoPopup.textBody9;
            infoPopupStrings[10] = infoPopup.textBody10;
            infoPopupStrings[11] = infoPopup.textBody11;
        }
    }

    // Use this for initialization
    void Start()
    {
        buttonObjectNames = new string[4] {button1,button2,button3,button4};
        //actionGroupList = new ActionGroupList(vessel);        

        for (int i = 0; i < numButtons; i++)
        {
            GameObject buttonObject = base.internalProp.FindModelTransform(buttonObjectNames[i]).gameObject;            
            FSgenericButtonHandlerID buttonHandler = buttonObject.AddComponent<FSgenericButtonHandlerID>();
            buttonHandler.ID = i;
            buttonHandler.mouseDownFunction = buttonClick;
        }             
        
        targetScript = base.internalProp.FindModelComponent<FSmonitorScript>();
        targetScript.textMode = FSmonitorScript.TextMode.singleString;
      
        textArray = new string[linesPerPage];
        for (int i = 0; i < textArray.Length; i++)
        {
            textArray[i] = "";
        }
        
        buttonArray = new bool[numButtons];
        for (int i = 0; i < buttonArray.Length; i++)
        {
            buttonArray[i] = false;
        }

        // action group numbers
        gearGroupNumber = BaseAction.GetGroupIndex(KSPActionGroup.Gear);
        brakeGroupNumber = BaseAction.GetGroupIndex(KSPActionGroup.Brakes);
        SASGroupNumber = BaseAction.GetGroupIndex(KSPActionGroup.SAS);
        lightGroupNumber = BaseAction.GetGroupIndex(KSPActionGroup.Light);

        useInfoPopup = getInfoPopupObject();

        menuState = menuSplashScreen;
    }

    public override void OnUpdate()
    {
        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA && vessel == FlightGlobals.ActiveVessel)            //|| CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
        {
            // clear all the text lines
            for (int i = 0; i < linesPerPage; i++)
            {
                textArray[i] = "";
            }

            //updateFuel();

            // get flight data numbers. Metric and imperial/aviation


            //updateFlightData();

            //updateActionGroupStates();

            //FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Gear);

            menuState();                        

            //textArray[0] = "Testing";
            targetScript.textArray = textArray;
        }
    }

    #region update display data

    private void updateActionGroupStates()
    {
        gearState = FlightGlobals.ActiveVessel.ActionGroups.groups[gearGroupNumber];
        brakeState = FlightGlobals.ActiveVessel.ActionGroups.groups[brakeGroupNumber];
        SASState = FlightGlobals.ActiveVessel.ActionGroups.groups[SASGroupNumber];
        lightState = FlightGlobals.ActiveVessel.ActionGroups.groups[lightGroupNumber];
    }

    private void updateFlightData()
    {
        switch (speedMode)
        {
            case "Surface":
                displaySpeed = FlightGlobals.ship_srfSpeed;
                break;
            case "Orbit":
                displaySpeed = FlightGlobals.ship_obtSpeed;
                break;
            case "Target":
                displaySpeed = FlightGlobals.ship_tgtSpeed;
                break;
            default:
                displaySpeed = FlightGlobals.ship_srfSpeed;
                break;
        }

        if (unitType == "Metric")
        {
            altitudeString = Math.Floor(FlightGlobals.ship_altitude).ToString().PadLeft(6) + " m";
            radarAltitudeString = Math.Floor(vessel.altitude - Math.Max(vessel.pqsAltitude, 0D)).ToString().PadLeft(6) + " m";
            climbrateString = Math.Round(FlightGlobals.ship_verticalSpeed, 1).ToString().PadLeft(6) + " m/s";
            speedString = Math.Round(displaySpeed, 1).ToString().PadLeft(6) + " m/s";
            hoverString = Math.Round(hoverHeight, 1).ToString().PadLeft(6) + " m";
        }
        else
        {
            double altitude = FlightGlobals.ship_altitude;
            double radarAltitude = vessel.altitude - Math.Max(vessel.pqsAltitude, 0D);
            double climbrate = FlightGlobals.ship_verticalSpeed;
            double speed = displaySpeed;
            float hover = hoverHeight * 3.2808399f;
            altitude *= 3.2808399f;
            radarAltitude *= 3.2808399f;
            climbrate = climbrate * 3.2808399f * 60f;
            speed *= 1.944;
            altitudeString = Math.Floor(altitude).ToString().PadLeft(6) + " ft";
            radarAltitudeString = Math.Floor(radarAltitude).ToString().PadLeft(6) + " ft";
            climbrateString = Math.Floor(climbrate).ToString().PadLeft(6) + " ft/m";
            speedString = Math.Floor(speed).ToString().PadLeft(6) + " kt";
            hoverString = Math.Floor(hover).ToString().PadLeft(6) + " ft";
        }
    }

    private void updateFuel()
    {
        // do resource update if enought ime has passed or the number of parts in the vessel has changed
        if (vesselNumParts != vessel.Parts.Count || resourceUpdateCountdown <= 0)
        {
            getResourceList();
            resourceUpdateCountdown = 60;
            vesselNumParts = vessel.Parts.Count;
            getHoverHeight();
        }
        else
        {
            resourceUpdateCountdown--;
        }
    }

    #endregion

    #region menu states

    private void menuAbort()
    {
        textArray[5] = "--ABORT SEQUENCE--";
        FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
        if (!pauseInitialized)
        {
            pause(120);
        }
        else
        {
            if (!pause())
                menuState = menuMainMenu;
        }
    }

    private void menuFlightData()
    {
        updateFlightData();
        updateFuel();

        int i = 0; // putting ++i in the [] gives it the value before incrementation
        textArray[++i] = "climbRate: " + climbrateString;
        textArray[++i] = "radar alt: " + radarAltitudeString;
        textArray[++i] = "sea alt  : " + altitudeString;
        textArray[++i] = "speed    : " + speedString;
        textArray[++i] = "heading  : " + Math.Round(FlightGlobals.ship_heading, 1).ToString().PadLeft(6) + " deg";
        textArray[++i] = "hover ht.: " + hoverString;
        i++;

        foreach (KeyValuePair<string, Vector2d> kvp in resourceDictionary)
        {
            if (kvp.Key == "LiquidFuel" || kvp.Key == "ElectricCharge")
            {
                string fuelName = kvp.Key;
                if (kvp.Key == "ElectricCharge") fuelName = "Battery  ";
                else if (kvp.Key == "LiquidFuel") fuelName = "Lqd Fuel ";
                textArray[++i] = fuelName + ": " + (int)kvp.Value.x + " / " + (int)kvp.Value.y;
            }
        }

        textArray[linesPerPage - 1] = "root/flight_data";
        if (buttonArray[(int)ButtonNames.back]) menuState = menuMainMenu;
        clearButtons();
    }

    private void menuFuel()
    {
        updateFuel();

        int r = 0;
        foreach (KeyValuePair<string, Vector2d> kvp in resourceDictionary)
        {
            textArray[r] = kvp.Key + ": " + (int)kvp.Value.x + " / " + (int)kvp.Value.y;
            r++;
        }

        textArray[linesPerPage - 1] = "root/fuel";
        if (buttonArray[(int)ButtonNames.back]) menuState = menuMainMenu;
        clearButtons();
    }

    private void menuGunner()
    {
        textArray[5] = "   Gunner module not";
        textArray[6] = "   installed. Contact";
        textArray[7] = "   your customer rep";
        textArray[8] = "   for a quote today!";
        textArray[linesPerPage - 1] = "root/gunner";
        if (buttonArray[(int)ButtonNames.back]) menuState = menuMainMenu;
        clearButtons();
    }

    private void menuInfo()
    {
        if (useInfoPopup)
        {
            for (int j = 0; j < infoPopupStrings.Length; j++)
            {
                textArray[j] = infoPopupStrings[j];
            }
            textArray[linesPerPage - 1] = "root/settings/info";
            if (buttonArray[(int)ButtonNames.confirm]) getInfoPopupText();
        }
        else
            menuState = menuMainMenu;
        if (buttonArray[(int)ButtonNames.back]) menuState = menuMainMenu;
        clearButtons();
    }

    private void menuMainMenu()
    {
        updateActionGroupStates();

        menuSelectionTop = 2;
        menuSelectionBottom = Enum.GetValues(typeof(MainMenuItems)).Length + 1;
        textArray[0] = "Main Menu";
        textArray[(int)MainMenuItems.flightdata + menuSelectionTop] = " Flight data";
        textArray[(int)MainMenuItems.settings + menuSelectionTop] = " Settings";
        textArray[(int)MainMenuItems.abort + menuSelectionTop] = " ABORT!";
        textArray[(int)MainMenuItems.fuel + menuSelectionTop] = " Fuel";
        textArray[(int)MainMenuItems.hover + menuSelectionTop] = " Hover";

        string gearStateString = "Up";
        if (gearState) gearStateString = "Down";
        textArray[(int)MainMenuItems.gear + menuSelectionTop] = " Gear " + gearStateString;

        string brakeStateString = "Off";
        if (brakeState) brakeStateString = "On";
        textArray[(int)MainMenuItems.brakes + menuSelectionTop] = " Brakes " + brakeStateString;

        string SASStateString = "Off";
        if (SASState) SASStateString = "On";
        textArray[(int)MainMenuItems.SAS + menuSelectionTop] = " (A)SAS " + SASStateString;

        string lightStateString = "Off";
        if (lightState) lightStateString = "On";
        textArray[(int)MainMenuItems.light + menuSelectionTop] = " Lights " + lightStateString;

        textArray[(int)MainMenuItems.reboot + menuSelectionTop] = " Reboot";
        textArray[linesPerPage - 1] = "root/main_menu";
        textArray[menuSelection] += " <<<";

        if (buttonArray[(int)ButtonNames.up]) menuSelection--;
        if (buttonArray[(int)ButtonNames.down]) menuSelection++;
        if (menuSelection < menuSelectionTop) menuSelection = menuSelectionTop; //up pressed
        if (menuSelection > menuSelectionBottom) menuSelection = menuSelectionBottom; // down pressed
        if (menuSelection > linesPerPage - 1) menuSelection = linesPerPage - 1; // down pressed

        if (buttonArray[(int)ButtonNames.confirm])  //confirm pressed
        {
            switch (menuSelection)
            {
                case (int)MainMenuItems.flightdata + 2:
                    menuState = menuFlightData;                    
                    break;
                case (int)MainMenuItems.settings + 2:
                    menuState = menuSettings;
                    break;
                case (int)MainMenuItems.abort + 2:
                    pauseInitialized = false;
                    menuState = menuAbort;
                    break;
                case (int)MainMenuItems.fuel + 2:
                    menuState = menuFuel;
                    break;
                case (int)MainMenuItems.gear + 2:
                    FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Gear);
                    break;
                case (int)MainMenuItems.brakes + 2:
                    FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
                    break;
                case (int)MainMenuItems.SAS + 2:
                    FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.SAS);
                    break;
                case (int)MainMenuItems.light + 2:
                    FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Light);
                    break;
                case (int)MainMenuItems.hover + 2:
                    toggleHover();
                    break;
                case (int)MainMenuItems.reboot + 2:
                    menuState = menuSplashScreen;
                    RAMcount = 0;
                    break;
            }
        }

        clearButtons();
    }

    private void menuSettings()
    {
        menuSelectionTop = 2;
        menuSelectionBottom = 3;
        textArray[0] = "Settings";
        textArray[2] = " Units: " + unitType;
        textArray[3] = " Speed: " + speedMode;
        if (useInfoPopup)
        {
            textArray[4] = " Craft info";
            menuSelectionBottom++;
        }
        textArray[linesPerPage - 1] = "root/settings";
        textArray[menuSelection] += " <<<";

        if (buttonArray[(int)ButtonNames.up]) menuSelection--;
        if (buttonArray[(int)ButtonNames.down]) menuSelection++;
        if (menuSelection < menuSelectionTop) menuSelection = menuSelectionTop; //up pressed
        if (menuSelection > menuSelectionBottom) menuSelection = menuSelectionBottom; // down pressed
        if (menuSelection > linesPerPage - 1) menuSelection = linesPerPage - 1; // down pressed

        if (buttonArray[(int)ButtonNames.confirm])  //confirm pressed
        {
            switch (menuSelection)
            {
                case 2:
                    if (unitType == "Metric") unitType = "Aviation";
                    else unitType = "Metric";
                    break;
                case 3:
                    if (speedMode == "Surface") speedMode = "Orbit";
                    else if (speedMode == "Orbit") speedMode = "Target";
                    else if (speedMode == "Target") speedMode = "Surface";
                    break;
                case 4:
                    if (useInfoPopup)
                    {
                        getInfoPopupText();
                        menuState = menuInfo;
                    }
                    break;
            }
        }
        if (buttonArray[(int)ButtonNames.back]) menuState = menuMainMenu;
        clearButtons();
    }    

    private void menuSplashScreen()
    {
        textArray[0] = "Firespitter v4.0";
        textArray[2] = "Booting OS";
        textArray[4] = "Checking RAM ";
        textArray[5] = RAMcount + "/512KB";
        if (RAMcount < 512)
        {
            RAMcount += 4;
            pause(60);
        }
        else
        {
            if (!pause())
                menuState = getMenuHandlerFromEnum(startState);                
        }
    }

    private void menuDefault()
    {
        textArray[0] = "Please insert";
        textArray[1] = "boot disk";
        if (buttonArray[(int)ButtonNames.back]) menuState = menuSplashScreen;
        clearButtons();
    }

    private void menuOff()
    {

    }

    private MenuStateHandler getMenuHandlerFromEnum(MenuState state)
    {
        switch (state)
        {
            case MenuState.flightData:
                return menuFlightData;
            case MenuState.abort:
                return menuAbort;
            case MenuState.fuel:
                return menuFuel;
            case MenuState.gunner:
                return menuGunner;
            case MenuState.info:
                return menuInfo;
            case MenuState.mainMenu:
                return menuMainMenu;
            case MenuState.settings:
                return menuSettings;
            case MenuState.splashScreen:
                return menuSplashScreen;
            default:
                return menuOff;
        }
    }

#endregion
}
