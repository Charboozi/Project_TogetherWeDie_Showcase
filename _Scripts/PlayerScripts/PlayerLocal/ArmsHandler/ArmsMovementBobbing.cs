using UnityEngine;

public class WeaponMovementBobbing : MonoBehaviour, IArmsOffsetProvider
{
    public float speed = 5f;
    public float amount = 0.05f;

    private float timer = 0f;
    private Vector2 moveInput;
    private Vector3 offset;

    void OnEnable() => PlayerInput.OnMoveInput += SetInput;
    void OnDisable() => PlayerInput.OnMoveInput -= SetInput;

    void SetInput(Vector2 input) => moveInput = input;

    void Update()
    {
        float mag = moveInput.magnitude;
        if (mag > 0.1f)
        {
            timer += Time.deltaTime * speed;
            offset = new Vector3(Mathf.Sin(timer) * amount, Mathf.Cos(timer * 2f) * amount * 0.5f, 0);
        }
        else
        {
            timer = 0;
            offset = Vector3.zero;
        }
    }

    public Vector3 GetOffset() => offset;
    public Quaternion GetRotation() => Quaternion.identity;
}
