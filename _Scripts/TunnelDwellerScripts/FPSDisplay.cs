/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    public class FPSDisplay : MonoBehaviour
    {
        private GUIStyle style = new GUIStyle();
        private Rect labelRect = new Rect(Screen.width - 160, 10, 150, 20);

        private float updateInterval = 0.2f;

        private float fpsAccum = 0.0f;
        private int frames = 0;
        private float timeLeft;
        private float fps = 0; // The FPS displayed

        private void Start()
        {
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            timeLeft = updateInterval;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            fpsAccum += 1 / Time.deltaTime;
            frames++;

            // Interval ended, update the FPS display
            if (timeLeft <= 0.0f)
            {
                fps = fpsAccum / frames;

                // Reset variables for the next interval
                timeLeft = updateInterval;
                fpsAccum = 0.0f;
                frames = 0;
            }
        }

        private void OnGUI()
        {
            GUI.Label(labelRect, $"FPS: {fps:F1}", style);
        }
    }
}