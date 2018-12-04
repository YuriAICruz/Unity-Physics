using System;
using UnityEngine;

namespace Graphene.Physics.SideScroller
{
    public class Basic2DPhysics
    {
        public Collider2D Collider;
        public Rigidbody2D Rigidbody;
        protected bool _debug = true;
        protected bool _grounded;
        protected bool _jumping;

        protected float _radius;

        protected Collider _standingCollider;

        public event Action<bool> JumpState, GroundState;

        protected Vector3[] _sides = new Vector3[]
        {
            new Vector3(0, 0),
            new Vector3(1, 0),
            new Vector3(0, 1),
            new Vector3(-1, 0),
            new Vector3(0, -1),
        };

        protected void CheckGround()
        {
            RaycastHit hit;

            for (int i = 0; i < _sides.Length; i++)
            {
                var pos = Collider.transform.position + (_sides[i] * _radius)  + Vector3.up;

                if (!UnityEngine.Physics.Raycast(pos, Vector3.down, out hit, 1.1f)) continue;

                if (_debug)
                    Debug.DrawRay(pos, Vector3.down * 1.1f, Color.green);

                _standingCollider = hit.collider;

                if (!_grounded)
                {
                    Collider.transform.position = new Vector3(Collider.transform.position.x, hit.point.y, Collider.transform.position.z);

                    SetJumpState(false);
                }

                SetGrounded(true);

                return;
            }

            _standingCollider = null;
            
            SetGrounded(false);
        }

        void SetGrounded(bool state)
        {
            if (_grounded != state)
                GroundState?.Invoke(state);

            _grounded = state;
        }

        public virtual void SetCollider(Collider2D collider, Rigidbody2D rigidbody)
        {
            Collider = collider;
            Rigidbody = rigidbody;
        }

        protected void SetJumpState(bool state)
        {
            _jumping = state;
            JumpState?.Invoke(state);
        }
    }
}