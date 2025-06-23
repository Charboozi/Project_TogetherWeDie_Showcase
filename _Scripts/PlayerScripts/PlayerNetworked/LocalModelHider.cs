using UnityEngine;
using Unity.Netcode;

public class LocalModelHider : NetworkBehaviour
{
    [SerializeField] private GameObject modelRoot;
    [SerializeField] private GameObject nameTagRoot; // Drag the nametag canvas here

    void Start()
    {
        if (!IsOwner)
            return;

        // Hide model
        if (modelRoot != null)
        {
            foreach (var skinnedMesh in modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                skinnedMesh.enabled = false;

            foreach (var mesh in modelRoot.GetComponentsInChildren<MeshRenderer>(true))
                mesh.enabled = false;
        }

        // Hide nametag
        if (nameTagRoot != null)
            nameTagRoot.SetActive(false);
    }
}
