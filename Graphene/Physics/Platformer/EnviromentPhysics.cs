using System;
using System.Collections;
using Graphene.Utils;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Graphene.Physics.Platformer
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class EnviromentPhysics : BasicPhysics
    {
        private Vector3 _position;

        public bool CheckCollision(Vector3 pos, Vector3 dir)
        {
            RaycastHit hit;
            UnityEngine.Physics.Raycast(pos, dir, out hit, dir.magnitude);
            
            if(hit.collider == null) return false;

            return true;
        }
        
        void Update()
        {
            if(Collider == null) return;
            
            var pos = CheckGround();

            _position = transform.position;

            if (_grounded)
            {
                var col = (BoxCollider) Collider;
                transform.position = new Vector3(_position.x, col.center.y + col.size.y/2 + pos.y, _position.z);
                return;
            }

            _position.y += Time.deltaTime * UnityEngine.Physics.gravity.y;

            transform.position = _position;
        }
    }
}