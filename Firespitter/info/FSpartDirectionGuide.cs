using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Transform guidePointArrowLineLeft;
        Transform guidePointArrowLineRight;

        private GameObject worldDirection;

        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            worldDirection = new GameObject();

            //parseGuideDirectionString();
            guideLineTexCorrect = createTexture(correctColor);
            guideLineTexWrong = createTexture(wrongColor);
            createLineRenderer();

            guidePointArrowLineLeft = new GameObject().transform;
            guidePointArrowLineRight = new GameObject().transform;
        }

        private void createLineRenderer()
        {
            guideLine = part.gameObject.GetComponent<LineRenderer>();
            if (guideLine == null)
                guideLine = part.gameObject.AddComponent<LineRenderer>();
            guideLine.SetWidth(0.02f, 0.02f);
            guideLine.material = new Material(Shader.Find("Unlit/Texture"));
            guideLine.material.SetTexture("_MainTex", guideLineTexCorrect);
            guideLine.SetVertexCount(5);
            guideLine.useWorldSpace = true;
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
            guideLine.SetPosition(2, guidePointArrowLineLeft.position);
            guideLine.SetPosition(3, guidePointForward);
            guideLine.SetPosition(4, guidePointArrowLineRight.position);
        }

        private void setupArrowLines(Vector3 arrowHeadRear, Vector3 direction)
        {
            guidePointArrowLineLeft.position = arrowHeadRear;
            guidePointArrowLineLeft.Translate(-direction * lineLength * 0.2f);

            guidePointArrowLineRight.position = arrowHeadRear;
            guidePointArrowLineRight.Translate(direction * lineLength * 0.2f);            
        }

        private void updateVisibility()
        {
            if (part.parent != null)
            {
                visible = visibleWhenAttached;
                guideLine.enabled = visible;
            }
            else
            {
                visible = visibleWhenNotAttached;
            }
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
