public interface ICollectible
{
    void Collect();
    void CollectRpc(); // for sync with all clients
}
