using UnityEngine;

namespace Interactable
{
    public class IAFirstPersonInputModule : IAPointerInputModule
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private float doubleClickTime = 0.3f;
        [SerializeField] private float dragThresholdMulti = 0.01f;
        
        public string HorizontalAxis
        {
            get => horizontalAxis;
            set => horizontalAxis = value;
        }
        public string VerticalAxis
        {
            get => verticalAxis;
            set => verticalAxis = value;
        }
        
        #region private
        private bool ShouldIgnoreEventsOnNoFocus()
        {
#if UNITY_EDITOR
            return !UnityEditor.EditorApplication.isRemoteConnected;
#else
            return true;
#endif
        }
        private void ReleaseMouse(IAPointerData pointerData, GameObject currentOverGo)
        {
            ExecuteInteraction.Execute(pointerData.PointerPress, pointerData, ExecuteInteraction.PointerUpHandler);

            var pointerClickHandler = ExecuteInteraction.GetEventHandler<IIAPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerData.PointerClick == pointerClickHandler && pointerData.EligibleForClick)
            {
                ExecuteInteraction.Execute(pointerData.PointerClick, pointerData, ExecuteInteraction.PointerClickHandler);
            }
            if (pointerData.PointerDrag != null && pointerData.Dragging)
            {
                ExecuteInteraction.ExecuteHierarchy(currentOverGo, pointerData, ExecuteInteraction.DropHandler);
            }

            pointerData.EligibleForClick = false;
            pointerData.PointerPress = null;
            pointerData.RawPointerPress = null;
            pointerData.PointerClick = null;

            if (pointerData.PointerDrag != null && pointerData.Dragging)
                ExecuteInteraction.Execute(pointerData.PointerDrag, pointerData, ExecuteInteraction.EndDragHandler);

            pointerData.Dragging = false;
            pointerData.PointerDrag = null;

            // redo pointer enter / exit to refresh state
            // so that if we moused over something that ignored it before
            // due to having pressed on something else
            // it now gets it.
            if (currentOverGo != pointerData.PointerEnter)
            {
                HandlePointerMovement(pointerData, null);
                HandlePointerMovement(pointerData, currentOverGo);
            }

            inputPointerData = pointerData;
        }
        
        #endregion

        #region public
        
        private IAPointerData inputPointerData; // only record current dragging object
        
        public override void UpdateModule()
        {
            if (!System.IsFocused && ShouldIgnoreEventsOnNoFocus())
            {
                if (inputPointerData != null && inputPointerData.PointerDrag != null && inputPointerData.Dragging)
                {
                    ReleaseMouse(inputPointerData, inputPointerData.PointerCurrentRaycast.gameObject);
                }

                inputPointerData = null;
            }
            
        }
        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            var shouldActivate = false;
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(HorizontalAxis), 0.0f);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(VerticalAxis), 0.0f);
            shouldActivate |= Input.GetMouseButtonDown(0);

            if (Input.TouchCount > 0)
                shouldActivate = true;

            return shouldActivate;
        }
        public override void ActivateModule()
        {
            if (!System.IsFocused && ShouldIgnoreEventsOnNoFocus())
                return;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            base.ActivateModule();
        }
        public override void InactivateModule()
        {
            base.InactivateModule();
            ClearSelection();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public override void Process()
        {
            if (!System.IsFocused && ShouldIgnoreEventsOnNoFocus())
                return;
            
            if (!ProcessTouchEvents() && Input.MousePresent)
            {
                ProcessMouseEvent();
            }
        }

        #endregion

        #region Process
        protected bool ProcessTouchEvents()
        {
            for (int i = 0; i < Input.TouchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.type == TouchType.Indirect)
                    continue;

                var pointer = GetTouchIAPointerData(touch, out var pressed, out var released);

                ProcessTouchPress(pointer, pressed, released);

                if (!released)
                {
                    ProcessMove(pointer);
                    ProcessDrag(pointer);
                }
                else
                    RemovePointerData(pointer);
            }
            return Input.TouchCount > 0;
        }
        protected void ProcessTouchPress(IAPointerData pointerData, bool pressed, bool released)
        {
            var currentOverGo = pointerData.PointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerData.EligibleForClick = true;
                pointerData.Delta = Vector2.zero;
                pointerData.Dragging = false;
                pointerData.UseDragThreshold = true;
                pointerData.PressPosition = pointerData.Position;
                pointerData.PointerPressRaycast = pointerData.PointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerData);

                if (pointerData.PointerEnter != currentOverGo)
                {
                    // send a pointer enter to the touched element if it isn't the one to select...
                    HandlePointerMovement(pointerData, currentOverGo);
                    pointerData.PointerEnter = currentOverGo;
                }

                var resetDiffTime = Time.unscaledTime - pointerData.ClickTime;
                
                if (resetDiffTime >= doubleClickTime)
                {
                    pointerData.ClickCount = 0;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press handler to be what would receive a click.
                var newPressed = ExecuteInteraction.ExecuteHierarchy(currentOverGo, pointerData, ExecuteInteraction.PointerDownHandler);
                
                var newClick = ExecuteInteraction.GetEventHandler<IIAPointerClickHandler>(currentOverGo);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = newClick;
                
                var time = Time.unscaledTime;
                
                if (newPressed == pointerData.LastPress)
                {
                    var diffTime = time - pointerData.ClickTime;
                    if (diffTime < doubleClickTime)
                        ++pointerData.ClickCount;
                    else
                        pointerData.ClickCount = 1;

                    pointerData.ClickTime = time;
                }
                else
                {
                    pointerData.ClickCount = 1;
                }

                pointerData.PointerPress = newPressed;
                pointerData.RawPointerPress = currentOverGo;
                pointerData.PointerClick = newClick;

                pointerData.ClickTime = time;

                // Save the drag handler as well
                pointerData.PointerDrag = ExecuteInteraction.GetEventHandler<IIADragHandler>(currentOverGo);

                if (pointerData.PointerDrag != null)
                    ExecuteInteraction.Execute(pointerData.PointerDrag, pointerData, ExecuteInteraction.InitializePotentialDragHandler);
            }

            // PointerUp notification
            if (released)
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteInteraction.Execute(pointerData.PointerPress, pointerData, ExecuteInteraction.PointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerClickHandler = ExecuteInteraction.GetEventHandler<IIAPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerData.PointerClick == pointerClickHandler && pointerData.EligibleForClick)
                {
                    ExecuteInteraction.Execute(pointerData.PointerClick, pointerData, ExecuteInteraction.PointerClickHandler);
                }

                if (pointerData.PointerDrag != null && pointerData.Dragging)
                {
                    ExecuteInteraction.ExecuteHierarchy(currentOverGo, pointerData, ExecuteInteraction.DropHandler);
                }

                pointerData.EligibleForClick = false;
                pointerData.PointerPress = null;
                pointerData.RawPointerPress = null;
                pointerData.PointerClick = null;

                if (pointerData.PointerDrag != null && pointerData.Dragging)
                    ExecuteInteraction.Execute(pointerData.PointerDrag, pointerData, ExecuteInteraction.EndDragHandler);

                pointerData.Dragging = false;
                pointerData.PointerDrag = null;

                // send exit events as we need to simulate this on touch up on touch device
                ExecuteInteraction.ExecuteHierarchy(pointerData.PointerEnter, pointerData, ExecuteInteraction.PointerExitHandler);
                pointerData.PointerEnter = null;
            }

            inputPointerData = pointerData;
        }
        protected void ProcessMouseEvent(int id = 0)
        {
            var mouseData = GetMouseState(id);
            var leftButtonData = mouseData.GetButtonState(IAPointerData.InputButton.Left).ButtonData;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.pointerData);
            ProcessDrag(leftButtonData.pointerData);

            // Now process right / middle clicks
            var rightButtonData = mouseData.GetButtonState(IAPointerData.InputButton.Right).ButtonData;
            ProcessMousePress(rightButtonData);
            ProcessDrag(rightButtonData.pointerData);

            var middleButtonData = mouseData.GetButtonState(IAPointerData.InputButton.Middle).ButtonData;
            ProcessMousePress(middleButtonData);
            ProcessDrag(middleButtonData.pointerData);

            if (!Mathf.Approximately(leftButtonData.pointerData.ScrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteInteraction.GetEventHandler<IIAScrollHandler>(leftButtonData.pointerData.PointerCurrentRaycast.gameObject);
                ExecuteInteraction.ExecuteHierarchy(scrollHandler, leftButtonData.pointerData, ExecuteInteraction.ScrollHandler);
            }
        }
        protected void ProcessMousePress(MouseButtonData data)
        {
            var pointerData = data.pointerData;
            var currentOverGo = pointerData.PointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (data.PressedThisFrame())
            {
                pointerData.EligibleForClick = true;
                pointerData.Delta = Vector2.zero;
                pointerData.Dragging = false;
                pointerData.UseDragThreshold = true;
                pointerData.PressPosition = pointerData.Position;
                pointerData.PointerPressRaycast = pointerData.PointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerData);

                var resetDiffTime = Time.unscaledTime - pointerData.ClickTime;
                if (resetDiffTime >= doubleClickTime)
                {
                    pointerData.ClickCount = 0;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteInteraction.ExecuteHierarchy(currentOverGo, pointerData, ExecuteInteraction.PointerDownHandler);
                var newClick = ExecuteInteraction.GetEventHandler<IIAPointerClickHandler>(currentOverGo);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = newClick;

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerData.LastPress)
                {
                    var diffTime = time - pointerData.ClickTime;
                    if (diffTime < doubleClickTime)
                        ++pointerData.ClickCount;
                    else
                        pointerData.ClickCount = 1;

                    pointerData.ClickTime = time;
                }
                else
                {
                    pointerData.ClickCount = 1;
                }

                pointerData.PointerPress = newPressed;
                pointerData.RawPointerPress = currentOverGo;
                pointerData.PointerClick = newClick;
                pointerData.ClickTime = time;

                // Save the drag handler as well
                pointerData.PointerDrag = ExecuteInteraction.GetEventHandler<IIADragHandler>(currentOverGo);

                if (pointerData.PointerDrag != null)
                    ExecuteInteraction.Execute(pointerData.PointerDrag, pointerData, ExecuteInteraction.InitializePotentialDragHandler);

                inputPointerData = pointerData;
            }

            // PointerUp notification
            if (data.ReleasedThisFrame())
            {
                ReleaseMouse(pointerData, currentOverGo);
            }
        }
        protected override MouseState GetMouseState(int id)
        {
            // Populate the left button...
            var created = GetPointerData(KMouseLeftId, out var leftData, true);
            leftData.Reset();

            if (created)
                leftData.Position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            leftData.Delta = Vector2.zero;
            leftData.Position = new Vector2(Screen.width / 2f, Screen.height / 2f);
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
        
        protected override void ProcessMove(IAPointerData pointerData)
        {
            var go = pointerData.PointerCurrentRaycast.gameObject;
            HandlePointerMovement(pointerData, go);
        }
        protected override void ProcessDrag(IAPointerData pointerData)
        {
            if (pointerData.PointerDrag == null)
                return;
            
            if (!pointerData.Dragging
                && ShouldStartDrag(pointerData.PointerPressRaycast.worldPosition, pointerData.PointerCurrentRaycast.worldPosition, 
                    System.PixelDragThreshold*dragThresholdMulti, pointerData.UseDragThreshold))
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

        protected static bool ShouldStartDrag(Vector3 pressHitPos, Vector3 currentHitPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressHitPos - currentHitPos).sqrMagnitude >= threshold * threshold;
        }

        #endregion
        
    }
}