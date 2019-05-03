using System;
using UnityEngine;

namespace Graphene.Physics.Platformer
{
    public abstract class BasicPhysics
    {
        public Collider Collider;
        public Rigidbody Rigidbody;
        protected bool _debug = true;
        protected bool _grounded;
        protected bool _jumping;

        protected float _radius;

        protected Collider _standingCollider;

        public event Action<bool> JumpState, GroundState;

        protected Vector3[] _sides = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 0),
            //new Vector3(0, 0, -1),
        };

        protected void CheckGround()
        {
            RaycastHit hit;

            for (int i = 0; i < _sides.Length; i++)
            {
                var pos = Collider.transform.position + (Collider.transform.TransformDirection(_sides[i]) * _radius)  + Vector3.up;

                var height = 1.1f;
                if (!UnityEngine.Physics.Raycast(pos, Vector3.down, out hit, height)) continue;

                if (_debug)
                    Debug.DrawRay(pos, Vector3.down * height, Color.green);

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

        public virtual void SetCollider(Collider collider, Rigidbody rigidbody)
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