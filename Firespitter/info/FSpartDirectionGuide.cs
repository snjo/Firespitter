using UnityEngine;

namespace Firespitter.info
{
    public class FSpartDirectionGuide : PartModule
    {
        [KSPField]
        public Vector3 guideDirection = Vector3.forward;
        [KSPField]
        public Vector3 correctWorldDirection = Vector3.forward;
        [KSPField]
        public bool visibleWhenAttached = false;
        [KSPField]
        public bool visibleWhenNotAttached = true;
        [KSPField]
        public float lineLength = 5f;
        [KSPField]
        public string guideText = "This way forward";
        [KSPField]
        public Vector4 correctColor = new Vector4(0f, 1f, 0f, 1f);
        [KSPField]
        public Vector4 wrongColor = new Vector4(0f, 1f, 0f, 1f);

        private bool visible = false;

        //private enum TransformDirection
        //{
        //    forward,
        //    back,
        //    up,
        //    down,
        //    right,
        //    left
        //}

        //private TransformDirection transformDirection = TransformDirection.forward;

        LineRenderer guideLine;
        Texture2D guideLineTexCorrect;
        Texture2D guideLineTexWrong;

        Vector3 centerPoint;
        Vector3 guidePointForward;

        Vector3 guidePointArrowLineLeft = Vector3.zero;
        Vector3 guidePointArrowLineRight = Vector3.zero;
        //GameObject guidePointArrowLineLeft;
        //GameObject guidePointArrowLineRight;

        private GameObject worldDirection;

        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            worldDirection = new GameObject();

            //parseGuideDirectionString();
            guideLineTexCorrect = createTexture(correctColor);
            guideLineTexWrong = createTexture(wrongColor);
            createLineRenderer();

            //guidePointArrowLineLeft = new GameObject();
            //guidePointArrowLineRight = new GameObject();
            //guidePointArrowLineLeft.transform.parent = part.transform;
            //guidePointArrowLineRight.transform.parent = part.transform;
        }

        private void createLineRenderer()
        {
            guideLine = part.gameObject.GetComponent<LineRenderer>();
            if (guideLine != null)
            {
                Debug.Log("destroying existing linerenderer " + part.GetInstanceID());
                DestroyImmediate(guideLine);
            }
            Debug.Log("creating linerenderer" + part.GetInstanceID());
            guideLine = part.gameObject.AddComponent<LineRenderer>();

            Debug.Log("setting line properties" + part.GetInstanceID());
            guideLine.SetWidth(0.02f, 0.02f);
            guideLine.material = new Material(Shader.Find("Unlit/Texture"));
            guideLine.material.SetTexture("_MainTex", guideLineTexCorrect);
            guideLine.SetVertexCount(5);
            guideLine.useWorldSpace = true;
            Debug.Log("create line done" + part.GetInstanceID());
        }

        private void Update()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            updateVisibility();
            if (visible)
            {
                updateLinePosition();
                updateLineColor();
            }
        }

        private void updateLineColor()
        {
            if (Vector3.Dot(part.transform.TransformDirection(guideDirection), worldDirection.transform.TransformDirection(correctWorldDirection)) > 0.5f)
            {
                guideLine.material.SetTexture("_MainTex", guideLineTexCorrect);
            }
            else
            {
                guideLine.material.SetTexture("_MainTex", guideLineTexWrong);
            }
        }

        private void updateLinePosition()
        {
            centerPoint = part.transform.position;
            guidePointForward = part.transform.position + part.transform.TransformDirection(guideDirection) * lineLength;
            Vector3 arrowHeadRear = Vector3.Lerp(centerPoint, guidePointForward, 0.8f);

            setupArrowLines(arrowHeadRear, part.transform.right);                
            
            guideLine.SetPosition(0, centerPoint);
            guideLine.SetPosition(1, guidePointForward);
            guideLine.SetPosition(2, guidePointArrowLineLeft);
            guideLine.SetPosition(3, guidePointForward);
            guideLine.SetPosition(4, guidePointArrowLineRight);
        }

        private void setupArrowLines(Vector3 arrowHeadRear, Vector3 direction)
        {
            guidePointArrowLineLeft = arrowHeadRear;
            guidePointArrowLineLeft += -direction * lineLength * 0.2f;

            guidePointArrowLineRight = arrowHeadRear;
            guidePointArrowLineRight += direction * lineLength * 0.2f;
        }

        private void updateVisibility()
        {
            if (part.parent != null)
            {
                visible = visibleWhenAttached;
                //if (guideLine != null)
                //    guideLine.enabled = visible;
            }
            else
            {
                visible = visibleWhenNotAttached;
            }
            if (guideLine != null)
                guideLine.enabled = visible;
        }

        private void drawText(Vector3 worldPosition, string value)
        {
            Vector3 labelPos = Camera.main.WorldToScreenPoint(worldPosition);
            GUI.Label(new Rect(labelPos.x, Screen.height - labelPos.y - 15f, 100f, 100f), value);

        }

        public void OnGUI()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            if (visible)
            {
                drawText(guidePointForward, guideText);
            }
        }

        private Texture2D createTexture(Vector4 color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(1, 1, new Color(color.x, color.y, color.z, color.w));
            tex.Apply();
            return tex;
        }
    }
}
