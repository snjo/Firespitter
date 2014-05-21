using System;
using UnityEngine;

namespace Firespitter.wheel
{
    public class FSwheelAlignment : PartModule
    {
        public bool showGuides = false;
        private GameObject wheel;
        LineRenderer guideLine;
        LineRenderer wheelLine;
        Texture2D guideLineTex;
        Texture2D wheelLineTex;
        int textureSize = 1;
        float lineLength = 5f;

        [KSPField]
        public bool showToggle = true;

        [KSPEvent(guiActiveEditor = true, guiName = "Alignment guide")]
        public void toggleAlignmentGuide()
        {
            showGuides = !showGuides;
            setGuideVisibility(showGuides);
        }

        public override void OnStart(PartModule.StartState state)
        {
            findWheel();

            createTextures();

            Debug.Log("create guideLine");

            guideLine = part.gameObject.GetComponent<LineRenderer>();
            if (guideLine == null)
                guideLine = part.gameObject.AddComponent<LineRenderer>();
            guideLine.SetWidth(0.02f, 0.02f);
            guideLine.material = new Material(Shader.Find("Unlit/Texture"));
            guideLine.material.SetTexture("_MainTex", guideLineTex);
            guideLine.SetVertexCount(4);

            Debug.Log("create wheelLine");

            wheelLine = wheel.GetComponent<LineRenderer>();
            if (wheelLine == null)
                wheelLine = wheel.AddComponent<LineRenderer>();
            wheelLine.SetWidth(0.02f, 0.02f);
            wheelLine.material = new Material(Shader.Find("Unlit/Texture"));
            wheelLine.material.SetTexture("_MainTex", wheelLineTex);
            wheelLine.SetVertexCount(4);

            guideLine.useWorldSpace = true;
            wheelLine.useWorldSpace = true;            
        }

        private void findWheel()
        {
            WheelCollider firstWheelCollider = part.GetComponentInChildren<WheelCollider>();
            if (firstWheelCollider != null)
            {
                wheel = firstWheelCollider.gameObject;
            }
        }

        private void createTextures()
        {
            guideLineTex = new Texture2D(textureSize, textureSize);
            for (int i = 0; i < textureSize; i++)
            {
                guideLineTex.SetPixel(i, i, Color.red);
            }
            guideLineTex.Apply();

            wheelLineTex = new Texture2D(textureSize, textureSize);
            for (int i = 0; i < textureSize; i++)
            {
                wheelLineTex.SetPixel(i, i, Color.green);
            }
            wheelLineTex.Apply();
        }

        public void setGuideVisibility(bool newState)
        {
            wheelLine.enabled = newState;
            guideLine.enabled = newState;
        }

        void Update()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                showGuides = !showGuides;
                setGuideVisibility(showGuides);
            }
            if (showGuides)
            {
                updateLinePositions();
            }
        }

        private void updateLinePositions()
        {
            if (wheel != null)
            {
                guideLine.SetPosition(0, wheel.transform.position + Vector3.up.normalized * lineLength);
                guideLine.SetPosition(1, wheel.transform.position);
                guideLine.SetPosition(2, wheel.transform.position + Vector3.forward.normalized * lineLength);
                guideLine.SetPosition(3, wheel.transform.position - Vector3.forward.normalized * lineLength);

                wheelLine.SetPosition(0, wheel.transform.position + wheel.transform.up.normalized * lineLength);
                wheelLine.SetPosition(1, wheel.transform.position);
                wheelLine.SetPosition(2, wheel.transform.position + wheel.transform.forward.normalized * lineLength);
                wheelLine.SetPosition(3, wheel.transform.position - wheel.transform.forward.normalized * lineLength);
            }
        }
    }
}
