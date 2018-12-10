using System;
using System.Collections;
using Graphene.Utils;
using UnityEngine;

namespace Graphene.Physics.SideScroller
{
    public class SideScrollerCharacterPhysics : Basic2DPhysics
    {
        public CapsuleCollider2D _collider;
        private Transform _camera;
        private LayerMask _movementMask;
        private bool _climbing;
        private Vector2 _velocity;
        private float _stepAngle;
        private float _gravity;
        private bool _blockMovement;

        public bool Sliding;

        public event Action OnEdge;
        public event Action<int> OnWallClose;

        private int _wall = 0;

        private bool _canJump;
        private bool _freeze;
        private float _wallDistance;
        private float _wallSlide;
        private Coroutine _throwRoutine;
        private Coroutine _dashRoutine;
        private bool _dashEffect;
        private float _dashSpeedModifier = 1.4f;
        private Vector2 _lastV;

        public bool Dashing;

        public SideScrollerCharacterPhysics(Rigidbody2D rigidbody, CapsuleCollider2D collider, Transform camera, float gravity, float wallSlide) : base(rigidbody, collider, camera)
        {
            _collider = collider;
            _camera = camera;
            _gravity = gravity;

            _radius = collider.size.x / 2;
            _height = collider.size.y / 2;

            _wallSlide = wallSlide;

            SetCollider(collider, rigidbody);

            _movementMask |= 1 << LayerMask.NameToLayer("Level");
        }

        public void Jump(bool jump, float speed, float wallJumpSpeed)
        {
            if (jump && (_canJump || _wall != 0 && _wallDistance <= _radius * 1.1f))
            {
                if (_wall != 0)
                {
                    _velocity.x = -wallJumpSpeed * _sides[_wall].x * (_dashEffect ? _dashSpeedModifier : 1);
                }
                _velocity.y = speed;
            }
            else if (!jump)
            {
                _velocity.y = Mathf.Min(0, _velocity.y);
            }
            _canJump = false;
            SetJumpState(jump);
        }

        public float Speed()
        {
            return Mathf.Abs(_velocity.x);
        }

        public void Move(Vector2 dir, float speed, bool transformDir = true)
        {
            if (_freeze) return;

            if (_dashEffect)
                speed *= _dashSpeedModifier;

            CheckGround();

            if (_blockMovement)
            {
                Rigidbody.velocity += Vector2.down * _gravity * Time.deltaTime;
                return;
            }

            dir = Vector2.ClampMagnitude(dir, 1);
            var roundDir = (int) (Mathf.Sign(dir.x) * Mathf.Ceil(Mathf.Abs(dir.x)));

            Vector2 wdir = transformDir ? _camera.TransformDirection(new Vector3(dir.x, dir.y)) : new Vector3(dir.x, dir.y);

            var moveDirection = GetGroundOrient(wdir).normalized;

            CheckSurround(wdir);

            if (_wall == 0 ||
                roundDir != _sides[_wall].x ||
                Grounded ||
                (!_jumping && _wallDistance > _radius * 1.1f)
            )
            {
                _velocity.x = moveDirection.x * speed;

                if (!Grounded || _velocity.magnitude <= 0)
                {
                    _velocity.x = wdir.x * speed;
                }
            }

            Sliding = false;
            if (Grounded)
            {
                _canJump = true;

                if (!_jumping)
                    _velocity.y = moveDirection.y * speed;

                _velocity.y = Mathf.Max(_velocity.y, 0);
            }
            else
            {
                if (_wall != 0 && roundDir == _sides[_wall].x && _wallDistance <= _radius * 1.1f && !_jumping)
                {
                    _velocity.y = _wallSlide;
                    Sliding = true;
                }
                else
                {
                    _velocity.y -= _gravity * Time.deltaTime;
                }

                _velocity.y = Mathf.Max(_velocity.y, -_gravity);
            }

            Rigidbody.velocity = _velocity;
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

            _wall = 0;
            for (int i = 1, n = _sides.Length; i < n; i++)
            {
                var dir = _collider.transform.TransformDirection(_sides[i]);
                var rayhit = Physics2D.Raycast(pos, new Vector2(dir.x, dir.y), _radius * 5, _movementMask);

                if (rayhit.collider == null) continue;

                _wall = i;
                _wallDistance = rayhit.distance;
                if (rayhit.distance <= _radius * 1.1f)
                {
                    OnWallClose?.Invoke(i);

                    var heigt = CheckWallHeigt(rayhit);

                    // Set position
                    //_collider.transform.position = new Vector3( rayhit.point.x - _radius * _sides[i].x, Collider.transform.position.y, Collider.transform.position.z );

                    Debug.DrawLine(pos, rayhit.point, Color.red);
                    Debug.DrawRay(pos, Vector2.up * heigt, Color.blue);
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


        public void DashStop()
        {
            if (_throwRoutine != null)
                GlobalCoroutineManager.Instance.StopCoroutine(_throwRoutine);

            if (!_dashEffect) return;

            Dashing = false;
            _velocity = Vector2.zero;
            _blockMovement = false;
            _dashEffect = false;
        }

        public void Dash(float dir, float speed, float duration)
        {
            if (Sliding) return;

            if (_dashRoutine != null)
                GlobalCoroutineManager.Instance.StopCoroutine(_dashRoutine);

            _dashRoutine = GlobalCoroutineManager.Instance.StartCoroutine(DashRoutine(dir, speed, duration));
        }

        IEnumerator DashRoutine(float dir, float speed, float duration)
        {
            _blockMovement = true;
            _dashEffect = true;
            Dashing = true;

            _velocity.y = 0;

            var t = duration;
            var v = _velocity;

            _velocity = Vector2.right * dir * speed;

            while (t > 0)
            {
                Rigidbody.velocity = Vector2.Lerp(v, _velocity, t / duration);

                t -= Time.deltaTime;

                yield return null;
            }

            _velocity = v;

            _blockMovement = false;
            Dashing = false;

            yield return new WaitForSeconds(0.4f);

            _dashEffect = false;
        }


        public void Throw(Vector3 dir, float force)
        {
            DashStop();
            
            if (_throwRoutine != null)
                GlobalCoroutineManager.Instance.StopCoroutine(_throwRoutine);

            _throwRoutine = GlobalCoroutineManager.Instance.StartCoroutine(ThrowRoutine(dir, force));
        }

        IEnumerator ThrowRoutine(Vector3 dir, float force)
        {
            _blockMovement = true;

            var t = 0.2f;
            var v = Vector2.zero;


            _velocity = dir.normalized * force;

            while (t > 0)
            {
                Rigidbody.velocity = Vector2.Lerp(v, _velocity, t / 0.4f);
                ;

                t -= Time.deltaTime;

                yield return null;
            }

            _velocity = v;

            _blockMovement = false;
        }

        public void Block(bool state)
        {
            if (state)
            {
                _lastV = _velocity;
                _velocity = Vector2.zero;
            }
            else
            {
                _velocity = _lastV;
            }
            _freeze = state;
        }
    }
}