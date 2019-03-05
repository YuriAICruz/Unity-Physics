using UnityEngine;

namespace Graphene.Physics.TwinStick
{
    public class TwinStickShipPhysics
    {
        public Rigidbody Rigidbody;
        
        private readonly ShipStatus _shipStatus;
        private Vector3 _velocity;

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

            _velocity = Vector3.ClampMagnitude(_velocity, _shipStatus.Thrusters*10);
            
            Rigidbody.velocity = _velocity;
            
            _velocity -= _velocity.normalized * pwd * 0.1f;
            
            Debug.Log($"_velocity: {_velocity}");

        }

        public void Look(Vector2 dir)
        {
            //Debug.Log($"Look: {dir}");
        }
    }
}