/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    namespace Raycasting
    {
        /*
        Abstract class representing a raycast with origin and end point.
        Provides us with functions for casting as well as debug drawing.
        Implementations are RayCast and SphereCast.
        */
        public abstract class Cast
        {
            public Vector3 origin;
            public Vector3 end;

            public Cast(Vector3 origin, Vector3 end)
            {
                this.origin = origin;
                this.end = end;
            }

            public Cast(Vector3 origin, Vector3 direction, float distance)
            {
                this.origin = origin;
                this.end = origin + direction.normalized * distance;
            }

            public Vector3 GetDirection() { return (end - origin).normalized; }
            public float GetDistance() { return (end - origin).magnitude; }

            public abstract bool CastRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore);
            public abstract RaycastHit[] CastRayAll(LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore);
            public abstract int CastRayAllNonAlloc(LayerMask layerMask, RaycastHit[] hitInfo, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore);

            public abstract void Draw(Color col);
        }


        public class RayCast : Cast
        {
            public RayCast(Vector3 origin, Vector3 end)
            : base(origin, end) { }

            public RayCast(Vector3 origin, Vector3 direction, float distance)
            : base(origin, direction, distance) { }

            public override bool CastRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
            {
                return Physics.Raycast(origin, GetDirection(), out hitInfo, GetDistance(), layerMask, q);
            }

            public override RaycastHit[] CastRayAll(LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
            {
                return Physics.RaycastAll(origin, GetDirection(), GetDistance(), layerMask, q);
            }

            public override int CastRayAllNonAlloc(LayerMask layerMask, RaycastHit[] hitInfo, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
            {
                return Physics.RaycastNonAlloc(origin, GetDirection(), hitInfo, GetDistance(), layerMask, q);
            }

            public override void Draw(Color col)
            {
                GizmosDrawer.DrawLine(origin, end, col);
            }
        }


        public class SphereCast : Cast
        {
            public float radius;

            public SphereCast(Vector3 origin, Vector3 end, float radius)
              : base(origin, end)
            {
                this.radius = radius;
            }

            public SphereCast(Vector3 origin, Vector3 direction, float distance, float radius)
            : base(origin, direction, distance)
            {
                this.radius = radius;
            }

            public override bool CastRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
            {
                return Physics.SphereCast(origin, radius, GetDirection(), out hitInfo, GetDistance(), layerMask, q);
            }

            public override RaycastHit[] CastRayAll(LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
            {
                return Physics.SphereCastAll(origin, radius, GetDirection(), GetDistance(), layerMask, q);
            }

            public override int CastRayAllNonAlloc(LayerMask layerMask, RaycastHit[] hitInfo, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
            {
                return Physics.SphereCastNonAlloc(origin, radius, GetDirection(), hitInfo, GetDistance(), layerMask, q);
            }

            public override void Draw(Color col)
            {
                GizmosDrawer.DrawSphereRay(origin, end, radius, 5, col);
            }
        }
    }
}