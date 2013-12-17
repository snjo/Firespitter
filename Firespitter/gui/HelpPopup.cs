using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.gui
{
    class HelpPopup
    {
        private string text = string.Empty;
        private bool textInitialized = false;
        public string windowTitle = string.Empty;
        public Rect windowRect = FSGUIwindowID.tallRect;
        private Rect scrollRect;
        private Rect textRect;
        public bool scrollBar = true;
        public bool showMenu = false;
        public bool showCloseButton = true;            
        public int GUIlayer = 0;
        public GUIStyle style;
        Color textColor = Color.white;
        private Vector2 scrollPosition = Vector2.zero;
        private GUIContent content;

        private float textAreaHeight;

        public HelpPopup(string _windowTitle, string _text)
        {
            text = _text;
            windowTitle = _windowTitle;
            GUIlayer = FSGUIwindowID.getNextID();            
        }

        public void setText(string _text)
        {            
            content = new GUIContent(_text);
            scrollRect = new Rect(2f, 25f, windowRect.width - 4f, windowRect.height - 25f);
            textAreaHeight = style.CalcHeight(content, scrollRect.width-20f);
            textRect = new Rect(0f, 0f, scrollRect.width - 20f, textAreaHeight);               
        }

        private void drawWindow(int ID)
        {            
            if (showCloseButton)
            {
                if (GUI.Button(new Rect(windowRect.width - 18f, 2f, 16f, 16f), ""))
                {
                    showMenu = false;
                }
            }

            scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, textRect);
            GUI.TextArea(textRect, content.text, style);            
            GUI.EndScrollView();
            GUI.DragWindow();
        }

        private void createStyle()
        {
            style = new GUIStyle(GUI.skin.textArea);
            style.wordWrap = true;
            style.richText = true;
            style.normal.textColor = textColor;
        }

        public void draw()
        {
            if (showMenu)
            {
                if (style == null)
                {
                    createStyle();
                }
                if (!textInitialized)
                {
                    setText(text);
                    textInitialized = true;
                }
                windowRect = GUI.Window(GUIlayer, windowRect, drawWindow, windowTitle);
            }
        }
    }
}
