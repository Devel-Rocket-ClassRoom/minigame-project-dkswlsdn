using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class CharacterMovement : MonoBehaviour
{
    // 의존성
    private Rigidbody rigid;
    private CharacterAim aim;
    private StateManager state;
    private CharacterStat stat;
    private CharacterCommander commander;

    // 기본 물리 정보
    public float moveSpeed = 10f;
    public readonly float defaultGravity = 20f;
    private float activeGravity = 20f;
    private float friction = 0;
    [SerializeField] private LayerMask groundLayer;

    // 캐릭터 간 막힘 판정 (물리 충돌은 끄고, 이동만 막는다 → 밀림 없음)
    [SerializeField] private LayerMask characterBlockLayer; // 다른 캐릭터들이 속한 레이어
    [SerializeField] private CapsuleCollider bodyCollider;  // 막힘 판정에 쓸 본체 캡슐
    private const float blockSkin = 0.02f;                  // 살짝 띄워서 끼임 방지

    // 현재 물리 정보
    protected Vector3 inputDirection;
    private Vector3 localDirection;
    private Vector3 staticLocalDirection;
    private Vector3 horizontalVelocity;
    [SerializeField] private float verticalVelocity;
    public float VerticalVelocity => verticalVelocity;
    private float horizontalSpeed;
    protected Vector3 surfaceNormal;
    protected bool isFreeMoveEnabled = true;

    // 지상판정 정보
    public event Action onLand;
    protected bool isNearGround;
    [SerializeField] protected bool isOnGround;
    private bool wasGrounded;
    

    // 현재 스킬 정보
    private MovementMethod method;
    private float skillEndTime;

    private Coroutine freezeCoroutine;
    private bool isFrozen;


    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        aim = GetComponent<CharacterAim>();
        stat = GetComponent<CharacterStat>();
        state = GetComponent<StateManager>();
        commander = GetComponent<CharacterCommander>();
        if (bodyCollider == null) bodyCollider = GetComponentInChildren<CapsuleCollider>();
    }

    private void OnEnable()
    {
        stat.onDamageTake += StunMove;
        onLand += state.OnLand;
        state.onIdle += ReturnToIdle;
        state.onKnockdown += OnKnockdown;
        state.onFreeze += OnFrozen;
    }

    private void OnDisable()
    {
        stat.onDamageTake -= StunMove;
        onLand -= state.OnLand;
        state.onIdle -= ReturnToIdle;
        state.onKnockdown -= OnKnockdown;
        state.onFreeze -= OnFrozen;
    }

    protected virtual void FixedUpdate()
    {
        CheckGround();
        if (isFrozen)
        {
            rigid.linearVelocity = Vector3.zero;
            return;
        }

        SetDirection();

        if (state.State == CharacterState.Climb)
        {
            ClimbMove();
            Move();
            return;
        }

        if (!isOnGround || verticalVelocity > 0f) verticalVelocity -= activeGravity * Time.fixedDeltaTime;

        if (!state.CanNotMove)
        {
            if (isFreeMoveEnabled && skillEndTime < Time.time) FreeMove();
            else if (isFreeMoveEnabled && skillEndTime > Time.time) SkillFreeMove();
            else if (skillEndTime > Time.time) SkillMove();
        }

        ApplyFriction();
        BlockAgainstCharacters();
        Move();
    }

    public void SetDirection()
    {
        var input = commander.MoveInput;
        inputDirection = new Vector3(input.x, 0f, input.y);
    }

    private void Move()
    {
        rigid.linearVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
    }

    // 다른 캐릭터를 향해 가는 수평 속도 성분만 깎아낸다.
    // 물리 충돌(레이어)은 꺼져 있어 밀림은 없고, 이동만 벽처럼 막히며 표면을 따라 미끄러진다.
    private void BlockAgainstCharacters()
    {
        if (bodyCollider == null || characterBlockLayer == 0) return;

        Vector3 horiz = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
        if (horiz.sqrMagnitude < 0.0001f) return;

        // 캡슐의 월드 양 끝점 계산
        float height = Mathf.Max(bodyCollider.height, bodyCollider.radius * 2f);
        float radius = bodyCollider.radius;
        Vector3 center = bodyCollider.transform.TransformPoint(bodyCollider.center);
        Vector3 up = bodyCollider.transform.up;
        float half = Mathf.Max(0f, height * 0.5f - radius);
        Vector3 p1 = center + up * half;
        Vector3 p2 = center - up * half;

        Vector3 dir = horiz.normalized;
        float dist = horiz.magnitude * Time.fixedDeltaTime + blockSkin;

        if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist,
                                characterBlockLayer, QueryTriggerInteraction.Ignore))
        {
            Vector3 n = hit.normal;
            n.y = 0f;
            if (n.sqrMagnitude < 0.0001f) return;
            n.Normalize();

            float into = Vector3.Dot(horiz, n);
            if (into < 0f) horiz -= n * into; // 벽으로 파고드는 성분 제거 → 접선 방향만 남아 미끄러짐

            horizontalVelocity.x = horiz.x;
            horizontalVelocity.z = horiz.z;
        }
    }

    protected void FreeMove()
    {
        if (inputDirection.z > 0.01f)
        {
            horizontalSpeed = moveSpeed;
        }
        else if (Mathf.Abs(inputDirection.x) < 0.01f && inputDirection.z < -0.01f)
        {
            horizontalSpeed = moveSpeed * 0.3f;
        }
        else if (inputDirection.x != 0)
        {
            horizontalSpeed = moveSpeed * 0.8f;
        }
        else
        {
            horizontalSpeed = 0f;
        }

        Vector3 relativeDir = transform.TransformDirection(inputDirection.normalized);
        Vector3 slopeMoveDir = Vector3.ProjectOnPlane(relativeDir, surfaceNormal).normalized;
        localDirection = inputDirection;
        horizontalVelocity = slopeMoveDir * horizontalSpeed;
    }

    protected void SkillFreeMove()
    {
        horizontalSpeed = method.freeMoveSpeed;
        Vector3 relativeDir = transform.TransformDirection(inputDirection.normalized);
        Vector3 slopeMoveDir = Vector3.ProjectOnPlane(relativeDir, surfaceNormal).normalized;
        localDirection = inputDirection;
        horizontalVelocity = slopeMoveDir * method.freeMoveSpeed;
    }

    protected void ApplyFriction()
    {
        if (horizontalSpeed > 0.01f)
        {
            float reduction = friction * Time.deltaTime;
            horizontalSpeed = Mathf.Max(horizontalSpeed - reduction, 0);
            horizontalVelocity = horizontalVelocity.normalized * horizontalSpeed;
        }
    }

    private void SkillMove()
    {
        if (method.isStaticDirection)
        {
            horizontalVelocity = staticLocalDirection * horizontalSpeed;
        }
        else
        {
            horizontalVelocity = transform.TransformDirection(localDirection) * horizontalSpeed;
        }

        if (method.followTerrain)
        {
            horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, surfaceNormal);
        }

        if (method.isKeepSpeed && isOnGround)
        {
            horizontalVelocity = Vector3.zero;
        }

        if (!isOnGround && method.useAltitudeModifire && !method.isVerticalSpeedAutoCalc)
        {
            verticalVelocity = method.verticalSpeed;
        }
    }

    public void SkillMove(SkillAction action)
    {
        var time = action.actionTime;

        if (time <= 0f)
        {
            Debug.LogError("ActionTime <= 0");
            return;
        }

        var method = action.movementMethod;


        this.method = method;
        skillEndTime = time + Time.time;
        isFreeMoveEnabled = method.canFreeMove;

        localDirection = method.rightSide ? Vector3.right : Vector3.forward;
        localDirection = method.directionReverse ? -localDirection : localDirection;
        localDirection = method.useInputDirection ? inputDirection : localDirection;
        staticLocalDirection = transform.TransformDirection(localDirection);

        if (!method.isKeepSpeed)
        {
            float distance = method.distance;
            float targetY = float.MaxValue;
            float actionTime = 0f;

            verticalVelocity = 0f;

            switch (method.calcType)
            {
                case DistanceCalculateType.Fixed:
                    aim.GetLookAtDistance(action.targetting, groundLayer, distance, out targetY);
                    break;
                case DistanceCalculateType.UseInput:
                    if (commander.GetInput(ConditionInput.MoveForward, true)) { }
                    else if (commander.GetInput(ConditionInput.MoveBackward, true))
                    {
                        distance = method.backwardDistance;
                    }
                    else
                    {
                        distance = method.neutralDistance;
                    }
                    break;
                case DistanceCalculateType.UseAim:
                    distance = aim.GetLookAtDistance(action.targetting, groundLayer, distance, out targetY);
                    break;
                case DistanceCalculateType.Mixed:
                    if (commander.GetInput(ConditionInput.MoveBackward, true))
                    {
                        distance = method.backwardDistance;
                    }
                    else
                    {
                        distance = aim.GetLookAtDistance(action.targetting, groundLayer, distance, out targetY);
                    }
                    break;
            }

            

            if (method.jumpHeight != 0)
            {
                actionTime = method.regularTime;
                activeGravity = 8f * method.jumpHeight / (actionTime * actionTime);
                isOnGround = false;
                verticalVelocity = Mathf.Sqrt(2f * activeGravity * method.jumpHeight);
            }
            else
            {
                actionTime = time;
                activeGravity = method.gravity;
            }

            friction = method.isFrictionAutoCalc ? (2f * distance) / (actionTime * actionTime) : method.friction;

            if (method.minAltitude < targetY && method.maxAltitude > targetY && method.useAltitudeModifire)
            {
                if (method.isVerticalSpeedAutoCalc) verticalVelocity = targetY / actionTime;
            }

            horizontalSpeed = (distance / actionTime) + (0.5f * friction * actionTime);
        }
        else
        {
            horizontalSpeed = moveSpeed;
        }
    }

    public void StunMove(Character character, AttackInfo hit)
    {
        StopAllCoroutines();
        StartCoroutine(CoFreeze(hit));
    }

private IEnumerator CoFreeze(AttackInfo hit)
    {
        isFrozen = true;
        rigid.linearVelocity = Vector3.zero;
        if (hit.reaction == HitReactionType.Gaurded) { isFrozen = false; yield break; }

        // 피격 직후 상태를 캡처. StateManager가 onDamageTake에서 즉시 Airborne으로 바꾸므로
        // 대기 후 state.State를 보면 항상 Airborne이라 스탠딩 분기를 탈 수 없다.
        bool wasGrounded = state.State != CharacterState.Airborne && state.State != CharacterState.Knockdown;

        yield return new WaitForSeconds(hit.fixedStun);
        if (state.State == CharacterState.Grapped) yield break;

        var dir = Vector3.zero;
        rigid.linearVelocity = Vector3.zero;

        switch (hit.forceDirectionType)
        {
            case ForceDirectionType.Fixed:
                dir = hit.origin.forward;
                break;
            case ForceDirectionType.Spread:
                dir = transform.position - hit.origin.position;
                dir.y = 0;
                dir = dir == Vector3.zero ? hit.origin.forward : dir.normalized;
                break;
            case ForceDirectionType.Random:
                break;
            default:
                dir = -transform.forward;
                break;
        }

        if (wasGrounded)
        {
            switch (hit.reaction)
            {
                case HitReactionType.HitStun:
                    horizontalVelocity = dir * hit.stunForce;
                    horizontalSpeed = horizontalVelocity.magnitude;
                    isFreeMoveEnabled = false;
                    activeGravity = defaultGravity; 
                    friction = 8f;
                    break;
                case HitReactionType.Airborne:
                    rigid.position += Vector3.up * 0.5f;
                    isOnGround = false;
                    yield return new WaitForFixedUpdate();

                    horizontalVelocity = dir * hit.airborneForce.x;
                    horizontalSpeed = horizontalVelocity.magnitude;
                    verticalVelocity = hit.airborneForce.y;
                    isFreeMoveEnabled = false;
                    activeGravity = defaultGravity;
                    friction = 0f;
                    break;
            }
        }
        else
        {
            horizontalVelocity = dir * hit.airborneForce.x;
            horizontalSpeed = horizontalVelocity.magnitude;
            verticalVelocity = hit.airborneForce.y;
            isFreeMoveEnabled = false;
            activeGravity = defaultGravity;
            friction = 0f;
        }
        isFrozen = false;
    }

    public void EnterClimb(Vector3 ropePosition)
    {
        transform.position = new Vector3(ropePosition.x, transform.position.y, ropePosition.z);
        activeGravity      = 0f;
        verticalVelocity   = 0f;
        horizontalVelocity = Vector3.zero;
        horizontalSpeed    = 0f;
        isFreeMoveEnabled  = false;
    }

    private void ClimbMove()
    {
        horizontalVelocity = Vector3.zero;
        horizontalSpeed    = 0f;

        if      (inputDirection.z >  0.01f) verticalVelocity =  5f;
        else if (inputDirection.z < -0.01f) verticalVelocity = -5f;
        else                                verticalVelocity  =  0f;
    }

    public void OnStunEnd() { isFreeMoveEnabled = true; friction = 0; }
    public void OnWakeUpEnd() { isFreeMoveEnabled = true; }

    protected void CheckGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.5f, groundLayer))
        {
            surfaceNormal = hit.normal;
            if (!isNearGround)
            {
                isNearGround = true;
            }
        }
        else
        {
            surfaceNormal = Vector3.up;
            isNearGround = false;
            isOnGround = false;
        }

        bool isGrounded = isNearGround && isOnGround;
        if (!wasGrounded && isGrounded)
        {
            onLand?.Invoke();
            state.SetGrounded(true);
        }
        else if (wasGrounded && !isGrounded)
        {
            state.SetGrounded(false);
        }
        wasGrounded = isGrounded;
    }


    // 위로 향하는 정도(코사인). 0.5 ≈ 약 60도 이내 경사까지 바닥으로 인정, 그보다 가파르면 벽 취급
    private const float groundNormalThreshold = 0.5f;

    // Ground 콜라이더와의 접점 중 '발밑 바닥'(위로 향하는 법선)이 있는지 검사. 측면(벽) 접촉은 제외
    private bool HasFloorContact(Collision collision)
    {
        if (!collision.collider.CompareTag("Ground")) return false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y >= groundNormalThreshold)
                return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (HasFloorContact(collision) && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
            isOnGround = true;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (HasFloorContact(collision) && verticalVelocity <= 0f)
        {
            if (verticalVelocity < 0f) verticalVelocity = 0f;
            isOnGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isOnGround = false;
        }
    }


    public void SkillEnd()
    {
        horizontalVelocity = Vector3.zero;
        activeGravity = defaultGravity;
        friction = 0;
        isFreeMoveEnabled = true;
    }
    public void ReturnToIdle()
    {
        friction = 0f;
        activeGravity = defaultGravity;
        horizontalVelocity = Vector3.zero;
        isFreeMoveEnabled = true;
    }

    // 풀 재사용/부활 시 초기화. 속도·중력·이동 상태를 기본값으로 되돌린다.
    public void ResetState()
    {
        if (rigid != null) rigid.linearVelocity = Vector3.zero;
        verticalVelocity = 0f;
        horizontalVelocity = Vector3.zero;
        horizontalSpeed = 0f;
        activeGravity = defaultGravity;
        friction = 0f;
        isFreeMoveEnabled = true;
        isFrozen = false;
        enabled = true;   // StopImmediately로 꺼졌을 수 있으니 재활성
    }

    public void OnKnockdown()
    {
        horizontalVelocity = Vector3.zero;
    }

    // 죽음 연출용 즉시 정지. 속도를 0으로 만들고 이동 갱신을 멈춘다.
    public void StopImmediately()
    {
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        horizontalSpeed = 0f;
        if (rigid != null) rigid.linearVelocity = Vector3.zero;
        enabled = false;
    }

    public bool GetOnGrounded()
    {
        return isNearGround && isOnGround;
    }

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }

    public void StartGrabbed()
    {
        isOnGround = false;
        activeGravity = 0f;
        verticalVelocity = 0f;
    }

    public void EndGrabbed()
    {
        activeGravity = defaultGravity;
    }

    public void MoveToPosition(Transform worldPosition)
    {
        rigid.MovePosition(worldPosition.position);
        rigid.MoveRotation(worldPosition.rotation);
    }

    public void OnFrozen(float duration)
    {
        skillEndTime += duration;
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(CoOnFrozen(duration));
    }

    IEnumerator CoOnFrozen(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }
}

public class MovementRequest
{
    public MovementMethod method;
    public Vector3 point;
}