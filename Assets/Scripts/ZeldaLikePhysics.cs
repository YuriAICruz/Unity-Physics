using System;
using System.Collections;
using System.IO;
using System.Threading;
using Physics.GroundSystem;
using UnityEngine;
using Utils;

namespace Physics
{
    [Serializable]
    public class ZeldaLikePhysics : PhysycsBase
    {
        [HideInInspector] public bool Grounded;
        private Vector2 _iniVelocity;

        public float DodgeSpeed = 3;
        public float DodgeTime = 0.6f;

        public float JumpSpeed = 3;
        public float JumpTime = 0.5f;

        private Ground _currentGround;
        private bool _hasBounds;
        private Bounds _groundBounds;

        public event Action<Vector2> OnJump;

        public void Move(Vector2 dir)
        {
            if (CheckInCollider()) return;

            var displacement = dir * Speed * Time.deltaTime;

            var count = CheckCollision(displacement);

            if (count > 0)
            {
                Velocity = Vector2.zero;
                return;
            }

            UpdateGround(displacement);

            Vector2 edgeDir;
            bool block;
            if (_hasBounds && CheckLedge(displacement, out edgeDir, out block))
            {
                if (block)
                {
                    Velocity = displacement;
                    Position = edgeDir;
                    return;
                }

                DoJump(displacement);
                return;
            }

            Velocity = displacement;
            Position += displacement;
        }

        private void DoJump(Vector2 dir)
        {
            if (OnJump != null) OnJump(dir);
        }

        public void Jump(Vector2 dir, Action onUpdate, Action onEnd)
        {
            ClearGround();
            GlobalCoroutineManager.Instance.StartCoroutine(JumpRoutine(dir, onUpdate, onEnd));
        }

        IEnumerator JumpRoutine(Vector2 dir, Action onUpdate, Action onEnd)
        {
            dir = dir.normalized;
            var time = 0f;
            var displacement = dir * JumpSpeed * Time.deltaTime;

            while (time <= JumpTime)
            {
                var count = CheckCollision(displacement);
                time += Time.deltaTime;

                if (count > 0)
                {
                    Velocity = Vector2.zero;
                    if (onUpdate != null) onUpdate();
                    break;
                }

                Velocity = displacement;
                Position += displacement;

                if (onUpdate != null) onUpdate();

                yield return new WaitForChangedResult();
            }

            if (onEnd != null) onEnd();
        }

        private bool CheckLedge(Vector2 displacement, out Vector2 dir, out bool block)
        {
#if UNITY_EDITOR
            DrawBounds();
#endif
            block = false;

            var center = new Vector2(_groundBounds.center.x, _groundBounds.center.y);
            var centerDir = center - (Position + displacement);

            dir = -centerDir;

            if (!_hasBounds) return false;

            var side = GetDir(dir);

            switch (side)
            {
                case Sides.None:
                    dir = new Vector2(0, 0);
                    break;
                case Sides.Up:
                    dir = new Vector2(0, 1);
                    break;
                case Sides.Right:
                    dir = new Vector2(1, 0);
                    break;
                case Sides.Down:
                    dir = new Vector2(0, -1);
                    break;
                case Sides.Left:
                    dir = new Vector2(-1, 0);
                    break;
            }

            if ((_currentGround.SidesToJump & side) == 0) return false;

            var point = CheckInsideBounds(centerDir);
            var lastPoint = CheckInsideBounds(center - Position);

            block = lastPoint;
            if (block)
            {
                var dist = 0f;
                var pos = (Position + displacement);
                switch (side)
                {
                    case Sides.None:
                        dir = Position;
                        break;
                    case Sides.Up:
                        dist = pos.y - (center.y + _groundBounds.extents.y);
                        dist = Mathf.Max(0, dist);
                        dir = pos * new Vector2(1, 0) + center * dir + _groundBounds.extents * dir + new Vector2(0, dist);
                        break;
                    case Sides.Right:
                        dist = pos.x - (center.x + _groundBounds.extents.x);
                        dist = Mathf.Max(0, dist);
                        dir = pos * new Vector2(0, 1) + center * dir + _groundBounds.extents * dir + new Vector2(dist, 0);
                        break;
                    case Sides.Down:
                        dist = -pos.y + (center.y - _groundBounds.extents.y);
                        dist = Mathf.Max(0, dist);
                        dir = pos * new Vector2(1, 0) + center * new Vector2(0, 1) + _groundBounds.extents * dir + new Vector2(0, -dist);
                        break;
                    case Sides.Left:
                        dist = -pos.x + (center.x - _groundBounds.extents.x);
                        dist = Mathf.Max(0, dist);
                        dir = pos * new Vector2(0, 1) + center * new Vector2(1, 0) + _groundBounds.extents * dir + new Vector2(-dist, 0);
                        break;
                }
            }

            return point;
        }

        private bool CheckInsideBounds(Vector2 centerDir)
        {
            if (_groundBounds.extents.x - Mathf.Abs(centerDir.x) < 0.1f ||
                _groundBounds.extents.y - Mathf.Abs(centerDir.y) < 0.1f
            )
            {
                return true;
            }
            return false;
        }

        private Sides GetDir(Vector2 dir)
        {
            var ratio = _groundBounds.extents.x / _groundBounds.extents.y;
            dir = new Vector2(dir.x, dir.y * ratio);

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                if (dir.x > 0)
                {
                    return Sides.Right;
                }
                if (dir.x < 0)
                {
                    return Sides.Left;
                }
            }
            else
            {
                if (dir.y > 0)
                {
                    return Sides.Up;
                }
                if (dir.y < 0)
                {
                    return Sides.Down;
                }
            }

            return Sides.Up;
        }

        private void UpdateGround(Vector2 displacement)
        {
            var mask = 0;
            mask |= 1 << LayerMask.NameToLayer("Ground");

            var hit = Physics2D.CircleCast(Position + displacement, 0.1f, Vector2.zero, 0.1f, mask, 0, 1000);

            if (hit.collider != null)
            {
                var target = hit.collider.gameObject.GetComponent<Ground>();

                if (target != null)
                {
                    _currentGround = target;
                    _groundBounds = hit.collider.bounds;
                    _hasBounds = true;
                }
                return;
            }

            ClearGround();
        }

        private void ClearGround()
        {
            if (_hasBounds)
                _groundBounds = new Bounds();

            _hasBounds = false;
            _currentGround = null;
        }

        private void DrawBounds()
        {
            Debug.DrawLine(Position, _groundBounds.max, Color.magenta);
            Debug.DrawLine(Position, _groundBounds.min, Color.magenta);
            Debug.DrawLine(Position, _groundBounds.center + new Vector3(-_groundBounds.extents.x, _groundBounds.extents.y, _groundBounds.extents.z), Color.magenta);
            Debug.DrawLine(Position, _groundBounds.center + new Vector3(_groundBounds.extents.x, -_groundBounds.extents.y, _groundBounds.extents.z), Color.magenta);

            var center = new Vector2(_groundBounds.center.x, _groundBounds.center.y);
            var dist = center - Position;

            if (_groundBounds.extents.x - Mathf.Abs(dist.x) < -0.05 ||
                _groundBounds.extents.y - Mathf.Abs(dist.y) < -0.05
            )
            {
                Debug.DrawRay(Position, dist, Color.gray);
                return;
            }

            if (_groundBounds.extents.x - Mathf.Abs(dist.x) < 0.1f ||
                _groundBounds.extents.y - Mathf.Abs(dist.y) < 0.1f
            )
            {
                Debug.DrawRay(Position, dist, Color.blue);
            }
            else
            {
                Debug.DrawRay(Position, dist, Color.red);
            }
        }

        public void Dodge(Action onUpdate, Action onEnd)
        {
            GlobalCoroutineManager.Instance.StartCoroutine(DodgeRoutine(Velocity, onUpdate, onEnd));
        }

        IEnumerator DodgeRoutine(Vector2 dir, Action onUpdate, Action onEnd)
        {
            dir = dir.normalized;
            var time = 0f;
            var displacement = dir * DodgeSpeed * Time.deltaTime;
            while (time <= DodgeTime)
            {
                var count = CheckCollision(displacement);
                time += Time.deltaTime;

                if (count > 0)
                {
                    Velocity = Vector2.zero;
                    if (onUpdate != null) onUpdate();
                    break;
                }

                Velocity = displacement;
                Position += displacement;

                if (onUpdate != null) onUpdate();

                yield return new WaitForChangedResult();
            }

            if (onEnd != null) onEnd();
        }

        public GroundType GetGround()
        {
            if (_currentGround == null) return GroundType.Grass;

            return _currentGround.Type;
        }
    }
}