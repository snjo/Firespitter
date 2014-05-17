using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSpropellerTweak : PartModule
    {
        [KSPField]
        public string propellerRootName = "propellerRoot";
        [KSPField]
        public string bladeRootName = "bladeRoot";
        [KSPField]
        public string bladeName = "blade";
        [KSPField(isPersistant=true, guiActive=true, guiName="Blades")]
        public int bladeNumber = 2;
        [KSPField]
        public int minBlades = 2;
        [KSPField]
        public int maxBlades = 8;
        [KSPField]
        public float originalBladeRotation = 0f;
        [KSPField]
        public Vector3 propellerRotationAxis = Vector3.forward;

        [KSPField]
        public float minThrust = 0f;
        [KSPField]
        public float maxThrust = 0f;

        [KSPField]
        public string engineEndPointName = "engineEndPoint";
        [KSPField]
        public string movableSectionName = "movableSection";
        [KSPField]
        public string engineExtenderName = "engineExtender";
        [KSPField(isPersistant=true)]
        public float engineLengthSlider = 0f;
        [KSPField]
        public Vector3 engineExtensionAxis = Vector3.forward;
        [KSPField]
        public string exhaustName = "exhaust";
        [KSPField]
        public float exhaustFrequency = 1f;
        [KSPField]
        public float exhaustDistance = 0.25f;
        [KSPField]
        public Vector3 exhaustTranslateAxis = Vector3.up;
        [KSPField]
        public float exhaustScale = 1.0f;


        [KSPField(isPersistant=true)]
        public float bladeLengthSlider = 1f;
        [KSPField]
        public float widthMultiplier = 0.5f;
        
        [KSPField(guiName="Blade Length"), UI_FloatRange(minValue=0.3f, maxValue=3f, stepIncrement=0.1f)]
        private float BladeLengthSliderRaw = 1f;
        private float oldBladeLength = 0f;

        private Rect sliderRectBase = new Rect(30f, 30f, 200f, 30f);
        private float lineHeight = 50f;
        private int currentGUILine = 0;
        //private Rect sliderLabelRect;
        private GUIStyle sliderStyle = new GUIStyle();
        [KSPField(guiName = "Blades"), UI_FloatRange(minValue = 2f, maxValue = 12.1f, stepIncrement = 1f)]
        private float bladeNumberRaw = 2f;
        private List<GameObject> blades = new List<GameObject>();

        private int exhaustNumber = 0;
        [KSPField]
        public float engineMaxScale = 5f;
        [KSPField(guiName = "Engine Size"), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.1f)]
        private float engineLengthSliderRaw = 0f;
        private GameObject engineExtension;
        private List<GameObject> exhausts = new List<GameObject>();

        private Transform propellerRoot;
        private Transform movableSection;
        public Transform engineEndPoint;

        private FSengine engine;

        private bool initialized = false;

        private void updateBladeList()
        {            
            if (bladeNumber != blades.Count)
            {
                while (blades.Count < bladeNumber) // should convert that to not a while loop
                {
                    GameObject newBlade = (GameObject)GameObject.Instantiate(blades[0]);
                    blades.Add(newBlade);
                    newBlade.transform.parent = propellerRoot;
                    newBlade.transform.position = blades[0].transform.position;
                    newBlade.transform.localScale = Vector3.one;
                }
                if (blades.Count > bladeNumber)
                {
                    //Debug.Log("too many blades");
                    for (int i = bladeNumber; i < blades.Count; i++)
                    {
                        //Debug.Log("destroying blade " + i);
                        GameObject.Destroy(blades[i]);
                        blades.RemoveAt(i);
                    }
                }

                for (int i = 1; i < blades.Count; i++)
                {
                    blades[i].transform.localRotation = Quaternion.Euler(propellerRotationAxis * (originalBladeRotation + ((360f / bladeNumber) * (i))));
                }
            }
        }

        private void updateBladeLength()
        {
            if (oldBladeLength != bladeLengthSlider)
            {
                for (int i = 0; i < blades.Count; i++)
                {                    
                    foreach (Transform t in blades[i].GetComponentsInChildren<Transform>())
                    {
                        if (t.gameObject.name == bladeName)
                        {                            
                            t.localScale = new Vector3(bladeLengthSlider, 1f + ((bladeLengthSlider - 1f) * widthMultiplier), 1f);
                            Debug.Log("FSpropellerTweak: updating blade length for blade " + i + " scale: " + t.localScale);
                        }
                    }
                }
            }
        }

        private void updateEngineLength()
        {
            if (engineExtension != null)
            {
                engineExtension.transform.localScale = Vector3.one + (engineExtensionAxis * engineLengthSlider * engineMaxScale);
                updateExhaustNumber((int)(engineLengthSlider * exhaustFrequency));

                if (engineEndPoint != null && movableSection != null)
                {
                    movableSection.position = engineEndPoint.position;
                }

                if (engine != null)
                {
                    engine.maxThrust = Mathf.Lerp(minThrust, maxThrust, engineLengthSlider);
                }
            }
        }

        private void updateExhaustNumber(int amount)
        {
            if (amount <= 0)
            {
                exhausts[0].renderer.enabled = false;
            }
            else
            {
                exhausts[0].renderer.enabled = true;
            }

            if (amount != exhausts.Count)
            {

                while (exhausts.Count < amount) // booo!
                {
                    GameObject newExhaust = (GameObject)GameObject.Instantiate(exhausts[0]);
                    exhausts.Add(newExhaust);
                    newExhaust.transform.parent = gameObject.transform;
                    newExhaust.transform.position = exhausts[0].transform.position;
                    newExhaust.transform.localRotation = exhausts[0].transform.localRotation;
                    newExhaust.transform.localScale = Vector3.one * exhaustScale;
                }

                if (exhausts.Count > amount && exhausts.Count > 1)
                {
                    //Debug.Log("too many exhausts");
                    for (int i = amount; i < exhausts.Count; i++)
                    {
                        if (i > 0)
                        {
                            //Debug.Log("destroying exhaust " + i);
                            GameObject.Destroy(exhausts[i]);
                            exhausts.RemoveAt(i);
                        }
                    }
                }

                for (int i = 1; i < exhausts.Count; i++)
                {
                    exhausts[i].transform.localPosition = exhausts[0].transform.localPosition + exhaustTranslateAxis * exhaustDistance * i;
                }
            }
        }

        // Use this for initialization
        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) return;

            Debug.Log("FSpropellerTweak: Onstart");

            bladeNumberRaw = bladeNumber;
            engineLengthSliderRaw = engineLengthSlider;
            BladeLengthSliderRaw = bladeLengthSlider;

            Debug.Log("FSpropellerTweak: find blade root");

            Transform originalBlade = part.FindModelTransform(bladeRootName);
            if (originalBlade != null)
            {
                blades.Add(originalBlade.gameObject);
            }

            Debug.Log("FSpropellerTweak: find exhaust");

            //GameObject originalExhaust = GameObject.Find(exhaustName);
            Transform originalExhaust = part.FindModelTransform(exhaustName);
            if (originalExhaust != null)
            {
                exhausts.Add(originalExhaust.gameObject);
            }

            Debug.Log("FSpropellerTweak: find engine extension");

            Transform engineExtensionTransform = part.FindModelTransform(engineExtenderName);
            if (engineExtensionTransform != null)
                engineExtension = engineExtensionTransform.gameObject;

            Debug.Log("FSpropellerTweak: find propeller root");

            propellerRoot = part.FindModelTransform(propellerRootName);
            if (propellerRoot == null) Debug.Log("FSpropellerTweak: Nasty error, no propeller root found named " + propellerRootName);

            movableSection = part.FindModelTransform(movableSectionName);
            engineEndPoint = part.FindModelTransform(engineEndPointName);

            Debug.Log("FSpropellerTweak: Blades: " + blades.Count);

            updateBladeList();
            updateEngineLength();
            updateBladeLength();

            engine = part.Modules.OfType<FSengine>().FirstOrDefault();

            initialized = true;
        }

        // In hangar
        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor && initialized)
            {
                bladeNumber = Mathf.FloorToInt(bladeNumberRaw);
                exhaustNumber = Mathf.FloorToInt(engineLengthSliderRaw);
                engineLengthSlider = (float)Math.Round(engineLengthSliderRaw, 2);
                bladeLengthSlider = (float)Math.Round(BladeLengthSliderRaw, 2);

                updateBladeList();
                updateEngineLength();
                updateBladeLength();               
            }
        }

        private Rect sliderRect(int current)
        {
            return new Rect(sliderRectBase.x, sliderRectBase.y + lineHeight * currentGUILine, sliderRectBase.width, sliderRectBase.height);
        }

        private Rect nextRect()
        {
            currentGUILine++;
            return sliderRect(currentGUILine - 1);
        }

        private Rect labelRect()
        {
            Rect newRect = sliderRect(currentGUILine);
            return new Rect(newRect.x + newRect.width + 10f, newRect.y, 150f, newRect.height);
        }

        //public void OnGUI()
        //{
        //    currentGUILine = 0;
        //    bladeNumberRaw = GUI.HorizontalSlider(nextRect(), bladeNumberRaw, minBlades, maxBlades + 0.9f);
        //    bladeNumber = Mathf.FloorToInt(bladeNumberRaw);
        //    GUI.Label(labelRect(), bladeNumber.ToString() + " : No of Blades");

        //    engineLengthSliderRaw = GUI.HorizontalSlider(nextRect(), engineLengthSliderRaw, 0f, 4f);
        //    exhaustNumber = Mathf.FloorToInt(engineLengthSliderRaw);
        //    engineLengthSlider = (float)Math.Round(engineLengthSliderRaw, 2);
        //    GUI.Label(labelRect(), engineLengthSlider.ToString() + " : Engine Power");

        //    BladeLengthSliderRaw = GUI.HorizontalSlider(nextRect(), BladeLengthSliderRaw, 0.5f, 3f);
        //    bladeLengthSlider = (float)Math.Round(BladeLengthSliderRaw, 2);
        //    GUI.Label(labelRect(), bladeLengthSlider.ToString() + " : Blade Length");            
        //}
    }
}
