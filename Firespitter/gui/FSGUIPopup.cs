using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSGUIPopup
{
    public bool showMenu = false;
    public bool partSelected = true;
    public Rect windowRect = new Rect(500f, 300f, 150f, 100f);
    //public Vector2 elementSize = new Vector2(130f, 25f);    
    public float elementWidth = 130f;
    public float marginLeft = 10f;
    public float marginRight = 10f;
    public float marginTop = 22f;
    public float marginBottom = 10f;
    public float subElementSpacing = 8f;
    public float lineSpacing = 8f;
    //public bool optionEnabled = false;
    public bool showCloseButton = true;
    public string optionEnabledString = "On";
    public string optionDisabledString = "Off";
    public string windowTitle;
    public Part parentPart;
    public string moduleName;
    public int GUIlayer = 5878;
    public int moduleID = 0;    

    /// <summary>
    /// Popup will only be displayed if the part is highlighted in the Action Groups setup
    /// </summary>
    public bool useInActionEditor = true;
    /// <summary>
    /// Popup can be shown in the regular SPH/VAB Craft Editor, if showMenu is true. Beware window ID conflicts from multiple popup sources
    /// </summary>
    public bool useInEditor = false;
    /// <summary>
    /// Popup can be shown in the the flight scene, if showMenu is true. Beware window ID conflicts from multiple popup sources
    /// </summary>
    public bool useInFlight = false;
    /// <summary>
    /// Popup is used in main menus, space center or similar. No reference to vessel etc.
    /// </summary>
    public bool useInMenus = false;

    public delegate void HideMenuEvent();
    public HideMenuEvent hideMenuEvent;

    private float lastSectionTop = 0f;
    private float lastElementTop = 0f;

    public List<PopupSection> sections = new List<PopupSection>();
    //public List<PopupElement> elementList = new List<PopupElement>();

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
        sections = new List<PopupSection>();
        sections.Add(new PopupSection());
        sections[0].elements.Add(defaultElement);
        //elementList.Add(defaultElement);
        parentPart = part;
        moduleName = module;
        moduleID = ID;
        GUIlayer = Windowlayer;
        windowRect = windowDimensions;
        windowTitle = windowName;
        windowRect.y += (moduleID * windowRect.height) + 20;        
    }

    public FSGUIPopup(Part part, string module, int ID, int Windowlayer, Rect windowDimensions, string windowName)
    {
        parentPart = part;
        moduleName = module;
        moduleID = ID;
        GUIlayer = Windowlayer;
        windowRect = windowDimensions;
        windowTitle = windowName;
        windowRect.y += (moduleID * windowRect.height) + 20;
    }

    public FSGUIPopup(int Windowlayer, Rect windowDimensions, string windowName)
    {
        GUIlayer = Windowlayer;
        windowRect = windowDimensions;
        windowTitle = windowName;
        windowRect.y += (moduleID * windowRect.height) + 20;
    }

    private void drawElement(PopupElement element)
    {
        if (element.showElement)
        {
            if (element.style == null)
            {
                element.setStyle(GUI.skin.textArea); //GUI.skin.textArea
            }
            int activeElements = 0;
            if (element.useTitle) activeElements++;
            if (element.useInput) activeElements++;
            activeElements += element.buttons.Count;

            if (activeElements < 1)
                return;

            Rect subElementRect = new Rect(marginLeft, marginTop + lastSectionTop + lastElementTop, element.titleSize, element.height);

            if (element.useTitle)
            {
                if (subElementRect.width == 0)
                    subElementRect.width = (elementWidth / activeElements) - (subElementSpacing);
                if (element.useTextArea)
                    GUI.TextArea(subElementRect, element.titleText, element.style);
                else
                    GUI.Label(subElementRect, element.titleText, element.style);                
                subElementRect.x += subElementRect.width + subElementSpacing;
            }

            if (element.useInput)
            {
                subElementRect.width = element.inputSize;
                if (subElementRect.width == 0)
                    subElementRect.width = (elementWidth / activeElements) - (subElementSpacing);
                element.inputText = GUI.TextField(subElementRect, element.inputText);
                subElementRect.x += subElementRect.width + subElementSpacing;
            }

            for (int i = 0; i < element.buttons.Count; i++)
            {
                subElementRect.width = element.buttons[i].buttonWidth;
                if (subElementRect.width == 0)
                    subElementRect.width = (elementWidth / activeElements) - (subElementSpacing);

                if (element.buttons[i].style == null)
                {
                    if (element.buttons[i].isGUIToggle)
                        element.buttons[i].style = new GUIStyle(GUI.skin.toggle);
                    else
                        element.buttons[i].style = new GUIStyle(GUI.skin.button);
                }

                bool resultTrue = false;
                if (element.buttons[i].isGUIToggle)
                {
                    resultTrue = GUI.Toggle(subElementRect, element.buttons[i].toggleState, element.buttons[i].buttonText, element.buttons[i].style);
                    element.buttons[i].toggleState = resultTrue;
                }
                else
                {
                    resultTrue = GUI.Button(subElementRect, element.buttons[i].buttonText, element.buttons[i].style);
                }

                if (resultTrue)
                {
                    if (element.buttons[i].genericFunction != null)
                        element.buttons[i].genericFunction();
                    if (element.buttons[i].buttonSpecificFunction != null)
                        element.buttons[i].buttonSpecificFunction(element.buttons[i]);
                    if (element.buttons[i].IDfunctionInt != null)
                        element.buttons[i].IDfunctionInt(element.buttons[i].buttonIDInt);
                    if (element.buttons[i].IDfunctionString != null)
                        element.buttons[i].IDfunctionString(element.buttons[i].buttonIDString);                    
                }
                subElementRect.x += subElementRect.width + subElementSpacing;
            }

            lastElementTop += element.height + lineSpacing;
        }
    }

    private void drawWindow(int windowID)
    {
        windowRect.height = marginTop + marginBottom - lineSpacing;
        lastSectionTop = 0f;
        foreach (PopupSection section in sections)
        {
            drawSection(section);
            lastSectionTop += lastElementTop;
        }

        if (showCloseButton)
        {
            if (GUI.Button(new Rect(windowRect.width - 18f, 2f, 16f, 16f), ""))
            {
                showMenu = false;
            }
        }

        GUI.DragWindow();
    }

    private void drawSection(PopupSection section)
    {
        if (section.showSection)
        {
            lastElementTop = 0f;
            elementWidth = windowRect.width - marginLeft - marginRight + subElementSpacing;
            //windowRect.height = ((float)section.elements.Count * (elementSize.y + lineSpacing)) + marginTop + marginBottom;
            for (int i = 0; i < section.elements.Count; i++)
            {
                drawElement(section.elements[i]);
                windowRect.height += section.elements[i].height + lineSpacing;
            }
        }
        //if (showCloseButton)
        //{
        //    if (GUI.Button(new Rect(windowRect.width - 18f, 2f, 16f, 16f), ""))
        //    {
        //        showMenu = false;
        //        hideMenuEvent();
        //    }
        //}
    }

    public void popup()
    {
        if (HighLogic.LoadedSceneIsEditor)
        {
            if (useInEditor)
            {
                if (showMenu)
                {
                    windowRect = GUI.Window(GUIlayer, windowRect, drawWindow, windowTitle);
                }
            }
            else if (useInActionEditor)
            {
                if (parentPart != null)
                {
                    if (showMenu)
                    {
                        windowRect = GUI.Window(GUIlayer, windowRect, drawWindow, windowTitle);
                    }

                    showMenu = false;
                    partSelected = false;

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
                                        partSelected = true;
                                    }
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
        if (useInMenus && !HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
        {
            if (showMenu)
            {
                windowRect = GUI.Window(GUIlayer, windowRect, drawWindow, windowTitle);
            }
        }
        //return optionEnabled;        
    }
}

public class ProxyModuleWithGUI : PartModule
{
    public bool GUIchild = false;
    public bool GUIparent = false;
    public List<PopupSection> popupSections;
}

public class PopupSection
{
    public bool showSection = true;
    public List<PopupElement> elements = new List<PopupElement>();
    public void AddElement(PopupElement element, float height)
    {
        element.height = height;
        elements.Add(element);
    }
    public void AddElement(PopupElement element)
    {        
        elements.Add(element);
    }
}

public class PopupElement
{
    public bool showElement = true;
    public string titleText = "";
    public string inputText = "";
    public List<PopupButton> buttons = new List<PopupButton>();
    public float height = 25f;
    public float titleSize = 0f;
    public float inputSize = 0f;        

    public bool useTitle = false;
    public bool useInput = false;
    public bool useTextArea = false;

    public GUIStyle style; // GUIStyle(GUI.skin.button);
    Color selectedColor = Color.red;
    Color normalColor = Color.white;
    Color disabledColor = Color.gray;
    Color textColor = Color.white;
    bool wordWrap = true;
    bool richText = true;    

    public void setStyle(GUIStyle _baseStyle)
    {
        setStyle(textColor, wordWrap, richText, _baseStyle);        
    }

    public void setStyle(Color _textColor, bool _wordWrap, bool _richText, GUIStyle _baseStyle)
    {
        style = new GUIStyle(_baseStyle);
        style.normal.textColor = _textColor;
        style.wordWrap = _wordWrap;
        style.richText = _richText;        
    }

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

    public PopupElement(string title, bool textArea)
    {
        titleText = title;
        useTitle = true;
        useTextArea = textArea;
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
    public RunFunction genericFunction;
    public delegate void RunButtonSpecificFunction(PopupButton button);
    public RunButtonSpecificFunction buttonSpecificFunction;
    public delegate void RunIDFunctionInt(int ID);
    public RunIDFunctionInt IDfunctionInt;
    public delegate void RunIDFunctionString(string ID);
    public RunIDFunctionString IDfunctionString;
    public int buttonIDInt;
    public string buttonIDString;
    public string buttonText;
    public string buttonTextOn;
    public string buttonTextOff;
    public Texture2D texture;
    public float buttonWidth;
    public bool isToggleButton;
    public bool toggleState;
    public PopupElement popupElement;
    public bool isGUIToggle = false;

    public GUIStyle style; // GUIStyle(GUI.skin.button);
    Color selectedColor = Color.red;
    Color normalColor = Color.white;
    Color disabledColor = Color.gray;

    public void setupGUIStyle()
    {
        style = new GUIStyle(); //GUI.skin.GetStyle("Button"));
        //style.normal.background = texture;
        //style.border = new RectOffset(1,1,1,1);
        style.alignment = TextAnchor.LowerCenter;
    }

    public void setTexture(Texture2D normalTexture, Texture2D hoverTexture, Texture2D focusedTexture, Texture2D activeTexture)
    {
        style.normal.background = normalTexture;
        style.hover.background = hoverTexture;
        style.focused.background = focusedTexture;
        style.active.background = activeTexture;
    }

    public void setTexture(Texture2D allStatesTextures)
    {
        setTexture(allStatesTextures, allStatesTextures, allStatesTextures, allStatesTextures);
    }

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
        //setupGUIStyle();
    }

    public PopupButton(string text, float width, RunFunction function)
    {
        buttonText = text;
        buttonWidth = width;
        genericFunction = function;
        isToggleButton = false;
    }

    public PopupButton(string textOn, string textOff, float width, RunFunction function)
    {
        buttonText = textOff;
        buttonTextOn = textOn;
        buttonTextOff = textOff;
        isToggleButton = true;
        buttonWidth = width;
        genericFunction = function;
    }
    
    public PopupButton(string toggleTitle, float width)
    {
        buttonText = toggleTitle;        
        isGUIToggle = true;
        buttonWidth = width;        
    }

    public PopupButton(string text, float width, RunButtonSpecificFunction function)
    {
        buttonText = text;
        buttonWidth = width;
        buttonSpecificFunction = function;
    }

    public PopupButton(string text, float width, RunIDFunctionInt function, int ID)
    {
        buttonText = text;
        buttonWidth = width;
        IDfunctionInt = function;
        buttonIDInt = ID;
        //setupGUIStyle();
    }

    public PopupButton(string text, float width, RunIDFunctionString function, string ID)
    {
        buttonText = text;
        buttonWidth = width;
        IDfunctionString = function;
        buttonIDString = ID;
        //setupGUIStyle();
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