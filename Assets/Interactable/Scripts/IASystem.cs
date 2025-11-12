using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactable
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Interactable/Interactable System")]
    public class IASystem: MonoBehaviour
    {
        private IAManager m_manager;
        
        [Tooltip("活跃状态")]
        [SerializeField] private bool active;
        
        [Tooltip("进入活跃状态时默认选中的物体")]
        [SerializeField] private GameObject firstSelected;
        
        [Tooltip("是否允许导航事件（与UGUI类似）")]
        [SerializeField] private bool sendNavigationEvents = true;
        
        [Tooltip("触发拖动事件的最小位移")]
        [SerializeField] private int dragThreshold = 1;

        [Tooltip("进入活跃状态时是否还原上次已选中的物体")]
        [SerializeField] private bool keepSelectionState;
        
        public GameObject FirstSelectedGameObject
        {
            get => firstSelected;
            set => firstSelected = value;
        }
        public bool SendNavigationEvents
        {
            get => sendNavigationEvents;
            set => sendNavigationEvents = value;
        }
        public int PixelDragThreshold
        {
            get => dragThreshold;
            set => dragThreshold = value;
        }
        
        #region MonoBehaviour lifecycle
        
        private bool isFocused = true;
        public bool IsFocused => isFocused;

        private void Awake()
        {
            m_manager = IAManager.GetInstance();
        }
        private void OnEnable()
        {
            GetRaycasters();
            m_manager.RegisterIASystem(this);
        }
        private void OnDisable()
        {
            if (m_currentInputModule != null)
            {
                m_currentInputModule.InactivateModule();
                m_currentInputModule = null;
            }
            
            m_manager.RemoveIASystem(this);
            m_raycastersList.Clear();
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            isFocused = hasFocus;
            if (!isFocused)
                TickInputModules();
        }
        
        #endregion
        
        #region public
        
        private bool m_inited;
        
        private GameObject currentSelected;
        private GameObject lastSelected;
        private bool selectionLock;     //lock the selection process and ensure safety
        private IABaseData dummyData;
        private IABaseData DummyData => dummyData ??= new IABaseData(this);
        
        /// <summary>
        /// 当前选中的物体
        /// </summary>
        public GameObject CurrentSelected => currentSelected;
        /// <summary>
        /// 上一次选中的物体
        /// </summary>
        public GameObject LastSelected => lastSelected;
        
        /// <summary>
        /// 系统是否活跃
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return isActiveAndEnabled && active;
        }

        /// <summary>
        /// 设置系统活跃/不活跃
        /// </summary>
        /// <param name="value"></param>
        public void SetActive(bool value)
        {
            active = value;
        }

        /// <summary>
        /// 启用系统
        /// </summary>
        internal void ActivateSystem()
        {
            if (!m_inited)
            {
                m_inited = true;
                SetSelectedGameObject(firstSelected);
            }
            else if (keepSelectionState)
            {
                SetSelectedGameObject(lastSelected);
            }
            else
            {
                SetSelectedGameObject(firstSelected);
            }

            var module = m_iaInputModules.FirstOrDefault(
                m => m.IsModuleSupported() && m.ShouldActivateModule());
            if(module != null)
                ChangeInputModule(module);
        }
        
        /// <summary>
        /// 关闭系统
        /// </summary>
        internal void InactivateSystem()
        {
            SetSelectedGameObject(null);
            ChangeInputModule(null);
        }
        
        /// <summary>
        /// 选中某个物体
        /// </summary>
        /// <param name="selected">被选中的物体</param>
        public void SetSelectedGameObject(GameObject selected)
        {
            SetSelectedGameObject(selected, DummyData);
        }
        
        /// <summary>
        /// 选中某个物体
        /// </summary>
        /// <param name="selected">被选中的物体</param>
        /// <param name="pointer">传递的交互数据</param>
        internal void SetSelectedGameObject(GameObject selected, IABaseData pointer)
        {
            if (selectionLock)
            {
                Debug.LogError("Attempting to select " + selected +  "while already selecting an object.");
                return;
            }

            selectionLock = true;
            if (selected == currentSelected)
            {
                selectionLock = false;
                return;
            }

            lastSelected = currentSelected;
            ExecuteInteraction.Execute(currentSelected, pointer, ExecuteInteraction.DeselectHandler);
            currentSelected = selected;
            ExecuteInteraction.Execute(currentSelected, pointer, ExecuteInteraction.SelectHandler);
            selectionLock = false;
        }
        
        /// <summary>
        /// 交互系统的处理方法，由IAManager每帧调用
        /// </summary>
        internal void SystemProcess()
        {
            if(!active)
                return;
            //触发每一个输入模块的Update
            TickInputModules();
            //检查是否需要切换输入模块
            var changedModule = CheckChangeInputModule();
            //如果不需要切换，则处理当前活跃的输入模块
            ProcessInputModule(changedModule);
        }
        
        #endregion
        
        #region Input Modules
        
        private readonly List<IABaseInputModule> m_iaInputModules = new();
        
        private IABaseInputModule m_currentInputModule;
        public IABaseInputModule CurrentInputModule => m_currentInputModule;
        
        private void TickInputModules()
        {
            var count = m_iaInputModules.Count;
            for (var i = 0; i < count; i++)
            {
                if (m_iaInputModules[i] != null)
                    m_iaInputModules[i].UpdateModule();
            }
        }
        
        //始终使用m_iaInputModules第一个有效的输入模块
        private bool CheckChangeInputModule()
        {
            var changedModule = false;
            var modulesCount = m_iaInputModules.Count;
            for (var i = 0; i < modulesCount; i++)
            {
                var module = m_iaInputModules[i];
                if (module.IsModuleSupported() && module.ShouldActivateModule())
                {
                    if (m_currentInputModule != module)
                    {
                        ChangeInputModule(module);
                        changedModule = true;
                    }
                    break;
                }
            }

            // no input module set... set the first valid one...
            if (m_currentInputModule == null)
            {
                for (var i = 0; i < modulesCount; i++)
                {
                    var module = m_iaInputModules[i];
                    if (module.IsModuleSupported())
                    {
                        ChangeInputModule(module);
                        changedModule = true;
                        break;
                    }
                }
            }

            return changedModule;
        }
        private void ChangeInputModule(IABaseInputModule module)
        {
            if (m_currentInputModule == module)
                return;

            if (m_currentInputModule != null)
                m_currentInputModule.InactivateModule();

            if (module != null)
                module.ActivateModule();
            
            m_currentInputModule = module;
        }
        //处理当前活跃的输入模块
        private void ProcessInputModule(bool changedModule)
        {
            if (!changedModule && m_currentInputModule != null)
                m_currentInputModule.Process();
        }
        internal void UpdateModules()
        {
            //获取当前GameObject上类型为IABaseInputModule的所有组件并存入m_iaInputModules
            GetComponents(m_iaInputModules);
            
            var count = m_iaInputModules.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (m_iaInputModules[i] && m_iaInputModules[i].IsActive())
                    continue;
                
                //移出所有空或者未启用的组件
                m_iaInputModules.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Raycaster
        
        private readonly List<IABaseRaycaster> m_raycastersList = new();
        
        private static readonly Comparison<IARaycastResult> raycastComparer = IARaycastComparer;
        private static int IARaycastComparer(IARaycastResult lhs, IARaycastResult rhs)
        {
            const float tolerance = 0.001f;
            
            //如果左右投射器不一样的话，那就先比较相机的depth (rendering order)，数值越小越先渲染，越先渲染的越靠后
            if (lhs.raycaster != rhs.raycaster)
            {
                var lhsCamera = lhs.raycaster.RayCamera;
                var rhsCamera = rhs.raycaster.RayCamera;
                if (lhsCamera != null && rhsCamera != null && Math.Abs(lhsCamera.depth - rhsCamera.depth) > tolerance)
                {
                    return rhsCamera.depth.CompareTo(lhsCamera.depth);
                }
            }
            
            //其次比较设定的depth
            if (lhs.depth != rhs.depth && lhs.raycaster.RootRaycaster == rhs.raycaster.RootRaycaster)
                return rhs.depth.CompareTo(lhs.depth);
            
            //最后比较射线的碰撞距离
            if (Math.Abs(lhs.distance - rhs.distance) > tolerance)
                return lhs.distance.CompareTo(rhs.distance);

            return lhs.index.CompareTo(rhs.index);
        }
        
        private void GetRaycasters()
        {
            var iaBaseRaycasters = GetComponentsInParent<IABaseRaycaster>();
            foreach (var r in iaBaseRaycasters)
            {
                m_raycastersList.Add(r);
            }
            
        }
        
        /// <summary>
        /// 添加新的射线投射器
        /// </summary>
        /// <param name="baseRaycaster">射线投射器</param>
        public void AddRaycaster(IABaseRaycaster baseRaycaster)
        {
            if (m_raycastersList.Contains(baseRaycaster))
                return;

            m_raycastersList.Add(baseRaycaster);
        }
        
        /// <summary>
        /// 移出射线投射器
        /// </summary>
        /// <param name="baseRaycaster">射线投射器</param>
        public void RemoveRaycaster(IABaseRaycaster baseRaycaster)
        {
            if (!m_raycastersList.Contains(baseRaycaster))
                return;
            m_raycastersList.Remove(baseRaycaster);
        }
        
        
        internal void RaycastAll(IAPointerData pointerData, List<IARaycastResult> raycastResults)
        {
            raycastResults.Clear();
            var raycastersCount = m_raycastersList.Count;
            for (int i = 0; i < raycastersCount; ++i)
            {
                var raycaster = m_raycastersList[i];
                if (raycaster == null || !raycaster.IsActive())
                    continue;

                raycaster.Raycast(pointerData, raycastResults);
            }
            raycastResults.Sort(raycastComparer);
        }
        
        
        #endregion
    }
}