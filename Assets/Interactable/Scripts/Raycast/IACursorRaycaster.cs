using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class IACursorRaycaster: IABaseRaycaster
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

        private bool ComputeRayAndDistance(IAPointerData pointerData, 
            ref Ray ray, ref int rayDisplayIndex, ref float distanceToClipPlane)
        {
            if (RayCamera == null)
                return false;

            var cursorPos = IAUtilities.RelativeMouseAtScaled(pointerData.Position, pointerData.DisplayIndex);
            if (cursorPos != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.
                rayDisplayIndex = (int)cursorPos.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (rayDisplayIndex != RayCamera.targetDisplay)
                    return false;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                cursorPos = pointerData.Position;
            }

            // Cull ray casts that are outside of the view rect. (case 636595)
            if (!RayCamera.pixelRect.Contains(cursorPos))
                return false;

            ray = RayCamera.ScreenPointToRay(cursorPos);

            float projectionDirection = Vector3.Dot(ray.direction, RayCamera.transform.forward);
            distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                ? Mathf.Infinity
                : Mathf.Abs((RayCamera.farClipPlane - RayCamera.nearClipPlane) / projectionDirection);
            return true;
        }
        
        public override void Raycast(IAPointerData pointerData, List<IARaycastResult> resultAppendList)
        {
            Ray ray = new Ray();
            var displayIndex = 0;
            float distanceToClipPlane = 0;
            
            if (!ComputeRayAndDistance(pointerData, ref ray, ref displayIndex, ref distanceToClipPlane))
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