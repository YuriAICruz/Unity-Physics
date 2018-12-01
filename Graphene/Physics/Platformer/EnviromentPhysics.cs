using System;
using System.Collections;
using Graphene.Utils;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Graphene.Physics.Platformer
{
    [Serializable]
    public class EnviromentPhysics : BasicPhysics
    {
        private Vector3 _position;

        public override void SetCollider(Collider collider, Rigidbody rigidbody)
        {
            base.SetCollider(collider, rigidbody);

            // GlobalCoroutineManager.Instance.StartCoroutine(Update());
        }

        public bool CheckCollision(Vector3 pos, Vector3 dir)
        {
            RaycastHit hit;
            UnityEngine.Physics.Raycast(pos, dir, out hit, dir.magnitude);
            
            if(hit.collider == null) return false;

            return true;
        }
        
        IEnumerator Update()
        {
            while (true)
            {
                CheckGround();

                _position = Collider.transform.position;

                if (_grounded)
                {
                    yield return null;
                    continue;
                }

                _position.y += Time.deltaTime * UnityEngine.Physics.gravity.y;

                Collider.transform.position = _position;
                yield return null;
            }
        }
    }
}