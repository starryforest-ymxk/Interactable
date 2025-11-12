using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class IAPointerData : IABaseData
    {
        public enum InputButton
        {
            Left = 0,
            Right = 1,
            Middle = 2
        }
        public enum FramePressState
        {
            Pressed,
            Released,
            PressedAndReleased,
            NotChanged
        }
        public GameObject PointerEnter { get; set; }
        
        private GameObject m_pointerPress;
        
        /// <summary>
        /// 具有Handler的物体
        /// </summary>
        public GameObject PointerPress
        {
            get => m_pointerPress;
            set
            {
                if (m_pointerPress == value)
                    return;

                LastPress = m_pointerPress;
                m_pointerPress = value;
            }
        }
        public GameObject LastPress { get; private set; }
        
        /// <summary>
        /// 原本射线投射到物体
        /// </summary>
        public GameObject RawPointerPress { get; set; }
        public GameObject PointerDrag { get; set; }
        public GameObject PointerClick { get; set; }
        public IARaycastResult PointerCurrentRaycast { get; set; }
        public IARaycastResult PointerPressRaycast { get; set; }
        
        public readonly List<GameObject> Hovered = new();
        public bool EligibleForClick { get; set; }
        public int DisplayIndex { get; set; }
        public int PointerId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Delta { get; set; }
        public Vector2 PressPosition { get; set; }
        public float ClickTime { get; set; }
        public int ClickCount { get; set; }
        public Vector2 ScrollDelta { get; set; }
        public bool UseDragThreshold { get; set; }
        public bool Dragging { get; set; }
        public InputButton Button { get; set; }
        public float Pressure { get; set; }
        public float TangentialPressure { get; set; }
        public float AltitudeAngle { get; set; }
        public float AzimuthAngle { get; set; }
        public float Twist { get; set; }
        public Vector2 Tilt { get; set; }
        public PenStatus PenStatus { get; set; }
        public Vector2 Radius { get; set; }
        public Vector2 RadiusVariance { get; set; }
        public bool FullyExited { get; set; }
        public bool Reentered { get; set; }

        public IAPointerData(IASystem system) : base(system)
        {
            EligibleForClick = false;

            DisplayIndex = 0;
            PointerId = -1;
            Position = Vector2.zero;
            Delta = Vector2.zero;
            PressPosition = Vector2.zero;
            ClickTime = 0.0f;
            ClickCount = 0;

            ScrollDelta = Vector2.zero;
            UseDragThreshold = true;
            Dragging = false;
            Button = InputButton.Left;

            Pressure = 0f;
            TangentialPressure = 0f;
            AltitudeAngle = 0f;
            AzimuthAngle = 0f;
            Twist = 0f;
            Tilt = new Vector2(0f, 0f);
            PenStatus = PenStatus.None;
            Radius = Vector2.zero;
            RadiusVariance = Vector2.zero;
        }
        
        public bool IsPointerMoving()
        {
            return Delta.sqrMagnitude > 0.0f;
        }
        
        public bool IsScrolling()
        {
            return ScrollDelta.sqrMagnitude > 0.0f;
        }

        public Camera EnterCamera => PointerCurrentRaycast.raycaster == null ? null : PointerCurrentRaycast.raycaster.RayCamera;

        public Camera PressCamera => PointerPressRaycast.raycaster == null ? null : PointerPressRaycast.raycaster.RayCamera;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Position</b>: " + Position);
            sb.AppendLine("<b>delta</b>: " + Delta);
            sb.AppendLine("<b>eligibleForClick</b>: " + EligibleForClick);
            sb.AppendLine("<b>pointerEnter</b>: " + PointerEnter);
            sb.AppendLine("<b>pointerPress</b>: " + PointerPress);
            sb.AppendLine("<b>lastPointerPress</b>: " + LastPress);
            sb.AppendLine("<b>pointerDrag</b>: " + PointerDrag);
            sb.AppendLine("<b>Use Drag Threshold</b>: " + UseDragThreshold);
            sb.AppendLine("<b>Current Raycast:</b>");
            sb.AppendLine(PointerCurrentRaycast.ToString());
            sb.AppendLine("<b>Press Raycast:</b>");
            sb.AppendLine(PointerPressRaycast.ToString());
            sb.AppendLine("<b>Display Index:</b>");
            sb.AppendLine(DisplayIndex.ToString());
            sb.AppendLine("<b>pressure</b>: " + Pressure);
            sb.AppendLine("<b>tangentialPressure</b>: " + TangentialPressure);
            sb.AppendLine("<b>altitudeAngle</b>: " + AltitudeAngle);
            sb.AppendLine("<b>azimuthAngle</b>: " + AzimuthAngle);
            sb.AppendLine("<b>twist</b>: " + Twist);
            sb.AppendLine("<b>tilt</b>: " + Tilt);
            sb.AppendLine("<b>penStatus</b>: " + PenStatus);
            sb.AppendLine("<b>radius</b>: " + Radius);
            sb.AppendLine("<b>radiusVariance</b>: " + RadiusVariance);
            return sb.ToString();
        }
    }
}
