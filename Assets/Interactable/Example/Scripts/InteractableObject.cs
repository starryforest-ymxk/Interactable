using Interactable;
using UnityEngine;

namespace Interactable.Example
{

    [RequireComponent(typeof(Collider))]
    public abstract class InteractableObject :
        MonoBehaviour,
        IIAPointerEnterHandler,
        IIAPointerExitHandler,
        IIAPointerDownHandler,
        IIAPointerUpHandler
    {
        protected bool pointerOnObject;
        protected bool pointerDown;

        public virtual void OnPointerEnter(IAPointerData pointerData)
        {
            pointerOnObject = true;
            OnInteractionAvailable(true);
        }

        public virtual void OnPointerExit(IAPointerData pointerData)
        {
            pointerOnObject = false;
            if (!pointerDown)
                OnInteractionAvailable(false);
        }

        public virtual void OnPointerDown(IAPointerData pointerData)
        {
            if (pointerData.Button != IAPointerData.InputButton.Left)
                return;

            pointerDown = true;
            OnInteractionAvailable(false);
        }

        public virtual void OnPointerUp(IAPointerData pointerData)
        {
            if (pointerData.Button != IAPointerData.InputButton.Left)
                return;

            pointerDown = false;
            if (pointerOnObject)
                OnInteractionAvailable(true);
        }

        protected abstract void OnInteractionAvailable(bool available);

    }

}
