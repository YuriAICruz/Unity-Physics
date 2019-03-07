using UnityEngine;

namespace Graphene.Physics.TwinStick
{
    public class TwinStickShipPhysics
    {
        public Rigidbody Rigidbody;

        private readonly ShipStatus _shipStatus;
        private Vector3 _velocity;

        private Vector3 _lastLook;
        private Vector3 _turnVelocity;

        public TwinStickShipPhysics(Rigidbody rigidbody, ShipStatus shipStatus)
        {
            Rigidbody = rigidbody;

            _shipStatus = shipStatus;
        }

        public void Move(Vector3 dir)
        {
            var pwd = _shipStatus.Thrusters / _shipStatus.Weight;

            dir = new Vector3(dir.x, 0, dir.y);

            _velocity += dir * pwd;

            _velocity = Vector3.ClampMagnitude(_velocity, _shipStatus.Thrusters * 10);

            Rigidbody.velocity = _velocity;

            _velocity -= _velocity.normalized * pwd * 0.1f;
        }

        public void Look(Vector2 dir, bool isMouse)
        {
            var pwd = _shipStatus.Thrusters / _shipStatus.Weight;
            Vector3 pos;
            if (isMouse)
            {
                var cam = Camera.main;
                pos = cam.ScreenToWorldPoint(new Vector3(dir.x, dir.y, (Rigidbody.transform.position - cam.transform.position).magnitude));
                pos.y = Rigidbody.transform.position.y;
            }
            else
            {
                pos = Rigidbody.transform.position + new Vector3(dir.x, 0, dir.y);
            }

            //pos = _lastLook + (pos - _lastLook) * _shipStatus.Thrusters * Time.deltaTime;
            //pos = Vector3.SmoothDamp(_lastLook, pos, ref _turnVelocity, 1, 1, Time.deltaTime);

            //_lastLook = pos;

            Rigidbody.transform.LookAt(pos);
        }
    }
}