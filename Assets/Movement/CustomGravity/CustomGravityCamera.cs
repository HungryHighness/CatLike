using UnityEngine;

namespace Movement.CustomGravity
{
    public class CustomGravityCamera : MonoBehaviour
    {
        [SerializeField] private Transform focus = default;
        [SerializeField] private float focusRadius = 1f;
        [SerializeField, Range(1f, 20f)] private float distance = 5f;
        [SerializeField, Range(0f, 1f)] private float focusCentering = 0.5f;
        [SerializeField, Range(1f, 360f)] private float rotationSpeed = 90f;
        [SerializeField, Range(-89f, 89f)] private float minVerticalAngele = -30f, maxVerticalAngle = 60f;
        [SerializeField, Range(0f, 90f)] private float alignSmoothRange = 45f;
        [SerializeField, Min(0f)] private float alignDelay = 5f;
        [SerializeField] private LayerMask obstructionMask = -1;
        private Vector3 _focusPoint, _previousFocusPoint;
        private Vector2 _orbitAngles = new Vector2(45f, 0f);
        private float _lastManualRotationTime;
        private Camera _regularCamera;
        private Quaternion _gravityAlignment = Quaternion.identity;
        private Quaternion _orbitRotation;

        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y = _regularCamera.nearClipPlane *
                                Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
                halfExtends.x = halfExtends.y * _regularCamera.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }

        private void Awake()
        {
            _regularCamera = GetComponent<Camera>();
            _focusPoint = focus.position;
            transform.localRotation = _orbitRotation = Quaternion.Euler(_orbitAngles);
        }


        private void LateUpdate()
        {
            _gravityAlignment =
                Quaternion.FromToRotation(_gravityAlignment * Vector3.up, CustomGravity.GetUpAxis(_focusPoint)) *
                _gravityAlignment;

            UpdateFocusPoint();


            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                _orbitRotation = Quaternion.Euler(_orbitAngles);
            }

            Quaternion lookRotation = _gravityAlignment * _orbitRotation;
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = _focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * _regularCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine / castDistance;
            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance,
                    obstructionMask))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        private void UpdateFocusPoint()
        {
            _previousFocusPoint = _focusPoint;
            Vector3 targetPoint = focus.position;
            float distance = Vector3.Distance(targetPoint, _focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }

            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }

            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }

        private bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Vertical Camera"),
                Input.GetAxis("Horizontal Camera")
            );
            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                _orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
                _lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        private bool AutomaticRotation()
        {
            if (Time.unscaledTime - _lastManualRotationTime < alignDelay)
            {
                return false;
            }

            Vector3 alignedDelta = Quaternion.Inverse(_gravityAlignment) * (_focusPoint - _previousFocusPoint);
            Vector2 movement = new Vector2(
                alignedDelta.x, alignedDelta.z
            );
            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.000001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
            float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
            if (deltaAbs < alignSmoothRange)
            {
                rotationChange *= deltaAbs / alignSmoothRange;
            }
            else if (180f - deltaAbs < alignSmoothRange)
            {
                rotationChange *= (180f - deltaAbs) / alignSmoothRange;
            }

            _orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);
            return true;
        }

        private void OnValidate()
        {
            if (maxVerticalAngle < minVerticalAngele)
            {
                maxVerticalAngle = minVerticalAngele;
            }
        }

        private void ConstrainAngles()
        {
            _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, minVerticalAngele, maxVerticalAngle);
            if (_orbitAngles.y < 0f)
            {
                _orbitAngles.y += 360f;
            }
            else if (_orbitAngles.y >= 360f)
            {
                _orbitAngles.y -= 360f;
            }
        }

        static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x < 0f ? 360f - angle : angle;
        }
    }
}