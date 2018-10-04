using UnityEngine;

namespace Graphene.Physics
{
    public class Tester: MonoBehaviour
    {
        public SideShooterPhysics Physics;

        private void Start()
        {
            Physics.SetCollider(GetComponent<Collider2D>());
            Physics.SetPosition(transform.position);

            Physics.OnCollisionEnter += Collision;
            Physics.OnTriggerEnter += Trigger;
        }

        private void Trigger(RaycastHit2D obj)
        {
            Debug.Log("Triggered on " + obj.collider.name);
        }

        private void Collision(RaycastHit2D obj)
        {
            Debug.Log("Collided on " + obj.collider.name);
        }

        private void Update()
        {
            var dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Physics.Move(dir);
            transform.position = Physics.Position;
        }
    }
}