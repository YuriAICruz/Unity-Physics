using System;
using System.Collections;
using Graphene.Utils;
using UnityEngine;

namespace Graphene.Physics
{
    [Serializable]
    public class TesterPhysics : PhysycsBase
    {
        public void Play(Vector2 position, Collider2D collider)
        {
            SetCollider(collider);
            SetPosition(position);
            _results = new RaycastHit2D[5];
            _dir = Vector2.down;
            GlobalCoroutineManager.Instance.StartCoroutine(PlayRoutine());
        }

        private Vector2 _dir;

        public void Move()
        {
            var dir = _dir * Time.deltaTime * Speed;

            var res = CheckCollision(dir);

            if (res == 0)
            {
                Position += dir;
                return;
            }
            
            _dir = new Vector2(UnityEngine.Random.value-0.5f, UnityEngine.Random.value-0.5f);
            _dir.Normalize();
            
            Move();
        }


        IEnumerator PlayRoutine()
        {
            while (true)
            {
                Move();

                yield return null;
            }
        }
    }
}