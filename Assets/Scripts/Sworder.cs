using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Sworder
{
    public class Sworder : NetworkBehaviour
    {
        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform projectileMount;

        private CameraController CamCon;
        private float targetMoveSpeed = 0;
        private float MoveSpeed_F = 0;
        private Vector3 tarForward = Vector3.zero;

        private void Start()
        {
            if (isLocalPlayer)
            {
                CamCon = CameraController.Instance;
                CamCon.ResetTarget(this.transform);
            }
        }

        void Update()
        {
            // movement for local player
            if (!isLocalPlayer) return;

            // rotate
            float horizontal = Input.GetAxis("Horizontal");

            // move
            float vertical = Input.GetAxis("Vertical");
            Vector2 inputDir = new Vector2(horizontal, vertical).normalized;
            //Vector3 forward = transform.TransformDirection(Vector3.forward);
            if(vertical>0)
                targetMoveSpeed = inputDir.magnitude;
            else
                targetMoveSpeed = inputDir.magnitude * -1;

            MoveSpeed_F = Mathf.Lerp(MoveSpeed_F, targetMoveSpeed, 0.3f);
            animator.SetFloat(AnimConfig.MoveSpeed_F, MoveSpeed_F);


            //transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
            Vector3 inputForward =  Vector3.Lerp(transform.forward, CamCon.Forward, 0.5f);
            inputForward.y = 0;

            if(targetMoveSpeed > 0)
            {
                transform.forward = inputForward;
            }


    




            agent.velocity = transform.forward * targetMoveSpeed * agent.speed;


            // shoot
            if (Input.GetKeyDown(shootKey))
            {
                CmdFire();
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }
    }
}
