using EGamePlay.Combat;
using EGamePlay;
using ET;
using System.Threading;
using UnityEngine;

public class MiniExampleInit : MonoBehaviour
{
    public bool EntityLog;
    public AbilityConfigObject SkillConfigObject;
    public ReferenceCollector ConfigsCollector;
    private async void Awake()
    {
        SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
        Entity.EnableLog = EntityLog;
        ECSNode.Create();
        Entity.Create<TimerManager>();
        Entity.Create<CombatContext>();
        ECSNode.Instance.AddComponent<ConfigManageComponent>(ConfigsCollector);

        await TimerManager.Instance.WaitAsync(2000);
        var monster = CombatContext.Instance.AddChild<CombatEntity>();
        var hero = CombatContext.Instance.AddChild<CombatEntity>();
        var heroSkillAbility = hero.GetComponent<SkillComponent>().AttachSkill(SkillConfigObject);

        Debug.Log($"1 monster.CurrentHealth={monster.CurrentHealth.Value}");
        hero.GetComponent<SpellComponent>().SpellWithTarget(heroSkillAbility, monster);
        await TimerManager.Instance.WaitAsync(2000);
        Debug.Log($"2 monster.CurrentHealth={monster.CurrentHealth.Value}");
    }
    private void Update()
    {
        ECSNode.Instance?.Update();
        TimerManager.Instance?.Update();
    }

    private void OnApplicationQuit()
    {
        ECSNode.Destroy();
    }
}
