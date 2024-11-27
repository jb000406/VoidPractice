using UnityEngine;

namespace VoidProject
{
    public class NpcController : MonoBehaviour
    {
        [Header("NPC 설정")]
        [SerializeField] private float raderRadius = 5f;     // NPC의 감지 범위
        [SerializeField] private float moveSpeed = 2f;      // NPC 이동 속도
        [SerializeField] private float attackDistance = 1.5f; // 공격 거리
        [SerializeField] private float viewAngleThreshold = 60f; // 플레이어가 바라보는 각도 (NPC를 인식하는 시야)

        [Header("Fog 조건")]
        [SerializeField] private float fogThreshold = 0.5f;    // 움직일 수 있는 Fog 최소값
        [SerializeField] private float speedFogMultiplier = 1.5f; // Fog 증가 시 속도 배율

        private CharacterController npc_Controller;         // NPC의 CharacterController
        private Animator npc_Animator;                     // NPC의 Animator
        private SphereCollider npc_SphereCollider;         // 감지용 SphereCollider
        private Transform player_Transform;                // 플레이어 Transform

        public bool IsAttack { get; private set; }         // 공격 상태
        public bool IsMove { get; private set; }           // 이동 상태
        private bool isHiding = true;                      // NPC가 숨는 상태인지 여부

        private void OnEnable()
        {
            npc_Controller = GetComponent<CharacterController>();
            npc_Animator = transform.GetChild(0).GetComponent<Animator>();
            npc_SphereCollider = GetComponent<SphereCollider>();

            // 감지 범위를 설정
            npc_SphereCollider.radius = raderRadius;

            // 플레이어 참조 초기화
            player_Transform = null;
            SetHideObject(true); // 초기 상태는 숨김
        }

        private void Update()
        {
            // 현재 Fog Density 가져오기
            float fogDensity = RenderSettings.fogDensity;

            // 플레이어 감지 여부 확인
            if (player_Transform == null || fogDensity < 0.05f)
            {
                Debug.Log("플레이어를 감지하지 못하거나 Fog가 낮아 숨기 상태");
                HideZone();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player_Transform.position);

            // 플레이어의 Forward 방향과 NPC 간의 각도 계산
            Vector3 toNPC = (transform.position - player_Transform.position).normalized;
            float angle = Vector3.Angle(player_Transform.forward, toNPC);

            if (distanceToPlayer <= attackDistance)
            {
                // 공격 거리 안에 있다면 공격
                Debug.Log("플레이어 공격");
                AttackTarget(player_Transform);
            }
            else if (fogDensity < fogThreshold)
            {
                // Fog가 설정된 Threshold보다 낮으면 숨기
                Debug.Log("Fog가 낮아 숨기 상태");
                HideZone();
            }
            else if (angle < viewAngleThreshold && !isHiding)
            {
                // 플레이어가 NPC를 바라볼 각도 안에 있을 경우 숨기
                Debug.Log("플레이어 시야 안에 들어가 숨기 상태");
                HideZone();
                isHiding = true;
            }
            else if (angle >= viewAngleThreshold && isHiding)
            {
                // 플레이어가 NPC를 보지 않을 경우 다시 접근
                Debug.Log("플레이어가 보지 않아 접근");
                isHiding = false;
                SetHideObject(true);
                ApproachToNear(player_Transform, fogDensity);
            }
            else if (!isHiding)
            {
                // 기본적으로 접근
                Debug.Log("기본 접근 상태");
                ApproachToNear(player_Transform, fogDensity);
                isHiding = false;
                SetHideObject(true);
            }
        }

        // 상태 설정
        public void SetState(bool isAttack, bool isMove)
        {
            // 상태 값 설정
            IsAttack = isAttack;
            IsMove = isMove;

            // 애니메이터에 상태 값 반영
            npc_Animator.SetBool("IsAttack", IsAttack);
            npc_Animator.SetBool("MoveX", IsMove);
        }

        // 대기 상태
        private void HideZone()
        {
            SetState(false, false);
            SetHideObject(true); // 숨김
            isHiding = true;
        }

        private void SetHideObject(bool hide)
        {
            // 자식 오브젝트 렌더링 활성/비활성화
            transform.GetChild(0).GetChild(0).gameObject.SetActive(hide);
            transform.GetChild(0).GetChild(1).gameObject.SetActive(hide);
        }

        // 플레이어를 향해 이동
        private void ApproachToNear(Transform target, float fogDensity)
        {
            SetState(false, true);

            // Fog가 0.13 이상이면 속도 증가
            float currentSpeed = fogDensity >= 0.13f ? moveSpeed * speedFogMultiplier : moveSpeed;

            // 플레이어 방향 계산
            Vector3 direction = (target.position - transform.position).normalized;

            // NPC 회전
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // 이동 (중력 추가)
            Vector3 move = direction * currentSpeed * Time.deltaTime;
            move.y = Physics.gravity.y * Time.deltaTime; // 중력 보정
            npc_Controller.Move(move);
        }

        // 플레이어를 공격
        private void AttackTarget(Transform target)
        {
            SetState(true, false);
            SoundManager.Instance.PlayClipAtPoint(1, transform.position, 1f);
            // NPC가 공격 상태에서 타겟을 바라보도록 회전
            Vector3 direction = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }

        // 플레이어가 감지 영역에 들어올 때
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("플레이어 감지");
                player_Transform = other.transform; // 플레이어 참조
            }
        }

        // 플레이어가 감지 영역에서 나갈 때
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("플레이어 영역에서 나감");
                player_Transform = null; // 플레이어 참조 초기화
            }
        }
    }
}