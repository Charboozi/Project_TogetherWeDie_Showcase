using UnityEngine;
using Unity.Netcode;

public class GameFeedManager : NetworkBehaviour
{
    public static GameFeedManager Instance;

    [SerializeField] private Transform feedPanel;
    [SerializeField] private FeedEntry feedEntryPrefab;

    private void Awake()
    {
        Instance = this;
    }

    // ✅ Local messages (not networked)
    public void PostLocalFeedMessage(string message, Color? color = null)
    {
        Debug.Log($"📩 [Local] PostLocalFeedMessage: {message}");

        var entry = Instantiate(feedEntryPrefab, feedPanel);
        entry.SetMessage(message, color);
    }

    // ✅ Call this on server from client
    [ServerRpc(RequireOwnership = false)]
    public void PostFeedMessageServerRpc(string message, Vector4 colorVec)
    {
        Debug.Log($"📨 [ServerRpc] PostFeedMessageServerRpc called with message: {message}");
        PostFeedMessageClientRpc(message, colorVec);
    }

    // ✅ Call this from server to all clients
    [ClientRpc]
    public void PostFeedMessageClientRpc(string message, Vector4 colorVec)
    {
        Debug.Log($"📩 [ClientRpc] PostFeedMessageClientRpc received: {message}");

        var entry = Instantiate(feedEntryPrefab, feedPanel);
        entry.SetMessage(message, new Color(colorVec.x, colorVec.y, colorVec.z, colorVec.w));
    }

    // ✅ Convenience method to avoid manual Vector4 creation
    public void PostFeedMessage(string message, Color? color = null)
    {
        Vector4 colorVec = (color ?? Color.white);
        PostFeedMessageServerRpc(message, colorVec);
    }
}
