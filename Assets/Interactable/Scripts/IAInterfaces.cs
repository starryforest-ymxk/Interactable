namespace Interactable
{
    public interface IIAHandler { }
    
    public interface IIAPointerMoveHandler : IIAHandler
    {
        void OnPointerMove(IAPointerData pointerData);
    }
    
    public interface IIAPointerEnterHandler : IIAHandler
    {
        void OnPointerEnter(IAPointerData pointerData);
    }
    
    public interface IIAPointerExitHandler : IIAHandler
    {
        void OnPointerExit(IAPointerData pointerData);
    }
    
    public interface IIAPointerDownHandler : IIAHandler
    {
        void OnPointerDown(IAPointerData pointerData);
    }
    
    public interface IIAPointerUpHandler : IIAHandler
    {
        void OnPointerUp(IAPointerData pointerData);
    }
    
    public interface IIAPointerClickHandler : IIAHandler
    {
        void OnPointerClick(IAPointerData pointerData);
    }
    
    public interface IIAInitializePotentialDragHandler : IIAHandler
    {
        void OnInitializePotentialDrag(IAPointerData pointerData);
    }    
    
    public interface IIABeginDragHandler : IIAHandler
    {
        void OnBeginDrag(IAPointerData pointerData);
    }
    
    public interface IIADragHandler : IIAHandler
    {
        void OnDrag(IAPointerData pointerData);
    }
    
    public interface IIAEndDragHandler : IIAHandler
    { 
        void OnEndDrag(IAPointerData pointerData);
    }
    
    public interface IIADropHandler : IIAHandler
    { 
        void OnDrop(IAPointerData pointerData);
    }
    
    public interface IIAScrollHandler : IIAHandler
    {
        void OnScroll(IAPointerData pointerData);
    }

    public interface IIAUpdateSelectedHandler : IIAHandler
    {
        void OnUpdateSelected(IABaseData baseData);
    }
    
    public interface IIASelectHandler : IIAHandler
    {
        void OnSelect(IABaseData baseData);
    }
    
    public interface IIADeselectHandler : IIAHandler
    {
        void OnDeselect(IABaseData baseData);
    }
    
    public interface IIAMoveHandler : IIAHandler
    {
        void OnMove(IAAxisData axisData);
    }


    public interface IIASubmitHandler : IIAHandler
    {
        void OnSubmit(IABaseData baseData);
    }


    public interface IIACancelHandler : IIAHandler
    {
        void OnCancel(IABaseData baseData);
    }
    
}
