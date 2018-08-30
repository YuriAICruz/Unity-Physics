using System;
using UnityEngine;

namespace Physics
{
    public class CharacterPhysics : PhysycsBase
    {
        private static float GravityForce = 3;
        [HideInInspector] public bool Grounded;
        [SerializeField] private float _jumpSpeed = 6f;

        [SerializeField] private float _height = 1.1f;
        [SerializeField] private LayerMask _mask;

        public float JumpDuration = 0.1f;
        [HideInInspector] public float JumpStart;

        public void Gravity(Transform transform)
        {
            CheckGround(transform);

            if (Grounded)
            {
                Velocity.y = 0;
                return;
            }

            Velocity.y -= GravityForce;
        }

        private void CheckGround(Transform transform)
        {
            var pos = transform.position + Vector3.down * _height + Vector3.left;
            Debug.DrawRay(pos, Vector3.right * 2, Color.magenta);
            var hit = Physics2D.Raycast(pos, Vector2.right, 2, _mask);
            if (hit.collider != null)
            {
                Grounded = true;
                return;
            }

            Grounded = false;
        }

        public void Jump()
        {
            JumpStart = Time.time;
        }

        public void Jumping(float timeDiff)
        {
            Velocity.y += _jumpSpeed * (-timeDiff / JumpDuration + 1);
        }


        public void Move(float dir)
        {
            Velocity.x = dir * Speed;
        }
    }
}