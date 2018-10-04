using System;
//using CameraSystem;
using UnityEngine;

namespace Graphene.Physics
{
    [Serializable]
    public class SideShooterPhysics : PhysycsBase
    {
//        private CameraManagement _cam;
//        private Vector2 _iniVelocity = Vector2.zero;
//        private Vector3 _camLastPos;

        public void Move(Vector2 dir)
        {
            Debug.LogError("Need to be in shooter assembly");
//            if (_cam == null)
//            {
//                _cam = UnityEngine.Object.FindObjectOfType<CameraManagement>();
//                if (_cam == null)
//                    return;
//                else
//                    _camLastPos = _cam.transform.position;
//            }
//
//            var displacement = dir * Speed * Time.deltaTime;
//            var diff = _cam.transform.position - _camLastPos;
//            displacement += new Vector2(diff.x, diff.y);
//
//            if (!_cam.GetInsideBounds(Position + displacement.normalized))
//            {
//                displacement = Vector2.zero;
//            }
//
//            var count = CheckCollision(displacement);
//            _camLastPos = _cam.transform.position;
//
//            if (count > 0)
//            {
//                return;
//            }
//
//            Velocity = Vector2.SmoothDamp(Velocity, dir * Speed, ref _iniVelocity, 0.4f);
//            Position += displacement;
        }
    }
}