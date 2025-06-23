/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    A CameraAbstract implementation that will not follow the parents rotation and will always rotate around the global Y axis.
    */

    public class CameraSpectating : CameraAbstract
    {
        private Vector3 lastPosition;

        protected override void Awake()
        {
            base.Awake();
            lastPosition = observedObject.position;
        }

        protected override void Update()
        {
            base.Update();
            UpdateCameraTarget();
        }

        private void UpdateCameraTarget()
        {
            // Position
            Vector3 translation = observedObject.position - lastPosition;
            camTarget.position += translation;
            lastPosition = observedObject.position;

            //Rotation
            Vector3 newForward = Vector3.ProjectOnPlane(observedObject.position - camTarget.position, Vector3.up);
            if (newForward != Vector3.zero)
                camTarget.rotation = Quaternion.LookRotation(observedObject.position - camTarget.position, Vector3.up);
        }

        protected override Vector3 GetHorizontalRotationAxis()
        {
            return Vector3.up;
        }

        protected override Vector3 GetVerticalRotationAxis()
        {
            return camTarget.right;
        }
    }
}