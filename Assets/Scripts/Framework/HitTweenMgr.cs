using System.Collections;
using UnityEngine;
using DG.Tweening;
namespace XiaoCao
{
    public class HitTweenMgr : MonoSingleton<HitTweenMgr>
    {
        public CharacterController cc;



        private void Update()
        {

        }

        private void AddTween(CharacterController cc, Vector3 delta, float duration = 0.1f)
        {

            //DOPunchPosition() 冲击弹回
            //Vector3 punch               要被击打到的最远位置（相对值，相对于局部坐标）
            //float duration            总持续时间

            //   int vibrato             物体振动频率

            //   float elasticity          值一般在0到1之间，0表示起点到冲击方向的震荡，1表示为一个完整的震荡，可能会超过起点，个人感觉后者效果更好。
            //bool snapping            是否进行平滑插值

            Vector3 lastPos = cc.transform.position;
            //cc.transform.DOPunchPosition(lastPos + delta, 0.2f).SetEase();



            //DOTween.To(value =>
            //{

            //}, startValue: 0, endValue: 100f, duration: duration);

            //var move = GetVec3Value(t) - GetVec3Value(lastTime);
            //lastTime = t;
            //if (IsAddOnVec)
            //    ExcuteAddVec(move);
        }

        //public Vector3 GetVec3Value(float t)
        //{
        //    return EaseTool.GetVec3Value(startVec, endVec, moveType, t);
        //}

        private void fun1()
        {
            //DOTween.To(value => { text.text = Mathf.Floor(value).ToString(); }, startValue: 0, endValue: 4396, duration: 1f);

            //cc.Move(cc.transform.TransformDirection(detalMove));
        }

    }



    public static class DoTweenExtend
    {
        public static Tween DOHit(this CharacterController cc, float totalY, Vector3 horVec,float duration)
        {
            Transform tf = cc.transform;

            if (totalY == 0)
            {
                totalY = 0.1f;
            }

            float time = 0; 
            float lastT = 0, deltaT = 0;
            //0 ->1的数值动画
            Tween tween = DOTween.To(x => time = x, 0, 1, duration);
            tween.SetEase(Ease.OutQuart);
            //tween.SetLoops(2,LoopType.Yoyo);

            Vector3 targetMove = horVec;
            targetMove.y += totalY; //目标移动量

            tween.OnUpdate(() =>
            {
                deltaT = time - lastT;
                lastT = time;

                Vector3 delta = targetMove * deltaT;
                cc.Move(delta);
            });
            return tween;
        }

        public static void DOHit2(this CharacterController cc, float totalY, Vector3 hordir, float duration, bool snapping = false)
        {
            float time = 0;
            Tween tween = DOTween.To(x => time = x, 0, 1, duration);
            tween.SetEase(Ease.OutQuad);
            float targetY = DOVirtual.EasedValue(0, totalY, tween.ElapsedPercentage(), Ease.OutQuad);

        }



    }

    public class HitTweenInfo
    {
        public Vector3 targetVec;

        public int line;

        public int time;
    }
}