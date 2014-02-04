using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSGUIPopup
{
    public bool showMenu = false;
    public Rect windowRect = new Rect(500f, 300f, 150f, 100f);
    public Vector2 elementSize = new Vector2(130f, 25f);
    public float lastElementTop = 0f;
    public float marginLeft = 10f;
    public float marginRight = 10f;
    public float marginTop = 22f;
    public float marginBottom = 10f;
    public float subElementSpacing = 8f;
    //public bool optionEnabled = false;
    public bool showCloseButton = true;
    public string optionEnabledString = "On";
    public string optionDisabledString = "Off";
    public string windowTitle;
    public Part parentPart;
    public string moduleName;
    public int GUIlayer = 5878;
    public int moduleID = 0;    

    public bool useInActionEditor = true;
    public bool useInFlight = false;
    //public delegate void RunFunction();
    //private RunFunction run;

    public List<PopupElement> elementList = new List<PopupElement>();

    //private string currentOptionString;    

    /// <summary>
    /// Create the editor popup adn define its settings. Example use at the end of this modules' source.
    /// </summary>
    /// <param name="part">The calling part, just use this.part</param>
    /// <param name="module">The class name of the module where this is being used</param>
    /// <param name="ID">If you have more than one of a module on a part the ID should be unique to prevent window stealing and overlap. Should be passed on from the cfg.</param>
    /// <param name="Windowlayer">the GUI windows layer. Should be unique for each class, possibly with a span of numbers reserved for duplicate modules. the final layer used is layer + ID</param>
    /// <param name="windowDimensions">Left, Top, Width and Height of the GUI window. These are not static, but changed by dragging the window and by resizing functions.</param>
    public FSGUIPopup(Part part, string module, int ID, int Windowlayer, Rect windowDimensions, string windowName, PopupElement defaultElement)    
    {
        elementList.Add(defaultElement);
        parentPart = part;
        moduleName = module;
        moduleID = ID;
        GUIlayer = Windowlayer;
        windowRect = windowDimensions;
        windowTitle = windowName;
        windowRect.y += (moduleID * windowRect.height) + 20;
        //run = function;
    }

    private void drawElement(PopupElement element)
    {        
        int activeElements = 0;
        if (element.useTitle) activeElements++;
        if (element.useInput) activeElements++;        
        activeElements += element.buttons.Count;

        if (activeElements < 1)
            return;
        
        Rect subElementRect = new Rect(marginLeft, marginTop + lastElementTop, element.titleSize, elementSize.y);
        
        if (element.useTitle)
        {
            if (subElementRect.width == 0)
                subElementRect.width = (elementSize.x / activeElements) - (subElementSpacing);
            GUI.Label(subElementRect, element.titleText);
            subElementRect.x += subElementRect.width + subElementSpacing;
        }

        if (element.useInput)
        {
            subElementRect.width = element.inputSize;
            if (subElementRect.width == 0)
                subElementRect.width = (elementSize.x / activeElements) - (subElementSpacing);
            element.inputText = GUI.TextField(subElementRect, element.inputText);
            subElementRect.x += subElementRect.width + subElementSpacing;
        }

        for (int i = 0; i < element.buttons.Count; i++)
        {
            subElementRect.width = element.buttons[i].buttonWidth;
            if (subElementRect.width == 0)
                subElementRect.width = (elementSize.x / activeElements) - (subElementSpacing);

            if (element.buttons[i].style == null)
            {
                element.buttons[i].style = new GUIStyle(GUI.skin.button);
            }
            if (GUI.Button(subElementRect, element.buttons[i].buttonText, element.buttons[i].style))
            {
                if (element.buttons[i].runFunction != null)
                    element.buttons[i].runFunction();
                if (element.buttons[i].buttonSpecificFunction != null)
                    element.buttons[i].buttonSpecificFunction(element.buttons[i]);
            }
            subElementRect.x += subElementRect.width + subElementSpacing;
        }

        lastElementTop += elementSize.y + subElementSpacing;
    }

    private void drawWindow(int windowID)
    {
        lastElementTop = 0f;
        elementSize.x = windowRect.width - marginLeft - marginRight + subElementSpacing;
        windowRect.height = ((float)elementList.Count * (elementSize.y + subElementSpacing)) + marginTop + marginBottom;
        for (int i = 0; i < elementList.Count; i++)
        {
            drawElement(elementList[i]);
        }
        if (showCloseButton)
        {
            if (GUI.Button(new Rect(windowRect.width - 18f, 2f, 16f, 16f),""))
            {
                showMenu = false;
            }
        }
        GUI.DragWindow();        
    }

    public void popup()
    {        
        if (useInActionEditor && HighLogic.LoadedSceneIsEditor)
        {
            if (parentPart != null)
            {                
                if (showMenu)
                {
                    windowRect = GUI.Window(GUIlayer, windowRect, drawWindow, windowTitle);
                }

                showMenu = false;

                EditorLogic editor = EditorLogic.fetch;
                if (editor)
                {
                    if (editor.editorScreen == EditorLogic.EditorScreen.Actions)
                    {
                        List<Part> partlist = EditorActionGroups.Instance.GetSelectedParts();
                        if (partlist.Count > 0)
                        {
                            if (partlist[0] == parentPart)
                            {
                                if (partlist[0].Modules.Contains(moduleName))
                                {
                                    showMenu = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (useInFlight && HighLogic.LoadedSceneIsFlight)
        {
            if (showMenu && parentPart.vessel.isActiveVessel)
            {
                windowRect = GUI.Window(GUIlayer, windowRect, drawWindow, windowTitle);
            }
        }
        //return optionEnabled;        
    }
}

public class PopupElement
{        
    public string titleText = "";
    public string inputText = "";
    public List<PopupButton> buttons = new List<PopupButton>();

    public float titleSize = 0f;
    public float inputSize = 0f;
    

    public bool useTitle = false;
    public bool useInput = false;

    public PopupElement()
    {
    }

    public PopupElement(string title, PopupButton button)
    {        
        titleText = title;        
        useTitle = true;
        buttons.Add(button);
    }

    public PopupElement(PopupButton button)
    {        
        buttons.Add(button);        
    }

    public PopupElement(string title)
    {        
        titleText = title;
        useTitle = true;        
    }

    public PopupElement(string title, string input)
    {        
        titleText = title;
        useTitle = true;
        inputText = input;
        useInput = true;
    }

    public PopupElement(string title, string input, PopupButton button)
    {
        titleText = title;
        useTitle = true;
        inputText = input;
        useInput = true;
        buttons.Add(button); 
    }
}

public class PopupButton
{
    public delegate void RunFunction();
    public RunFunction runFunction;
    public delegate void RunButtonSpecificFunction(PopupButton button);
    public RunButtonSpecificFunction buttonSpecificFunction;
    public string buttonText;
    public string buttonTextOn;
    public string buttonTextOff;
    public float buttonWidth;
    public bool isToggleButton;
    public bool toggleState;
    public PopupElement popupElement;

    public GUIStyle style; // GUIStyle(GUI.skin.button);
    Color selectedColor = Color.red;
    Color normalColor = Color.white;
    Color disabledColor = Color.gray;

    private bool _styleSelected;
    public bool styleSelected
    {
        set
        {
            _styleSelected = value;
            if (value)
            {
                style.normal.textColor = selectedColor;
            }
            else
            {
                style.normal.textColor = normalColor;
            }
        }
        get
        {
            return _styleSelected;
        }
    }

    private bool _styleDisabled;
    public bool styleDisabled
    {
        set
        {
            _styleDisabled = value;
            if (value)
            {
                style.normal.textColor = disabledColor;
            }
            else
            {
                style.normal.textColor = normalColor;
            }
        }
        get
        {
            return _styleDisabled;
        }
    }

    public PopupButton()
    {

    }

    public PopupButton(string text, float width, RunFunction function)
    {
        buttonText = text;
        buttonWidth = width;
        runFunction = function;
        isToggleButton = false;
    }

    public PopupButton(string textOn, string textOff, float width, RunFunction function)
    {
        buttonText = textOff;
        buttonTextOn = textOn;
        buttonTextOff = textOff;
        isToggleButton = true;
        buttonWidth = width;
        runFunction = function;
    }

    public PopupButton(string text, float width, RunButtonSpecificFunction function)
    {
        buttonText = text;
        buttonWidth = width;
        buttonSpecificFunction = function;
    }

    public void toggle(bool newState)
    {
        toggleState = newState;
        if (newState)
        {
            buttonText = buttonTextOn;            
        }
        else
        {
            buttonText = buttonTextOff;            
        }
    }
}