/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

//#define ENABLE_DEBUG_LOGGING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralSpider.Raycasting;
using System;

namespace ProceduralSpider
{
    /*
    This class itself does not perform any kind of updates.
    It simply supplies the public functions IsStepNeeded and CreateStep.
    The IKStepManager calls these functions to create steps.
    */

    [Serializable]
    public class IKStepper
    {
        [SerializeField]
        public IKChain[] asyncChains = new IKChain[0];
        [SerializeField]
        private float anchorReach = 0.1f;
        [SerializeField]
        private float anchorStride = 0;
        [SerializeField]
        private float anchorMultiplier = 1.3f;

        private const float stepTolerance = 0.02f; // If the error is greater than this tolerance, we step.
        private const float rayFrontalHeight = 0.6f;
        private const float rayFrontalLength = 0.75f;
        private const float rayFrontalOriginOffset = 0.5f;
        private Vector3 rayTopFocalPoint = new Vector3(0, 0.2f, 0);
        private const float rayOutwardsEndOffset = 1.25f;
        private const float downRayHeight = 3;
        private const float downRayDepth = 3;
        private const float rayInwardsEndOffset = 0.6f;

        private IKChain ikChain;
        private Spider spider;
        private float minTargetDistance;
        private float minOutRayDistance;

        private enum CastType
        {
            PredictionFrontal = 0,
            PredictionOut,
            PredictionDown,
            PredictionInFar,
            PredictionInMid,
            PredictionInClose,
            AnchorFrontal, // = 6
            AnchorOut,
            AnchorDown,
            AnchorInFar,
            AnchorInMid,
            AnchorInClose
        }
        private Dictionary<CastType, RayCast> casts;
        private RaycastHit hitInfo;

        private Vector3 anchorPositionLocal;
        private Vector3 frontalStartPositionLocal;
        private Vector3 prediction;
        private Vector3 surfaceTarget;

        //Debug Variables
        private Vector3 zeroTangentLocal;
        private CastType lastHitRay;

        public void Initialize(Spider spider, IKChain ikChain)
        {
            this.ikChain = ikChain;
            this.spider = spider;

            //Although chain length is already calculated on IKChain Awake, we have no control over execution order, so we do it again here.
            ikChain.CalculateChainLength();

            //Set the distance which the root joint and the endeffector are allowed to have. If below this distance, stepping is forced.
            minTargetDistance = 0.1f * ikChain.GetChainLength();

            //Set Anchor Position
            anchorPositionLocal = CalculateAnchor();
            frontalStartPositionLocal = new Vector3(0, rayFrontalHeight * 2 * spider.GetColliderHeight(), 0);

            // Initialize prediction
            prediction = GetAnchorPosition();
            surfaceTarget = GetAnchorPosition();

            // Initialize Casts as either RayCast or SphereCast
            casts = new Dictionary<CastType, RayCast>();
            UpdateCasts();

            //Set Start Target for IKChain
            ikChain.SetTarget(GetDefaultTarget());
        }

        /*
         * This function calculates the anchor position using the parameters Stride and Length.
         * This anchor position is the point used for new step point calculation.
         */
        private Vector3 CalculateAnchor()
        {
            JointHinge firstJoint = ikChain.GetFirstJoint();
            Vector3 normal = spider.transform.up;
            Vector3 zeroTangent = Vector3.ProjectOnPlane(ikChain.GetEndEffector().position - firstJoint.transform.position, normal).normalized;
            zeroTangentLocal = spider.transform.InverseTransformDirection(zeroTangent); // Store for debug drawing of DOF Arc
            Vector3 minTangent = Quaternion.AngleAxis(firstJoint.minAngle, normal) * zeroTangent;
            Vector3 maxTangent = Quaternion.AngleAxis(firstJoint.maxAngle, normal) * zeroTangent;
            Vector3 origin = spider.GetColliderBottomPoint() + Vector3.ProjectOnPlane(firstJoint.transform.position - spider.transform.position, normal);
            Vector3 dir = Vector3.Slerp(minTangent, maxTangent, (anchorStride + 1) / 2);
            float dist = Mathf.Lerp(minTargetDistance, ikChain.GetChainLength(), (anchorReach + 1) / 2);
            return spider.transform.InverseTransformPoint(origin + dist * dir);
        }

        /*
         * This method defines the RayCasts/SphereCasts and stores them in a dictionary with a corresponding key
         * The order in which they appear in the dictionary is the order in which they will be casted.
         * This order is of very high importance, so choose smartly.
         */
        private void UpdateCasts()
        {
            Vector3 anchorPos = GetAnchorPosition();
            Vector3 normal = spider.transform.up;
            float colHeight = spider.GetColliderHeight();
            float chainLength = ikChain.GetChainLength();

            //Frontal Parameters
            Vector3 frontal = GetFrontalStartPosition();
            Vector3 frontalPredictionEnd = frontal + chainLength * rayFrontalLength * Vector3.ProjectOnPlane(prediction - frontal, normal).normalized;
            Vector3 frontalAnchorEnd = frontal + chainLength * rayFrontalLength * Vector3.ProjectOnPlane(anchorPos - frontal, normal).normalized;
            Vector3 frontalPredictionOrigin = Vector3.Lerp(frontal, frontalPredictionEnd, rayFrontalOriginOffset);
            Vector3 frontalAnchorOrigin = Vector3.Lerp(frontal, frontalAnchorEnd, rayFrontalOriginOffset);

            //Outwards Parameters
            Vector3 top = GetTopFocalPoint();
            Vector3 outwardsPredictionEnd = Vector3.LerpUnclamped(top, prediction, rayOutwardsEndOffset);
            Vector3 outwardsAnchorEnd = Vector3.LerpUnclamped(top, anchorPos, rayOutwardsEndOffset);
            minOutRayDistance = 0.75f * (anchorPos - top).magnitude;

            //Downwards Parameters
            float height = downRayHeight * colHeight / 2;
            float depth = downRayDepth * colHeight / 2;
            Vector3 downwardsPredictionOrigin = prediction + normal * height;
            Vector3 downwardsPredictionEnd = prediction - normal * depth;
            Vector3 downwardsAnchorOrigin = anchorPos + normal * height;
            Vector3 downwardsAnchorEnd = anchorPos - normal * depth;

            //Inwards Parameters
            Vector3 bottomBorder = spider.transform.position - 0.75f * colHeight * normal;
            Vector3 bottomMid = spider.transform.position - 2f * colHeight * normal;
            Vector3 bottom = spider.transform.position - 3f * colHeight * normal;
            Vector3 bottomFarPrediction = Vector3.Lerp(prediction, bottom, rayInwardsEndOffset);
            Vector3 bottomFarAnchor = Vector3.Lerp(anchorPos, bottom, rayInwardsEndOffset);

            casts.Clear();
            casts = new Dictionary<CastType, RayCast> {
            { CastType.PredictionFrontal, new RayCast(frontalPredictionOrigin, frontalPredictionEnd)},
            { CastType.PredictionOut, new RayCast(top, outwardsPredictionEnd) },
            { CastType.PredictionDown, new RayCast(downwardsPredictionOrigin,downwardsPredictionEnd) },
            { CastType.PredictionInFar, new RayCast(prediction,bottomFarPrediction) },
            { CastType.PredictionInMid, new RayCast(prediction, bottomMid) },
            { CastType.PredictionInClose, new RayCast(prediction, bottomBorder) },

            { CastType.AnchorFrontal, new RayCast(frontalAnchorOrigin, frontalAnchorEnd) },
            { CastType.AnchorOut, new RayCast(top, outwardsAnchorEnd) },
            { CastType.AnchorDown, new RayCast(downwardsAnchorOrigin,downwardsAnchorEnd) },
            { CastType.AnchorInFar, new RayCast(anchorPos,bottomFarAnchor) },
            { CastType.AnchorInMid, new RayCast(anchorPos, bottomMid) },
            { CastType.AnchorInClose, new RayCast(anchorPos, bottomBorder) },
        };
        }

        /* Checks whether we want to step or not. */
        public bool IsStepNeeded()
        {
            // If target set disallowed, we don't want to step.
            if (!ikChain.IsIKTargetSetAllowed()) return false;

            // If current target not grounded, step.
            if (!ikChain.GetTarget().isGround) return true;

            // If the error of the IK solver gets too big, that is if it cant solve for the current target appropriately anymore, step.
            if (ikChain.GetErrorSqr() > GetStepToleranceSqr()) return true;

            // Alternatively step if too close to first joint
            if (Vector3.SqrMagnitude(ikChain.GetFirstJoint().transform.position - ikChain.GetTarget().position) < minTargetDistance * minTargetDistance) return true;

            return false;
        }

        /* Creates a step animation to a new target. The new target position is determined through the raycasting setup. */
        public StepAnimation CreateStep(float duration, float height, AnimationCurve curve, LayerMask layer)
        {
            //Calculate desired position
            Vector3 desiredPosition = CalculateDesiredPosition();

            //Get the current velocity of the end effector and correct the desired position with it since the spider will move away while stepping
            //Set the this new value as the prediction
            Vector3 endEffectorVelocity = Vector3.ProjectOnPlane(ikChain.GetEndEffectorVelocity(), spider.transform.up);
            prediction = desiredPosition + endEffectorVelocity * duration;

            // Finally find an actual target which lies on a surface point using the calculated prediction with raycasting
            IKTargetInfo from = ikChain.GetTarget();
            IKTargetInfo to = FindTargetOnSurface(layer);

            //We only have a step height if we step from ground to ground
            if (!from.isGround || !to.isGround) height = 0;

            return new StepAnimation(from, to, duration, height, curve, spider.transform);
        }

        /*
         * This function calculates the position the leg desires to step to if a step would be performed.
         * A line is drawn from the current end effector position to the anchor position,
         * but the line will be overextended a bit , where the amount is given by the overshootMultiplier.
         * All of this happens on the plane given by the spiders up direction at anchor position height.
         */
        private Vector3 CalculateDesiredPosition()
        {
            Vector3 endeffectorPosition = ikChain.GetEndEffector().position;
            Vector3 anchorPosition = GetAnchorPosition();
            Vector3 normal = spider.transform.up;

            // Option 1: Include spider movement in the prediction process: prediction += SpiderMoveVector * stepTime
            //      Problem:    Spider might stop moving while stepping, if this happens i will over predict
            //                  Spider might change direction while stepping, if this happens i could predict out of range
            //      Solution:   Keep the stepTime short such that not much will happen

            // Option 2: Dynamically update the prediction in a the stepping coroutine where i keep up with the spider with its local coordinates
            //      Problem:    I will only know if the foot lands on a surface point after the stepping is already done
            //                  This means the foot could land in a bump on the ground or in the air, and i will have to look what i will do from there
            //                  Update the position within the last frame (unrealistic) or start another different stepping coroutine?
            //                  Or shoot more rays in the stepping process to somewhat adjust to the terrain changes?
            // I choose Option 1

            // Level end effector position with the anchor position in regards to the normal
            Vector3 start = Vector3.ProjectOnPlane(endeffectorPosition, normal);
            start = spider.transform.InverseTransformPoint(start);
            start.y = anchorPositionLocal.y;
            start = spider.transform.TransformPoint(start);

            // Overshoot by multiplier
            return start + (anchorPosition - start) * anchorMultiplier;
        }

        /*
         * This function tries to find a new target on any valid surface.
         * The parameter 'prediction' is used to construct ray casts that will scan the surrounding topology.
         * This function will return the target it has found. If no surface point was found, the default target at anchor position is returned.
         */
        private IKTargetInfo FindTargetOnSurface(LayerMask layer)
        {
            //If there is no collider in reach there is no need to try to find a surface point, just return default target here.
            //This should cut down runtime cost if the spider is not grounded (e.g. in the air).
            //However this does add an extra calculation if grounded, increasing it slightly.
            if (Physics.OverlapSphere(ikChain.GetFirstJoint().transform.position, ikChain.GetChainLength(), layer, QueryTriggerInteraction.Ignore) == null)
            {
                return GetDefaultTarget();
            }

            //Update Casts for new prediction point.
            UpdateCasts();

            //Now shoot rays using the casts to find an actual point on a surface.
            foreach (var cast in casts)
            {
                if (cast.Value.CastRay(out hitInfo, layer))
                {
                    //For the frontal ray we only allow not too steep slopes, that is +-65°
                    if (cast.Key == CastType.PredictionFrontal && Vector3.Angle(cast.Value.GetDirection(), -hitInfo.normal) > 65) continue;

                    //For the outwards ray we only allow if not too close
                    if ((cast.Key == CastType.AnchorOut || cast.Key == CastType.PredictionOut) && hitInfo.distance < minOutRayDistance) continue;

#if ENABLE_DEBUG_LOGGING
                    Debug.Log("Got a target point from the cast '" + cast.Key);
#endif
                    lastHitRay = cast.Key;
                    surfaceTarget = hitInfo.point;
                    return new IKTargetInfo(hitInfo.point, hitInfo.normal);
                }
            }

            // Return default target
#if ENABLE_DEBUG_LOGGING
            Debug.Log("No ray was able to find a target position. Therefore i will return default target.");
#endif
            return GetDefaultTarget();
        }

        public float GetStepToleranceSqr()
        {
            float tolerance = stepTolerance * ikChain.GetScale();
            return tolerance * tolerance;
        }

        private Vector3 GetAnchorPosition()
        {
            return spider.transform.TransformPoint(anchorPositionLocal);
        }

        public IKTargetInfo GetDefaultTarget()
        {
            return new IKTargetInfo(GetAnchorPosition(), spider.transform.up, false);
        }

        private Vector3 GetTopFocalPoint()
        {
            return spider.transform.TransformPoint(rayTopFocalPoint);
        }

        private Vector3 GetFrontalStartPosition()
        {
            return spider.transform.TransformPoint(frontalStartPositionLocal);
        }

        public void DrawDebug()
        {
            if (spider == null) return;
            float scale = spider.GetColliderHeight() * 0.05f;

            // Anchor Position
            GizmosDrawer.DrawCube(GetAnchorPosition(), scale, Color.magenta);

            //Draw the top and bottom ray points
            GizmosDrawer.DrawCube(GetTopFocalPoint(), scale, Color.green);

            //IK Target Point
            GizmosDrawer.DrawCube(ikChain.GetTarget().position, scale, Color.cyan);

            //Prediction Point
            GizmosDrawer.DrawCube(prediction, scale, Color.yellow);

            //Surface Point
            GizmosDrawer.DrawCube(surfaceTarget, scale, Color.red);

            //Draw the step tolerance as a sphere
            GizmosDrawer.DrawWireSphere(ikChain.endEffector.position, Mathf.Sqrt(GetStepToleranceSqr()), Color.cyan);

            //Draw Raycasts
            Color col;
            foreach (var cast in casts)
            {
                if ((int)cast.Key < 6) col = Color.yellow; // Prediction Casts
                else col = Color.magenta; // Anchor Casts
                if (cast.Key != lastHitRay) col = Color.Lerp(col, Color.white, 0.5f);
                cast.Value.Draw(col);
            }

            //Draw DOF Arc
            JointHinge firstJoint = ikChain.GetFirstJoint();
            if (!firstJoint.IsInitialized()) firstJoint.Initialize();
            Vector3 n = firstJoint.GetRotationAxisWorld();
            Vector3 minTangentLocal = Quaternion.AngleAxis(firstJoint.minAngle, firstJoint.GetRotationAxisWorld()) * zeroTangentLocal;
            Vector3 maxTangentLocal = Quaternion.AngleAxis(firstJoint.maxAngle, firstJoint.GetRotationAxisWorld()) * zeroTangentLocal;
            Vector3 minTangent = spider.transform.TransformDirection(minTangentLocal);
            Vector3 maxTangent = spider.transform.TransformDirection(maxTangentLocal);
            Vector3 pLocal = spider.transform.InverseTransformPoint(firstJoint.transform.position);
            Vector3 p = spider.transform.TransformPoint(new Vector3(pLocal.x, anchorPositionLocal.y, pLocal.z));
            GizmosDrawer.DrawCircleSection(p, minTangent, maxTangent, n, minTargetDistance, ikChain.GetChainLength(), Color.red);
        }
    }
}