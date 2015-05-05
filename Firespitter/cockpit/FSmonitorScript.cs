using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

namespace Firespitter.cockpit
{
    public class FSmonitorScript : InternalModule
    {
        FSmonitorInterface[] fsMon;

        [KSPField]
        public string charPlateObject = "charPlate";
        [KSPField]
        public float plateSize = 0.003f;
        [KSPField]
        public float charSpacing = 0.0036f;
        [KSPField]
        public float lineSpacing = 0.0055f;
        [KSPField]
        public int charPerLine = 23;
        [KSPField]
        public int linesPerPage = 17;
        [KSPField]
        public int useCustomStartStatesCfg = 1;
        [KSPField]
        public bool autoCreateGrid = true;
        public bool useCustomStartStates = true;

        public Material spriteSheetMat;
        [KSPField]
        public float spriteScale = 0.0625f;
        [KSPField]
        public float spriteShift = 0.0f;

        public enum TextMode
        {
            singleString,
            lineArray
        }
        public TextMode textMode = TextMode.singleString;

        public bool arrayCreated = false;
        private GameObject baseCharPlate;
        private bool monitorDefaultStateSet = false;

        FSmonitorInterface.MenuState[] monitorStartState;

        public string[] textArray;
        public string[] oldTextArray;
        [KSPField]
        public string text = "";
        private string oldText = "";
        //Material mat_A;

        List<List<GameObject>> lineList = new List<List<GameObject>>();

        public Vector2 getSheetCharPosition(char input) // this is hard coded to a 16x16 grid at the moment
        {
            int charInt = (int)input;
            int charPos = charInt % 16;
            int linePos = (charInt - charPos) / 16;
            linePos = 16 - linePos - 1;
            charPos++;

            //Debug.Log("In: " + input + " : " + linePos + " / " + charPos);

            return new Vector2((float)charPos, (float)linePos);
        }

        // Use this for initialization
        public override void OnAwake()
        {
            //When checking for multiple monitors, use this as their start states
            useCustomStartStates = useCustomStartStatesCfg == 1;

            monitorStartState = new FSmonitorInterface.MenuState[]
        {
            FSmonitorInterface.MenuState.flightData, //rear left
            FSmonitorInterface.MenuState.mainMenu, // rear right
            FSmonitorInterface.MenuState.info, //front right
            FSmonitorInterface.MenuState.flightData // front left
        };

            if (autoCreateGrid) createTextGrid();
        }

        public void createTextGrid()
        {
            int charCount = 0;
            int lineCount = 0;
            spriteSheetMat = base.internalProp.FindModelTransform(charPlateObject).renderer.material;

            //baseCharPlate = GameObject.Find(charPlateObject);
            baseCharPlate = base.internalProp.FindModelTransform(charPlateObject).gameObject;
            if (baseCharPlate != null)
            {
                baseCharPlate.transform.localScale = new Vector3(plateSize, plateSize, plateSize);
                for (lineCount = 0; lineCount < linesPerPage; lineCount++)
                {
                    //lineList.Add(List<GameObject>);
                    List<GameObject> charList = new List<GameObject>();
                    for (charCount = 0; charCount < charPerLine; charCount++)
                    {
                        //List<GameObject> charList = new List<GameObject>();
                        //GameObject newPlate = (GameObject)Instantiate(baseCharPlate, baseCharPlate.transform.position + new Vector3(charSpacing * (charCount), 0f, lineSpacing * (lineCount)), Quaternion.Euler(new Vector3(-90f,0f,0f)));
                        GameObject newPlate = (GameObject)Instantiate(baseCharPlate, baseCharPlate.transform.position, baseCharPlate.transform.rotation);
                        newPlate.transform.parent = base.transform;
                        //newPlate.transform.localPosition += new Vector3(charSpacing * (charCount), 0f, lineSpacing * (lineCount));
                        newPlate.transform.localPosition += new Vector3(-charSpacing * charCount, -lineSpacing * lineCount, 0f);
                        newPlate.name = "cpl" + lineCount + "c" + charCount;
                        newPlate.renderer.material = new Material(spriteSheetMat.shader);
                        newPlate.renderer.material.mainTexture = spriteSheetMat.mainTexture;
                        newPlate.renderer.material.mainTextureScale = new Vector2(-spriteScale, spriteScale); ;
                        charList.Add(newPlate);
                    }
                    lineList.Add(charList);
                }
                arrayCreated = true;
                Transform baseCharPlateTransform = base.internalProp.FindModelTransform(charPlateObject);
                //baseCharPlateTransform.GetComponent<MeshRenderer>().enabled = false;           
            }
            else Debug.Log("FSmonitorScript: no char plate");

            textArray = new string[linesPerPage];
            oldTextArray = new string[linesPerPage];

            for (int i = 0; i < textArray.Length; i++)
            {
                textArray[i] = "";
                oldTextArray[i] = "";
            }
        }

        // Update is called once per frame
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;

            // Run once. (Must be run after all parts have been created, so it can't be in the OnAwake)
            if (!monitorDefaultStateSet)
            {
                //Debug.Log("initializing monitors"); // -------------------------------------------------<<<<<<<<
                fsMon = new FSmonitorInterface[20];
                fsMon = base.transform.parent.GetComponentsInChildren<FSmonitorInterface>();
                //Debug.Log("found " + fsMon.Length + " monitors");
                for (int i = 0; i < fsMon.Length; i++)
                {
                    //Debug.Log("setting monitor " + i);
                    if (i < monitorStartState.Length && useCustomStartStates)
                    {
                        fsMon[i].startState = monitorStartState[i];
                    }
                    else
                    {
                        fsMon[i].startState = FSmonitorInterface.MenuState.mainMenu;
                    }

                }
                monitorDefaultStateSet = true;
            }

            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)            //|| CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
            { //Only run this code when IVA

                if (arrayCreated)
                {
                    updateCharTextureOffsets();
                }
            }
        }

        private void updateCharTextureOffsets()
        {            
            if (textMode == TextMode.singleString && text != oldText)
            {
                parseSingleString(text);
            }
            else
            {
                parseStringArray(textArray);
            }
        }

        private void parseStringArray(string[] inputTextArray)
        {
            for (int lineCount = 0; lineCount < inputTextArray.Length; lineCount++)
            {
                if (lineCount < linesPerPage)
                {
                    if (inputTextArray[lineCount] != oldTextArray[lineCount])
                    {
                        char[] charArray = inputTextArray[lineCount].ToCharArray();
                        for (int charCount = 0; charCount < charPerLine; charCount++)
                        {
                            char paddedChar = ' ';
                            if (charCount < charArray.Length) paddedChar = charArray[charCount];
                            lineList[lineCount][charCount].renderer.material.mainTextureOffset = (getSheetCharPosition(paddedChar) * spriteScale) - new Vector2(spriteShift, 0f);
                        }
                        oldTextArray[lineCount] = inputTextArray[lineCount];
                    }
                }
            }
        }

        private void parseSingleString(string inputText)
        {
            char[] c = inputText.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                int charNum = i % charPerLine;
                int lineNum = (i - charNum) / 10; // hmmm, seems hard coded somehow...
                //int lineNum = (i - charNum) / charPerLine; // should try this instead later.
                if (lineNum >= linesPerPage) break;
                lineList[lineNum][charNum].renderer.material.mainTextureOffset = (getSheetCharPosition(c[i]) * spriteScale) - new Vector2(spriteShift, 0f);
                oldText = inputText;
            }
        }

    }
}