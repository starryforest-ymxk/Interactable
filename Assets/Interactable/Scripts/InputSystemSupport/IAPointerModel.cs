#if ENABLE_INPUT_SYSTEM
using UnityEngine;

namespace Interactable.InputSystemSupport
{
    // A pointer is identified by a single unique integer ID. It has an associated position and the ability to press
    // on up to three buttons. It can also scroll.
    //
    // There's a single ExtendedIAPointerData instance allocated for the pointer which is used to retain the pointer's
    // event state. As part of that state is specific to button presses, each button retains a partial copy of press-specific
    // event information.
    //
    // A pointer can operate in 2D (mouse, pen, touch) or 3D (tracked). For 3D, screen-space 2D positions are derived
    // via raycasts based on world-space position and orientation.
    public struct IAPointerModel
    {
        public bool changedThisFrame;
        public UIPointerType pointerType => iaData.PointerType;

        public Vector2 screenPosition
        {
            get => m_ScreenPosition;
            set
            {
                if (m_ScreenPosition != value)
                {
                    m_ScreenPosition = value;
                    changedThisFrame = true;
                }
            }
        }

        public Vector3 worldPosition
        {
            get => m_WorldPosition;
            set
            {
                if (m_WorldPosition != value)
                {
                    m_WorldPosition = value;
                    changedThisFrame = true;
                }
            }
        }

        public Quaternion worldOrientation
        {
            get => m_WorldOrientation;
            set
            {
                if (m_WorldOrientation != value)
                {
                    m_WorldOrientation = value;
                    changedThisFrame = true;
                }
            }
        }

        public Vector2 scrollDelta
        {
            get => m_ScrollDelta;
            set
            {
                if (m_ScrollDelta != value)
                {
                    changedThisFrame = true;
                    m_ScrollDelta = value;
                }
            }
        }

        public float pressure
        {
            get => m_Pressure;
            set
            {
                if (m_Pressure != value)
                {
                    changedThisFrame = true;
                    m_Pressure = value;
                }
            }
        }

        public float azimuthAngle
        {
            get => m_AzimuthAngle;
            set
            {
                if (m_AzimuthAngle != value)
                {
                    changedThisFrame = true;
                    m_AzimuthAngle = value;
                }
            }
        }

        public float altitudeAngle
        {
            get => m_AltitudeAngle;
            set
            {
                if (m_AltitudeAngle != value)
                {
                    changedThisFrame = true;
                    m_AltitudeAngle = value;
                }
            }
        }

        public float twist
        {
            get => m_Twist;
            set
            {
                if (m_Twist != value)
                {
                    changedThisFrame = true;
                    m_Twist = value;
                }
            }
        }

        public Vector2 radius
        {
            get => m_Radius;
            set
            {
                if (m_Radius != value)
                {
                    changedThisFrame = true;
                    m_Radius = value;
                }
            }
        }

        public ButtonState leftButton;
        public ButtonState rightButton;
        public ButtonState middleButton;
        public ExtendedIAPointerData iaData;

        public IAPointerModel(ExtendedIAPointerData iaData)
        {
            this.iaData = iaData;

            changedThisFrame = false;

            leftButton = default; leftButton.OnEndFrame();
            rightButton = default; rightButton.OnEndFrame();
            middleButton = default; middleButton.OnEndFrame();

            m_ScreenPosition = default;
            m_ScrollDelta = default;
            m_WorldOrientation = default;
            m_WorldPosition = default;

            m_Pressure = default;
            m_AzimuthAngle = default;
            m_AltitudeAngle = default;
            m_Twist = default;
            m_Radius = default;
        }

        public void OnFrameFinished()
        {
            changedThisFrame = false;
            scrollDelta = default;
            leftButton.OnEndFrame();
            rightButton.OnEndFrame();
            middleButton.OnEndFrame();
        }

        private Vector2 m_ScreenPosition;
        private Vector2 m_ScrollDelta;
        private Vector3 m_WorldPosition;
        private Quaternion m_WorldOrientation;

        private float m_Pressure;
        private float m_AzimuthAngle;
        private float m_AltitudeAngle;
        private float m_Twist;
        private Vector2 m_Radius;

        public void CopyTouchOrPenStateFrom(IAPointerData iaData)
        {
#if UNITY_2021_1_OR_NEWER
            pressure = iaData.Pressure;
            azimuthAngle = iaData.AzimuthAngle;
            altitudeAngle = iaData.AltitudeAngle;
            twist = iaData.Twist;
            radius = iaData.Radius;
#endif
        }

        // State related to pressing and releasing individual bodies. Retains those parts of
        // PointerInputEvent that are specific to presses and releases.
        public struct ButtonState
        {
            private bool m_IsPressed;
            private IAPointerData.FramePressState m_FramePressState;
            private float m_PressTime;

            public bool isPressed
            {
                get => m_IsPressed;
                set
                {
                    if (m_IsPressed != value)
                    {
                        m_IsPressed = value;

                        if (m_FramePressState == IAPointerData.FramePressState.NotChanged && value)
                            m_FramePressState = IAPointerData.FramePressState.Pressed;
                        else if (m_FramePressState == IAPointerData.FramePressState.NotChanged && !value)
                            m_FramePressState = IAPointerData.FramePressState.Released;
                        else if (m_FramePressState == IAPointerData.FramePressState.Pressed && !value)
                            m_FramePressState = IAPointerData.FramePressState.PressedAndReleased;
                    }
                }
            }

            /// <summary>
            /// When we "release" a button other than through user interaction (e.g. through focus switching),
            /// we don't want this to count as an actual release that ends up clicking. This flag will cause
            /// generated events to have <c>eligibleForClick</c> to be false.
            /// </summary>
            public bool ignoreNextClick
            {
                get => m_IgnoreNextClick;
                set => m_IgnoreNextClick = value;
            }

            public float pressTime
            {
                get => m_PressTime;
                set => m_PressTime = value;
            }

            public bool clickedOnSameGameObject
            {
                get => m_ClickedOnSameGameObject;
                set => m_ClickedOnSameGameObject = value;
            }

            public bool wasPressedThisFrame => m_FramePressState == IAPointerData.FramePressState.Pressed ||
            m_FramePressState == IAPointerData.FramePressState.PressedAndReleased;
            public bool wasReleasedThisFrame => m_FramePressState == IAPointerData.FramePressState.Released ||
            m_FramePressState == IAPointerData.FramePressState.PressedAndReleased;

            private IARaycastResult m_PressRaycast;
            private GameObject m_PressObject;
            private GameObject m_RawPressObject;
            private GameObject m_LastPressObject;
            private GameObject m_DragObject;
            private Vector2 m_PressPosition;
            private float m_ClickTime; // On Time.unscaledTime timeline, NOT input event time.
            private int m_ClickCount;
            private bool m_Dragging;
            private bool m_ClickedOnSameGameObject;
            private bool m_IgnoreNextClick;

            public void CopyPressStateTo(IAPointerData iaData)
            {
                iaData.PointerPressRaycast = m_PressRaycast;
                iaData.PressPosition = m_PressPosition;
                iaData.ClickCount = m_ClickCount;
                iaData.ClickTime = m_ClickTime;
                // We can't set lastPress directly. Old input module uses three different event instances, one for each
                // button. We share one instance and just switch press states. Set pointerPress twice to get the lastPress
                // we need.
                //
                // NOTE: This does *NOT* quite work as stated in the docs. pointerPress is nulled out on button release which
                //       will set lastPress as a side-effect. This means that lastPress will only be non-null while no press is
                //       going on and will *NOT* refer to the last pressed object when a new object has been pressed on.
                iaData.PointerPress = m_LastPressObject;
                iaData.PointerPress = m_PressObject;
                iaData.RawPointerPress = m_RawPressObject;
                iaData.PointerDrag = m_DragObject;
                iaData.Dragging = m_Dragging;

                if (ignoreNextClick)
                    iaData.EligibleForClick = false;
            }

            public void CopyPressStateFrom(IAPointerData eventData)
            {
                m_PressRaycast = eventData.PointerPressRaycast;
                m_PressObject = eventData.PointerPress;
                m_RawPressObject = eventData.RawPointerPress;
                m_LastPressObject = eventData.LastPress;
                m_PressPosition = eventData.PressPosition;
                m_ClickTime = eventData.ClickTime;
                m_ClickCount = eventData.ClickCount;
                m_DragObject = eventData.PointerDrag;
                m_Dragging = eventData.Dragging;
            }

            public void OnEndFrame()
            {
                m_FramePressState = IAPointerData.FramePressState.NotChanged;
            }
        }
    }
}
#endif
