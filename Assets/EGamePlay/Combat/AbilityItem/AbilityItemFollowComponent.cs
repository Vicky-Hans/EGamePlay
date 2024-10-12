namespace EGamePlay.Combat
{
    public class AbilityItemFollowComponent : Component
    {
        public long FollowInterval { get; set; } = 50;
        public long NextFollowTime { get; set; }
        public override void Awake() { }
        public override void Update()
        {
            var abilityItem = GetEntity<AbilityItem>();
            var moveComp = abilityItem.GetComponent<AbilityItemPathMoveComponent>();
            moveComp.FollowMove();
        }
    }
}