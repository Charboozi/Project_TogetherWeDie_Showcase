/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    A CameraAbstract implementation that stays parented to the parent transform and will follow the parents rotation.
    It will rotate around the parents Y-Axis.
    How much it will follow the rotation can be customized.
    */

    public class CameraThirdPerson : CameraAbstract
    {
        [Header("Observed Object Follow")]
        [Range(0, 1)]
        [Tooltip("If set to 0, camera will not follow any X-Axis rotation of the observed object. If set to 1, camera will fully follow it.")]
        public float rollIgnore = 0.5f;

        [Tooltip("If enabled, the camera will not follow any Y-Axis Rotation of the observed object.")]
        public bool yawIgnore = true;

        private Quaternion observedObjectPrevRotation;


        protected override void Awake()
        {
            base.Awake();
            observedObjectPrevRotation = observedObject.rotation;
            camTarget.parent = observedObject;
        }

        protected override void Update()
        {
            base.Update();

            if (yawIgnore)
            {
                Vector3 axis = GetHorizontalRotationAxis();
                Vector3 v = Vector3.ProjectOnPlane(observedObjectPrevRotation * Vector3.right, axis);
                Vector3 w = Vector3.ProjectOnPlane(observedObject.rotation * Vector3.right, axis);
                float angle = Vector3.SignedAngle(v, w, axis);
                RotateCameraHorizontal(-angle);
            }

            if (rollIgnore > 0)
            {
                Vector3 axis = GetVerticalRotationAxis();
                Vector3 v = Vector3.ProjectOnPlane(observedObjectPrevRotation * Vector3.up, axis);
                Vector3 w = Vector3.ProjectOnPlane(observedObject.rotation * Vector3.up, axis);
                float angle = Vector3.SignedAngle(v, w, axis);
                RotateCameraVertical(rollIgnore * -angle);
            }
            observedObjectPrevRotation = observedObject.rotation;
        }

        protected override Vector3 GetHorizontalRotationAxis()
        {
            return observedObject.transform.up;
        }

        protected override Vector3 GetVerticalRotationAxis()
        {
            return camTarget.right;
        }
    }
}