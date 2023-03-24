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
public class FixCamera : MonoBehaviour
{
    public Camera _camera;
    [OnValueChanged(nameof(ChangeCameraType))]
    public CameraMode cameraType;
    [ReadOnly]
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

    private Transform characterCameraContainer;
    private int envLayerMask;

    //由上往下看的限制角度
    private float clampX_Up = 60;
    //由下往上看的限制角度
    private float clampX_Down = 60;



    private void Awake()
    {
        //character = followTarget.GetComponent<Character>();
        int layer = LayerMask.NameToLayer("Env");
        characterCameraContainer = transform.Find("cameraContainer");
        envLayerMask = (int)Mathf.Pow(2, layer);
        //cameraAvoid = GetComponent<CameraAvoid>();
    }


    private void CameraMove()
    {
        //相机先旋转后移动，不然抖动
        //transform.RotateAround(followTarget.position, Vector3.up, character.mouseX * mouseXSpeed);
        //对于上下轴，下面的注释代码是错误的，这个api中，指定围绕的x轴，应该是当前相机的x轴而不是世界空间的x轴，所以填transform.right，而不是vector3.right
        //transform.RotateAround(followTarget.position, Vector3.right, character.mouseY * mouseYSpeed);
        //正确的代码：
        //transform.RotateAround(followTarget.position, transform.right.normalized, character.mouseY * mouseYSpeed);

        //对相机的x轴旋转进行clamp，太上太下都不行
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

        //鉴以为逊
        //丢弃z旋转
        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        ////好看的相机追踪。smoothTime大概0.1，maxSpeed大概15.
        //Vector3 currentVelocity = Vector3.zero;
        //transform.position = Vector3.SmoothDamp(transform.position, target.position +offset, ref currentVelocity, smoothTime, maxSpeed);

    }





    private void Start()
    {
        if (_camera == null)
        {
            _camera = transform.GetComponent<Camera>();
        }
        ChangeCameraType();
        StartCoroutine(CallPlayer());
    }

    IEnumerator CallPlayer()
    {
        float t = 0;
        while (t < 5)
        {
            t += Time.deltaTime;
            if (PlayerMgr.Instance.LocalPlayer)
            {
                target = PlayerMgr.Instance.LocalPlayer.transform;
                yield break;
            }
            yield return null;
        }
        yield return null;
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

        CameraMove();
        return;

        Vector3 pos = target.transform.position + offset;

        //float x = Mathf.Abs(pos.x - transform.position.x);
        //float y = Mathf.Abs(pos.y - transform.position.y);
        //float z = Mathf.Abs(pos.z - transform.position.z);

        pos.x = Mathf.Clamp(transform.position.x, pos.x -keepArea.x, pos.x + keepArea.x);
        pos.y = Mathf.Clamp(transform.position.y, pos.y - keepArea.y,pos.y +keepArea.y);
        pos.z = Mathf.Clamp(transform.position.z, pos.z - keepArea.z, pos.z + keepArea.z);

        transform.position = Vector3.Lerp(transform.position, pos, smoth);
        
    }

    private void ShakeCam(float shakeLen = 1)
    {
        transform.DOShakePosition(0.5f, shakeLen, 0);
    }

    [Button]
    public void GetOffSet()
    {
        offset = transform.position - target.transform.position;
    }


    [Button]
    public void EditorSave()
    {
        CameraSettingSo setting = ResFinder.SoUsingFinder.CameraSetting;
        if(GameSetting.cameraType == CameraMode.Follow)
        {
            setting.FollowCameraData.Angle = transform.localEulerAngles;
            setting.FollowCameraData.Offset = offset;
            setting.FollowCameraData.KeepArea = keepArea;

        }
#if UNITY_EDITOR

        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
#endif
    }

}
