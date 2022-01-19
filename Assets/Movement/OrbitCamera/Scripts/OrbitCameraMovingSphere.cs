using UnityEngine;

namespace Movement.OrbitCamera.Scripts
{
    public class OrbitCameraMovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0f, 10f)] private float maxSpeed = 5f;
        [SerializeField] Vector3 velocity = Vector3.zero, desiredVelocity = Vector3.zero;
        [SerializeField, Range(0f, 10f)] private float maxAcceleration = 3f, maxAirAcceleration = 1f;
        [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f, maxStairsAngle = 50f;
        [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
        [SerializeField, Range(0, 5)] private int maxAirJumps = 0;
        [SerializeField, Range(0f, 100f)] float maxSnapSpeed = 100f;
        [SerializeField, Min(0f)] private float probeDistance = 1f;
        [SerializeField] private LayerMask probeMask = -1, stairsMask = -1;
        [SerializeField] private Transform playerInputSpace = default;
        private int _jumpPhase;
        private int _groundContactCount, _steepContactCount;
        private Rigidbody _rigidbody;
        private bool _desiredJump;
        private Vector3 _contactNormal, _steepNormal;
        private float _minGroundDotProduct, _minStairsDotProduct;
        private int _stepsSinceLastGrounded, _stepSSinceLastJump;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private bool OnGround => _groundContactCount > 0;
        private bool OnSteep => _steepContactCount > 0;

        // Start is called before the first frame update
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            OnValidate();
        }

        void Start()
        {
        }

        // Update is called once per frame

        void Update()
        {
            Vector2 playerInput;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);
            if (playerInputSpace)
            {
                Vector3 forward = playerInputSpace.forward;
                forward.y = 0f;
                forward.Normalize();
                Vector3 right = playerInputSpace.right;
                right.y = 0f;
                right.Normalize();

                desiredVelocity = (forward * playerInput.y + right * (playerInput.x)) * maxSpeed;
            }
            else
            {
                desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
            }

            _desiredJump |= Input.GetButtonDown("Jump");
        }

        private void FixedUpdate()
        {
            UpdateState();
            AdjustVelocity();

            if (_desiredJump)
            {
                _desiredJump = false;
                Jump();
            }

            _rigidbody.velocity = velocity;

            ClearState();
        }

        private void ClearState()
        {
            _groundContactCount = _steepContactCount = 0;
            _contactNormal = _steepNormal = Vector3.zero;
        }

        private void Jump()
        {
            Vector3 jumpDirection;
            if (OnGround)
            {
                jumpDirection = _contactNormal;
            }
            else if (OnSteep)
            {
                jumpDirection = _steepNormal;
                _jumpPhase = 0;
            }
            else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
            {
                if (_jumpPhase == 0)
                {
                    _jumpPhase = 1;
                }

                jumpDirection = _contactNormal;
            }
            else
            {
                return;
            }

            _stepSSinceLastJump = 0;
            _jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            jumpDirection = (jumpDirection + Vector3.up).normalized;
            float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }

            velocity += jumpDirection * jumpSpeed;
        }

        private void UpdateState()
        {
            velocity = _rigidbody.velocity;
            _stepsSinceLastGrounded += 1;
            _stepSSinceLastJump += 1;
            if (OnGround || SnapToGround() || CheckSteepContacts())
            {
                _stepsSinceLastGrounded = 0;
                if (_stepSSinceLastJump > 1)
                {
                    _jumpPhase = 0;
                }

                if (_groundContactCount > 1)
                {
                    _contactNormal.Normalize();
                }
            }
            else
                _contactNormal = Vector3.up;
        }

        private void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        private void EvaluateCollision(Collision collision)
        {
            float minDot = GetMinDot(collision.gameObject.layer);
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                if (normal.y >= minDot)
                {
                    _groundContactCount += 1;
                    _contactNormal += normal;
                }
                else if (normal.y > -0.01f)
                {
                    _steepContactCount += 1;
                    _steepNormal += normal;
                }
            }
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            _minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
        }

        private void AdjustVelocity()
        {
            Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        private bool SnapToGround()
        {
            if (_stepsSinceLastGrounded > 1 || _stepSSinceLastJump <= 2)
            {
                return false;
            }

            float speed = velocity.magnitude;
            if (speed > maxSnapSpeed)
            {
                return false;
            }

            if (!Physics.Raycast(_rigidbody.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
            {
                return false;
            }

            if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
            {
                return false;
            }

            _groundContactCount = 1;
            _contactNormal = hit.normal;
            float dot = Vector3.Dot(velocity, hit.normal);
            if (dot > 0)
            {
                velocity = (velocity - hit.normal * dot).normalized * speed;
            }

            return false;
        }

        private bool CheckSteepContacts()
        {
            if (_steepContactCount > 0)
            {
                _steepNormal.Normalize();
                if (_steepNormal.y >= _minGroundDotProduct)
                {
                    _groundContactCount = 1;
                    _contactNormal = _steepNormal;
                    return true;
                }
            }

            return false;
        }

        private float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
        }
    }
}