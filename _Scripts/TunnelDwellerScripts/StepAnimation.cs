/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    public class StepAnimation
    {
        private IKTargetInfo from;
        private IKTargetInfo to;
        private float duration;
        private float height;
        private AnimationCurve curve;
        private Transform transform; // Is used to determine up vector

        private float time = 0;

        public StepAnimation(IKTargetInfo from, IKTargetInfo to, float duration, float height, AnimationCurve curve, Transform transform)
        {
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.height = height;
            this.curve = curve;
            this.transform = transform;
        }

        public IKTargetInfo Update(float dt, out bool isDone)
        {
            time += dt;
            float tN = (duration != 0) ? Mathf.Clamp(time / duration, 0, 1) : 1;
            Vector3 position = Vector3.Lerp(from.position, to.position, tN) + height * curve.Evaluate(tN) * transform.up;
            Vector3 normal = Vector3.Slerp(from.normal, to.normal, tN).normalized;
            if (Vector3.Angle(from.normal, to.normal) > 45f)
            normal = to.normal; // Snap to prevent spinning
            isDone = time >= duration;
            bool isGrounded = isDone ? to.isGround : false;
            return new IKTargetInfo(position, normal, isGrounded);
        }

        public IKTargetInfo GetTarget()
        {
            return to;
        }
    }
}