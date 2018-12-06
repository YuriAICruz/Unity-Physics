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
        protected float _height;

        protected Collider2D _standingCollider;

        public event Action<bool> JumpState, GroundState;
        private LayerMask _groundMask;

        protected Vector3Int[] _floor = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
        };

        protected Vector3Int[] _sides = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
        };

        public Basic2DPhysics(Rigidbody2D rigidbody, CapsuleCollider2D collider, Transform camera)
        {
            _groundMask |= 1 << LayerMask.NameToLayer("Level");
        }

        protected void CheckGround()
        {
            for (int i = 0; i < _sides.Length; i++)
            {
                var pos = Collider.transform.position + ((Vector3) _sides[i] * _radius * 0.4f) + Vector3.up * _height;

                var hit = Physics2D.Raycast(pos, Vector2.down, _height * 1.1f, _groundMask);

                if (hit.collider == null) continue;

                if (_debug)
                    Debug.DrawRay(pos, Vector2.down * 1.1f, Color.green);

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