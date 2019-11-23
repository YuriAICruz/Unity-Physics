using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Graphene.Physics.FirstPerson
{
    public class BasicPhysics : MonoBehaviour
    {
        [HideInInspector] public CharacterController CharacterController;

        protected bool _debug = true;

        [SerializeField] protected bool _grounded;

        public float AirSpeedBuff = 0.8f;

        [SerializeField] protected int _jumpCount = 1;
        protected int _jumps;
        [SerializeField] protected bool _jumping;
        protected bool _blocked;

        protected float _radius;
        private float _height;

        public float Gravity = 9;

        public event Action<bool> JumpState, GroundState;

        public LayerMask Level;
        private Vector3 _velocity;

        protected Vector3[] _sides = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0),
        };

        protected float[] _heights = new float[]
        {
            0.2f, 0.4f, 0.6f, 0.8f
        };

        private float _stepAngle;

        private void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            _radius = CharacterController.bounds.size.x / 2 * 1.1f;
            _height = CharacterController.bounds.size.y * 1.1f;

            SendMessage("Setup", SendMessageOptions.DontRequireReceiver);
        }

        public void Jump(float speed)
        {
            if (_jumps >= _jumpCount || !_grounded && _jumps == 0) return;

            _jumping = true;
            _grounded = false;

            _jumps++;

            _velocity.y = speed;
        }

        private void JumpMove(Vector3 dir)
        {
//            _velocity.x = dir.x * 0.5f;
//            _velocity.z = dir.z * 0.5f;
                
            if (dir.x != 0)
                _velocity.x = dir.x;

            if (dir.z != 0)
                _velocity.z = dir.z;

            _velocity.y += Gravity * Time.deltaTime;

            if (_velocity.y < 0)
            {
                _jumping = false;
            }
            
            UpdateRigidbody();
        }

        public void Move(Vector3 dir, float speed)
        {
            var block = CalculateSurrounds();

            if (_blocked) return;

            if (_jumping)
            {
                dir = RecalculateDir(dir, speed, block);

                JumpMove(dir);
                return;
            }

            dir = CheckGround(dir);

            dir = RecalculateDir(dir, speed, block);

            _velocity.x = dir.x;
            _velocity.z = dir.z;

            if (_grounded)
            {
                if (!_jumping)
                    _velocity.y = dir.y;

                UpdateRigidbody();
                return;
            }

            _velocity.y += Gravity * Time.deltaTime;

            UpdateRigidbody();
        }

        private Vector3 RecalculateDir(Vector3 dir, float speed, Vector3 block)
        {
            dir -= block;

            dir.Normalize();

            Vector3.ClampMagnitude(dir, 1);

            dir *= speed * (_grounded ? 1 : AirSpeedBuff) * Time.deltaTime;

            return dir;
        }

        private Vector3 CalculateSurrounds()
        {
            if (_blocked) return Vector3.zero;

            RaycastHit hit;

            var block = Vector3.zero;

            for (int j = 0; j < _heights.Length; j++)
            {
                for (int i = 0, n = _sides.Length; i < _sides.Length; i++)
                {
                    var pos = transform.position + Vector3.up * _heights[j] * _height;
                    var side = transform.TransformDirection(_sides[i]);

                    if (UnityEngine.Physics.Raycast(pos, side, out hit, _radius * 5))
                    {
                        if (hit.distance <= _radius * 1.5f)
                        {
                            if (i == 0 && j > 0)
                            {
                                var top = hit.collider.bounds.center + Vector3.up * hit.collider.bounds.size.y / 2f;

                                var ledge = hit.point;
                                ledge.y = top.y;

                                Debug.DrawRay(ledge, Vector3.up, Color.blue);

                                if (ledge.y > hit.point.y && ledge.y - hit.point.y < _height)
                                {
                                    GrabLedge(ledge + transform.forward * 0.1f);

                                    return Vector3.zero;
                                }

                                //LedgeDetection(hit, top);
                            }
                        }

                        if (hit.distance <= _radius * 1.2f)
                        {
                            block += side;

                            Debug.DrawRay(pos, -pos + hit.point, Color.red);
                        }
                    }
                }
            }

            return block.normalized;
        }

        private void GrabLedge(Vector3 ledge)
        {
            StartCoroutine(LedgeClimb(ledge));
        }

        IEnumerator LedgeClimb(Vector3 ledge)
        {
            _blocked = true;

            CharacterController.enabled = false;

            var v = _velocity;
            _velocity = Vector3.zero;
            UpdateRigidbody();

            var t = 0f;
            var p = transform.position;
            var d = 0.25f;

            while (t <= d)
            {
                transform.position = Vector3.Lerp(p, ledge, t / d);

                t += Time.deltaTime;
                yield return null;
            }

            transform.position = ledge;

            _blocked = false;

            CharacterController.enabled = true;

            _velocity = v;
            UpdateRigidbody();
        }

        private static void LedgeDetection(RaycastHit hit, Vector3 top)
        {
            var extends = hit.collider.bounds.extents;

            Vector3[] edges;

            var box = (BoxCollider) hit.collider;

            if (box)
            {
                extends = Vector3.Scale(box.size / 2, hit.transform.lossyScale);

                edges = new Vector3[]
                {
                    top + hit.transform.TransformDirection(new Vector3(extends.x, 0, extends.z)),
                    top + hit.transform.TransformDirection(new Vector3(extends.x, 0, -extends.z)),
                    top + hit.transform.TransformDirection(new Vector3(-extends.x, 0, -extends.z)),
                    top + hit.transform.TransformDirection(new Vector3(-extends.x, 0, extends.z))
                };
            }
            else
            {
                edges = new Vector3[]
                {
                    top + new Vector3(extends.x, 0, extends.z),
                    top + new Vector3(extends.x, 0, -extends.z),
                    top + new Vector3(-extends.x, 0, -extends.z),
                    top + new Vector3(-extends.x, 0, extends.z)
                };
            }

            var closer = Enumerable.Range(0, edges.Length).Aggregate((a, b) => (edges[a] - hit.point).magnitude < (edges[b] - hit.point).magnitude ? a : b);

            Debug.DrawRay(edges[closer], Vector3.up, Color.blue);
        }

        private void UpdateRigidbody()
        {
            CharacterController.Move(_velocity);
        }

        private Vector3 CheckGround(Vector3 dir)
        {
            RaycastHit hit;

            var sides = _sides.ToList();

            sides.Add(Vector3.zero);

            for (int i = 0, n = sides.Count; i < n; i++)
            {
                var side = transform.TransformDirection(sides[i]);
                var ray = new Ray(transform.position + 0.5f * CharacterController.bounds.size.y * Vector3.up, Vector3.down);

                Debug.DrawRay(ray.origin, ray.direction, Color.red);
                if (UnityEngine.Physics.Raycast(ray, out hit, _height * 0.5f, Level))
                {
                    if (hit.distance <= _height)
                    {
                        _grounded = true;
                        _jumps = 0;

                        var rot = Quaternion.AngleAxis(90, transform.up) * dir;

                        var cross = Vector3.Cross(rot, hit.normal);

                        _stepAngle = Vector3.Angle(cross, Vector3.down);

                        dir = cross;

                        return dir;
                    }
                }
            }

            _grounded = false;

            return dir;
        }

        public float Speed()
        {
            return CharacterController.velocity.magnitude;
        }

        public void Dodge(float dodgeDuration, float f, Action action)
        {
            throw new NotImplementedException();
        }

        public void Climb(float height, float speed)
        {
            throw new NotImplementedException();
        }
    }
}