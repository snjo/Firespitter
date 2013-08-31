using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CrewMember
{
    public ProtoCrewMember crew;
    public Part part;
    public int index;
    public float Stupidity;
    public float Courage;
    public bool isBadass;
    public string Name;

    public CrewMember(ProtoCrewMember _crew, Part _part, int _index)
    {
        crew = _crew;
        part = _part;
        index = _index;
        Stupidity = _crew.stupidity;
        Courage = _crew.courage;
        isBadass = _crew.isBadass;
        Name = _crew.name;
    }
}

// Don't use this module. It will break you save game :(

// This module should not be used on parts without an internal space. Switching IVA cameras in a vessel with a crew member in an internal-less part will send the camera to a nullref postition.
// This is not a bug with this module, just a general KSP bug, but this module makes it obvious

public class FScrewTransfer : PartModule
{
    private List<CrewMember> availableCrew; // = new List<ProtoCrewMember>();
    public bool showMenu = false;
    public float enabledAtTime = 0f;


    /*[KSPEvent(name = "addCrew", guiName = "Add Crew", active = true, guiActive = false)]
    public void addCrewEvent()
    {
        ProtoCrewMember newKerbal = new ProtoCrewMember();
        newKerbal.name = "Snjo";
        addCrew(this.part, newKerbal);
    }*/

    /*[KSPEvent(name = "removeCrew", guiName = "Remove Crew", active = true, guiActive = true)]
    public void removeCrewEvent()
    {
        if (part.protoModuleCrew.Count > 0)
            part.RemoveCrewmember(part.protoModuleCrew[0]);
    }*/

    [KSPEvent(name = "menuEvent", guiName = "Move Crew", active = true, guiActive = true)]
    public void menuEvent()
    {
        createCrewList();
        enabledAtTime = Time.time;
        showMenu = true;
    }

    [KSPAction("Crew Transfer")]
    public void menuAction(KSPActionParam param)
    {
        createCrewList();
        enabledAtTime = Time.time;
        showMenu = true;
    }

    /*int numEvents = 5;
    #region Fetch Events
    [KSPEvent(name = "fetchCrewEvent0", guiName = "Fetch 0", active = true, guiActive = false)]
    public void fetchCrewEvent0()
    {
        fetchCrew(0);
    }
    [KSPEvent(name = "fetchCrewEvent1", guiName = "Fetch 1", active = true, guiActive = false)]
    public void fetchCrewEvent1()
    {
        fetchCrew(1);
    }
    [KSPEvent(name = "fetchCrewEvent2", guiName = "Fetch 2", active = true, guiActive = false)]
    public void fetchCrewEvent2()
    {
        fetchCrew(2);
    }
    [KSPEvent(name = "fetchCrewEvent3", guiName = "Fetch 3", active = true, guiActive = false)]
    public void fetchCrewEvent3()
    {
        fetchCrew(3);
    }
    [KSPEvent(name = "fetchCrewEvent4", guiName = "Fetch 4", active = true, guiActive = false)]
    public void fetchCrewEvent4()
    {
        fetchCrew(4);
    }
    #endregion*/

    /*private void setEventState(int index, bool active)
    {
        if (index > numEvents-1) return;
        string targetEvent = "fetchCrewEvent" + index;
        Events[targetEvent].guiActive = active;
        Events[targetEvent].guiName = "Fetch " + availableCrew[index].crew.name + " from " + availableCrew[index].part.name;
    }*/

    public void fetchCrew(int index)
    {
        //createCrewList();
        /*if (availableCrew[index].part.isControlSource && availableCrew[index].part.protoModuleCrew.Count == 1)
        {
            Debug.Log("Can't empty the controlling pod");            
        }
        else
        {*/

            if (part.protoModuleCrew.Count < part.CrewCapacity && availableCrew.Count > index)
            {
                removeCrew(availableCrew[index]);
                addCrew(part, availableCrew[index]);
            }
            createCrewList();
        //}
    }

    private void createCrewList()
    {
        availableCrew = new List<CrewMember>();

        //for (int i = 0; i < numEvents; i++)
        //    Events["fetchCrewEvent" + i].guiActive = false;

        foreach (Part p in vessel.parts)
        {
            if (p != part && p.protoModuleCrew.Count > 0)
            {
                for (int i = 0; i < p.protoModuleCrew.Count; i++)
                {
                    availableCrew.Add(new CrewMember(p.protoModuleCrew[i], p, i));
                    Debug.Log("Added " + availableCrew[i].crew.name + " to the list of move candidates");
                    //setEventState(i, true);
                }
            }
        }
    }    

    private void addCrew(Part part, CrewMember crew)  //from kerbal crew manifest by vXSovereignXv
    {
        ProtoCrewMember kerbal = new ProtoCrewMember();
        kerbal.name = crew.Name;
        kerbal.isBadass = crew.isBadass;
        kerbal.stupidity = crew.Stupidity;
        kerbal.courage = crew.Courage;
        kerbal.rosterStatus = ProtoCrewMember.RosterStatus.ASSIGNED;
        //kerbal.seat = null;
        //kerbal.seatIdx = -1;
        part.AddCrewmember(kerbal);        
        if (kerbal.seat != null)
            kerbal.seat.SpawnCrew();
    }

    private void removeCrew(CrewMember targetCrew)
    {
        if (targetCrew.part.protoModuleCrew.Count > 0)
        {
            targetCrew.part.RemoveCrewmember(targetCrew.part.protoModuleCrew[targetCrew.index]);                        
            //targetCrew.part.protoModuleCrew[targetCrew.index].rosterStatus = ProtoCrewMember.RosterStatus.AVAILABLE;
            //targetCrew.part.protoModuleCrew[targetCrew.index].seat.DespawnCrew();
        }        
        /*member.seat.DespawnCrew();
        p.RemoveCrewmember(member);*/
    }

    public void OnGUI()
    {
        if (showMenu)
        {
            foreach (Part p in vessel.parts)
            {
                if (p != part)
                {
                    FScrewTransfer ct = p.GetComponents<FScrewTransfer>().FirstOrDefault();
                    if (ct != null)
                        if (enabledAtTime > ct.enabledAtTime)
                            ct.showMenu = false;
                }

            }


            Vector2 menuBasePosition = new Vector2(100f, 300f);
            Vector2 menuItemPosition = new Vector2(0f, 0f);
            Vector2 menuItemSize = new Vector2(300f, 40f);
            Vector2 buttonSize = new Vector2(30f, 25f);

            Rect menuItemRect = new Rect(menuBasePosition.x + menuItemPosition.x, menuBasePosition.y + menuItemPosition.y, menuItemSize.x, menuItemSize.y);

            GUI.Label(menuItemRect, "Move Crew Members");
            if (GUI.Button(new Rect(menuItemRect.x + menuItemSize.x, menuItemRect.y, buttonSize.x, buttonSize.y), "X"))
                showMenu = false;
            menuItemRect.y += menuItemSize.y;

            for (int i = 0; i < availableCrew.Count; i++)
            {
                GUIContent contents = new GUIContent("Fetch " + availableCrew[i].crew.name + " from " + availableCrew[i].part.name);
                GUI.Label(menuItemRect, contents);
                if (GUI.Button(new Rect(menuItemRect.x + menuItemSize.x, menuItemRect.y, buttonSize.x, buttonSize.y), " "))
                {
                    fetchCrew(i);
                    //showMenu = false;
                }                                
                menuItemRect.y += menuItemSize.y;
            }
        }
    }
}


