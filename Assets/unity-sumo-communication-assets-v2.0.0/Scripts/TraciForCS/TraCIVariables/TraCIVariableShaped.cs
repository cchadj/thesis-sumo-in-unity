using System.Collections.Generic;
using System;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public abstract class TraCIVariableShaped : TraCIVariable
    {
        [SerializeField]
        private List<Vector2> shapeVertexPoints;
        public List<Vector2> ShapeVertexPoints
        {
            get => shapeVertexPoints;
            set => shapeVertexPoints = value;
        }
    }
}