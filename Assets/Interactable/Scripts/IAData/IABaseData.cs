using UnityEngine;

namespace Interactable
{
    public abstract class IAAbstractData
    {
        protected bool m_used;
        
        public virtual bool Used => m_used;
        
        public virtual void Reset()
        {
            m_used = false;
        }
        
        public virtual void Use()
        {
            m_used = true;
        }
        
    }

    public class IABaseData : IAAbstractData
    {
        private readonly IASystem m_system;
        public IABaseData(IASystem system)
        {
            m_system = system;
        }

        public IABaseInputModule CurrentInputModule => m_system.CurrentInputModule;

        public GameObject SelectedObject
        {
            get => m_system.CurrentSelected;
            set => m_system.SetSelectedGameObject(value, this);
        }

    }
}
