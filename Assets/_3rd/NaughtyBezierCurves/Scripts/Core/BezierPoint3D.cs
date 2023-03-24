using UnityEngine;

namespace NaughtyBezierCurves
{
    public class BezierPoint3D : MonoBehaviour
    {
        public enum HandleType
        {
            Connected,
            Broken
        }

        // Serializable Fields
        [SerializeField]
        [Tooltip("The curve that the point belongs to")]
        private BezierCurve3D curve = null;

        [SerializeField]
        private HandleType handleType = HandleType.Connected;

        [SerializeField]
        private Vector3 leftHandleLocalPosition = new Vector3(-0.5f, 0f, 0f);

        [SerializeField]
        private Vector3 rightHandleLocalPosition = new Vector3(0.5f, 0f, 0f);

        // Properties

        /// <summary>
        /// Gets or sets the curve that the point belongs to.
        /// </summary>
        public BezierCurve3D Curve
        {
            get
            {
                return this.curve;
            }
            set
            {
                this.curve = value;
            }
        }

        /// <summary>
        /// Gets or sets the type/style of the handle.
        /// </summary>
        public HandleType HandleStyle
        {
            get
            {
                return this.handleType;
            }
            set
            {
                this.handleType = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of the transform.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.transform.position;
            }
            set
            {
                this.transform.position = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of the transform.
        /// </summary>
        public Vector3 LocalPosition
        {
            get
            {
                return this.transform.localPosition;
            }
            set
            {
                this.transform.localPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets the local position of the left handle.
        /// If the HandleStyle is Connected, the local position of the right handle is automaticaly set.
        /// </summary>
        public Vector3 LeftHandleLocalPosition
        {
            get
            {
                return this.leftHandleLocalPosition;
            }
            set
            {
                this.leftHandleLocalPosition = value;
                if (this.handleType == HandleType.Connected)
                {
                    this.rightHandleLocalPosition = -value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the local position of the right handle.
        /// If the HandleType is Connected, the local position of the left handle is automaticaly set.
        /// </summary>
        public Vector3 RightHandleLocalPosition
        {
            get
            {
                return this.rightHandleLocalPosition;
            }
            set
            {
                this.rightHandleLocalPosition = value;
                if (this.handleType == HandleType.Connected)
                {
                    this.leftHandleLocalPosition = -value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position of the left handle.
        /// If the HandleStyle is Connected, the position of the right handle is automaticaly set.
        /// </summary>
        public Vector3 LeftHandlePosition
        {
            get
            {
                return this.transform.TransformPoint(this.LeftHandleLocalPosition);
            }
            set
            {
                this.LeftHandleLocalPosition = this.transform.InverseTransformPoint(value);
            }
        }

        /// <summary>
        /// Gets or sets the position of the right handle.
        /// If the HandleType is Connected, the position of the left handle is automaticaly set.
        /// </summary>
        public Vector3 RightHandlePosition
        {
            get
            {
                return this.transform.TransformPoint(this.RightHandleLocalPosition);
            }
            set
            {
                this.RightHandleLocalPosition = this.transform.InverseTransformPoint(value);
            }
        }
    }
}
