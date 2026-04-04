public interface ICollectible
{
    void Collect(PlayerSkillController playerSkillController);
    void CollectRpc(); // for sync with all clients
}
