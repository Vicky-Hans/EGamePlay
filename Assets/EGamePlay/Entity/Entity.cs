using System;
using System.Collections.Generic;
namespace EGamePlay
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class EnableUpdateAttribute : Attribute { }
    public abstract partial class Entity
    {
        public long Id { get; private set; }
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
#if !NOT_UNITY
                GetComponent<GameObjectComponent>().OnNameChanged(name);
#endif
            }
        }
        public long InstanceId { get; private set; }
        public Entity Parent { get; private set; }
        public bool IsDisposed => InstanceId == 0;
        private List<Entity> Children { get; set; } = new ();
        private Dictionary<long, Entity> Id2Children { get; set; } = new ();
        private Dictionary<Type, List<Entity>> Type2Children { get; set; } = new ();
        public Dictionary<Type, Component> Components { get; set; } = new ();
        protected Entity()
        {
#if !NOT_UNITY
            if (this is ECSNode) { }
            else if (GetType().Name.Contains("OnceWaitTimer")) { }
            else AddComponent<GameObjectComponent>();
#endif
        }
        protected virtual void Awake() { }
        protected virtual void Awake(object initData) { }
        protected virtual void Start() { }
        protected virtual void Start(object initData) { }
        protected virtual void OnSetParent(Entity preParent, Entity nowParent) { }
        public virtual void Update() { }
        protected virtual void OnDestroy() { }
        private void Dispose()
        {
            if (EnableLog) Log.Debug($"{GetType().Name}->Dispose");
            if (Children.Count > 0)
            {
                for (var i = Children.Count - 1; i >= 0; i--)
                {
                    Destroy(Children[i]);
                }
                Children.Clear();
                Type2Children.Clear();
            }
            Parent?.RemoveChild(this);
            foreach (var component in Components.Values)
            {
                component.Enable = false;
                Component.Destroy(component);
            }
            Components.Clear();
            InstanceId = 0;
            if (EcsNode.Entities.ContainsKey(GetType()))
            {
                EcsNode.Entities[GetType()].Remove(this);
            }
        }
        #region 组件
        public T GetParent<T>() where T : Entity
        {
            return Parent as T;
        }
        public T As<T>() where T : class
        {
            return this as T;
        }
        public bool As<T>(out T entity) where T : Entity
        {
            entity = this as T;
            return entity != null;
        }
        public T AddComponent<T>() where T : Component
        {
            var component = Activator.CreateInstance<T>();
            component.Entity = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            EcsNode.AllComponents.Add(component);
            if (EnableLog) Log.Debug($"{GetType().Name}->AddComponent, {typeof(T).Name}");
            component.Awake();
            component.Setup();
#if !NOT_UNITY
            GetComponent<GameObjectComponent>().OnAddComponent(component);
#endif
            component.Enable = component.DefaultEnable;
            return component;
        }
        public T AddComponent<T>(object initData) where T : Component
        {
            var component = Activator.CreateInstance<T>();
            component.Entity = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            EcsNode.AllComponents.Add(component);
            if (EnableLog) Log.Debug($"{GetType().Name}->AddComponent, {typeof(T).Name} initData={initData}");
            component.Awake(initData);
            component.Setup(initData);
#if !NOT_UNITY
            GetComponent<GameObjectComponent>().OnAddComponent(component);
#endif
            component.Enable = component.DefaultEnable;
            return component;
        }
        public T Add<T>() where T : Component
        {
            return AddComponent<T>();
        }
        public T Add<T>(object initData) where T : Component
        {
            return AddComponent<T>(initData);
        }
        public void RemoveComponent<T>() where T : Component
        {
            var component = Components[typeof(T)];
            if (component.Enable) component.Enable = false;
            Component.Destroy(component);
            Components.Remove(typeof(T));
#if !NOT_UNITY
            GetComponent<GameObjectComponent>().OnRemoveComponent(component);
#endif
        }
        public T GetComponent<T>() where T : Component
        {
            if (Components.TryGetValue(typeof(T), out var component)) return component as T;
            return null;
        }
        public bool HasComponent<T>() where T : Component
        {
            return Components.TryGetValue(typeof(T), out var component);
        }
        public Component GetComponent(Type componentType)
        {
            return Components.GetValueOrDefault(componentType);
        }
        public bool TryGet<T>(out T component) where T : Component
        {
            if (Components.TryGetValue(typeof(T), out var c))
            {
                component = c as T;
                return true;
            }
            component = null;
            return false;
        }
        public bool TryGet<T, T1>(out T component, out T1 component1) where T : Component  where T1 : Component
        {
            component = null;
            component1 = null;
            if (Components.TryGetValue(typeof(T), out var c)) component = c as T;
            if (Components.TryGetValue(typeof(T1), out var c1)) component1 = c1 as T1;
            return component != null && component1 != null;
        }
        public bool TryGet<T, T1, T2>(out T component, out T1 component1, out T2 component2) where T : Component where T1 : Component where T2 : Component
        {
            component = null;
            component1 = null;
            component2 = null;
            if (Components.TryGetValue(typeof(T), out var c)) component = c as T;
            if (Components.TryGetValue(typeof(T1), out var c1)) component1 = c1 as T1;
            if (Components.TryGetValue(typeof(T2), out var c2)) component2 = c2 as T2;
            return component != null && component1 != null && component2 != null;
        }
        #endregion
        #region 子实体
        private void SetParent(Entity parent)
        {
            var preParent = Parent;
            preParent?.RemoveChild(this);
            Parent = parent;
#if !NOT_UNITY
            parent.GetComponent<GameObjectComponent>().OnAddChild(this);
#endif
            OnSetParent(preParent, parent);
        }
        public void SetChild(Entity child)
        {
            Children.Add(child);
            Id2Children.Add(child.Id, child);
            if (!Type2Children.ContainsKey(child.GetType())) Type2Children.Add(child.GetType(), new List<Entity>());
            Type2Children[child.GetType()].Add(child);
            child.SetParent(this);
        }
        private void RemoveChild(Entity child)
        {
            Children.Remove(child);
            Id2Children.Remove(child.Id);
            if (Type2Children.ContainsKey(child.GetType())) Type2Children[child.GetType()].Remove(child);
        }
        private Entity AddChild(Type entityType)
        {
            var entity = NewEntity(entityType);
            if (EnableLog) Log.Debug($"AddChild {this.GetType().Name}, {entityType.Name}={entity.Id}");
            SetupEntity(entity, this);
            return entity;
        }
        public Entity AddChild(Type entityType, object initData)
        {
            var entity = NewEntity(entityType);
            if (EnableLog) Log.Debug($"AddChild {this.GetType().Name}, {entityType.Name}={entity.Id}");
            SetupEntity(entity, this, initData);
            return entity;
        }
        public T AddChild<T>() where T : Entity
        {
            return AddChild(typeof(T)) as T;
        }
        public T AddIdChild<T>(long id) where T : Entity
        {
            var entityType = typeof(T);
            var entity = NewEntity(entityType, id);
            if (EnableLog) Log.Debug($"AddChild {this.GetType().Name}, {entityType.Name}={entity.Id}");
            SetupEntity(entity, this);
            return entity as T;
        }
        public T AddChild<T>(object initData) where T : Entity
        {
            return AddChild(typeof(T), initData) as T;
        }
        public Entity GetIdChild(long id)
        {
            Id2Children.TryGetValue(id, out var entity);
            return entity;
        }
        public T GetIdChild<T>(long id) where T : Entity
        {
            Id2Children.TryGetValue(id, out var entity);
            return entity as T;
        }
        public T GetChild<T>(int index = 0) where T : Entity
        {
            if (Type2Children.ContainsKey(typeof(T)) == false)  return null;
            if (Type2Children[typeof(T)].Count <= index) return null;
            return Type2Children[typeof(T)][index] as T;
        }
        public Entity[] GetChildren()
        {
            return Children.ToArray();
        }
        public T[] GetTypeChildren<T>() where T : Entity
        {
            return Type2Children[typeof(T)].ConvertAll(x => x.As<T>()).ToArray();
        }
        public Entity Find(string nameStr)
        {
            foreach (var item in Children)
            {
                if (item.name == nameStr) return item;
            }
            return null;
        }
        public T Find<T>(string nameStr) where T : Entity
        {
            if (!Type2Children.TryGetValue(typeof(T), out var chidren)) return null;
            foreach (var item in chidren)
            {
                if (item.name == nameStr) return item as T;
            }
            return null;
        }
        #endregion
        #region 事件
        public T Publish<T>(T @event) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                return @event;
            }
            eventComponent.Publish(@event);
            return @event;
        }
        public SubscribeSubject Subscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>() ?? AddComponent<EventComponent>();
            return eventComponent.Subscribe(action);
        }
        public SubscribeSubject Subscribe<T>(Action<T> action, Entity disposeWith) where T : class
        {
            var eventComponent = GetComponent<EventComponent>() ?? AddComponent<EventComponent>();
            return eventComponent.Subscribe(action).DisposeWith(disposeWith);
        }
        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            eventComponent?.UnSubscribe(action);
        }
        public void FireEvent(string eventType)
        {
            FireEvent(eventType, this);
        }
        private void FireEvent(string eventType, Entity entity)
        {
            var eventComponent = GetComponent<EventComponent>();
            eventComponent?.FireEvent(eventType, entity);
        }
        public void OnEvent(string eventType, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>() ?? AddComponent<EventComponent>();
            eventComponent.OnEvent(eventType, action);
        }
        public void OffEvent(string eventType, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>();
            eventComponent?.OffEvent(eventType, action);
        }
        #endregion
        private static ECSNode EcsNode => ECSNode.Instance;
        public static bool EnableLog { get; set; }
        private static Entity NewEntity(Type entityType, long id = 0)
        {
            if (Activator.CreateInstance(entityType) is not Entity entity) return null;
            entity.InstanceId = IdFactory.NewInstanceId();
            entity.Id = id == 0 ? entity.InstanceId : id;
            if (!EcsNode.Entities.ContainsKey(entityType))
            {
                EcsNode.Entities.Add(entityType, new List<Entity>());
            }
            EcsNode.Entities[entityType].Add(entity);
            return entity;
        }
        public static T Create<T>() where T : Entity
        {
            return Create(typeof(T)) as T;
        }
        public static T Create<T>(object initData) where T : Entity
        {
            return Create(typeof(T), initData) as T;
        }
        private static void SetupEntity(Entity entity, Entity parent)
        {
            parent.SetChild(entity);
            entity.Awake();
            entity.Start();
        }
        private static void SetupEntity(Entity entity, Entity parent, object initData)
        {
            parent.SetChild(entity);
            entity.Awake(initData);
            entity.Start(initData);
        }
        private static Entity Create(Type entityType)
        {
            var entity = NewEntity(entityType);
            if (EnableLog) Log.Debug($"Create {entityType.Name}={entity.Id}");
            SetupEntity(entity, EcsNode);
            return entity;
        }
        private static Entity Create(Type entityType, object initData)
        {
            var entity = NewEntity(entityType);
            if (EnableLog) Log.Debug($"Create {entityType.Name}={entity.Id}, {initData}");
            SetupEntity(entity, EcsNode, initData);
            return entity;
        }
        public static void Destroy(Entity entity)
        {
            if (entity == null) return;
            if (entity.IsDisposed) return;
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
    }
}