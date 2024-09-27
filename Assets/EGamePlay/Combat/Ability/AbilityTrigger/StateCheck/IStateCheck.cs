namespace EGamePlay.Combat
{
    public interface IStateCheck
    {
        bool IsInvert { get; }
        bool CheckWith(Entity target);
    }
}
