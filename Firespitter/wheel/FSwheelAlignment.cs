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

        Vector3 anglePointForwardHorizontal;
        Vector3 anglePointForwardVertical;
        Vector3 anglePointBackHorizontal;
        Vector3 anglePointBackVertical;
        Vector3 anglePointUpX;
        Vector3 anglePointUpZ;

        Vector3 centerPoint;
        Vector3 guidePointForward;
        Vector3 guidePointBack;
        Vector3 guidePointUp;

        Vector3 wheelPointForward;
        Vector3 wheelPointBack;
        Vector3 wheelPointUp;

        float forwardAngleHorizontal;
        float forwardAngleVertical;
        float upAngleX;
        float upAngleZ;

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
            if (!HighLogic.LoadedSceneIsEditor) return;

            findWheel();

            createTextures();

            Debug.Log("create guideLine");
            guideLine = part.gameObject.GetComponent<LineRenderer>() ?? part.gameObject.AddComponent<LineRenderer>();
            guideLine.SetWidth(0.02f, 0.02f);
			// TODO: A way to ressurrect lost Shaders.
			guideLine.material = new Material(Shader.Find("Unlit/Texture") ?? Shader.Find("Standard")); 
			guideLine.material.SetTexture("_MainTex", guideLineTex);
            guideLine.SetVertexCount(14);

            Debug.Log("create wheelLine");
            wheelLine = wheel.GetComponent<LineRenderer>() ?? wheel.AddComponent<LineRenderer>();
            wheelLine.SetWidth(0.02f, 0.02f);
			wheelLine.material = new Material(Shader.Find("Unlit/Texture") ?? Shader.Find("Standard"));
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
            else
            {
                wheel = new GameObject();
                wheel.transform.parent = part.gameObject.transform;
                wheel.transform.localRotation = Quaternion.identity;
                wheel.transform.localPosition = Vector3.zero;
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
                centerPoint = wheel.transform.position;
                guidePointForward = wheel.transform.position + Vector3.forward.normalized * lineLength;
                guidePointBack = wheel.transform.position - Vector3.forward.normalized * lineLength;
                guidePointUp = wheel.transform.position + Vector3.up.normalized * lineLength;

                wheelPointForward = wheel.transform.position + wheel.transform.forward.normalized * lineLength;
                wheelPointBack = wheel.transform.position - wheel.transform.forward.normalized * lineLength;
                wheelPointUp = wheel.transform.position + wheel.transform.up.normalized * lineLength;

                if (Vector3.Distance(wheelPointForward, guidePointForward) < Vector3.Distance(wheelPointForward, guidePointBack))
                {
                    anglePointForwardHorizontal = new Vector3(wheelPointForward.x, guidePointBack.y, guidePointForward.z);
                    anglePointForwardVertical = new Vector3(guidePointBack.x, wheelPointForward.y, guidePointForward.z);
                    anglePointBackHorizontal = new Vector3(wheelPointBack.x, guidePointForward.y, guidePointBack.z);
                    anglePointBackVertical = new Vector3(guidePointForward.x, wheelPointBack.y, guidePointBack.z);

                }
                else
                {
                    anglePointForwardHorizontal = new Vector3(wheelPointBack.x, guidePointBack.y, guidePointForward.z);
                    anglePointForwardVertical = new Vector3(guidePointBack.x, wheelPointBack.y, guidePointForward.z);
                    anglePointBackHorizontal = new Vector3(wheelPointForward.x, guidePointForward.y, guidePointBack.z);
                    anglePointBackVertical = new Vector3(guidePointForward.x, wheelPointForward.y, guidePointBack.z);
                }
                anglePointUpX = new Vector3(wheelPointUp.x, guidePointUp.y, guidePointUp.z);
                anglePointUpZ = new Vector3(guidePointUp.x, guidePointUp.y, wheelPointUp.z);

                //Debug.Log("guideLine " + part.GetInstanceID());

                guideLine.SetPosition(0, anglePointUpX);
                guideLine.SetPosition(1, guidePointUp);
                guideLine.SetPosition(2, anglePointUpZ);
                guideLine.SetPosition(3, guidePointUp);
                guideLine.SetPosition(4, centerPoint);
                guideLine.SetPosition(5, guidePointForward);
                guideLine.SetPosition(6, anglePointForwardHorizontal);
                guideLine.SetPosition(7, guidePointForward);
                guideLine.SetPosition(8, anglePointForwardVertical);
                guideLine.SetPosition(9, guidePointForward);
                guideLine.SetPosition(10, guidePointBack);
                guideLine.SetPosition(11, anglePointBackHorizontal);
                guideLine.SetPosition(12, guidePointBack);
                guideLine.SetPosition(13, anglePointBackVertical);

                //Debug.Log("wheelLine " + part.GetInstanceID());

                wheelLine.SetPosition(0, wheelPointUp);
                wheelLine.SetPosition(1, centerPoint);
                wheelLine.SetPosition(2, wheelPointForward);
                wheelLine.SetPosition(3, wheelPointBack);

                Vector3 forwardLineHorizontal = new Vector3(anglePointForwardHorizontal.x, centerPoint.y, anglePointForwardHorizontal.z) - centerPoint;
                forwardAngleHorizontal = Vector3.Angle(Vector3.forward, forwardLineHorizontal);
                Vector3 forwardLineVertical = new Vector3(centerPoint.x, anglePointForwardVertical.y, anglePointForwardVertical.z) - centerPoint;
                forwardAngleVertical = Vector3.Angle(Vector3.forward, forwardLineVertical);

                Vector3 upLineX = new Vector3(anglePointUpX.x, anglePointUpX.y, centerPoint.z) - centerPoint;
                upAngleX = Vector3.Angle(Vector3.up, upLineX);
                Vector3 upLineZ = new Vector3(centerPoint.x, anglePointUpZ.y, anglePointUpZ.z) - centerPoint;
                upAngleZ = Vector3.Angle(Vector3.up, upLineZ);

            }
        }

        private float clampAngle(float angle)
        {
            if (angle > 180f) angle = 180f - angle;
            if (angle > 90f) angle = 180f - angle;
            return angle;
        }

        private void drawAngleText(Vector3 worldPosition, float value)
        {
            Vector3 labelPos = Camera.main.WorldToScreenPoint(worldPosition);
            GUI.Label(new Rect(labelPos.x, Screen.height - labelPos.y - 15f, 100f, 100f), ((int)(clampAngle(value))).ToString());

        }

        public void OnGUI()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            if (showGuides)
            {
                drawAngleText(anglePointForwardHorizontal, forwardAngleHorizontal);
                drawAngleText(anglePointForwardVertical, forwardAngleVertical);
                drawAngleText(anglePointBackHorizontal, forwardAngleHorizontal);
                drawAngleText(anglePointBackVertical, forwardAngleVertical);
                drawAngleText(anglePointUpX, upAngleX);
                drawAngleText(anglePointUpZ, upAngleZ);
            }
        }
    }
}
