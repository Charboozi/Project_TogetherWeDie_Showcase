using UnityEngine;

public class ArmsLookSway : MonoBehaviour, IArmsOffsetProvider
{
    public float amount = 0.02f;

    private Vector2 mouseDelta;
    private Vector3 offset;

    void OnEnable() => PlayerInput.OnMouseLook += SetDelta;
    void OnDisable() => PlayerInput.OnMouseLook -= SetDelta;

    void SetDelta(Vector2 delta) => mouseDelta = delta;

    void Update()
    {
        offset = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * amount;
    }

    public Vector3 GetOffset() => offset;
    public Quaternion GetRotation() => Quaternion.identity;
}
