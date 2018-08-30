using System;
using UnityEngine;

namespace Graphene.Physics
{
    [Serializable]
    public abstract class PhysycsBase
    {
        public float Speed = 3;

        public Vector2 Velocity;
        public Vector2 Position;

        public int PositionZ;

        private Collider2D _collider;
        protected RaycastHit2D[] _results = new RaycastHit2D[1];
        protected RaycastHit2D[] _triggerResults = new RaycastHit2D[1];

        public Action<RaycastHit2D> OnTriggerEnter;
        public Action<RaycastHit2D> OnCollisionEnter;

        private RaycastHit2D _lastTrigger, _lastCollider;

        private ContactFilter2D _contactFilter;

        public void SetCollider(Collider2D collider)
        {
            _collider = collider;

            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask((LayerMask) Physics2D.GetLayerCollisionMask(collider.gameObject.layer));
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }
        

        protected bool CheckInCollider()
        {
            var count = CheckCollision(Position);

            if (count == 0) return false;

            var dir = Position - new Vector2(_results[0].transform.position.x, _results[0].transform.position.y);
            Position += dir.normalized*0.001f;
            return true;
        }

        public int CheckCollision(Vector2 position)
        {
            _contactFilter.useTriggers = false;
            CheckTriggerCollision(position);
            var res = _collider.Cast(position, _contactFilter, _results, 0); //position.magnitude);
            
            if (res > 0)
            {
                if (_results[0].collider.isTrigger)
                {
                    res--;
                }
                else
                {
                    if (_lastCollider != _results[0] && OnCollisionEnter != null) OnCollisionEnter(_results[0]);
                    _lastCollider = _results[0];
                }
            }
            else
            {
                _lastCollider = new RaycastHit2D();
            }
            
            return res;
        }

        protected int CheckTriggerCollision(Vector2 direction)
        {
            _contactFilter.useTriggers = true;
            var res = _collider.Cast(direction, _contactFilter, _triggerResults, direction.magnitude);

            if (res > 0)
            {
                if (_triggerResults[0].collider.isTrigger)
                {
                    if (_lastTrigger != _triggerResults[0] && OnTriggerEnter != null) OnTriggerEnter(_triggerResults[0]);
                    _lastTrigger = _triggerResults[0];
                }
                else
                {
                    res--;
                }
            }
            else
            {
                _lastTrigger = new RaycastHit2D();
            }

            return res;
        }
    }
}