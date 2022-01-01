using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float maxSpeed = 5f;

    [SerializeField] Vector3 speed = Vector3.zero;

    [SerializeField, Range(0f, 10f)] private float maxAcceleration = 1f;

    [SerializeField] private Rect allowedArea = new Rect(-4.5f, -4.5f, 9f, 9f);

    [SerializeField, Range(0f, 1f)] private float bounciness = 0.5f;

    // Start is called before the first frame update
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

        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        speed.x = Mathf.MoveTowards(speed.x, desiredVelocity.x, maxSpeedChange);
        speed.z = Mathf.MoveTowards(speed.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = speed * Time.deltaTime;
        Vector3 newPosition = transform.localPosition + displacement;

        if (newPosition.x < allowedArea.xMin)
        {
            newPosition.x = allowedArea.xMin;
            speed.x = -speed.x * bounciness;
        }
        else if (newPosition.x > allowedArea.xMax)
        {
            newPosition.x = allowedArea.xMax;
            speed.x = -speed.x * bounciness;
        }
        else if (newPosition.z < allowedArea.yMin)
        {
            newPosition.z = allowedArea.yMin;
            speed.z = -speed.z * bounciness;
        }
        else if (newPosition.z > allowedArea.yMax)
        {
            newPosition.z = allowedArea.yMax;
            speed.z = -speed.z * bounciness;
        }

        transform.localPosition = newPosition;
    }
}