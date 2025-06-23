/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralSpider
{
    /*
    Simple class for some cursor locking and scene resetting.
    */
    public class DemoSceneSettings : MonoBehaviour
    {
        private void Awake()
        {
            // Lock Cursor in Build
            Cursor.lockState = CursorLockMode.Locked;

            //Unlock Cursor in Editor
#if UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
#endif
        }

        private void Update()
        {
            //On Press reset scene
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}