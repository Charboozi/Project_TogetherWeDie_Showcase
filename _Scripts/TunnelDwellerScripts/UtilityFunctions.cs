/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    class UtilityFunctions
    {
        static public float GetParentColliderScale(Transform transform)
        {
            Collider collider = transform.GetComponentInParent<Collider>();
            if (collider != null)
            {
                Vector3 bbSize = collider.bounds.size;
                float max = Mathf.Max(bbSize.x, Mathf.Max(bbSize.y, bbSize.z));
                return max;
            }
            return 1;
        }
    }
}