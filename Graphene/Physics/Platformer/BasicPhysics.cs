using System;
using UnityEngine;

namespace Graphene.Physics.Platformer
{
    public abstract class BasicPhysics : MonoBehaviour
    {
        [HideInInspector]
        public Collider Collider;
        [HideInInspector]
        public Rigidbody Rigidbody;

        public LayerMask  GroundMask;
        
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


        private void Start()
        {
            Collider = GetComponent<Collider>();
            Rigidbody = GetComponent<Rigidbody>();

            _radius = Collider.bounds.size.x/2;
            
            SendMessage("Setup", SendMessageOptions.DontRequireReceiver);
        }

        protected Vector3 CheckGround()
        {
            RaycastHit hit;

            for (int i = 0; i < _sides.Length; i++)
            {
                var height = Collider.bounds.size.y;

                var pos = Collider.transform.position + (Collider.transform.TransformDirection(_sides[i]) * _radius) + Vector3.up * height * 0.4f;

                if (!UnityEngine.Physics.Raycast(pos, Vector3.down*2, out hit, height, GroundMask)) continue;

                if (_debug)
                    Debug.DrawRay(pos, Vector3.down * height, Color.green);

                _standingCollider = hit.collider;

                if (!_grounded)
                {
                    Collider.transform.position = new Vector3(Collider.transform.position.x, hit.point.y, Collider.transform.position.z);

                    SetJumpState(false);
                }

                SetGrounded(true);

                return hit.point;
            }

            _standingCollider = null;

            SetGrounded(false);
            
            return Vector3.zero;
        }


        void SetGrounded(bool state)
        {
            if (_grounded != state)
                GroundState?.Invoke(state);

            _grounded = state;
        }

        protected void SetJumpState(bool state)
        {
            _jumping = state;
            JumpState?.Invoke(state);
        }
    }
}