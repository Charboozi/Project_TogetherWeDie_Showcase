/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

//#define ENABLE_DEBUG_LOGGING

using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace ProceduralSpider
{
    /*
    This component manages stepping behaviour of the spider for all legs.
    It will find all IKChains defined in children, which are further referred to as legs.
    It will set the IK targets for each leg to mimic stepping.
    A leg desires to step if the IK solver can not solve the leg sufficiently anymore.
    This component will orchestrate each legs desire to step and make sure asynchronicity to other legs is adhered to.
    A new step target is found using a system of ray casts and the current velocity.
    The leg will move in an arc to reach the new target.
    */

    [RequireComponent(typeof(Spider))]
    public class IKStepManager : NetworkBehaviour
    {
        /*
        We store the legs that want to step in a queue and perform the stepping in the order of the queue.
        However, we will not wait for a leg to be unblocked, we will simply continue iterating and give other legs the chance to step.
        A leg is blocked from stepping if the defined asynchronous legs are currently already stepping.
        */

        public LayerMask stepLayer = 1; //Default Layer
        public float stepHeight = 0.5f;
        public AnimationCurve stepCurve = new AnimationCurve(new Keyframe(0, 0, 0, 3), new Keyframe(0.5f, 1), new Keyframe(1, 0, -3, 0));
        public float stepTimeMultiplier = 0.6f;
        public float stepTimeMax = 0.2f;
        public bool showDebug = false;
        public IKStepper[] ikSteppers = new IKStepper[0];
        public IKChain[] ikChains = new IKChain[0];

        private const float waitTimeout = 0.1f;
        private const float disallowSteppingAfterSecondsStill = 0.1f;

        private int n;
        private Dictionary<IKChain, int> ikChainToIndexMap;
        private StepAnimation[] stepAnimation;
        private bool[] waitForStep;
        private float[] waitForStepTime;
        private Spider spider;

        private SpiderLocalMotionEstimator motionEstimator;

        private void Awake()
        {
            motionEstimator = GetComponent<SpiderLocalMotionEstimator>();

            Debug.Assert(ikChains.Length == ikSteppers.Length, "There should be exactly one IKStepper for each IKChain.");
            n = ikChains.Length;
            spider = GetComponent<Spider>();
            ikChainToIndexMap = new Dictionary<IKChain, int>();
            waitForStep = new bool[n];              //Init to false by default
            stepAnimation = new StepAnimation[n];   //Init to null by default
            waitForStepTime = new float[n];         //Init to 0 by default
            for (int i = 0; i < n; i++)
            {
                ikSteppers[i].Initialize(spider, ikChains[i]);
                ikChainToIndexMap[ikChains[i]] = i;
            }
        }

        private void OnEnable()
        {
            EnableAll();
        }

        private void OnDisable()
        {
            DisableAll();
        }

        private void EnableAll()
        {
            for (int i = 0; i < n; i++)
            {
                ikChains[i].SetTarget(new IKTargetInfo(ikChains[i].GetEndEffector().position, spider.transform.up, false));
                ikChains[i].enabled = true;
                waitForStep[i] = false;
                waitForStepTime[i] = 0;
                stepAnimation[i] = ikSteppers[i].CreateStep(0, 0, stepCurve, stepLayer); // Instant snap to target
            }
        }

        private void DisableAll()
        {
            for (int i = 0; i < n; i++)
            {
                ikChains[i].enabled = false;
                waitForStep[i] = false;
                waitForStepTime[i] = 0;
                stepAnimation[i] = null;
            }
        }

        private void Update()
        {
            if (IsServer)
            {
                // Only the server decides stepping
                Vector3 smoothVelocity = motionEstimator.LocalVelocity;

                for (int i = 0; i < n; i++)
                {
                    UpdateStep(i, Time.deltaTime);
                    EnqueueForStepIfNeeded(i, smoothVelocity);
                }

                for (int i = 0; i < n; i++)
                {
                    if (IsWaiting(i))
                    {
                        if (IsAllowedToStep(i)) StartStep(i);
                        else WaitStep(i);
                    }
                }
            }
            else
            {
                // Clients only animate existing steps
                for (int i = 0; i < n; i++)
                {
                    UpdateStep(i, Time.deltaTime);
                }
            }
        }


        private void EnqueueForStepIfNeeded(int i, Vector3 velocity)
        {
            if (IsWaiting(i)) return;
            if (IsStepping(i)) return;
            if (velocity.magnitude < 0.01f) return; // Optional: only step if actually moving
            if (spider.GetTimeStandingStill() > disallowSteppingAfterSecondsStill) return;
            if (!ikSteppers[i].IsStepNeeded()) return;
            waitForStep[i] = true;
        }

        private bool IsAllowedToStep(int i)
        {
            if (GetWaitTime(i) > waitTimeout) return true;
            if (!ikChains[i].GetTarget().isGround) return true;

            foreach (IKChain asyncChain in ikSteppers[i].asyncChains)
            {
                if (ikChainToIndexMap.TryGetValue(asyncChain, out int asyncIndex))
                {
                    if (IsStepping(asyncIndex))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void StartStep(int i)
        {
            ikChains[i].SetSolveEnabled(true);
            float duration = CalculateStepDuration(i);
            float height = stepHeight * spider.GetColliderHeight();
            Debug.Assert(!IsStepping(i));
            stepAnimation[i] = ikSteppers[i].CreateStep(duration, height, stepCurve, stepLayer);
            waitForStep[i] = false;
            waitForStepTime[i] = 0;

            // --- ADD THIS ---
            IKTargetInfo target = stepAnimation[i].GetTarget(); // Get final to-target
            NotifyStartStepClientRpc(i, target.position, target.normal, height, duration);
        }

        [ClientRpc]
        void NotifyStartStepClientRpc(int legIndex, Vector3 targetPos, Vector3 targetNormal, float height, float duration)
        {
            if (IsServer) return;

            var target = new IKTargetInfo(targetPos, targetNormal, true);
            ikChains[legIndex].SetTarget(target);

            stepAnimation[legIndex] = new StepAnimation(ikChains[legIndex].GetTarget(), target, duration, height, stepCurve, spider.transform);
        }

        private void UpdateStep(int i, float dt)
        {
            if (!IsStepping(i)) return;
            IKTargetInfo target = stepAnimation[i].Update(dt, out bool isDone);
            ikChains[i].SetTarget(target);
            if (isDone) EndStep(i);
        }

        private void EndStep(int i)
        {
            stepAnimation[i] = null;
        }

        private void WaitStep(int i)
        {
            ikChains[i].SetSolveEnabled(false); // Pause IK Solving for the legs that are waiting
            waitForStepTime[i] += Time.deltaTime;
        }

        private bool IsStepping(int i)
        {
            return stepAnimation[i] != null;
        }

        private bool IsWaiting(int i)
        {
            return waitForStep[i];
        }

        private float GetWaitTime(int i)
        {
            return waitForStepTime[i];
        }

        private float CalculateStepDuration(int i)
        {
            float k = stepTimeMultiplier * spider.GetColliderHeight(); // At velocity=1, this is the duration
            float speed = ikChains[i].GetEndEffectorVelocity().magnitude;
            return (speed == 0) ? stepTimeMax : Mathf.Clamp(k / speed, 0, stepTimeMax);
        }

        private void DebugLogQueue()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < n; i++) names.Add(IsWaiting(i) ? ikChains[i].name : "");
            Debug.Log($"Queue: [{string.Join<string>(", ", names)}]");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
            if (!showDebug) return;
            if (!UnityEditor.EditorApplication.isPlaying) Awake();

            for (int i = 0; i < n; i++)
            {
                ikSteppers[i].DrawDebug();
                ikChains[i].DrawDebug();
            }
        }
#endif
    }
}