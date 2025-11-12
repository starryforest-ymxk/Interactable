using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    [RequireComponent(typeof(IASystem))]
    public abstract class IABaseInputModule : MonoBehaviour
    {
        [NonSerialized] 
        protected readonly List<IARaycastResult> raycastResultCache = new();
        
        [SerializeField] 
        private bool sendPointerHoverToParent = true;
        
        internal bool SendPointerHoverToParent 
        { 
            get => sendPointerHoverToParent;
            set => sendPointerHoverToParent = value; 
        }
        
        private IAManager m_manager;
        
        protected IAManager Manager
        {
            get
            {
                if (m_manager == null)
                {
                    m_manager = IAManager.GetInstance();
                }

                return m_manager;
            }
        }
        
        private IASystem m_system;
        protected IASystem System
        {
            get
            {
                if (m_system == null)
                {
                    m_system = GetComponent<IASystem>();
                }

                if (m_system == null)
                {
                    Debug.LogError("Current Interaction System is null.");
                }

                return m_system;
            }
        }
        
        private IABaseInput m_input;
        public IABaseInput Input
        {
            get
            {
                if (m_input == null)
                {
                    var baseInput = GetComponent<IABaseInput>();
                    if (baseInput != null)
                    {
                        m_input = baseInput;
                    }

                    if (m_input == null)
                        m_input = gameObject.AddComponent<IABaseInput>();
                }

                return m_input;
            }
        }
        
        #region MonoBehaviour lifecycle
        
#if UNITY_EDITOR
        protected virtual void Reset() { }
#endif
        
        protected virtual void Awake(){}
        
        protected virtual void OnDestroy(){}
        
        protected virtual void OnEnable()
        {
            System.UpdateModules();
        }
        protected virtual void OnDisable()
        {
            System.UpdateModules();
        }
        
        #endregion
        
        #region static

        protected static IARaycastResult FindFirstRaycast(List<IARaycastResult> candidates)
        {
            var candidatesCount = candidates.Count;
            for (var i = 0; i < candidatesCount; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }
            return new IARaycastResult();
        }
        protected static IAMoveDirection DetermineMoveDirection(float x, float y, float deadZone = 0.6f)
        {
            // if vector is too small... just return
            if (new Vector2(x, y).sqrMagnitude < deadZone * deadZone)
                return IAMoveDirection.None;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                return x > 0 ? IAMoveDirection.Right : IAMoveDirection.Left;
            }

            return y > 0 ? IAMoveDirection.Up : IAMoveDirection.Down;
        }
        protected static GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
                return null;

            var t1 = g1.transform;
            while (t1 != null)
            {
                var t2 = g2.transform;
                while (t2 != null)
                {
                    if (t1 == t2)
                        return t1.gameObject;
                    t2 = t2.parent;
                }
                t1 = t1.parent;
            }
            return null;
        }
        
        #endregion
        
        #region protected
        
        private IAAxisData axisData;
        private IABaseData baseData;
        
        protected virtual IAAxisData GetIAAxisData(float x, float y, float moveDeadZone)
        {
            axisData ??= new IAAxisData(System);
            axisData.Reset();
            axisData.MoveVector = new Vector2(x, y);
            axisData.MoveDir = DetermineMoveDirection(x, y, moveDeadZone);
            return axisData;
        }
        protected virtual IABaseData GetIABaseData()
        {
            baseData ??= new IABaseData(System);
            baseData.Reset();
            return baseData;
        }
        
        // walk up the tree till a common root between the last entered and the current entered is found
        // send exit events up to (but not including) the common root. Then send enter events up to
        // (but not including) the common root.
        // Send move events before exit, after enter, and on hovered objects when pointer data has changed.
        protected void HandlePointerMovement(IAPointerData iaData, GameObject newEnterTarget)
        {
            
            // If the pointer moved, send move events to all UI elements the pointer is
            // currently over.
            var wasMoved = iaData.IsPointerMoving();
            if (wasMoved)
            {
                for (var i = 0; i < iaData.Hovered.Count; ++i)
                    ExecuteInteraction.Execute(iaData.Hovered[i], iaData, ExecuteInteraction.PointerMoveHandler);
            }
            
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || iaData.PointerEnter == null)
            {
                var hoveredCount = iaData.Hovered.Count;
                for (var i = 0; i < hoveredCount; ++i)
                {
                    iaData.FullyExited = true;
                    ExecuteInteraction.Execute(iaData.Hovered[i], iaData, ExecuteInteraction.PointerExitHandler);
                }

                iaData.Hovered.Clear();

                if (newEnterTarget == null)
                {
                    iaData.PointerEnter = null;
                    return;
                }
            }
            
            if (iaData.PointerEnter == newEnterTarget && newEnterTarget)
                return;

            GameObject commonRoot = FindCommonRoot(iaData.PointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (iaData.PointerEnter != null)
            {
                Transform t = iaData.PointerEnter.transform;

                while (t != null)
                {
                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    var o = t.gameObject;
                    iaData.FullyExited = o != commonRoot && iaData.PointerEnter != newEnterTarget;
                    
                    ExecuteInteraction.Execute(o, iaData, ExecuteInteraction.PointerExitHandler);
                    iaData.Hovered.Remove(o);

                    if (!sendPointerHoverToParent)
                        break;
                    
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            var oldPointerEnter = iaData.PointerEnter;
            iaData.PointerEnter = newEnterTarget;
            
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null)
                {
                    //只有第一次循环的时候才会触发这段代码，也就是说只有进入循环时o == commonRoot，即newEnterTarget=commonRoot才算作Reentered
                    var o = t.gameObject;
                    iaData.Reentered = o == commonRoot && o != oldPointerEnter;
                   
                    if (iaData.Reentered)
                        break;

                    ExecuteInteraction.Execute(o, iaData, ExecuteInteraction.PointerEnterHandler);
                    if (wasMoved)
                        ExecuteInteraction.Execute(o, iaData, ExecuteInteraction.PointerMoveHandler);

                    iaData.Hovered.Add(o);
                    
                    if (!sendPointerHoverToParent)
                        break;

                    t = t.parent;

                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;
                }
            }
        }
        
        
        #endregion

        #region public
        
        public virtual bool IsPointerOverGameObject(int pointerId)
        {
            return false;
        }
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }
        public virtual bool ShouldActivateModule()
        {
            return enabled && gameObject.activeInHierarchy;
        }
        public virtual void ActivateModule()
        {}
        public virtual void InactivateModule()
        {}
        public virtual void UpdateModule()
        {}
        public virtual bool IsModuleSupported()
        {
            return true;
        }
        public abstract void Process();
        
        /// <summary>
        /// Returns Id of the pointer following <see cref="UnityEngine.UIElements.PointerId"/> convention.
        /// </summary>
        /// <param name="sourcePointerData">PointerEventData whose pointerId will be converted to UI Toolkit pointer convention.</param>
        /// <seealso cref="UnityEngine.UIElements.IPointerEvent" />
        public virtual int ConvertUIToolkitPointerId(IAPointerData sourcePointerData)
        {
#if PACKAGE_UITOOLKIT
            return sourcePointerData.PointerId < 0 ?
                PointerId.mousePointerId :
                PointerId.touchPointerIdBase + sourcePointerData.PointerId;
#else
            return -1;
#endif
        }
        
        #endregion
    }
}