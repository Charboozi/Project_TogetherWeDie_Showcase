/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    public enum VisibilityEvent
    {
        None,
        GotVisible,
        GotInvisible
    }

    public enum Visibility
    {
        Undetermined,
        Visible,
        Invisible
    }
    /*
    A helper class that supplies visibility information.
    The spider uses it to cull expensive updates such as IK solving and Procedural Animation when applicable.
    */
    [DefaultExecutionOrder(-1)] // Should be executed early. We need this information frame perfect.
    public class VisibilityCulling : MonoBehaviour
    {
        [Tooltip("If enabled, will regard the object as invisible if its a certain distance away from the current camera.")]
        public bool useCameraDistanceCulling = true;

        [Tooltip("The distance from the main camera at which we regard the object as culled if distance culling is enabled.")]
        public float cullDistance = 300;

        [Tooltip("The mono behaviours that should be disabled when this component culls.")]
        public MonoBehaviour[] monoBehavioursToDisable;

        private Renderer rend;
        private Visibility visibility = Visibility.Undetermined;
        private VisibilityEvent visibilityEvent = VisibilityEvent.None;

        void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
            if (!rend) Debug.LogWarning($"The Visibility Culling Component did not find a renderer. Dependent components might assume {this.name} is invisible and perform culling.");
        }

        void Update()
        {
            UpdateStatus();
            ApplyCulling();
        }

        public void UpdateStatus()
        {
            if (!rend) return;
            bool isVisible = rend.isVisible; //Time.time % 4 < 2;

            if (isVisible && useCameraDistanceCulling)
            {
                Camera camera = Camera.main;
                if (camera && Vector3.SqrMagnitude(rend.transform.position - camera.transform.position) > cullDistance * cullDistance)
                {
                    isVisible = false;
                }
            }

            if (isVisible && visibility != Visibility.Visible)
            {
                visibilityEvent = VisibilityEvent.GotVisible;
            }
            else if (!isVisible && visibility != Visibility.Invisible)
            {
                visibilityEvent = VisibilityEvent.GotInvisible;
            }
            else
            {
                visibilityEvent = VisibilityEvent.None;
            }
            visibility = isVisible ? Visibility.Visible : Visibility.Invisible;
        }

        public void ApplyCulling()
        {
            foreach (MonoBehaviour behaviour in monoBehavioursToDisable)
            {
                switch (visibilityEvent)
                {
                    case VisibilityEvent.GotVisible:
                        if (behaviour && !behaviour.enabled) behaviour.enabled = true;
                        break;
                    case VisibilityEvent.GotInvisible:
                        if (behaviour && behaviour.enabled) behaviour.enabled = false;
                        break;
                }
            }
        }

        public VisibilityEvent GetVisibilityEvent()
        {
            return visibilityEvent;
        }

        public bool IsVisible()
        {
            return visibility == Visibility.Visible;
        }
    }
}