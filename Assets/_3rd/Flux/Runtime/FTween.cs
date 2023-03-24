using UnityEngine;
using System;

namespace Flux
{
    [Serializable]
    public abstract class FTweenBase
    {
        [SerializeField]
        protected FEasingType _easingType = FEasingType.EaseInOutQuad;
        public FEasingType EasingType { get { return _easingType; } set { _easingType = value; } }
    }

    [Serializable]
    public abstract class FTween<T> : FTweenBase
    {
        [SerializeField]
        protected T _from;
        public T From { get { return _from; } set { _from = value; } }

        [SerializeField]
        protected T _to;
        public T To { get { return _to; } set { _to = value; } }

        public abstract T GetValue(float t);
    }

    [Serializable]
    public class FTweenColor : FTween<Color>
    {
        public FTweenColor(Color from, Color to)
        {
            _from = from;
            _to = to;
        }

        public override Color GetValue(float t)
        {
            Color color;
            color.r = FEasing.Tween(_from.r, _to.r, t, _easingType);
            color.g = FEasing.Tween(_from.g, _to.g, t, _easingType);
            color.b = FEasing.Tween(_from.b, _to.b, t, _easingType);
            color.a = FEasing.Tween(_from.a, _to.a, t, _easingType);

            return color;
        }
    }

    [Serializable]
    public class FTweenFloat : FTween<float>
    {
        public FTweenFloat(float from, float to)
        {
            _from = from;
            _to = to;
        }

        public override float GetValue(float t)
        {
            return FEasing.Tween(_from, _to, t, _easingType);
        }
    }

    [Serializable]
    public class FTweenVector2 : FTween<Vector2>
    {
        public FTweenVector2(Vector2 from, Vector2 to)
        {
            _from = from;
            _to = to;
        }

        public override Vector2 GetValue(float t)
        {
            Vector2 v;
            v.x = FEasing.Tween(_from.x, _to.x, t, _easingType);
            v.y = FEasing.Tween(_from.y, _to.y, t, _easingType);
            return v;
        }
    }

    [Serializable]
    public class FTweenVector3_Ex : FTween<Vector3>
    {
        public bool isBezier;
        public bool lookForward;

        [NaughtyAttributes.ShowIf(nameof(isBezier))]
        public Vector3 _handlePoint;

        public Vector3 HandlePoint { get => _handlePoint; set => _handlePoint = value; }

        public FTweenVector3_Ex(Vector3 from, Vector3 to)
        {
            _from = from;
            _to = to;
        }

        public override Vector3 GetValue(float t)
        {
            float easingT = FEasing.Tween(0, 1, t, _easingType);
            if (isBezier)
            {
                return MathTool.GetBezierPoint2(_from, _to, HandlePoint, easingT);
            }
            else
            {
                return MathTool.LinearVec3(_from, _to, easingT);
            }
        }

        public Vector3 GetSpeed(float t)
        {
            float easingT = FEasing.Tween(0, 1, t, _easingType);

            return MathTool.GetBezierPoint2_Speed(_from, _to, HandlePoint, easingT);

        }
    }

    [Serializable]
    public class FTweenVector3 : FTween<Vector3>
    {

        public FTweenVector3(Vector3 from, Vector3 to)
        {
            _from = from;
            _to = to;
        }

        public override Vector3 GetValue(float t)
        {
            Vector3 v;
            v.x = FEasing.Tween(_from.x, _to.x, t, _easingType);
            v.y = FEasing.Tween(_from.y, _to.y, t, _easingType);
            v.z = FEasing.Tween(_from.z, _to.z, t, _easingType);
            return v;
        }
    }

    [Serializable]
    public class FTweenVector4 : FTween<Vector4>
    {
        public FTweenVector4(Vector4 from, Vector4 to)
        {
            _from = from;
            _to = to;
        }

        public override Vector4 GetValue(float t)
        {
            Vector4 v;
            v.x = FEasing.Tween(_from.x, _to.x, t, _easingType);
            v.y = FEasing.Tween(_from.y, _to.y, t, _easingType);
            v.z = FEasing.Tween(_from.z, _to.z, t, _easingType);
            v.w = FEasing.Tween(_from.w, _to.w, t, _easingType);
            return v;
        }
    }

    [Serializable]
    public class FTweenQuaternion : FTween<Quaternion>
    {
        public FTweenQuaternion(Quaternion from, Quaternion to)
        {
            _from = from;
            _to = to;
        }

        public override Quaternion GetValue(float t)
        {
            Quaternion q;
            q.x = FEasing.Tween(_from.x, _to.x, t, _easingType);
            q.y = FEasing.Tween(_from.y, _to.y, t, _easingType);
            q.z = FEasing.Tween(_from.z, _to.z, t, _easingType);
            q.w = FEasing.Tween(_from.w, _to.w, t, _easingType);
            return q;
        }
    }
}
