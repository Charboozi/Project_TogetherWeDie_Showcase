/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    A simple component that will switch between SpiderPlayerController and SpiderNPCController with a specified key code.
    It will always start with the former enabled.
    */

    [RequireComponent(typeof(SpiderPlayerController))]
    [RequireComponent(typeof(SpiderNPCController))]
    public class SpiderModeSwitch : MonoBehaviour
    {
        public KeyCode keyCodeSwitch = KeyCode.Tab;
        public Camera playerCamera;
        public Camera npcCamera;

        private SpiderPlayerController playerController;
        private SpiderNPCController npcController;

        void Awake()
        {
            playerController = GetComponent<SpiderPlayerController>();
            npcController = GetComponent<SpiderNPCController>();
        }

        void Start()
        {
            //Enable Player
            if (playerCamera) playerCamera.enabled = true;
            if (playerController) playerController.enabled = true;

            //Disable NPC
            if (npcCamera) npcCamera.enabled = false;
            if (npcController) npcController.enabled = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(keyCodeSwitch))
            {
                if (playerCamera) playerCamera.enabled = !playerCamera.enabled;
                if (npcCamera) npcCamera.enabled = !npcCamera.enabled;
                if (npcController) npcController.enabled = !npcController.enabled;
                if (playerController) playerController.enabled = !playerController.enabled;
            }
        }
    }
}