/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    public struct IKTargetInfo
    {
        public Vector3 position;
        public Vector3 normal;
        public bool isGround;

        public IKTargetInfo(Vector3 position, Vector3 normal, bool isGround = true)
        {
            this.position = position;
            this.normal = normal;
            this.isGround = isGround;
        }
    }
}