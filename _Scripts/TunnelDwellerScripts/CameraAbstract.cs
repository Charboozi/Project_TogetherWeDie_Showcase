/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;
using UnityEngine.Rendering;
using ProceduralSpider.Raycasting;

namespace ProceduralSpider
{
    /*
    An Abstract class for camera components.

    It is an implementation of a smoothly lerping camera towards a given target. This includes translational and rotational interpolation.
    Limits to vertical rotation can be set, the interpolation type for smooth movement can be chosen,
    as well as and sensitivity to mouse movement.

    The observed object must be set, which is usually the player (e.g. the spider) and this class handles other objects obstructing
    the view to this observed object in two ways:

    Obstruction Hiding: Simply render the geometry invisible while it is obstructing.
    Clip Zoom: Zoom in more closely to avoid the obstruction.

    Both have their own adjustable layers, so it is completely configurable for which objects what method should be used.
    It is advised to use the Clip Zoom for level borders where the camera would otherwise be out of bounds, and use the obstruction
    hiding method for simple small objects.

    The reason for this class to be abstract is that the camera target manipulation can have several different implementations.
    E.g. simply parenting it to the observed objects or ignoring rotational change of the observed object but following translational change.
    Moreover, the rotational axes for vertical and horizontal camera movement can vary, e.g. local Y or global Y axis, and must thus be defined separately.
    */

    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-2)] // Camera input has to update very early. Earlier than movement controllers as they might be relative to camera.
    public abstract class CameraAbstract : MonoBehaviour
    {
        [Header("Smoothness")]
        [Tooltip("The speed at which the camera will move to follow the target.")]
        public float translationSpeed = 4;

        [Tooltip("The speed at which the camera will rotate to follow the target.")]
        public float rotationSpeed = 4;

        [Header("Sensitivity")]
        [Tooltip("The sensitivity of horizontal mouse movement.")]
        [Range(1, 5)]
        public float XSensitivity = 2;

        [Tooltip("The sensitivity of vertical mouse movement.")]
        [Range(1, 5)]
        public float YSensitivity = 2;

        public enum PositionInterpolation { Slerp, Lerp, SmoothDamp }
        [Header("Interpolation")]
        [Tooltip("Determines how the translation should be interpolated smoothly.")]
        public PositionInterpolation positionInterpolationType = PositionInterpolation.Slerp;

        [Header("Angle Restrictions")]
        [Tooltip("The lower bound angle the camera should be constrained to.")]
        [Range(0.01f, 179.99f)]
        public float camUpperAngleMargin = 10.0f;

        [Tooltip("The upper bound angle the camera should be constrained to.")]
        [Range(0.01f, 179.99f)]
        public float camLowerAngleMargin = 80.0f;

        [Header("Camera Clip Zoom")]
        [Tooltip("Enables clip zoom. This will move the camera closer towards the target whenever an object obstructs sight.")]
        public bool enableClipZoom = false;

        [Tooltip("The layers used to determines obstructing objects for clip zoom.")]
        public LayerMask clipZoomLayer = 1;

        [Tooltip("Adds padding from camera position to obstructing object. Value of 0 means that camera will place itself directly at obstructing object.")]
        [Range(0, 1)]
        public float clipZoomPaddingFactor = 0.1f;

        [Tooltip("Determines the minimum distance the camera should keep from the target. Value of 0 means the camera can move into the target, value of 1 means the camera can not zoom at all.")]
        [Range(0, 1)]
        public float clipZoomMinDistanceFactor = 0.1f;

        [Header("Obstruction Hiding")]
        [Tooltip("Enables obstruction hiding. Will make all obstructing objects invisible.")]
        public bool enableObstructionHiding = true;

        [Tooltip("The layers used to determines obstructing objects for obstruction hiding.")]
        public LayerMask obstructionHidingLayer = 1;

        [Tooltip("The radius of the sphere cast used to determines obstructing objects.")]
        [Range(0, 1f)]
        public float rayRadiusObstructionHiding = 0.05f;

        [Header("Debug")]
        [Tooltip("Enable this to draw debug drawings in the viewport.")]
        public bool showDebug = false;

        protected Transform observedObject;
        protected Transform camTarget;
        protected Camera cam;
        protected abstract Vector3 GetHorizontalRotationAxis(); // Unimplemented Rotation Axis
        protected abstract Vector3 GetVerticalRotationAxis(); // Unimplemented Rotation Axis

        private Vector3 velocity = Vector3.zero;
        private float maxCameraDistance;
        private RaycastHit hitInfo;
        private int obstructionCount = 0;
        private RaycastHit[] obstructionsHitInfos = new RaycastHit[50];
        private ShadowCastingMode[] obstructionsShadowCastingMode = new ShadowCastingMode[50];

        protected virtual void Awake()
        {
            Initialize();
            SetupCamTarget();
            transform.parent = null; // Unparent the camera itself so it can move freely and use the target to lerp smoothly
        }

        private void Initialize()
        {
            observedObject = transform.parent;
            cam = GetComponent<Camera>();
            maxCameraDistance = Vector3.Distance(observedObject.position, transform.position);
            if (camUpperAngleMargin >= camLowerAngleMargin)
            {
                Debug.LogError("Upper Angle has to be smaller than Lower Angle");
                camUpperAngleMargin = 45f;
                camLowerAngleMargin = 90f;
            }
        }

        // For the target, create new Gameobject with same position and rotation as this camera currently is
        private void SetupCamTarget()
        {
            GameObject g = new GameObject(gameObject.name + " Target");
            camTarget = g.transform;
            camTarget.SetPositionAndRotation(transform.position, transform.rotation);
        }

        /*
        Update performs the cameras target movement. This includes mouse input as well as solving clipping problems.
        Override this Update call in an inheriting class but call this base implementation first.
        This allows the inheriting classes to implement their own camera target manipulation.
        */
        protected virtual void Update()
        {
            RotateCameraHorizontal(Input.GetAxis("Mouse X") * XSensitivity, false);
            RotateCameraVertical(-Input.GetAxis("Mouse Y") * YSensitivity, false);

            if (cam.enabled)
            {
                if (enableClipZoom) ClipZoom();
                if (enableObstructionHiding)
                {
                    UnhideObstructions();
                    FindObstructions();
                    HideObstructions();
                }
            }
        }

        /*
        Fixed Update performs the interpolation of the camera to the camera target. This must not be overridden by inheriting classes.
        Usually this should be in LateUpdate but as the cam target can be tied to physics movement this would cause jitter.
        */
        protected virtual void FixedUpdate()
        {
            // Translation Interpolation
            switch (positionInterpolationType)
            {
                case PositionInterpolation.Lerp:
                    transform.position = Vector3.Lerp(transform.position, camTarget.position, translationSpeed * Time.fixedDeltaTime);
                    break;
                case PositionInterpolation.Slerp:
                    Vector3 a = observedObject.InverseTransformPoint(transform.position);
                    Vector3 b = observedObject.InverseTransformPoint(camTarget.position);
                    Vector3 c = Vector3.Slerp(a, b, translationSpeed * Time.fixedDeltaTime);
                    transform.position = observedObject.TransformPoint(c);
                    break;
                case PositionInterpolation.SmoothDamp:
                    transform.position = Vector3.SmoothDamp(transform.position, camTarget.position, ref velocity, 1 / translationSpeed);
                    break;
            }

            // Rotation Interpolation
            transform.rotation = Quaternion.Slerp(transform.rotation, camTarget.rotation, rotationSpeed * Time.fixedDeltaTime);
        }

        public void RotateCameraHorizontal(float angle, bool onlyTarget = true)
        {
            Vector3 rotationAxis = GetHorizontalRotationAxis();

            //Apply Rotation
            camTarget.RotateAround(observedObject.position, rotationAxis, angle);
            if (!onlyTarget) transform.RotateAround(observedObject.position, rotationAxis, angle);
        }

        public void RotateCameraVertical(float angle, bool onlyTarget = true)
        {
            //Restrict Angle to Bounds
            ClampAngle(ref angle);
            Vector3 zeroOrientation = GetHorizontalRotationAxis();
            float currentAngle = Vector3.SignedAngle(zeroOrientation, camTarget.position - observedObject.transform.position, camTarget.right); //Should always be positive

            if (currentAngle + angle > -camUpperAngleMargin)
            {
                angle = -currentAngle - camUpperAngleMargin;
            }
            if (currentAngle + angle < -camLowerAngleMargin)
            {
                angle = -currentAngle - camLowerAngleMargin;
            }

            Vector3 rotationAxis = GetVerticalRotationAxis();

            // Apply Rotation
            camTarget.RotateAround(observedObject.position, rotationAxis, angle);
            if (!onlyTarget) transform.RotateAround(observedObject.position, rotationAxis, angle);
        }

        // Clamps angle to (-180,180]
        private void ClampAngle(ref float angle)
        {
            angle = angle % 360;
            if (angle == -180) angle = 180;
            if (angle > 180) angle -= 360;
            if (angle < -180) angle += 360;
        }

        private RayCast CreateClipZoomRay()
        {
            Vector3 direction = camTarget.position - observedObject.position;
            return new RayCast(observedObject.position, direction, maxCameraDistance);
        }

        private SphereCast CreateObstructionRay()
        {
            return new SphereCast(transform.position, observedObject.position, rayRadiusObstructionHiding);
        }

        private void ClipZoom()
        {
            RayCast clipZoomRay = CreateClipZoomRay();

            if (clipZoomRay.CastRay(out hitInfo, clipZoomLayer))
            {
                Vector3 direction = clipZoomRay.GetDirection();

                //Add padding
                Vector3 newPosition = hitInfo.point - clipZoomPaddingFactor * maxCameraDistance * direction;

                //If either the padding sent me beyond the observed object  OR if im too close to the object, resort to min distance
                Vector3 v = newPosition - observedObject.position;
                float minDistance = maxCameraDistance * clipZoomMinDistanceFactor;
                if (Vector3.Angle(v, direction) > 45f || Vector3.Distance(observedObject.position, newPosition) < minDistance)
                {
                    newPosition = observedObject.position + direction * minDistance;
                }

                // Move the target and cam to the new point
                transform.position = newPosition;
                camTarget.position = newPosition;
            }
            else
            {
                // Move the target to the end point, so cam can lerp smoothly back. This works only because the ray is constructed through the target and not actual cam
                camTarget.position = clipZoomRay.end;
            }
        }

        private void UnhideObstructions()
        {
            for (int k = 0; k < obstructionCount; k++)
            {
                MeshRenderer mesh = obstructionsHitInfos[k].transform.GetComponent<MeshRenderer>();
                if (mesh == null) continue;
                mesh.shadowCastingMode = obstructionsShadowCastingMode[k];
            }
        }

        private void FindObstructions()
        {
            SphereCast obstructionRay = CreateObstructionRay();
            obstructionCount = obstructionRay.CastRayAllNonAlloc(obstructionHidingLayer, obstructionsHitInfos);
        }

        private void HideObstructions()
        {
            for (int k = 0; k < obstructionCount; k++)
            {
                MeshRenderer mesh = obstructionsHitInfos[k].transform.GetComponent<MeshRenderer>();
                if (mesh == null) continue;
                obstructionsShadowCastingMode[k] = mesh.shadowCastingMode;
                mesh.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        public Transform GetCameraTarget()
        {
            return camTarget;
        }

        public Vector3 GetCamTargetPosition()
        {
            return camTarget.position;
        }

        public Quaternion GetCamTargetRotation()
        {
            return camTarget.rotation;
        }

        /* Setters */
        public void SetTargetPosition(Vector3 pos)
        {
            camTarget.position = pos;
        }

        public void SetTargetRotation(Quaternion rot)
        {
            camTarget.rotation = rot;
        }

        private void DrawDebug()
        {
            //Draw line from this cam to the observed object
            GizmosDrawer.DrawLine(transform.position, observedObject.position, Color.gray);

            //Draw Obstruction Ray
            if (enableObstructionHiding) CreateObstructionRay().Draw(Color.red);

            //Draw ClipZoom Ray
            if (enableClipZoom)
            {
                RayCast clipZoomRay = CreateClipZoomRay();
                CreateClipZoomRay().Draw(Color.white);
                GizmosDrawer.DrawRay(clipZoomRay.origin, clipZoomRay.GetDirection(), clipZoomMinDistanceFactor * maxCameraDistance, Color.blue);
                GizmosDrawer.DrawRay(clipZoomRay.end, -clipZoomRay.GetDirection(), clipZoomPaddingFactor * maxCameraDistance, Color.red);
            }

            //Draw the angle restrictions
            Vector3 zeroOrientation = GetHorizontalRotationAxis();
            Vector3 up = Quaternion.AngleAxis(-camUpperAngleMargin, camTarget.right) * zeroOrientation;
            Vector3 down = Quaternion.AngleAxis(-camLowerAngleMargin, camTarget.right) * zeroOrientation;
            GizmosDrawer.DrawSolidArc(observedObject.position, camTarget.right, down, Vector3.SignedAngle(down, up, camTarget.right), maxCameraDistance / 4, Color.white);

            //Draw Transform
            GizmosDrawer.DrawCube(transform.position, 0.25f, Color.blue);
            GizmosDrawer.DrawRay(transform.position, transform.forward, Color.blue);
            GizmosDrawer.DrawRay(transform.position, transform.right, Color.red);
            GizmosDrawer.DrawRay(transform.position, transform.up, Color.green);

            //Draw Target Transform
            GizmosDrawer.DrawCube(camTarget.position, 0.25f, Color.magenta);
            GizmosDrawer.DrawRay(camTarget.position, camTarget.forward, Color.blue);
            GizmosDrawer.DrawRay(camTarget.position, camTarget.right, Color.red);
            GizmosDrawer.DrawRay(camTarget.position, camTarget.up, Color.green);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showDebug) return;
            if (!GetComponent<Camera>()) return;
            if (!GetComponent<Camera>().enabled) return;
            if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                Initialize();
                camTarget = transform;
            }
            DrawDebug();
        }
#endif
    }
}