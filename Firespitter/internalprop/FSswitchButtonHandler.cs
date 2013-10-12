using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// For switches.
/// Assign this to a collider with isTrigger.
/// Activates the buttonClick function on the part's first available FSActionGroupSwitch.
/// Multiple buttons are told apart for the receiving function by the buttonNumber
/// </summary>
[Obsolete("Use FSgenericButtonHandler instead",false)]
public class FSswitchButtonHandler : InternalModule
{
    /// <summary>
    /// A part with an assigned FSActionGroupSwitch module
    /// </summary>
    public GameObject target;
    /// <summary>
    /// an identifying number to tell multiple buttons apart, if several are assigned to the target module.
    /// </summary>
    public int buttonNumber = 1;

    public void OnMouseDown()
    {
        //target.GetComponent<FSActionGroupSwitch>().buttonClick(buttonNumber);
        target.GetComponent<FSActionGroupSwitch>().buttonClick();
    }
}
