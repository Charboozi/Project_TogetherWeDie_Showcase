/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    This component will apply movement to Spider Component.
    It can either randomly move around, or move towards a given target (or a blend between the two).
    It can frequently stop movement erratically to mimic the stop and go nature of spiders.
    It uses perlin noise for the randomness of the movement.
    */

    [RequireComponent(typeof(Spider))]
    [DefaultExecutionOrder(-1)] // Controllers should update early. This makes sure the requested movement is used by the spider on the same frame.
    public class SpiderNPCController : MonoBehaviour
    {
        [Header("Speed")]
        [Tooltip("The speed at which we will move.")]
        [Range(0, 20)]
        public float speed = 4;

        [Header("Movement Random")]
        [Tooltip("Describes how often the movement direction will change. A higher value corresponds to changing direction more often. A lower value corresponds to committing to a direction for longer.")]
        [Range(0, 1)]
        public float randomMovementFrequency = 0.25f; //Was [0.01, 0..09]

        [Header("Movement Follow")]
        [Tooltip("Blend value between the random movement direction and the follow direction. A Value of 0 corresponds to full random movement. A value of 1 corresponds to full target follow.")]
        [Range(0, 1)]
        public float followWeight = 0;

        [Tooltip("If this target is set and a non-zero follow weight is set, this controller will move towards this transform. Leave unassigned if you want to set target position manually by calling SetTargetPosition.")]
        public Transform followTarget;

        [Header("Stop And Go")]
        [Tooltip("Weight that describes how much we will move and how much we will stop. A value of 0 corresponds to always moving. A value of 1 corresponds to always stopping. A value of 0.8 corresponds to 80% moving and 20% stopping.")]
        [Range(0, 1)]
        public float stopAndGoWeight = 0.4f;

        [Tooltip("Describes how frequent the movement will stop. A higher value corresponds to switching between stop and go more often. A lower value corresponds to committing to either stop or go for longer.")]
        [Range(0, 1)]
        public float stopAndGoFrequency = 0.6f;

        [Header("Debug")]
        [Tooltip("If enabled, will draw debug drawings to viewport.")]
        public bool showDebug;

        private Spider spider;
        private float[] perlinOffsets;
        private Vector3 x;
        private Vector3 y;
        private Vector3 z;
        private Vector3 velocity;
        private Vector3 velocitySmooth;
        private Vector3 targetPosition;

        private void Awake()
        {
            spider = GetComponent<Spider>();
            Random.InitState(System.DateTime.Now.Millisecond + this.GetInstanceID());

            //Initialize perlin offsets to be random and different from each other
            perlinOffsets = new float[3];
            for (int i = 0; i < perlinOffsets.Length; i++)
            {
                perlinOffsets[i] = (1 + Random.value) * 100000;
            }

            //Initialize Coordinate System
            z = transform.forward;
            x = transform.right;
            y = transform.up;
        }

        private void Update()
        {
            if (followTarget != null) targetPosition = followTarget.position;
            UpdateCoordinateSystem();
            UpdateVelocity();
        }

        private void UpdateCoordinateSystem()
        {
            Vector3 newY = spider.GetGroundInfo().normal;
            Quaternion fromTo = Quaternion.FromToRotation(y, newY);
            y = newY;
            x = fromTo * x;
            z = fromTo * z;
        }

        private void UpdateVelocity()
        {
            Vector3 direction = Vector3.zero;
            bool isMoving = SamplePerlinNoiseAsBinary(0, stopAndGoFrequency, 1 - stopAndGoWeight);
            if (isMoving)
            {
                if (targetPosition != Vector3.zero)
                {
                    // At weight=0.5, the two directions tend to be exactly opposite (if target is very close).
                    // The slerp then becomes instable as it jitters between +- direction.
                    // We therefore forbid the range (0.4, 0.6) so that one vector is always favored.
                    float weight = followWeight < 0.5f ? Mathf.Clamp(followWeight, 0, 0.4f) : Mathf.Clamp(followWeight, 0.6f, 1);
                    direction = Vector3.Slerp(GetRandomDirection(), GetToTargetDirection(), weight);
                }
                else
                {
                    direction = GetRandomDirection();
                }
            }
            velocity = direction * speed;
            velocitySmooth = Vector3.RotateTowards(velocitySmooth, velocity, Mathf.PI * Time.deltaTime, Mathf.Infinity);  //180°/sec
            spider.SetVelocity(velocitySmooth);
        }

        private Vector3 GetRandomDirection()
        {
            float tScale = randomMovementFrequency / 5;
            float vertical = 2 * (SamplePerlinNoise(1, tScale) - 0.5f);
            float horizontal = 2 * (SamplePerlinNoise(2, tScale) - 0.5f);
            return (x * horizontal + z * vertical).normalized;
        }

        private Vector3 GetToTargetDirection()
        {
            return Vector3.ProjectOnPlane(targetPosition - transform.position, y).normalized;
        }

        private float SamplePerlinNoise(int offsetIdx, float tScale)
        {
            return Mathf.PerlinNoise1D(perlinOffsets[offsetIdx] + (Time.time * tScale));
        }

        private bool SamplePerlinNoiseAsBinary(int offsetIdx, float tScale, float threshold)
        {
            return SamplePerlinNoise(offsetIdx, tScale) < threshold;
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }

        public void SetTargetFollowWeight(float weight)
        {
            followWeight = weight;
        }

        void DrawDebug()
        {
            Vector3 p = spider.transform.position;
            GizmosDrawer.DrawLine(p, p + 2 * spider.GetColliderLength() * velocity.normalized, Color.black);
            GizmosDrawer.DrawLine(p, p + 2 * spider.GetColliderLength() * velocitySmooth.normalized, Color.magenta);
            GizmosDrawer.DrawLine(p, p + 2 * spider.GetColliderLength() * GetRandomDirection(), Color.cyan);
            GizmosDrawer.DrawLine(p, p + 2 * spider.GetColliderLength() * GetToTargetDirection(), Color.yellow);
            GizmosDrawer.DrawLine(p, p + 2 * spider.GetColliderHeight() * x, Color.red);
            GizmosDrawer.DrawLine(p, p + 2 * spider.GetColliderHeight() * z, Color.blue);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!showDebug) return;
            if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
            if (!UnityEditor.EditorApplication.isPlaying) return;
            DrawDebug();
        }
#endif
    }
}