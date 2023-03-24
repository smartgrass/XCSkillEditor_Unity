using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using XiaoCao;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(Camera))]
public class Test_FixCamera : MonoBehaviour
{
    public Camera _camera;
    [OnValueChanged(nameof(ChangeCameraType))]
    public CameraMode cameraType;

    public Transform target;

    public Vector3 keepArea;
    public Vector3 offset;

    public float smoth = 0.1f;



    //private Character character;

    //private CameraAvoid cameraAvoid;

    public float mouseXSpeed = 1;
    public float mouseYSpeed = 1;

    public float smoothTime = 0.2f;
    public float maxSpeed = 10;




    //由上往下看的限制角度
    private float clampX_Up = 60;
    //由下往上看的限制角度
    private float clampX_Down = 60;



    private void Awake()
    {
        //character = followTarget.GetComponent<Character>();
        //cameraAvoid = GetComponent<CameraAvoid>();
    }


    private void CameraMove()
    {

        if (transform.eulerAngles.x > 0 && transform.eulerAngles.x < 180) //向上旋转
        {
            if (transform.eulerAngles.x > clampX_Up)
            {
                transform.eulerAngles = new Vector3(clampX_Up, transform.eulerAngles.y, 0);
            }
        }
        else if (transform.eulerAngles.x > 180 && transform.eulerAngles.x < 360)
        {
            if (transform.eulerAngles.x < (360 - clampX_Down))
            {
                transform.eulerAngles = new Vector3(-clampX_Down, transform.eulerAngles.y, 0);
            }
        }


    }





    private void Start()
    {
        if (_camera == null)
        {
            _camera = transform.GetComponent<Camera>();
        }
        //ChangeCameraType();
    }

    private void ChangeCameraType()
    {
        GameSetting.cameraType = cameraType;
        if(cameraType == CameraMode.Follow)
        {
            TramsFormData FollowCameraData = ResFinder.SoUsingFinder.CameraSetting.FollowCameraData;
            transform.localEulerAngles = FollowCameraData.Angle;
            offset = FollowCameraData.Offset;
            keepArea = FollowCameraData.KeepArea;
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;
        if (GameSetting.isResetCamara)
        {
            transform.position = target.transform.position + offset;
            GameSetting.isResetCamara = false;
            return;
        }

        //CameraMove();
        //return;

        Vector3 pos = target.transform.position + offset;

        pos.x = Mathf.Clamp(transform.position.x, pos.x -keepArea.x, pos.x + keepArea.x);
        pos.y = Mathf.Clamp(transform.position.y, pos.y - keepArea.y,pos.y +keepArea.y);
        pos.z = Mathf.Clamp(transform.position.z, pos.z - keepArea.z, pos.z + keepArea.z);

        transform.position = Vector3.Lerp(transform.position, pos, smoth);
        
    }


}
