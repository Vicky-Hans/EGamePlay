using System;
using System.Collections.Generic;
namespace EGamePlay
{
    public class SubscribeSubject : Entity
    {
        protected override void Awake(object initData)
        {
            Name = initData.GetHashCode().ToString();
        }
        public SubscribeSubject DisposeWith(Entity entity)
        {
            entity.SetChild(this);
            return this;
        }
    }
    public sealed class EventComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        private readonly Dictionary<Type, List<object>> typeEvent2ActionLists = new ();
        private readonly Dictionary<string, List<object>> fireEvent2ActionLists = new ();
        public new T Publish<T>(T @event) where T : class
        {
            if (!typeEvent2ActionLists.TryGetValue(typeof(T), out var actionList)) return @event;
            var tempList = actionList.ToArray();
            foreach (Action<T> action in tempList)
            {
                action.Invoke(@event);
            }
            return @event;
        }
        public new SubscribeSubject Subscribe<T>(Action<T> action) where T : class
        {
            var type = typeof(T);
            if (!typeEvent2ActionLists.TryGetValue(type, out var actionList))
            {
                actionList = new List<object>();
                typeEvent2ActionLists.Add(type, actionList);
            }
            actionList.Add(action);
            return Entity.AddChild<SubscribeSubject>(action);
        }
        public new void UnSubscribe<T>(Action<T> action) where T : class
        {
            if (typeEvent2ActionLists.TryGetValue(typeof(T), out var actionList)) actionList.Remove(action);
            var e = Entity.Find<SubscribeSubject>(action.GetHashCode().ToString());
            if (e != null) Entity.Destroy(e);
        }
        public void FireEvent<T>(string eventType, T entity) where T : Entity
        {
            if (!fireEvent2ActionLists.TryGetValue(eventType, out var actionList)) return;
            var tempList = actionList.ToArray();
            foreach (Action<T> action in tempList)
            {
                action.Invoke(entity);
            }
        }
        public void OnEvent<T>(string eventType, Action<T> action) where T : Entity
        {
            if (fireEvent2ActionLists.TryGetValue(eventType, out var actionList))
            { }
            else
            {
                actionList = new List<object>();
                fireEvent2ActionLists.Add(eventType, actionList);
            }
            actionList.Add(action);
        }
        public void OffEvent<T>(string eventType, Action<T> action) where T : Entity
        {
            if (fireEvent2ActionLists.TryGetValue(eventType, out var actionList)) actionList.Remove(action);
        }
    }
}