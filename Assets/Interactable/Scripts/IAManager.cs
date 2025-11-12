using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactable
{
    /// <summary>
    /// IAManager：交互管理器（单例）控制所有的交互子系统（IASystem）的注册、注销、活跃系统的切换；
    /// 始终只有一个活跃的交互子系统，交互管理器负责每帧调用该活跃系统的处理方法
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Interactable/Interactable Manager")]
    public class IAManager : MonoBehaviour
    {
        private static bool _createdInstance;
        private static IAManager instance;
        public static IAManager GetInstance()
        {
            if (instance != null || _createdInstance) return instance;
            instance = FindObjectOfType<IAManager>();
            if (instance == null)
            {
                GameObject obj = new() { name = nameof(IAManager) };
                instance = obj.AddComponent<IAManager>();
            }
            _createdInstance = true;
            DontDestroyOnLoad(instance);
            return instance;
        }
        
        
        private readonly List<IASystem> m_iaSystems = new();
        
        private IASystem m_currentActiveSystem;
        public IASystem CurrentActiveSystem => m_currentActiveSystem;
        
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else if (instance == null)
            {
                instance = this;
                _createdInstance = true;
                DontDestroyOnLoad(this);
            }
        }
        private void Update()
        {
            UpdateCurrentSystem();
            ProcessCurrentSystem();
        }
        
        private void UpdateCurrentSystem()
        {
            if (( m_currentActiveSystem != null && !m_currentActiveSystem.IsActive()) || m_iaSystems.Count == 0)
            {
                SwitchIASystem(null);
            }
            
            if(m_currentActiveSystem == null)
            {
                var system = m_iaSystems.FirstOrDefault(s => s.IsActive());
                SwitchIASystem(system);
            }
        }
        private void ProcessCurrentSystem()
        {
            if(m_currentActiveSystem != null)
                m_currentActiveSystem.SystemProcess();
        }
        /// <summary>
        /// 注册交互系统
        /// </summary>
        /// <param name="iaSystem">交互系统</param>
        internal void RegisterIASystem(IASystem iaSystem)
        {
            if (iaSystem!= null && !m_iaSystems.Contains(iaSystem))
            {
                m_iaSystems.Add(iaSystem);
            }
        }
        /// <summary>
        /// 注销交互系统
        /// </summary>
        /// <param name="iaSystem">交互系统</param>
        internal void RemoveIASystem(IASystem iaSystem)
        {
            if (iaSystem!= null && m_iaSystems.Contains(iaSystem))
            {
                m_iaSystems.Remove(iaSystem);
            }
        }
        /// <summary>
        /// 切换活跃的交互系统
        /// </summary>
        /// <param name="target">交互系统或null</param>
        public void SwitchIASystem(IASystem target)
        {
            if(m_currentActiveSystem == target)
                return;
            
            if (m_currentActiveSystem != null)
            {
                m_currentActiveSystem.InactivateSystem();
            }
            
            if (target != null)
            {
                target.ActivateSystem();
            }
            
            m_currentActiveSystem = target;
        }
        
        

    }
}