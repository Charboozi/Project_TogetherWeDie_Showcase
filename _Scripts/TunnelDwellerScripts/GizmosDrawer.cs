/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEditor;
using UnityEngine;

namespace ProceduralSpider
{
    /*
    Class for debug drawing certain shapes.
    Creates the specified shapes by the use of Gizmos.
    */

    public class GizmosDrawer
    {
        public static void DrawLine(Vector3 pos, Vector3 end, Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawLine(pos, end);
        }

        public static void DrawRay(Vector3 pos, Vector3 direction, Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawLine(pos, pos + direction);
        }

        public static void DrawRay(Vector3 pos, Vector3 direction, float distance, Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawLine(pos, pos + direction.normalized * distance);
        }

        public static void DrawSphereRay(Vector3 start, Vector3 direction, float distance, float radius, int amount, Color col)
        {
            Vector3 endPoint = start + (radius + distance) * direction;
            Vector3 endPointSphereCenter = endPoint - (radius * direction);

            for (int i = 0; i < amount; i++)
            {
                Gizmos.color = new Color(col.r, col.g, col.b, 0.3f);
                Gizmos.DrawSphere(Vector3.Lerp(start, endPointSphereCenter, (float)i / (amount - 1)), radius);
            }
            Gizmos.DrawLine(start, endPoint);
        }

        public static void DrawSphereRay(Vector3 start, Vector3 end, float radius, int amount, Color col)
        {
            Vector3 v = end - start;
            DrawSphereRay(start, v.normalized, v.magnitude, radius, amount, col);
        }

        public static void DrawCube(Vector3 pos, float scale, Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawCube(pos, Vector3.one * scale);
        }

        public static void DrawSphere(Vector3 pos, float radius, Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawSphere(pos, radius);
        }

        public static void DrawWireSphere(Vector3 pos, float radius, Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawWireSphere(pos, radius);
        }

        public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color col)
        {
#if UNITY_EDITOR
            Handles.color = col;
            UnityEditor.Handles.DrawSolidArc(center, normal, from, angle, radius);
#endif
        }

        public static void DrawCircle(Vector3 pos, Vector3 normal, float radius, Color col)
        {
            int l = 16;

            //Choose a perpendicular vector
            Vector3 perpendicular;
            perpendicular = Vector3.ProjectOnPlane(Vector3.forward, normal);
            if (perpendicular == Vector3.zero) perpendicular = Vector3.ProjectOnPlane(Vector3.right, normal);
            perpendicular = perpendicular.normalized;

            Vector3[] p = new Vector3[l];

            //Lerping is problematic with an angle greater than 180
            for (int k = 0; k < l; k++)
            {
                p[k] = pos + Quaternion.AngleAxis(360.0f * k / l, normal) * perpendicular * radius;
            }

            // Draw circle
            Gizmos.color = col;
            for (int k = 0; k < l - 1; k++)
            {
                Gizmos.DrawLine(p[k], p[k + 1]);
            }
            Gizmos.DrawLine(p[l - 1], p[0]);
        }

        public static void DrawCircleSection(Vector3 pos, Vector3 min, Vector3 max, Vector3 normal, float minRadius, float maxRadius, Color col, float duration = 0)
        {
            if (min == Vector3.zero || max == Vector3.zero) return;
            if (normal == Vector3.zero) return;
            Vector3 projMin = Vector3.ProjectOnPlane(min, normal).normalized;
            Vector3 projMax = Vector3.ProjectOnPlane(max, normal).normalized;
            if (projMin == Vector3.zero || projMax == Vector3.zero) return;
            int l = 9;
            Vector3[] p = new Vector3[l];
            Vector3[] P = new Vector3[l];
            float angle = Vector3.Angle(projMin, projMax);
            for (int k = 0; k < l; k++)
            {
                p[k] = pos + Quaternion.AngleAxis(angle * k / (l - 1), normal) * projMin * minRadius;
                P[k] = pos + Quaternion.AngleAxis(angle * k / (l - 1), normal) * projMin * maxRadius;
            }
            // Draw circles
            for (int k = 0; k < l - 1; k++)
            {
                Debug.DrawLine(p[k], p[k + 1], col, duration);
                Debug.DrawLine(P[k], P[k + 1], col, duration);
            }
            // Connect inner to outer circle
            Debug.DrawLine(p[0], P[0], col, duration);
            Debug.DrawLine(p[l - 1], P[l - 1], col, duration);
        }
    }
}