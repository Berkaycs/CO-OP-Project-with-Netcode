public interface ICollectible
{
    void Collect(PlayerSkillController playerSkillController, CameraShake cameraShake);
    void CollectRpc(); // for sync with all clients
}
