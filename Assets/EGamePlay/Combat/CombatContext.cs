using System.Collections.Generic;
using UnityEngine;
using ET;
using System.Linq;

namespace EGamePlay.Combat
{
    /// <summary>
    /// 战局上下文
    /// 像回合制、moba这种战斗按局来分的，可以创建这个战局上下文，如果是mmo，那么战局上下文应该是在角色进入战斗才会创建，离开战斗就销毁
    /// </summary>
    public class CombatContext : Entity
    {
        public static CombatContext Instance { get; private set; }
#if !SERVER
        public Dictionary<GameObject, CombatEntity> Object2Entities { get; set; } = new ();
        public Dictionary<GameObject, AbilityItem> Object2Items { get; set; } = new ();
#endif
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            AddComponent<UpdateComponent>();
            Subscribe<EntityDeadEvent>(OnEntityDead);
        }

        #region 回合制战斗
        private Dictionary<int, CombatEntity> HeroEntities { get; set; } = new ();
        private Dictionary<int, CombatEntity> EnemyEntities { get; set; } = new ();
        private List<RoundAction> RoundActions { get; set; } = new ();
        public override void Update() { }
        public CombatEntity AddHeroEntity(int seat)
        {
            var entity = AddChild<CombatEntity>();
            entity.IsHero = true;
            HeroEntities.Add(seat, entity);
            entity.SeatNumber = seat;
            return entity;
        }
        public CombatEntity AddMonsterEntity(int seat)
        {
            var entity = AddChild<CombatEntity>();
            entity.IsHero = false;
            EnemyEntities.Add(seat, entity);
            entity.SeatNumber = seat;
            return entity;
        }
        public CombatEntity GetHero(int seat)
        {
            return HeroEntities[seat];
        }
        public CombatEntity GetMonster(int seat)
        {
            return EnemyEntities[seat];
        }

        private void OnEntityDead(EntityDeadEvent evnt)
        {
            var deadEntity = evnt.DeadEntity;
            if (deadEntity is CombatEntity combatEntity)
            {
                if (combatEntity.IsHero) HeroEntities.Remove(combatEntity.SeatNumber);
                else EnemyEntities.Remove(combatEntity.SeatNumber);
            }
            Destroy(deadEntity);
        }
        public async void StartCombat()
        {
            RefreshRoundActions();
            CombatEntity previousCreator = null;
            foreach (var item in RoundActions)
            {
                if (item.Creator.GetComponent<HealthPointComponent>().CheckDead() || item.Target.GetComponent<HealthPointComponent>().CheckDead()) continue;
                if (item.Target == previousCreator)
                {
                    if (previousCreator != null)
                        await TimeHelper.WaitAsync(previousCreator.JumpToTime);
                }
                await item.ApplyRound();
                previousCreator = item.Creator;
            }
            await TimeHelper.WaitAsync(1000);
            if (HeroEntities.Count == 0 || EnemyEntities.Count == 0)
            {
                HeroEntities.Clear();
                EnemyEntities.Clear();
                await TimeHelper.WaitAsync(2000);
                Publish(new CombatEndEvent());
                return;
            }
            StartCombat();
        }
        private void RefreshRoundActions()
        {
            foreach (var item in RoundActions)
            {
                Destroy(item);
            }
            RoundActions.Clear();
            foreach (var item in HeroEntities)
            {
                if (!item.Value.RoundAbility.TryMakeAction(out var turnAction)) continue;
                turnAction.Target = EnemyEntities.TryGetValue(item.Key, value: out var entity) ? entity : EnemyEntities.Values.ToArray().First();
                RoundActions.Add(turnAction);
            }
            foreach (var item in EnemyEntities)
            {
                if (!item.Value.RoundAbility.TryMakeAction(out var roundAction)) continue;
                roundAction.Target = HeroEntities.TryGetValue(item.Key, out var entity) ? entity : HeroEntities.Values.ToArray().First();
                RoundActions.Add(roundAction);
            }
        }
        #endregion
    }
    public class CombatEndEvent { }
}