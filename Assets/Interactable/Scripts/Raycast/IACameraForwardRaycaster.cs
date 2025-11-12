using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class IACameraForwardRaycaster: IABaseRaycaster
    {
        [SerializeField] protected LayerMask rayMask = NoRayMaskSet;
        [SerializeField] protected int maxRayIntersections;
        [SerializeField] protected float maxRayLength;
        [SerializeField] protected bool allowTriggerColliders;
        
        private readonly RaycastHit[] m_raycastHits = new RaycastHit[MaxHits];

        private const int MaxHits = 5;
        private const int NoRayMaskSet = -1;

        private Camera rayCamera;
        public override Camera RayCamera
        {
            get
            {
                if (rayCamera == null)
                    rayCamera = Camera.main;
                return rayCamera;
            }
        }
        public int FinalRayMask => (RayCamera != null) ? RayCamera.cullingMask & rayMask : NoRayMaskSet;
        
        public int MaxRayIntersections => maxRayIntersections;

        public LayerMask RayMask
        {
            get => rayMask; 
            set => rayMask = value;
        }
        
        private bool ComputeRayAndDistance(ref Ray ray, ref float distanceToClipPlane)
        {
            if (RayCamera == null)
                return false;
            
            var camTrans = RayCamera.transform;
            ray = new Ray(camTrans.position, camTrans.forward);
            distanceToClipPlane = RayCamera.farClipPlane - RayCamera.nearClipPlane;
            return true;
        }
        
        public override void Raycast(IAPointerData pointerData, List<IARaycastResult> resultAppendList)
        {
            Ray ray = new Ray();
            var displayIndex = 0;
            float distanceToClipPlane = 0;
            
            if (!ComputeRayAndDistance(ref ray, ref distanceToClipPlane))
                return;

            var maxDistance = maxRayLength <= 0 ? distanceToClipPlane : Mathf.Min(maxRayLength, distanceToClipPlane);
            var hitCount = Physics.RaycastNonAlloc(ray, m_raycastHits, maxDistance, FinalRayMask);
            var maxResult = maxRayIntersections <= 0 ? MaxHits : maxRayIntersections; 

            if (hitCount != 0)
            {
                if (hitCount > 1)
                    Array.Sort(m_raycastHits, 0, hitCount, RaycastHitComparer.Instance);

                var num = 0;

                for (int i = 0, count = hitCount; i < count; ++i)
                {
                    if (!allowTriggerColliders && m_raycastHits[i].collider.isTrigger)
                    {
                        continue;
                    }

                    if (num >= maxResult)
                    {
                        break;
                    }

                    num++;
                    var result = new IARaycastResult
                    {
                        gameObject = m_raycastHits[i].collider.gameObject,
                        raycaster = this,
                        distance = m_raycastHits[i].distance,
                        worldPosition = m_raycastHits[i].point,
                        worldNormal = m_raycastHits[i].normal,
                        screenPosition = pointerData.Position,
                        displayIndex = displayIndex,
                        index = resultAppendList.Count,
                    };
                    resultAppendList.Add(result);
                }
            }
        }
    }
}