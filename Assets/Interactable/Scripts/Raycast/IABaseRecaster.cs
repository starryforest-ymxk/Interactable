using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public abstract class IABaseRaycaster: MonoBehaviour
    {
        private IABaseRaycaster rootRaycaster;
        public IABaseRaycaster RootRaycaster
        {
            get
            {
                if (rootRaycaster == null)
                {
                    var baseRaycasters = GetComponentsInParent<IABaseRaycaster>();
                    if (baseRaycasters.Length != 0)
                        rootRaycaster = baseRaycasters[^1];
                }
                return rootRaycaster;
            }
        }
        public abstract Camera RayCamera { get; }
        public abstract void Raycast(IAPointerData pointerData, List<IARaycastResult> resultAppendList);
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

        public override string ToString()
        {
            return "Name: " + gameObject + "\n" +
                   "rayCamera: " + RayCamera;
        }
        protected virtual void OnEnable(){}
        protected virtual void OnDisable(){}
    }
}