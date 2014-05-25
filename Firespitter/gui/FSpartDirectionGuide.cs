using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.gui
{
    public class FSpartDirectionGuide : PartModule
    {
        [KSPField]
        public string guideDirection = "forward";
        [KSPField]
        public bool visibleWhenAttached = false;
        [KSPField]
        public bool visibleWhenNotAttached = true;
        [KSPField]
        public float lineLength = 5f;
        [KSPField]
        public string guideText = "This way forward";
        [KSPField]
        public Vector4 colour = new Vector4(0f, 1f, 0f, 1f);

        private bool visible = false;

        private enum TransformDirection
        {
            forward,
            back,
            up,
            down,
            right,
            left
        }

        private TransformDirection transformDirection = TransformDirection.forward;

        LineRenderer guideLine;
        Texture2D guideLineTex;

        Vector3 centerPoint;
        Vector3 guidePointForward;
        Transform guidePointArrowLineLeft;
        Transform guidePointArrowLineRight;

        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            parseGuideDirectionString();
            createTexture();
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
            guideLine.material.SetTexture("_MainTex", guideLineTex);
            guideLine.SetVertexCount(5);
            guideLine.useWorldSpace = true;
        }

        private void parseGuideDirectionString()
        {
            switch (guideDirection)
            {
                case "forward":
                    transformDirection = TransformDirection.forward;
                    break;
                case "back":
                    transformDirection = TransformDirection.back;
                    break;
                case "up":
                    transformDirection = TransformDirection.up;
                    break;
                case "down":
                    transformDirection = TransformDirection.down;
                    break;
                case "right":
                    transformDirection = TransformDirection.right;
                    break;
                case "left":
                    transformDirection = TransformDirection.left;
                    break;
            }
        }

        private void Update()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;

            //part.attached and .connected seem simmilar, but I'll try attached.

            updateVisibility();
            if (visible)
                updateLinePosition();
        }

        private Vector3 getArrowForward()
        {
            switch (transformDirection)
            {
                case TransformDirection.forward:
                    return part.transform.forward.normalized;
                case TransformDirection.back:
                    return -part.transform.forward.normalized;
                case TransformDirection.up:
                    return part.transform.up.normalized;
                case TransformDirection.down:
                    return -part.transform.up.normalized;
                case TransformDirection.right:
                    return part.transform.right.normalized;
                case TransformDirection.left:
                    return -part.transform.right.normalized;
                default:
                    return part.transform.forward.normalized;
            }
        }

        private void updateLinePosition()
        {
            centerPoint = part.transform.position;
            guidePointForward = part.transform.position + getArrowForward() * lineLength;
            Vector3 arrowHeadRear = Vector3.Lerp(centerPoint, guidePointForward, 0.8f);

            switch (transformDirection)
            {
                case TransformDirection.right:
                    setupArrowLines(arrowHeadRear, part.transform.forward);
                    break;
                case TransformDirection.left:
                    setupArrowLines(arrowHeadRear, -part.transform.forward);
                    break;
                default:
                    setupArrowLines(arrowHeadRear, part.transform.right);
                    break;
            }
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

        private void createTexture()
        {
            guideLineTex = new Texture2D(1, 1);            
            guideLineTex.SetPixel(1, 1, new Color(colour.x, colour.y, colour.z, colour.w));            
            guideLineTex.Apply();
        }
    }
}
