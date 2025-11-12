using UnityEngine;

namespace Interactable
{
    public class IABaseInput: MonoBehaviour
    {
        public virtual string CompositionString => Input.compositionString;
        
        public virtual IMECompositionMode IMECompositionMode
        {
            get => Input.imeCompositionMode;
            set => Input.imeCompositionMode = value;
        }
        
        public virtual Vector2 CompositionCursorPos
        {
            get => Input.compositionCursorPos;
            set => Input.compositionCursorPos = value;
        }
        
        public virtual bool MousePresent => Input.mousePresent;

        public virtual bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }
        
        public virtual bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }
        
        public virtual bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }
        
        public virtual Vector2 MousePosition => Input.mousePosition;
        
        public virtual Vector2 MouseScrollDelta => Input.mouseScrollDelta;
        
        public virtual bool TouchSupported => Input.touchSupported;
        
        public virtual int TouchCount => Input.touchCount;
        
        public virtual Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }
        
        public virtual float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }
        
        public virtual bool GetButtonDown(string buttonName)
        {
            return Input.GetButtonDown(buttonName);
        }
    }
}