using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Interactable
{
    public abstract class IAPointerInputModule : IABaseInputModule
    {
        // id >= 0 留给触摸输入， 鼠标使用 id < 0
        public const int KMouseLeftId = -1;
        public const int KMouseRightId = -2;
        public const int KMouseMiddleId = -3;
        public const int KFakeTouchesId = -4;

        protected readonly Dictionary<int, IAPointerData> m_pointerDataDic = new();
        protected readonly MouseState m_mouseState = new();
        
        #region protected
        
        #region data
        protected bool GetPointerData(int id, out IAPointerData data, bool create)
        {
            if (!m_pointerDataDic.TryGetValue(id, out data) && create)
            {
                data = new IAPointerData(System)
                {
                    PointerId = id,
                };
                m_pointerDataDic.Add(id, data);
                return true;
            }
            return false;
        }
        protected void RemovePointerData(IAPointerData data)
        {
            m_pointerDataDic.Remove(data.PointerId);
        }
        protected IAPointerData GetTouchIAPointerData(Touch input, out bool pressed, out bool released)
        {
            var created = GetPointerData(input.fingerId, out var pointerData, true);

            pointerData.Reset();

            pressed = created || (input.phase == TouchPhase.Began);
            released = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);

            if (created)
                pointerData.Position = input.position;

            if (pressed)
                pointerData.Delta = Vector2.zero;
            else
                pointerData.Delta = input.position - pointerData.Position;

            pointerData.Position = input.position;
            pointerData.Button = IAPointerData.InputButton.Left;
            
            if (input.phase == TouchPhase.Canceled)
            {
                pointerData.PointerCurrentRaycast = new IARaycastResult();
            }
            else
            {
                //投射射线并对结果排序
                System.RaycastAll(pointerData, raycastResultCache);

                var raycast = FindFirstRaycast(raycastResultCache);
                pointerData.PointerCurrentRaycast = raycast;
                raycastResultCache.Clear();
            }

            pointerData.Pressure = input.pressure;
            pointerData.AltitudeAngle = input.altitudeAngle;
            pointerData.AzimuthAngle = input.azimuthAngle;
            pointerData.Radius = Vector2.one * input.radius;
            pointerData.RadiusVariance = Vector2.one * input.radiusVariance;

            return pointerData;
        }
        protected void CopyFromTo(IAPointerData from, IAPointerData to)
        {
            to.Position = from.Position;
            to.Delta = from.Delta;
            to.ScrollDelta = from.ScrollDelta;
            to.PointerCurrentRaycast = from.PointerCurrentRaycast;
            to.PointerEnter = from.PointerEnter;

            to.Pressure = from.Pressure;
            to.TangentialPressure = from.TangentialPressure;
            to.AltitudeAngle = from.AltitudeAngle;
            to.AzimuthAngle = from.AzimuthAngle;
            to.Twist = from.Twist;
            to.Radius = from.Radius;
            to.RadiusVariance = from.RadiusVariance;
        }
        
        #endregion

        #region mouse
        protected IAPointerData.FramePressState StateForMouseButton(int buttonId)
        {
            var pressed = Input.GetMouseButtonDown(buttonId);
            var released = Input.GetMouseButtonUp(buttonId);
            if (pressed && released)
                return IAPointerData.FramePressState.PressedAndReleased;
            if (pressed)
                return IAPointerData.FramePressState.Pressed;
            if (released)
                return IAPointerData.FramePressState.Released;
            return IAPointerData.FramePressState.NotChanged;
        }
        
        protected virtual MouseState GetMouseState()
        {
            return GetMouseState(0);
        }
        protected virtual MouseState GetMouseState(int id)
        {
            // Populate the left button...
            var created = GetPointerData(KMouseLeftId, out var leftData, true);
            leftData.Reset();

            if (created)
                leftData.Position = Input.MousePosition;

            Vector2 pos = Input.MousePosition;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // We don't want to do ANY cursor-based interaction when the mouse is locked
                leftData.Position = new Vector2(-1.0f, -1.0f);
                leftData.Delta = Vector2.zero;
            }
            else
            {
                leftData.Delta = pos - leftData.Position;
                leftData.Position = pos;
            }
            leftData.ScrollDelta = Input.MouseScrollDelta;
            
            System.RaycastAll(leftData, raycastResultCache);
            var raycast = FindFirstRaycast(raycastResultCache);
            leftData.PointerCurrentRaycast = raycast;
            raycastResultCache.Clear();

            leftData.Button = IAPointerData.InputButton.Left;
            
            // copy the appropriate data into right and middle slots
            GetPointerData(KMouseRightId, out var rightData, true);
            rightData.Reset();

            CopyFromTo(leftData, rightData);
            rightData.Button = IAPointerData.InputButton.Right;

            GetPointerData(KMouseMiddleId, out var middleData, true);
            middleData.Reset();

            CopyFromTo(leftData, middleData);
            middleData.Button = IAPointerData.InputButton.Middle;

            m_mouseState.SetButtonState(IAPointerData.InputButton.Left, StateForMouseButton(0), leftData);
            m_mouseState.SetButtonState(IAPointerData.InputButton.Right, StateForMouseButton(1), rightData);
            m_mouseState.SetButtonState(IAPointerData.InputButton.Middle, StateForMouseButton(2), middleData);

            return m_mouseState;
        }
        protected IAPointerData GetLastIAPointerData(int id)
        {
            GetPointerData(id, out var data, false);
            return data;
        }
        
        #endregion

        #region selection
        protected void ClearSelection()
        {
            var baseEventData = GetIABaseData();
            foreach (var pointer in m_pointerDataDic.Values)
            {
                HandlePointerMovement(pointer, null);
            }
            m_pointerDataDic.Clear();
            System.SetSelectedGameObject(null, baseEventData);
        }
        protected void DeselectIfSelectionChanged(GameObject currentOverGo, IABaseData pointerData)
        {
            var selectHandlerGo = ExecuteInteraction.GetEventHandler<IIASelectHandler>(currentOverGo);
            if (selectHandlerGo != System.CurrentSelected)
                System.SetSelectedGameObject(null, pointerData);
        }

        #endregion

        #region process
        protected virtual void ProcessMove(IAPointerData pointerData)
        {
            var go = (Cursor.lockState == CursorLockMode.Locked ? null : pointerData.PointerCurrentRaycast.gameObject);
            HandlePointerMovement(pointerData, go);
        }
        protected virtual void ProcessDrag(IAPointerData pointerData)
        {
            if (!pointerData.Dragging
                && ShouldStartDrag(pointerData.PressPosition, pointerData.Position, System.PixelDragThreshold, pointerData.UseDragThreshold))
            {
                ExecuteInteraction.Execute(pointerData.PointerDrag, pointerData, ExecuteInteraction.BeginDragHandler);
                pointerData.Dragging = true;
            }

            // Drag notification
            if (pointerData.Dragging)
            {
                // Before doing drag we should cancel any pointer down state and clear selection!
                if (pointerData.PointerPress != pointerData.PointerDrag)
                {
                    ExecuteInteraction.Execute(pointerData.PointerPress, pointerData, ExecuteInteraction.PointerUpHandler);

                    pointerData.EligibleForClick = false;
                    pointerData.PointerPress = null;
                    pointerData.RawPointerPress = null;
                }
                ExecuteInteraction.Execute(pointerData.PointerDrag, pointerData, ExecuteInteraction.DragHandler);
            }
        }

        #endregion
        
        protected static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }
        
        #endregion
        
        #region public
        
        public override bool IsPointerOverGameObject(int pointerId)
        {
            var lastPointer = GetLastIAPointerData(pointerId);
            if (lastPointer != null)
                return lastPointer.PointerEnter != null;
            return false;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("<b>Pointer Input Module of type: </b>" + GetType());
            sb.AppendLine();
            foreach (var pointer in m_pointerDataDic)
            {
                if (pointer.Value == null)
                    continue;
                sb.AppendLine("<B>Pointer:</b> " + pointer.Key);
                sb.AppendLine(pointer.Value.ToString());
            }
            return sb.ToString();
        }
        
        #endregion
        
        #region subclass

        protected class ButtonState
        {
            private MouseButtonData buttonButtonData;
            public MouseButtonData ButtonData
            {
                get => buttonButtonData;
                set => buttonButtonData = value;
            }

            private IAPointerData.InputButton inputButton = IAPointerData.InputButton.Left;
            public IAPointerData.InputButton InputButton
            {
                get => inputButton;
                set => inputButton = value;
            }
        }
        protected class MouseState
        {
            private readonly List<ButtonState> trackedButtons = new();
            public bool AnyPressesThisFrame()
            {
                var trackedButtonsCount = trackedButtons.Count;
                for (int i = 0; i < trackedButtonsCount; i++)
                {
                    if (trackedButtons[i].ButtonData.PressedThisFrame())
                        return true;
                }
                return false;
            }
            public bool AnyReleasesThisFrame()
            {
                var trackedButtonsCount = trackedButtons.Count;
                for (int i = 0; i < trackedButtonsCount; i++)
                {
                    if (trackedButtons[i].ButtonData.ReleasedThisFrame())
                        return true;
                }
                return false;
            }
            public ButtonState GetButtonState(IAPointerData.InputButton button)
            {
                ButtonState tracked = null;
                var trackedButtonsCount = trackedButtons.Count;
                for (int i = 0; i < trackedButtonsCount; i++)
                {
                    if (trackedButtons[i].InputButton == button)
                    {
                        tracked = trackedButtons[i];
                        break;
                    }
                }

                if (tracked == null)
                {
                    tracked = new ButtonState { InputButton = button, ButtonData = new MouseButtonData() };
                    trackedButtons.Add(tracked);
                }
                return tracked;
            }
            public void SetButtonState(IAPointerData.InputButton button, IAPointerData.FramePressState stateForMouseButton, IAPointerData data)
            {
                var toModify = GetButtonState(button);
                toModify.ButtonData.buttonState = stateForMouseButton;
                toModify.ButtonData.pointerData = data;
            }
        }
        public class MouseButtonData
        {
            public IAPointerData.FramePressState buttonState;
            public IAPointerData pointerData;
            public bool PressedThisFrame()
            {
                return buttonState is IAPointerData.FramePressState.Pressed or IAPointerData.FramePressState.PressedAndReleased;
            }
            public bool ReleasedThisFrame()
            {
                return buttonState is IAPointerData.FramePressState.Released or IAPointerData.FramePressState.PressedAndReleased;
            }
        }

        #endregion
    }
}