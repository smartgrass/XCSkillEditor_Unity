using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Cinemachine;

namespace Sworder
{

    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoSingleton<CameraController>
    {
        private void Awake()
        {
            _instance = this;
        }


        #region 配置
        public float mouseXSensitivity = 8f;
        public float mouseYSensitivity = 8f;
        public Transform lookTarget;
        public float distanceFromCenter = 3f;
        public Vector2 pitchConstrain = new Vector2(-45f, 80f);

        private float turningSmoothTime = 0.1f;
        private Vector3 turningSmoothRef;
        public Vector3 targetRotationAngle;
        public float offsetX, offsetY; //中心偏移量


        public bool isActive = true;
        #endregion

        private CinemachineVirtualCamera VirtualCamera;
        #region 属性
        private float eulerX, eulerY;
        private float scroll;
        #endregion

        public Vector3 Forward { get => transform.forward; }
        public Vector3 Forward_Hor { get => new Vector3(transform.forward.x, 0, transform.forward.z); } 

        private void FixedUpdate()
        {
            if(isActive)
                NovmalCam();
        }

        public void NovmalCam()
        {
            if(lookTarget == null)
            {
                if(isActive)
                    Debug.Log("yns  colse Camera");
                isActive = false;
                return;
            }


            eulerX += Input.GetAxis("Mouse X") * mouseXSensitivity;
            eulerY -= Input.GetAxis("Mouse Y") * mouseYSensitivity;
            //限制上下角
            eulerY = Mathf.Clamp(eulerY, pitchConstrain.x, pitchConstrain.y);



            scroll = Input.GetAxis("Mouse ScrollWheel");

           /*平滑处理
           targetRotationAngle = Vector3.SmoothDamp(targetRotationAngle, new Vector3(eulerY, eulerX, 0f),ref turningSmoothRef, turningSmoothTime);
           transform.eulerAngles = targetRotationAngle;
           */
           //非平滑
           transform.eulerAngles = new Vector3(eulerY, eulerX, 0f);
            //因为画面不会倾斜,所有欧拉角的z角永远为零
            transform.position = lookTarget.position - transform.forward * distanceFromCenter + transform.right * offsetX + transform.up * offsetY;
        }





        public void ResetTarget(Transform target , bool isActive = true)
        {
            lookTarget = target;
            this.isActive = isActive;
        }


    }


}

