using UnityEngine;

public class ArmsJumpReaction : MonoBehaviour, IArmsOffsetProvider
{
    public float jumpAmount = 0.1f;
    public float smooth = 5f;

    private Camera cam;
    private Vector3 lastCamPos;
    private Vector3 offset;

    void Start()
    {
        cam = Camera.main;
        lastCamPos = cam.transform.position;
    }

    void Update()
    {
        Vector3 delta = cam.transform.position - lastCamPos;
        if (delta.y < -0.05f)
            offset = Vector3.Lerp(offset, new Vector3(0, -jumpAmount, 0), Time.deltaTime * smooth);
        else
            offset = Vector3.Lerp(offset, Vector3.zero, Time.deltaTime * smooth);

        lastCamPos = cam.transform.position;
    }

    public Vector3 GetOffset() => offset;
    public Quaternion GetRotation() => Quaternion.identity;
}
