using UnityEngine;
using System.Collections;

/// <summary>
/// For one in a set of buttons (for instance monitor buttons)
/// Assign this to a collider with isTrigger.
/// Runs the function specified by assigning mouseDownFunction to a function in the form functionName(int ID)
/// Multiple buttons are told apart for the receiving function by the ID
/// </summary>
public class FSgenericButtonHandlerID : MonoBehaviour
{
    public delegate void MouseDownFunction(int ID);
    /// <summary>
    /// A function in the form buttonClick(int ID)
    /// </summary>
    public MouseDownFunction mouseDownFunction;
    /// <summary>
    /// The ID passed back to the function specified by mouseDownFunction. Allows for telling buttons apart. If not needed, consider using FSgenericButtonhandler (not ...ID)
    /// </summary>
    public int ID;
    public void OnMouseDown()
    {
        mouseDownFunction(ID);
    }
}
