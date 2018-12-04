using System;
using UnityEngine;

namespace Graphene.Physics.SideScroller
{
    public class SideScrollerCharacterPhysics : Basic2DPhysics
    {
        public CapsuleCollider2D _collider;
        private Transform _camera;
        private LayerMask _movementMask;
        private bool _climbing;
        private Vector3 _velocity;
        private float _stepAngle;
        private float _gravity = 9.8f;
        private bool _blockMovement;
        
        public event Action OnEdge;
        public event Action<int> OnWallClose;

        public SideScrollerCharacterPhysics(Rigidbody2D rigidbody, CapsuleCollider2D collider, Transform camera)
        {
            _collider = collider;
            _camera = camera;
            
            SetCollider(collider, rigidbody);
            
            _movementMask |= 1 << LayerMask.NameToLayer("Level");
        }
    
        public void Move(Vector2 dir, float speed, bool transformDir = true)
        {
            CheckGround();
            
            if (_blockMovement)
            {
                Rigidbody.velocity += Vector2.down * _gravity * Time.deltaTime;
                return;
            }

            dir = Vector2.ClampMagnitude(dir, 1);

            Vector2 wdir = transformDir ? _camera.TransformDirection(new Vector3(dir.x, dir.y)) : new Vector3(dir.x, dir.y);

            var moveDirection = GetGroundOrient(wdir).normalized;

            CheckSurround(wdir);

            _velocity.x = moveDirection.x * speed;
            _velocity.z = moveDirection.y * speed;

            if (!_grounded || _velocity.magnitude <= 0)
            {
                _velocity.x = wdir.x * speed;
                _velocity.z = wdir.y * speed;
            }

            if (_grounded)
            {
                if (!_jumping)
                    _velocity.y = moveDirection.y * speed;

                _velocity.y = Mathf.Max(_velocity.y, 0);
            }
            else
            {
                _velocity.y -= _gravity * Time.deltaTime;

                _velocity.y = Mathf.Max(_velocity.y, -_gravity * 2);
            }

            Rigidbody.velocity = _velocity;
        }

        public void Jump(bool b)
        {
            
        }

        public float Speed()
        {
            return _velocity.magnitude;
        }
        
        private Vector2 GetGroundOrient(Vector2 wdir)
        {
            if (wdir.magnitude <= 0) return Vector2.zero;

            Vector2 pos = _collider.transform.position;

            var rayhit = Physics2D.Raycast(pos + Vector2.up, -new Vector2(_collider.transform.up.x, _collider.transform.up.y), 2f, _movementMask);

            if (rayhit.collider == null) return Vector3.zero;

            var distance = CheckBounds(rayhit);

            if (distance < 0.2f)
            {
                if (!Physics2D.Raycast(
                    pos + Vector2.up,
                    (_collider.transform.forward - _collider.transform.up).normalized,
                    2f,
                    _movementMask
                ))
                {
                    OnEdge?.Invoke();
                }
            }

            var rot = Quaternion.AngleAxis(90, _collider.transform.up) * wdir;

            var cross = Vector3.Cross(rot, rayhit.normal);

            _stepAngle = Vector3.Angle(cross, Vector3.down);

            return cross;
        }
        
        private void CheckSurround(Vector2 wdir)
        {
            Vector2 pos = new Vector2(_collider.transform.position.x, _collider.transform.position.y) + Vector2.up;

            for (int i = 1, n = _sides.Length; i < n; i++)
            {
                var dir = _collider.transform.TransformDirection(_sides[i]);
                var rayhit = Physics2D.Raycast(pos, new Vector2(dir.x, dir.y), 2, _movementMask);
                if (rayhit.collider == null) continue;


                if (rayhit.distance < 1f)
                {
                    OnWallClose?.Invoke(i);

                    if (i == 2 || i == 4) // left - right
                        CheckWallHeigt(rayhit);

                    Debug.DrawLine(pos, rayhit.point, Color.red);
                    return;
                }
                else
                {
                    Debug.DrawLine(pos, rayhit.point, Color.blue);
                }
            }

            OnWallClose?.Invoke(0);
        }
        
        private float CheckBounds(RaycastHit2D rayhit)
        {
            var bounds = rayhit.collider.bounds;
            var corners = new Vector3[]
            {
                bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
                bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
                bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
                bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
            };

            var result = Mathf.Infinity;
            var side = -1;
            for (int i = 0, n = corners.Length; i < n; i++)
            {
                var line = -corners[i] + corners[(i + 1) % n];
                var value = Vector3.Cross(line, _collider.transform.position - corners[i]).magnitude;

                if (value / line.magnitude < result)
                {
                    result = value / line.magnitude;
                }
            }


            if (side >= 0)
            {
                Debug.DrawLine(corners[side], corners[(side + 1) % corners.Length], Color.magenta);
            }

//            Debug.Log(result);
            return result;
        }
        
        private float CheckWallHeigt(RaycastHit2D rayhit)
        {
            var bounds = rayhit.collider.bounds;
            var corners = new Vector3[]
            {
                bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
                bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
                bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
                bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
            };

            var result = Mathf.Infinity;
            var side = -1;
            var pos = _collider.transform.position;
            pos.y = bounds.extents.y;
            for (int i = 0, n = corners.Length; i < n; i++)
            {
                var line = -corners[i] + corners[(i + 1) % n];
                var value = Vector3.Cross(line, _collider.transform.position - corners[i]).magnitude;

                if (value / line.magnitude < result)
                {
                    result = value / line.magnitude;
                    side = i;
                }
            }

            if (side >= 0)
            {
                Debug.DrawLine(corners[side], corners[(side + 1) % corners.Length], Color.blue);
                var dist = bounds.extents.y - _collider.transform.position.y;
                
                return dist;
            }

            return 0;
        }
    }
}