using System;
using System.Collections.Generic;

namespace EGamePlay
{
    public class Component
    {
        public Entity Entity { get; set; }
        public bool IsDisposed { get; set; }
        public Dictionary<long, Entity> Id2Children { get; private set; } = new ();
        public virtual bool DefaultEnable { get; set; } = true;
        private bool enable = false;
        public bool Enable
        {
            set
            {
                if (enable == value) return;
                enable = value;
                if (enable) OnEnable();
                else OnDisable();
            }
            get => enable;
        }
        public bool Disable => enable == false;
        protected T GetEntity<T>() where T : Entity
        {
            return Entity as T;
        }
        public virtual void Awake() { }
        public virtual void Awake(object initData) { }
        public virtual void Setup() { }
        public virtual void Setup(object initData) { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public virtual void Update() { }
        protected virtual void OnDestroy() { }
        private void Dispose()
        {
            if (Entity.EnableLog) Log.Debug($"{GetType().Name}->Dispose");
            Enable = false;
            IsDisposed = true;
        }
        public static void Destroy(Component entity)
        {
            try
            {
                entity.OnDestroy();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            entity.Dispose();
        }
        protected T Publish<T>(T @event) where T : class
        {
            Entity.Publish(@event);
            return @event;
        }
        public void Subscribe<T>(Action<T> action) where T : class
        {
            Entity.Subscribe(action);
        }
        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            Entity.UnSubscribe(action);
        }
    }
}