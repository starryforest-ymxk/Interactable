#if ENABLE_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactable.InputSystemSupport
{
    public class IAInputSystemFirstPersonInputModule : IAInputSystemInputModule
    {
        [SerializeField] private float dragThresholdMulti = 0.01f;
        
        public override void ActivateModule()
        {
            if (!System.IsFocused && shouldIgnoreFocus)
                return;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            base.ActivateModule();
        }
        public override void InactivateModule()
        {
            base.InactivateModule();
            ResetPointers();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public override void Process()
        {
            if (m_NeedToPurgeStalePointers)
                PurgeStalePointers();

            // Reset devices of changes since we don't want to spool up changes once we gain focus.
            if (!System.IsFocused && !shouldIgnoreFocus)
            {
                for (var i = 0; i < m_PointerStates.length; ++i)
                    m_PointerStates[i].OnFrameFinished();
            }
            else
            {

                FilterPointerStatesByType();

                // Pointer input.
                for (var i = 0; i < m_PointerStates.length; i++)
                {
                    ref var state = ref GetPointerStateForIndex(i);

                    ProcessPointer(ref state);

                    // If it's a touch and the touch has ended, release the pointer state.
                    // NOTE: We defer this by one frame such that OnPointerUp happens in the frame of release
                    //       and OnPointerExit happens one frame later. This is so that IsPointerOverGameObject()
                    //       stays true for the touch in the frame of release (see UI_TouchPointersAreKeptForOneFrameAfterRelease).
                    if (state.pointerType == UIPointerType.Touch && !state.leftButton.isPressed && !state.leftButton.wasReleasedThisFrame)
                    {
                        RemovePointerAtIndex(i);
                        --i;
                        continue;
                    }

                    state.OnFrameFinished();
                }
            }
        }
        
        protected override void ProcessPointerButtonDrag(ref IAPointerModel.ButtonState button, ExtendedIAPointerData iaData)
        {
            if (iaData.PointerDrag == null)
                return;

            // Detect drags.
            if (!iaData.Dragging)
            {
                bool shouldStartDrag = ShouldStartDrag(iaData.PointerPressRaycast.worldPosition,
                    iaData.PointerCurrentRaycast.worldPosition,
                    System.PixelDragThreshold * dragThresholdMulti, iaData.UseDragThreshold);


                if (shouldStartDrag)
                {
                    // Started dragging. Invoke OnBeginDrag.
                    ExecuteInteraction.Execute(iaData.PointerDrag, iaData, ExecuteInteraction.BeginDragHandler);
                    iaData.Dragging = true;
                }
            }

            if (iaData.Dragging)
            {
                // If we moved from our initial press object, process an up for that object.
                if (iaData.PointerPress != iaData.PointerDrag)
                {
                    ExecuteInteraction.Execute(iaData.PointerPress, iaData, ExecuteInteraction.PointerUpHandler);

                    iaData.EligibleForClick = false;
                    iaData.PointerPress = null;
                    iaData.RawPointerPress = null;
                }

                ExecuteInteraction.Execute(iaData.PointerDrag, iaData, ExecuteInteraction.DragHandler);
                button.CopyPressStateFrom(iaData);
            }
        }
        
        protected static bool ShouldStartDrag(Vector3 pressHitPos, Vector3 currentHitPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressHitPos - currentHitPos).sqrMagnitude >= threshold * threshold;
        }
    }
}

#endif
