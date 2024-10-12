using System.Collections.Generic;
using UnityEngine;
using ET;
using System;
using System.Reflection;

#if !EGAMEPLAY_ET
namespace EGamePlay.Combat
{
    public static class ConfigHelper
    {
        public static T Get<T>(int id) where T : class, IConfig
        {
            return ConfigManageComponent.Instance.Get<T>(id);
        }
        public static Dictionary<int, T> GetAll<T>() where T : class, IConfig
        {
            return ConfigManageComponent.Instance.GetAll<T>();
        }
    }

    public class ConfigManageComponent : Component
    {
        public static ConfigManageComponent Instance { get; private set; }
        private Dictionary<Type, object> TypeConfigCategarys { get; set; } = new ();
        public override void Awake(object initData)
        {
            Instance = this;
            var assembly = Assembly.GetAssembly(typeof(TimerManager));
            var configsCollector = initData as ReferenceCollector;
            if (configsCollector == null) return;
            foreach (var item in configsCollector.data)
            {
                var configTypeName = $"ET.{item.gameObject.name}";
                var configType = assembly.GetType(configTypeName);
                var typeName = $"ET.{item.gameObject.name}Category";
                var configCategoryType = assembly.GetType(typeName);
                if (Activator.CreateInstance(configCategoryType) is not ACategory configCategory) continue;
                configCategory.ConfigText = (item.gameObject as TextAsset)?.text;
                configCategory.BeginInit();
                TypeConfigCategarys.Add(configType, configCategory);
            }
        }
        public T Get<T>(int id) where T : class, IConfig
        {
            if (TypeConfigCategarys[typeof(T)] is ACategory<T> category) return category.Get(id);
            return null;
        }
        public Dictionary<int, T> GetAll<T>() where T : class, IConfig
        {
            if (TypeConfigCategarys[typeof(T)] is ACategory<T> category) return category.GetAll();
            return null;
        }
    }
}
#endif