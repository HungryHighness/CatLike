using UnityEngine;

namespace Movement.Surface
{
    public class SurfaceMovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0f, 10f)] private float maxSpeed = 5f;

        [SerializeField] Vector3 speed = Vector3.zero, desiredVelocity = Vector3.zero;

        [SerializeField, Range(0f, 10f)] private float maxAcceleration = 3f, maxAirAcceleration = 1f;
        [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;
        [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
        [SerializeField, Range(0, 5)] private int maxAirJumps = 0;
        private int _jumpPhase;
        private int _groundContactCount;
        private Rigidbody _rigidbody;
        private bool _desiredJump;
        private Vector3 _contactNormal;
        private float _minGroundDotProduct;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private bool OnGround => _groundContactCount > 0;

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
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

            _desiredJump |= Input.GetButtonDown("Jump");

            GetComponent<Renderer>().material.SetColor(BaseColor, OnGround ? Color.black : Color.white);
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

            _rigidbody.velocity = speed;

            ClearState();
        }

        private void ClearState()
        {
            _groundContactCount = 0;
            _contactNormal = Vector3.zero;
        }

        private void Jump()
        {
            if (OnGround || _jumpPhase < maxAirJumps)
            {
                _jumpPhase += 1;
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
                float alignedSpeed = Vector3.Dot(speed, _contactNormal);
                if (alignedSpeed > 0f)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
                }

                speed += _contactNormal * jumpSpeed;
            }
        }

        private void UpdateState()
        {
            speed = _rigidbody.velocity;
            if (OnGround)
            {
                _jumpPhase = 0;
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
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                if (normal.y >= _minGroundDotProduct)
                {
                    _groundContactCount += 1;
                    _contactNormal += normal;
                }
            }
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
        }

        private void AdjustVelocity()
        {
            Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            float currentX = Vector3.Dot(speed, xAxis);
            float currentZ = Vector3.Dot(speed, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            speed += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        private bool SnapToGround()
        {
            return false;
        }
    }
}