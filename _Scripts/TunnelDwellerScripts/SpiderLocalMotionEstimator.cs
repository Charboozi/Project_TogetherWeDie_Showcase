using UnityEngine;

public class SpiderLocalMotionEstimator : MonoBehaviour
{
    public Vector3 LocalVelocity { get; private set; }

    private Vector3 previousPosition;

    void Start()
    {
        previousPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        LocalVelocity = (currentPosition - previousPosition) / Time.fixedDeltaTime;
        previousPosition = currentPosition;
    }
}
