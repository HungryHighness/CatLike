using System;
using UnityEngine;

namespace Movement.ComplexGravity
{
    public class GravitySource : MonoBehaviour
    {
        public Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        private void OnEnable()
        {
            CustomGravity.Register(this);
        }

        private void OnDisable()
        {
            CustomGravity.Unregister(this);
        }
    }
}