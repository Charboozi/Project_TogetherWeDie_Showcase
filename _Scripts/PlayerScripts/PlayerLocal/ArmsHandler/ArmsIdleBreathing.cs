using UnityEngine;

public class ArmsIdleBreathing : MonoBehaviour, IArmsOffsetProvider
{
    public float speed = 1f;
    public float amount = 0.01f;
    public float smooth = 2f;

    private float t;
    private float current;
    private Vector3 offset;

    void Update()
    {
        t += Time.deltaTime * speed;
        float target = Mathf.Sin(t) * amount;
        current = Mathf.Lerp(current, target, Time.deltaTime * smooth);
        offset = new Vector3(0, current, 0);
    }

    public Vector3 GetOffset() => offset;
    public Quaternion GetRotation() => Quaternion.identity;
}
