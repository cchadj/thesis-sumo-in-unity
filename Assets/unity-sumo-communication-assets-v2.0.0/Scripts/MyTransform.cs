using UnityEngine;

namespace RiseProject.Tomis.CustomTypes
{
    public class MyTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public float angle;

        public MyTransform()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            angle = 0f;
        }

        public MyTransform(Vector3 position, float angle = 0f)
        : this(position, Quaternion.identity, angle) { }

        public MyTransform(Vector3 position, Quaternion rotation, float angle = 0f)
            : this()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            angle = 0f;
        }

    }
}
