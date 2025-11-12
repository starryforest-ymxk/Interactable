using UnityEngine;


namespace Interactable.Example
{
    [RequireComponent(typeof(Collider))]
    public class InteractableDebug : MonoBehaviour, 
        IIAPointerEnterHandler,
        IIAPointerExitHandler,
        IIAPointerDownHandler,
        IIAPointerUpHandler,
        IIAPointerClickHandler,
        IIAInitializePotentialDragHandler,
        IIABeginDragHandler,
        IIADragHandler,
        IIAEndDragHandler,
        IIADropHandler,
        IIAScrollHandler,
        IIAUpdateSelectedHandler,
        IIASelectHandler,
        IIADeselectHandler,
        IIAMoveHandler,
        IIASubmitHandler,
        IIACancelHandler
    { 
        [SerializeField] private bool debug;


        public void OnPointerEnter(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Pointer Enter");
        }

        public void OnPointerExit(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Pointer Exit");
        }

        public void OnPointerDown(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Pointer Down");
        }

        public void OnPointerUp(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Pointer Up");
        }

        public void OnPointerClick(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Pointer Click");
        }

        public void OnInitializePotentialDrag(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Initialize Potential Drag");
        }

        public void OnBeginDrag(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Begin Drag");
        }

        public void OnDrag(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Drag");
        }

        public void OnEndDrag(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On End Drag");
        }

        public void OnDrop(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Drop");
        }

        public void OnScroll(IAPointerData pointerData)
        {
                if (!debug)
                        return;
                Debug.Log("On Scroll");
        }

        public void OnUpdateSelected(IABaseData baseData)
        {
                if (!debug)
                        return;
                Debug.Log("On Update Selected");
        }

        public void OnSelect(IABaseData baseData)
        {
                if (!debug)
                        return;
                Debug.Log("On Select");
        }

        public void OnDeselect(IABaseData baseData)
        {
                if (!debug)
                        return;
                Debug.Log("On Deselect");
        }

        public void OnMove(IAAxisData axisData)
        {
                if (!debug)
                        return;
                Debug.Log("On Move");
        }

        public void OnSubmit(IABaseData baseData)
        {
                if (!debug)
                        return;
                Debug.Log("On Submit");
        }

        public void OnCancel(IABaseData baseData)
        {
                if (!debug)
                        return;
                Debug.Log("On Cancel");
        }
    }

}
