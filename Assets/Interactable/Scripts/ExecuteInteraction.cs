using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Interactable
{
    public static class ExecuteInteraction
    {
        public delegate void IAFunction<in T>(T handler, IABaseData data);

        public static T ValidateIAData<T>(IABaseData data) where T : class
        {
            if ((data as T) == null)
                throw new ArgumentException($"Invalid type: {data.GetType()} passed to event expecting {typeof(T)}");
            return data as T;
        }
        
        private static readonly IAFunction<IIAPointerMoveHandler> pointerMoveHandler = Execute;
        
        private static void Execute(IIAPointerMoveHandler handler, IABaseData eventData)
        {
            handler.OnPointerMove(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAPointerEnterHandler> pointerEnterHandler = Execute;

        private static void Execute(IIAPointerEnterHandler handler, IABaseData eventData)
        {
            handler.OnPointerEnter(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAPointerExitHandler> pointerExitHandler = Execute;

        private static void Execute(IIAPointerExitHandler handler, IABaseData eventData)
        {
            handler.OnPointerExit(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAPointerDownHandler> pointerDownHandler = Execute;

        private static void Execute(IIAPointerDownHandler handler, IABaseData eventData)
        {
            handler.OnPointerDown(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAPointerUpHandler> pointerUpHandler = Execute;

        private static void Execute(IIAPointerUpHandler handler, IABaseData eventData)
        {
            handler.OnPointerUp(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAPointerClickHandler> pointerClickHandler = Execute;

        private static void Execute(IIAPointerClickHandler handler, IABaseData eventData)
        {
            handler.OnPointerClick(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAInitializePotentialDragHandler> initializePotentialDragHandler = Execute;

        private static void Execute(IIAInitializePotentialDragHandler handler, IABaseData eventData)
        {
            handler.OnInitializePotentialDrag(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIABeginDragHandler> beginDragHandler = Execute;

        private static void Execute(IIABeginDragHandler handler, IABaseData eventData)
        {
            handler.OnBeginDrag(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIADragHandler> dragHandler = Execute;

        private static void Execute(IIADragHandler handler, IABaseData eventData)
        {
            handler.OnDrag(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAEndDragHandler> endDragHandler = Execute;

        private static void Execute(IIAEndDragHandler handler, IABaseData eventData)
        {
            handler.OnEndDrag(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIADropHandler> dropHandler = Execute;

        private static void Execute(IIADropHandler handler, IABaseData eventData)
        {
            handler.OnDrop(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAScrollHandler> scrollHandler = Execute;

        private static void Execute(IIAScrollHandler handler, IABaseData eventData)
        {
            handler.OnScroll(ValidateIAData<IAPointerData>(eventData));
        }

        private static readonly IAFunction<IIAUpdateSelectedHandler> updateSelectedHandler = Execute;

        private static void Execute(IIAUpdateSelectedHandler handler, IABaseData eventData)
        {
            handler.OnUpdateSelected(eventData);
        }

        private static readonly IAFunction<IIASelectHandler> selectHandler = Execute;

        private static void Execute(IIASelectHandler handler, IABaseData eventData)
        {
            handler.OnSelect(eventData);
        }

        private static readonly IAFunction<IIADeselectHandler> deselectHandler = Execute;

        private static void Execute(IIADeselectHandler handler, IABaseData eventData)
        {
            handler.OnDeselect(eventData);
        }

        private static readonly IAFunction<IIAMoveHandler> moveHandler = Execute;

        private static void Execute(IIAMoveHandler handler, IABaseData eventData)
        {
            handler.OnMove(ValidateIAData<IAAxisData>(eventData));
        }

        private static readonly IAFunction<IIASubmitHandler> submitHandler = Execute;

        private static void Execute(IIASubmitHandler handler, IABaseData eventData)
        {
            handler.OnSubmit(eventData);
        }

        private static readonly IAFunction<IIACancelHandler> cancelHandler = Execute;

        private static void Execute(IIACancelHandler handler, IABaseData eventData)
        {
            handler.OnCancel(eventData);
        }
        
        public static IAFunction<IIAPointerMoveHandler> PointerMoveHandler => pointerMoveHandler;
        public static IAFunction<IIAPointerEnterHandler> PointerEnterHandler => pointerEnterHandler;
        public static IAFunction<IIAPointerExitHandler> PointerExitHandler => pointerExitHandler;
        public static IAFunction<IIAPointerDownHandler> PointerDownHandler => pointerDownHandler;
        public static IAFunction<IIAPointerUpHandler> PointerUpHandler => pointerUpHandler;
        public static IAFunction<IIAPointerClickHandler> PointerClickHandler => pointerClickHandler;
        public static IAFunction<IIAInitializePotentialDragHandler> InitializePotentialDragHandler =>
            initializePotentialDragHandler;
        public static IAFunction<IIABeginDragHandler> BeginDragHandler => beginDragHandler;
        public static IAFunction<IIADragHandler> DragHandler => dragHandler;
        public static IAFunction<IIAEndDragHandler> EndDragHandler => endDragHandler;
        public static IAFunction<IIADropHandler> DropHandler => dropHandler;
        public static IAFunction<IIAScrollHandler> ScrollHandler => scrollHandler;
        public static IAFunction<IIAUpdateSelectedHandler> UpdateSelectedHandler => updateSelectedHandler;
        public static IAFunction<IIASelectHandler> SelectHandler => selectHandler;
        public static IAFunction<IIADeselectHandler> DeselectHandler => deselectHandler;
        public static IAFunction<IIAMoveHandler> MoveHandler => moveHandler;
        public static IAFunction<IIASubmitHandler> SubmitHandler => submitHandler;
        public static IAFunction<IIACancelHandler> CancelHandler => cancelHandler;
        
        
        private static readonly List<Transform> internalTransformList = new(30);
        private static void GetEventChain(GameObject root, IList<Transform> eventChain)
        {
            eventChain.Clear();
            if (root == null)
                return;

            var t = root.transform;
            while (t != null)
            {
                eventChain.Add(t);
                t = t.parent;
            }
        }
        private static bool ShouldSendToComponent<T>(Component component) where T : IIAHandler
        {
            if (component is not T)
                return false;

            var behaviour = component as Behaviour;
            return behaviour == null || behaviour.isActiveAndEnabled;
        }
        private static void GetEventList<T>(GameObject go, IList<IIAHandler> results) where T : IIAHandler
        {
            if (results == null)
                throw new ArgumentException("Results array is null", nameof(results));

            if (go == null || !go.activeInHierarchy)
                return;

            var components = ListPool<Component>.Get();
            go.GetComponents(components);

            var componentsCount = components.Count;
            for (var i = 0; i < componentsCount; i++)
            {
                if (!ShouldSendToComponent<T>(components[i]))
                    continue;
                
                results.Add(components[i] as IIAHandler);
            }
            ListPool<Component>.Release(components);
        }
        public static bool CanHandleEvent<T>(GameObject go) where T : IIAHandler
        {
            var internalHandlers = ListPool<IIAHandler>.Get();
            GetEventList<T>(go, internalHandlers);
            var handlerCount = internalHandlers.Count;
            ListPool<IIAHandler>.Release(internalHandlers);
            return handlerCount != 0;
        }
        public static GameObject GetEventHandler<T>(GameObject root) where T : IIAHandler
        {
            if (root == null)
                return null;

            Transform t = root.transform;
            while (t != null)
            {
                if (CanHandleEvent<T>(t.gameObject))
                    return t.gameObject;
                t = t.parent;
            }
            return null;
        }
        public static bool Execute<T>(GameObject target, IABaseData eventData, IAFunction<T> functor) where T : IIAHandler
        {
            var internalHandlers = ListPool<IIAHandler>.Get();
            GetEventList<T>(target, internalHandlers);

            var internalHandlersCount = internalHandlers.Count;
            for (var i = 0; i < internalHandlersCount; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception($"Type {typeof(T).Name} expected {temp.GetType().Name} received.", e));
                    continue;
                }

                try
                {
                    functor(arg, eventData);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var handlerCount = internalHandlers.Count;
            ListPool<IIAHandler>.Release(internalHandlers);
            return handlerCount > 0;
        }
        public static GameObject ExecuteHierarchy<T>(GameObject root, IABaseData eventData, IAFunction<T> callbackFunction) where T : IIAHandler
        {
            GetEventChain(root, internalTransformList);

            var internalTransformListCount = internalTransformList.Count;
            for (var i = 0; i < internalTransformListCount; i++)
            {
                var transform = internalTransformList[i];
                if (Execute(transform.gameObject, eventData, callbackFunction))
                    return transform.gameObject;
            }
            return null;
        }
    }
}