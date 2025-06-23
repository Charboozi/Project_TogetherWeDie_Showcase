/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    public enum GroundType { None, Ground, Wall };

    public struct GroundInfo
    {
        public GroundInfo(GroundType groundType, Vector3 normal, float distance)
        {
            this.groundType = groundType;
            this.normal = normal;
            this.distance = distance;
        }

        public static GroundInfo CreateEmpty()
        {
            return new GroundInfo(GroundType.None, Vector3.up, float.PositiveInfinity);
        }

        public bool IsGrounded()
        {
            return groundType != GroundType.None;
        }

        public GroundType groundType;
        public Vector3 normal;
        public float distance;
    }
}