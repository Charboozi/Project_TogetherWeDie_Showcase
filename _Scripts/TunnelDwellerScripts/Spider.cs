/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;
using Unity.Netcode;

namespace ProceduralSpider
{
    /*
    This component provides the spider movement behaviour and is also responsible for glueing the spider to the surfaces around it.
    This is accomplished by creating a fake gravitational force in the direction of the surface normal it is standing on.
    The surface normal is determined by spherical raycasting downwards, as well as forwards for wall-climbing.
    The spider does not move on its own.
    A controller must call the provided functions SetVelocity and Jump from code for the desired control.
    */

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Spider : NetworkBehaviour
    {
        [Header("Movement")]
        [Tooltip("The turn speed of the spider. Determines how fast the spider will turn.")]
        [Range(1, 5)]
        public float turnSpeed = 2;

        [Header("Grounding")]
        [Tooltip("If enabled, the spider can walk on walls and ceilings and will rotate to match the ground normal. If disabled, it can't climb them and will not rotate to match the ground.")]
        public bool isWallWalking = true;

        [Tooltip("The layer on which the spider can walk. Will stick to colliders in this layer.")]
        public LayerMask walkableLayer = 1; //Default Layer

        [Tooltip("The spider uses fake gravity to either stick it to surfaces or make it fall. This is a multiplier on it.")]
        [Range(1, 10)]
        public float gravityMultiplier = 2;

        [Tooltip("Determines how fast the spider will rotate to match the ground surface it sticks to.")]
        [Range(1, 10)]
        public float groundAdjustRotationSpeed = 6;

        [Tooltip("Determines how fast the spider will rotate to match the ground surface it climbs onto.")]
        [Range(1, 10)]
        public float wallAdjustRotationSpeed = 2;

        [Tooltip("How long the forwards sphere ray is. Higher value will make spider notice walls to climb earlier.")]
        [Range(0.0f, 1.0f)]
        public float forwardRayLength = 0.4f;

        [Tooltip("How long the downwards sphere ray is. Higher value will make it notice ground underneath more aggressively.")]
        [Range(0.0f, 1.0f)]
        public float downRayLength = 0.99f;

        [Header("Debug")]
        [Tooltip("Enable this to draw debug drawings in the viewport.")]
        public bool showDebug = false;

        private CapsuleCollider col;
        private Rigidbody rb;
        private float downRayRadius;
        private Vector3 goalVelocity;
        private Vector3 currentVelocity;
        private int lastJumpFrame;
        private RaycastHit hitInfo;
        private GroundInfo groundInfo;
        private float timeStandingStill;

        private void Awake()
        {
            col = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();

            //Initialize the two Sphere Casts
            downRayRadius = 0.99f * GetColliderHeight() / 2;
        }

        void FixedUpdate()
        {
            groundInfo = GroundCheck();

            //Rotate to ground normal
            if (isWallWalking)
            {
                float normalAdjustSpeed = (groundInfo.groundType == GroundType.Wall) ? wallAdjustRotationSpeed : groundAdjustRotationSpeed;
                Vector3 slerpNormal = Vector3.Slerp(transform.up, groundInfo.normal, Time.fixedDeltaTime * normalAdjustSpeed);
                transform.rotation = GetLookRotation(Vector3.ProjectOnPlane(transform.right, slerpNormal), slerpNormal);
            }

            // Apply Gravity
            if (groundInfo.distance > GetGravityOffDistance())
            {
                rb.AddForce(-groundInfo.normal * gravityMultiplier * 9.81f); //Important using the ground normal and not the lerping normal here
            }

            //Apply movement
            Vector3 velocityXZ = Vector3.ProjectOnPlane(currentVelocity, transform.up);
            if (velocityXZ != Vector3.zero)
            {
                //transform.position += velocityXZ * Time.fixedDeltaTime;
            }

            //Apply Rotation to face velocity
            Vector3 goalVelocityXZ = Vector3.ProjectOnPlane(goalVelocity, transform.up);
            if (goalVelocityXZ != Vector3.zero)
            {
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(goalVelocityXZ.normalized, transform.up), turnSpeed);
            }

            // Update the moving status
            if (transform.hasChanged)
            {
                timeStandingStill = 0;
                transform.hasChanged = false;
            }
            else
            {
                timeStandingStill += Time.fixedDeltaTime;
            }
        }

        public void SetVelocity(Vector3 v)
        {
            goalVelocity = v;

            // Rescale and clamp the velocity
            float magnitude = v.magnitude;
            if (magnitude > 0)
            {
                Vector3 dir = v / magnitude;
                float directionDamp = Mathf.Pow(Mathf.Clamp(Vector3.Dot(dir, transform.forward), 0, 1), 2); //Blocks movement not facing forwards
                float sizeScale = GetColliderHeight(); //Scales with spider size
                float maxMagnitude = downRayRadius / Time.fixedDeltaTime; //Upper Limit so we are not faster than the ground we can register
                float newMagnitude = Mathf.Clamp(magnitude * directionDamp * sizeScale, 0, maxMagnitude);
                v = newMagnitude * dir;
            }

            currentVelocity = v;
        }

        public void Jump(float height)
        {
            int jumpCoolDownFrames = 60;
            if (lastJumpFrame + jumpCoolDownFrames < Time.frameCount && groundInfo.IsGrounded())
            {
                rb.AddForce(groundInfo.normal * 100 * height * GetColliderHeight());
                lastJumpFrame = Time.frameCount;
            }
        }


        private GroundInfo GroundCheck()
        {
            if (IsServer)
            {
                // Server does real physics raycast
                GroundInfo result = GroundInfo.CreateEmpty();
                Vector3 c = GetColliderCenter();
                QueryTriggerInteraction q = QueryTriggerInteraction.Ignore;

                if (isWallWalking)
                {
                    if (Physics.SphereCast(transform.position, GetForwardRayRadius(), transform.forward, out hitInfo, GetForwardRayLength(), walkableLayer, q))
                    {
                        result = new GroundInfo(GroundType.Wall, hitInfo.normal.normalized, Vector3.Distance(c, hitInfo.point) - GetColliderLength() / 2);
                    }
                    else if (Physics.SphereCast(transform.position, GetDownRayRadius(), -transform.up, out hitInfo, GetDownRayLength(), walkableLayer, q))
                    {
                        result = new GroundInfo(GroundType.Ground, hitInfo.normal.normalized, Vector3.Distance(c, hitInfo.point) - GetColliderHeight() / 2);
                    }
                    else
                    {
                        result = new GroundInfo(GroundType.Ground, transform.up, GetColliderHeight() / 2);
                    }
                }
                else
                {
                    result.normal = Vector3.up;
                }

                // Update a NetworkVariable with the result here
                UpdateGroundInfoClientRpc(result.normal, result.distance, (int)result.groundType);

                return result;
            }
            else
            {
                // On Clients: use synced ground normal
                return new GroundInfo((GroundType)groundType.Value, groundNormal.Value, groundDistance.Value);
            }
        }

        private NetworkVariable<Vector3> groundNormal = new NetworkVariable<Vector3>();
        private NetworkVariable<float> groundDistance = new NetworkVariable<float>();
        private NetworkVariable<int> groundType = new NetworkVariable<int>();

        [ClientRpc]
        private void UpdateGroundInfoClientRpc(Vector3 normal, float distance, int type)
        {
            groundNormal.Value = normal;
            groundDistance.Value = distance;
            groundType.Value = type;
        }

        /*
        * Returns the rotation with specified right and up direction
        */
        private Quaternion GetLookRotation(Vector3 right, Vector3 up)
        {
            if (up == Vector3.zero || right == Vector3.zero) return Quaternion.identity;
            right.Normalize();
            up.Normalize();
            if (Mathf.Abs(Vector3.Dot(up, right)) == 1) return Quaternion.identity;
            Vector3 forward = Vector3.Cross(right, up);
            return Quaternion.LookRotation(forward, up);
        }

        public GroundInfo GetGroundInfo()
        {
            return groundInfo;
        }

        public float GetColliderHeight()
        {
            if (col == null) col = GetComponent<CapsuleCollider>();
            return 2 * col.radius * transform.lossyScale.y;
        }

        public float GetColliderLength()
        {
            if (col == null) col = GetComponent<CapsuleCollider>();
            return col.height * transform.lossyScale.z;
        }

        public Vector3 GetColliderCenter()
        {
            if (col == null) col = GetComponent<CapsuleCollider>();
            return col.bounds.center;
        }

        public Vector3 GetColliderBottomPoint()
        {
            if (col == null) col = GetComponent<CapsuleCollider>();
            return col.bounds.center - new Vector3(0, GetColliderHeight() / 2, 0);
        }

        public float GetGravityOffDistance()
        {
            return 0.1f * GetColliderHeight();
        }

        public float GetTimeStandingStill()
        {
            return timeStandingStill;
        }

        public float clientRaycastMultiplier = 1.2f; // Adjust in Inspector if needed

        private float GetRaycastMultiplier() => IsServer ? 1f : clientRaycastMultiplier;

        private float GetDownRayLength()
        {
            return downRayLength * GetColliderHeight() * 2.5f * GetRaycastMultiplier();
        }

        private float GetForwardRayLength()
        {
            return forwardRayLength * GetColliderLength() * 2.5f * GetRaycastMultiplier();
        }


        public float GetDownRayRadius()
        {
            return downRayRadius;
        }

        public float GetForwardRayRadius()
        {
            return 0.49f * GetColliderHeight();
        }

        private void DrawDebug()
        {
            //Draw the two Sphere Rays
            Vector3 p = transform.position;
            GizmosDrawer.DrawSphereRay(p, -transform.up, GetDownRayLength(), GetDownRayRadius(), 5, Color.green);
            GizmosDrawer.DrawSphereRay(p, transform.forward, GetForwardRayLength(), GetForwardRayRadius(), 5, Color.blue);

            //Draw the current transform.up a
            Gizmos.color = new Color(1, 0.5f, 0, 1);
            Gizmos.DrawLine(transform.position, transform.position + GetColliderHeight() * transform.up);

            //Draw Bottom Point
            Vector3 borderPoint = GetColliderBottomPoint();
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(GetColliderBottomPoint(), Vector3.one * 0.1f);

            //Draw the Gravity off distance
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(borderPoint, borderPoint + GetGravityOffDistance() * -transform.up);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!showDebug) return;
            if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
            if (!UnityEditor.EditorApplication.isPlaying) Awake();
            DrawDebug();
        }
#endif
    }
}