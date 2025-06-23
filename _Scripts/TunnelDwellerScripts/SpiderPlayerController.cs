/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
     * This component will apply movement to Spider Component.
     * It uses player input and will request movement relative to the Camera Third Person Component that has to exist on a child object.
     */

    [RequireComponent(typeof(Spider))]
    [DefaultExecutionOrder(-1)] // Controllers should update early. This makes sure the requested movement is used by the spider on the same frame.
    public class SpiderPlayerController : MonoBehaviour
    {
        [Tooltip("The camera of this controller. The input will be relative to this camera.")]
        public CameraThirdPerson playerCamera;

        [Tooltip("The speed at which the spider will walk.")]
        [Range(0, 20)]
        public float walkSpeed = 4;

        [Tooltip("The speed at which the spider will run.")]
        [Range(0, 20)]
        public float runSpeed = 6;

        [Tooltip("The force at which the spider will jump.")]
        public float jumpForce = 7;

        [Tooltip("The key code associated to making the spider run.")]
        public KeyCode keyCodeRun = KeyCode.LeftShift;

        [Tooltip("The key code associated to making the spider jump.")]
        public KeyCode keyCodeJump = KeyCode.Space;

        private Spider spider;

        void Awake()
        {
            spider = GetComponent<Spider>();
            if (playerCamera == null) Debug.LogWarning("SpiderController has no camera set. Input will be zero.");
        }

        void Update()
        {
            //Set Velocity
            Vector3 input = GetInput();
            float speed = Input.GetKey(keyCodeRun) ? runSpeed : walkSpeed;
            spider.SetVelocity(input * speed);

            //Jump
            if (Input.GetKeyDown(keyCodeJump)) spider.Jump(jumpForce);
        }

        private Vector3 GetInput()
        {
            if (playerCamera == null) return Vector3.zero;

            //Create the input vector
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (input.magnitude > 1) input.Normalize();

            //Create the coordinate transformation where X->CamRight Y->SpiderUp Z->CamForward
            Vector3 up = spider.transform.up;
            Vector3 forward = Vector3.ProjectOnPlane(playerCamera.GetCameraTarget().forward, up).normalized;
            Quaternion input2Move = Quaternion.LookRotation(forward, up);

            // Note: We are using spiders up vector and not its ground normal. Therefore movement isn't strictly on ground plane.
            // However, the up vector smoothly adjusts to the ground normal, which suffices. Fake gravity should prohibit us from any normal movement.

            return input2Move * input;
        }
    }
}