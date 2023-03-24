using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiaoCao;
using NaughtyAttributes;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerMover : MonoBehaviour
{
    protected float groundCheckDistance = 0.4f;


    #region SettingValue

    SoUsing SoUsing => ResFinder.SoUsingFinder;
    PlayerMoveSettingSo PlayerMoveSettingSo;

    float MovingTurnSpeed => PlayerMoveSettingSo.m_MovingTurnSpeed;
    float StationaryTurnSpeed => PlayerMoveSettingSo.m_StationaryTurnSpeed;
    float NorMoveSpeed => PlayerMoveSettingSo.NorMoveSpeed;
    float Gravity => PlayerMoveSettingSo.Gravity;

    public bool enableGravity = true;
    public float enableGravityTime;
    public float disableGravityTimer;
    #endregion

    #region 计算数据
    [Header("Diagnostics")]

    protected float horizontal;
    protected float vertical;

    float curTurnAmount;//转向值
    float curForwardAmount;//前进值
    public float curMoveSpeedAnim = 0;
    public float curSpeedRate = 1;

    private Vector3 m_CamForward;  // 当前相机的正前方
    public Vector3 m_Move; //根据相机的正前方和用户的输入,计算世界坐标相关的移动方向
    private Vector2 look;
    private float scroll;

    public float jumpSpeed;
    public bool isGrounded = true;
    public bool isFalling;
    public Vector3 velocity;

    #endregion


    public bool IsLocalTruePlayer => playerState.IsLocalTruePlayer;

    #region 其他组件
    private Transform m_Cam; // 主摄像机的位置
    private Animator animator => playerState.animator;
    private CharacterController cc;
    public PlayerState playerState;
    public Rigidbody rb;
    private Cinemachine3rdPersonFollow VirtualCamera_3rd;
    private CinemachineBrain CinemachineBrain;

    #endregion

    #region public
    public bool isInit = false;
    public bool canMove = true;
    public bool canRotate = true;
    public bool isLocalAutoRotate = true;
    public bool CanRotate => canRotate && unMoveTime <= 0;
    public bool CanMove =>
        (
        canMove && unMoveTime <= 0
        && isGrounded && playerState.damageState != DamageState.OnBreak
        );

    public bool isOnBreakAnim => animator.GetCurrentAnimatorStateInfo(0).IsTag("OnBreak");


    public bool canUpdateMove = true;
    public bool isBackToIdle = false;
    public float unMoveTime = 0;// 不可移动的时间

    #endregion

    public void Init(PlayerState _playerState)
    {
        playerState = _playerState;

        m_Cam = Camera.main.transform;
        cc = playerState.cc;

        rb = cc.GetComponent<Rigidbody>();
        PlayerMoveSettingSo = _playerState.PlayerMoveSettingSo;
        GroundLayers = PlayerMoveSettingSo.GroundLayers;
        canMove = true;

        if (playerState.IsLocalTruePlayer)
        {
            CinemachineBrain = CinemachineCore.Instance.GetActiveBrain(0);
            CinemachineBrain.m_CameraActivatedEvent.AddListener(LocalTrue_OnVCamActive);
        }
        isInit = true;
    }


    #region 生命周期
    private void Awake()
    {
        SceneManager.sceneLoaded += SceneLoadedFinish;
    }

    private void LocalTrue_OnVCamActive(ICinemachineCamera arg0, ICinemachineCamera arg1)
    {
        CheckCamLookAt(arg0);
    }

    private void CheckCamLookAt(ICinemachineCamera camera)
    {
        if (camera != null && camera.VirtualCameraGameObject.CompareTag("PlayerCam"))
        {
            camera.Follow = CinemachineCameraTarget.transform;
            camera.LookAt = CinemachineCameraTarget.transform;
        }
    }

    private void CheckLocalCam()
    {
        if (!playerState.IsLocalTruePlayer)
        {
            return;
        }

        if (VirtualCamera_3rd == null)
        {
            CinemachineBrain = CinemachineCore.Instance.GetActiveBrain(0);
            if (CinemachineBrain.ActiveVirtualCamera!=null)
            {
                CinemachineVirtualCamera virtualCamera = CinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
                virtualCamera.Follow = CinemachineCameraTarget.transform;
                VirtualCamera_3rd = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                Debug.Log($"yns VirtualCamera_3rd init ");
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoadedFinish;

        //var cam = CinemachineCore.Instance.GetActiveBrain(0);
        //cam.m_CameraActivatedEvent.RemoveListener(OnCameraActiveEvent);
        //cam.m_CameraCutEvent.RemoveListener(OnCameraCutEvent);
    }


    private void SceneLoadedFinish(Scene arg0, LoadSceneMode arg1)
    {
        //if (isInit && IsLocalTruePlayer)
        //{
        //    InitCam();
        //}
    }

    private void CheckUnMoveTime()
    {
        if (unMoveTime > 0)
        {
            unMoveTime -= Time.deltaTime;
        }
    }

    public void MoveUpdate()
    {
        if (!isInit)
            return;

        GroundedCheck();
        CheckUnMoveTime();

        if (!IsLocalTruePlayer || !cc.enabled)
            return;

        ///===== IsLocalTruePlayer ===== ///

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        look.x = Input.GetAxis("Mouse X");
        look.y = Input.GetAxis("Mouse Y");

        scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (VirtualCamera_3rd)
            {
                VirtualCamera_3rd.CameraDistance = Mathf.Clamp(VirtualCamera_3rd.CameraDistance - scroll * 1.5f, 4, 10);
            }
            else
            {
                Debug.LogError($"yns VirtualCamera_3rd null");
            }
        }

        CheckEsc();

        if (isGrounded)
            isFalling = false;

        if ((isGrounded || !isFalling) && jumpSpeed < 1f && Input.GetKey(KeyCode.Space))
        {
            jumpSpeed = Mathf.Lerp(jumpSpeed, 1f, 0.5f);
        }
        else if (!isGrounded)
        {
            isFalling = true;
            jumpSpeed = 0;
        }

        if (m_Cam != null)
        {
            m_CamForward = m_Cam.forward;
            m_CamForward.y = 0;
            m_Move = (vertical * m_CamForward + horizontal * m_Cam.right).normalized;
        }
        else
        {
            m_Cam = Camera.main.transform;
        }
    }

    public void MoveFixUpdate()
    {
        CheckEnableGravity();
        //更新重力
        UpdateGravity();

        if (!playerState.IsLocal || cc == null || playerState.IsDie)
        {
            return;
        }

        //bool isForward  = Vector3.Dot(transform.forward, m_Move) >0;
        //本地自身先移动，然后将结果用CmdMove同步

        //isAutoRotate 本地开关
        OnNetMove_AutoRotate(m_Move, CanRotate && isLocalAutoRotate);

        OnNetMove(m_Move, CanMove); //远端通过RcpMove移动
        playerState.CmdMove(m_Move, CanMove, CanRotate, transform.position, transform.forward);
        playerState.MoveSlowDown(0.1f);
    }

    private void UpdateGravity()
    {
        if (enableGravity)
        {
            if (isGrounded)
            {
                if (velocity.y > Gravity * PlayerMoveSettingSo.GravityOnGrondRate)
                {
                    velocity.y = Mathf.Lerp(velocity.y, Gravity * PlayerMoveSettingSo.GravityOnGrondRate, 0.1f);
                }
                else
                {
                    velocity.y = Gravity * PlayerMoveSettingSo.GravityOnGrondRate;
                }
                //清空掉落状态
            }
            else
            {
                velocity.y = Mathf.Clamp(velocity.y + Gravity * PlayerMoveSettingSo.GravityOnAirAddRate * Time.deltaTime
                    , Gravity * PlayerMoveSettingSo.GravityMaxRate, 40);
            }
            OnMove(velocity * Time.deltaTime); //Y
        }
        else
        {
            velocity.y = Gravity * PlayerMoveSettingSo.GravityOnGrondRate;
        }
    }

    private void SetFalling(bool flag)
    {

    }

    public void SetUnMoveTime(float t)
    {
        if (t > unMoveTime)
        {
            unMoveTime = t;
        }
    }

    private void CheckEnableGravity()
    {
        if (disableGravityTimer > 0)
        {
            enableGravity = false;
            disableGravityTimer -= Time.fixedDeltaTime;
        }
        else
        {
            enableGravity = true;
        }
    }
    public void SetNoGravityT(float time)
    {
        if (time > disableGravityTimer)
        {
            disableGravityTimer = time;
        }
    }

    public void MoveLateUpdate()
    {
        if (IsLocalTruePlayer)
        {
            CameraRotation();
        }
    }

    #endregion

    #region Cinemachine & Grounded


    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    private LayerMask GroundLayers;


    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;

    public float TopClamp = 70.0f;

    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    //private bool IsCurrentDeviceMouse => true;
    private const float _threshold = 0.01f;
    //Cursor.lockState == CursorLockMode.Locked; //新版
    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private void CameraRotation()
    {

        if (!isInit)
        {
            return;
        }

        if (VirtualCamera_3rd == null || CinemachineBrain.ActiveVirtualCamera.Follow == null)
        {
            CheckLocalCam();
            return;
        }

        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += look.x;
            _cinemachineTargetPitch += look.y * -1f;
        }
        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);


        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);

    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    #endregion

    public void OnNetMove_AutoRotate(Vector3 m_Move, bool CanRotate)
    {
        if (CanRotate)
        {
            RotateBy_Move(m_Move);
        }
    }

    //Net
    public void OnNetMove(Vector3 m_Move, bool CanMove)
    {
        if (CanMove)
        {
            SetCurMoveSpeedAnim(m_Move);
            SetMoveAnimSpeed();
            OnMove(m_Move * NorMoveSpeed * curSpeedRate * Time.fixedDeltaTime);
        }
        if (!CanMove)
        {
            curMoveSpeedAnim = 0;
            curSpeedRate = 1;
            SetMoveAnimSpeed();
        }
    }

    private void OnMove(Vector3 vec)
    {
        cc.Move(vec);
    }

    //由前进位计算旋转
    void RotateBy_Move(Vector3 move)
    {
        //将世界坐标系的方向和位置转换为自身坐标系
        move = transform.InverseTransformDirection(move);

        curTurnAmount = Mathf.Atan2(move.x, move.z);

        curForwardAmount = move.z;//人物前进的数值
        // 帮助角色快速转向，这是动画中根旋转的附加项  
        float turnSpeed = Mathf.Lerp(StationaryTurnSpeed, MovingTurnSpeed, curForwardAmount);

        transform.Rotate(0, curTurnAmount * turnSpeed * Time.fixedDeltaTime, 0);//转向.  

    }

    #region Roll

    public int GetRollIndex(ref Vector3 rollDir)
    {
        Vector3 move = transform.TransformDirection(m_Move);
        curTurnAmount = Mathf.Atan2(move.x, move.z);
        rollDir = m_Move;
        //谁大取谁 顺时针
        if (m_Move.x * m_Move.x > m_Move.y * m_Move.y)
        {
            //横向 右
            if (m_Move.x > 0)
            {
                //Debug.Log($"yns  右");
                return (1);
            }
            else
            {//左
                //Debug.Log($"yns  左");
                return (3);
            }
        }
        else
        {
            return (0);
        }
    }

    //0在正前,1左 2右 3后
    public void DoRoll(int rollDir)
    {
        curMoveSpeedAnim = 3.5f;
        animator.SetFloat(AnimConfig.RollDir_F, rollDir);
        animator.Play(AnimHash.RollTree);
    }

    #endregion


    //PlayEventMsg
    public void SetBool(string name, bool flag)
    {
        if (name == PlayEventMsg.SetCanMove)
        {
            canMove = flag;
        }
        else if (name == PlayEventMsg.SetCanRotate)
        {
            canRotate = flag;
            canUpdateMove = flag;
        }
    }

    public void SetCanMoveAndRotate(bool flag)
    {
        canMove = flag;
        canRotate = flag;
        canUpdateMove = flag;
    }

    private float tmpMoveLen = 0;
    private float curMaxSpeed = 0;

    void SetCurMoveSpeedAnim(Vector3 move)
    {
        tmpMoveLen = move.magnitude;
        if (tmpMoveLen > 0.1)
        {
            if (isBackToIdle)
            {
                //结束技能后摇
                curMoveSpeedAnim = 0;
                isBackToIdle = false;
                Debug.Log("yns isBackToIdle");
            }

            //需要一个动态平衡
            curMoveSpeedAnim = Mathf.Lerp(curMoveSpeedAnim, 4, 0.04f);//加速

            if (tmpMoveLen < 0.5f)
            {
                curMoveSpeedAnim = curMoveSpeedAnim * tmpMoveLen;
            }
        }
        else
        {
            isBackToIdle = false;
            curSpeedRate = 1;
            curMoveSpeedAnim = Mathf.Lerp(curMoveSpeedAnim, 0, 0.1f);
            if (curMoveSpeedAnim < 0.1)
                curMoveSpeedAnim = 0;
        }
    }

    public void SetMoveAnimSpeed()
    {
        curSpeedRate = Mathf.Clamp(curMoveSpeedAnim / 2, 1, 1.5f);
        animator.SetFloat("MoveSpeed", curMoveSpeedAnim);
    }

    public void PlayAnim(int clipHash, float startOffset = 0, float crossFade = 0.1f)
    {
        animator.CrossFade(clipHash, startOffset, 0, crossFade);
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (animator)
        {
            animator.SetBool(AnimConfig.IsGround_B, isGrounded);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }

    private void CheckEsc()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                SetShowCursor();
                //Cursor.lockState = CursorLockMode.None;
            }
            Debug.Log($"yns  " + Cursor.lockState);
        }

        if (Cursor.visible)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
            //    if (Input.GetKeyDown(KeyCode.Mouse1))
            //{
            //    UnlockCursor();
            //    Debug.Log($"yns  UnlockCursor");
            //}
        }

    }

    private void SetShowCursor()
    {
        Cursor.visible = true;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    [NaughtyAttributes.Button]
    private void BackToStartPoint()
    {
        transform.position = Vector3.up;
        velocity.y = 0;
    }
}


public static class SkillStr
{
    public static string NorAck = "NorAck";

    readonly static string[] NorAcks = { "NorAck_0", "NorAck_1", "NorAck_2" };

    public static string RunAck = "RunAck";

    public static string Roll = "Roll";

    public static string GetNorAckName(int index)
    {
        return NorAcks[index];
    }

    public static bool IsNorAck(string str)
    {
        return str.StartsWith(NorAck);
    }

    //public static bool IsLockTarget(string name)
    //{
    //    return IsNorAck(name) || name == RunAck;
    //}
}

public static class AnimHash
{
    public static readonly int Idle = AnimNameStr.Idle.ToAnimtorHash();
    public static readonly int Break = AnimNameStr.Break.ToAnimtorHash();//break
    public static readonly int Hit = AnimNameStr.Hit.ToAnimtorHash();//轻受击
    public static readonly int RollTree = AnimNameStr.RollTree.ToAnimtorHash();
    public static readonly int Dead = AnimNameStr.Dead.ToAnimtorHash();

}

public static class AnimNameStr
{
    public const string Idle = "Idle";
    public const string Break = "Break"; 
    public const string Hit = "Hit";
    public const string RollTree = "RollTree";
    public const string Dead = "Dead";
}

//动画属性变量
public static class AnimConfig
{
    public static string IsDie = "IsDie";
    public static string IsGround_B = "IsGround";
    public static string MoveSpeed_F = "MoveSpeed";

    public static string MoveX_F = "MoveX";
    public static string MoveY_F = "MoveY";
    public static string RollDir_F = "RollDir";

}