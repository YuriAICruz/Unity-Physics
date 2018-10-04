using System;
using UnityEngine;
#if UNITY_EDITOR
using Graphene.Utils.Editor;
#endif

namespace Graphene.Physics.GroundSystem
{
    [Flags]
    public enum Sides
    {
        None = (1 << 0),
        Up = (1 << 1),
        Right = (1 << 2),
        Down = (1 << 3),
        Left = (1 << 4)
    }

    public class Ground : MonoBehaviour
    {
        public GroundType Type;

#if UNITY_EDITOR
        [EnumFlags]
#endif
        public Sides SidesToJump;
    }
}