using UnityEngine;

namespace Interactable
{
    public class IAFreeMouseInputModule : IAPointerInputModule
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private string submitButton = "Submit";
        [SerializeField] private string cancelButton = "Cancel";
        
        [SerializeField] private float doubleClickTime = 0.3f;
        [SerializeField] private float repeatDelay = 0.5f;
        [SerializeField] private float inputActionsPerSecond = 10;
        
        public float InputActionsPerSecond
        {
            get => inputActionsPerSecond;
            set => inputActionsPerSecond = value;
        }
        public float RepeatDelay
        {
            get => repeatDelay;
            set => repeatDelay = value;
        }
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
        public string SubmitButton
        {
            get => submitButton;
            set => submitButton = value;
        }
        public string CancelButton
        {
            get => cancelButton;
            set => cancelButton = value;
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
        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = Input.GetAxisRaw(HorizontalAxis);
            move.y = Input.GetAxisRaw(VerticalAxis);

            if (Input.GetButtonDown(HorizontalAxis))
            {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            if (Input.GetButtonDown(VerticalAxis))
            {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            return move;
        }
        
        #endregion

        #region public
        
        private IAPointerData inputPointerData; // only record current dragging object
        private Vector2 mousePosition;
        private Vector2 lastMousePosition;
        
        public override void UpdateModule()
        {
            if (!System.IsFocused && ShouldIgnoreEventsOnNoFocus())
            {
                if (inputPointerData != null && inputPointerData.PointerDrag != null && inputPointerData.Dragging)
                {
                    ReleaseMouse(inputPointerData, inputPointerData.PointerCurrentRaycast.gameObject);
                }

                inputPointerData = null;
                return;
            }

            lastMousePosition = mousePosition;
            mousePosition = Input.MousePosition;
        }
        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            var shouldActivate = false;
            shouldActivate |= Input.GetButtonDown(SubmitButton);
            shouldActivate |= Input.GetButtonDown(CancelButton);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(HorizontalAxis), 0.0f);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(VerticalAxis), 0.0f);
            shouldActivate |= (mousePosition - lastMousePosition).sqrMagnitude > 0.0f;
            shouldActivate |= Input.GetMouseButtonDown(0);

            if (Input.TouchCount > 0)
                shouldActivate = true;

            return shouldActivate;
        }
        public override void ActivateModule()
        {
            if (!System.IsFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            base.ActivateModule();
            
            mousePosition = Input.MousePosition;
            lastMousePosition = Input.MousePosition;
        }
        public override void InactivateModule()
        {
            base.InactivateModule();
            ClearSelection();
        }
        public override void Process()
        {
            if (!System.IsFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            // case 1004066 - touch / mouse events should be processed before navigation events in case
            // they change the current selected gameobject and the submit button is a touch / mouse button.

            // touch needs to take precedence because of the mouse emulation layer
            if (!ProcessTouchEvents() && Input.MousePresent)
            {
                ProcessMouseEvent();
            }
            
            
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (System.SendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
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
        protected override void ProcessDrag(IAPointerData pointerData)
        {
            if (!pointerData.IsPointerMoving() || Cursor.lockState == CursorLockMode.Locked || pointerData.PointerDrag == null)
                return;

            base.ProcessDrag(pointerData);
        }

        #endregion

        #region Send Events
        
        private float prevActionTime;
        private Vector2 lastMoveVector;
        private int consecutiveMoveCount;
        
        protected bool SendSubmitEventToSelectedObject()
        {
            if (System.CurrentSelected == null)
                return false;

            var data = GetIABaseData();
            if (Input.GetButtonDown(SubmitButton))
                ExecuteInteraction.Execute(System.CurrentSelected, data, ExecuteInteraction.SubmitHandler);

            if (Input.GetButtonDown(CancelButton))
                ExecuteInteraction.Execute(System.CurrentSelected, data, ExecuteInteraction.CancelHandler);
            return data.Used;
        }
        protected bool SendMoveEventToSelectedObject()
        {
            float time = Time.unscaledTime;
            Vector2 movement = GetRawMoveVector();
            
            if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
            {
                consecutiveMoveCount = 0;
                return false;
            }

            var similarDir = (Vector2.Dot(movement, lastMoveVector) > 0);

            // If direction didn't change at least 90 degrees, wait for delay before allowing consecutive event.
            if (similarDir && consecutiveMoveCount == 1)
            {
                if (time <= prevActionTime +repeatDelay)
                    return false;
            }
            // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
            else
            {
                if (time <= prevActionTime + 1f / inputActionsPerSecond)
                    return false;
            }

            var axisEventData = GetIAAxisData(movement.x, movement.y, 0.6f);

            if (axisEventData.MoveDir != IAMoveDirection.None)
            {
                ExecuteInteraction.Execute(System.CurrentSelected, axisEventData, ExecuteInteraction.MoveHandler);
                if (!similarDir)
                    consecutiveMoveCount = 0;
                consecutiveMoveCount++;
                prevActionTime = time;
                lastMoveVector = movement;
            }
            else
            {
                consecutiveMoveCount = 0;
            }

            return axisEventData.Used;
        }
        protected bool SendUpdateEventToSelectedObject()
        {
            if (System.CurrentSelected == null)
                return false;

            var data = GetIABaseData();
            ExecuteInteraction.Execute(System.CurrentSelected, data, ExecuteInteraction.UpdateSelectedHandler);
            return data.Used;
        }

        #endregion


        
        
    }
}