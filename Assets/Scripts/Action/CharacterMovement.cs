using System;
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
    public float moveSpeed = 1f;
    public readonly float defaultGravity = 20f;
    private float activeGravity = 20f;
    private float friction = 0;

    // 현재 물리 정보
    protected Vector3 inputDirection;
    private Vector3 localDirection;
    private Vector3 horizontalVelocity;
    [SerializeField] private float verticalVelocity;
    private float skillHorizontalSpeed;
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


    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        aim = GetComponent<CharacterAim>();
        stat = GetComponent<CharacterStat>();
        state = GetComponent<StateManager>();
        commander = GetComponent<CharacterCommander>();

        stat.onDamageTake += StunMove;
        onLand += state.OnLand;
        state.onIdle += ReturnToIdle;
        state.onKnockdown += OnKnockdown;
    }

    protected virtual void FixedUpdate()
    {
        CheckGround();
        if (!isOnGround || verticalVelocity > 0f) verticalVelocity -= activeGravity * Time.fixedDeltaTime;

        SetDirection();

        if (!state.CanNotMove)
        {
            if (isFreeMoveEnabled) FreeMove();
            else if (skillEndTime > Time.time) SkillMove();
        }

        ApplyFriction();
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

    protected void FreeMove()
    {
        Vector3 relativeDir = transform.TransformDirection(inputDirection.normalized);
        Vector3 slopeMoveDir = Vector3.ProjectOnPlane(relativeDir, surfaceNormal).normalized;
        localDirection = inputDirection;
        horizontalVelocity = slopeMoveDir * moveSpeed;
    }

    protected void ApplyFriction()
    {
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed > 0.01f)
        {
            float reduction = friction * Time.deltaTime;
            float newSpeed = Mathf.Max(horizontalSpeed - reduction, 0);
            horizontalVelocity = horizontalVelocity.normalized * newSpeed;
        }
    }

    private void SkillMove()
    {
        horizontalVelocity = transform.TransformDirection(localDirection) * skillHorizontalSpeed;

        if (method.followTerrain)
        {
            horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, surfaceNormal);
        }

        if (!method.startToMove && isOnGround)
        {
            horizontalVelocity = Vector3.zero;
        }
    }

    public void SkillMove(MovementMethod method, float time)
    {
        if (time <= 0f)
        {
            Debug.LogError("ActionTime <= 0");
            return;
        }

        this.method = method;
        skillEndTime = time + Time.time;
        isFreeMoveEnabled = method.canFreeMove;

        var direction = method.rightSide ? Vector3.right : Vector3.forward;
        direction = method.directionReverse ? -direction : direction;

        localDirection = method.useInputDirection ? inputDirection : direction;

        if (method.startToMove)
        {
            float distance = method.distance;
            float targetY = float.MaxValue;
            float actionTime = 0f;

            switch (method.calcType)
            {
                case DistanceCalculateType.Fixed:
                    break;
                case DistanceCalculateType.UseInput:
                    if (Mathf.Abs(inputDirection.z) < 0.01f)
                    {
                        distance = method.neutralDistance;
                    }
                    else if (inputDirection.z < -0.01f)
                    {
                        distance = method.backwardDistance;
                    }
                    break;
                case DistanceCalculateType.UseAim:
                    distance = aim.GetLookAtDistance(distance, out targetY);
                    break;
                case DistanceCalculateType.Mixed:
                    if (inputDirection.z < -0.01f)
                    {
                        distance = method.backwardDistance;
                    }
                    else
                    {
                        distance = aim.GetLookAtDistance(distance, out targetY);
                    }
                    break;
            }

            friction = method.friction;

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

            skillHorizontalSpeed = (distance / actionTime) + (0.5f * method.friction * actionTime);
        }
        else
        {
            skillHorizontalSpeed = moveSpeed;
        }
    }

    public void StunMove(AttackInfo hit)
    {
        var dir = Vector3.zero;

        switch (hit.forceDirectionType)
        {
            case ForceDirectionType.Fixed:
                dir = hit.origin.forward;
                break;
            case ForceDirectionType.Spread:
                dir = transform.position - hit.origin.position;
                dir.Normalize();
                break;
            case ForceDirectionType.Random:
                break;
            default:
                dir = hit.origin.forward;
                break;
        }

        if (state.State != CharacterState.Airborne)
        {
            switch (hit.reaction)
            {
                case HitReactionType.HitStun:
                    horizontalVelocity = dir * hit.stunForce;
                    isFreeMoveEnabled = false;
                    activeGravity = defaultGravity;
                    friction = 6f;
                    break;
                case HitReactionType.Airborne:
                    transform.Translate(Vector3.up * 0.5f);
                    var h = hit.airborneForce;
                    h.y = 0;
                    horizontalVelocity = dir * h.x;
                    verticalVelocity = hit.airborneForce.y;
                    isFreeMoveEnabled = false;
                    activeGravity = defaultGravity;
                    break;
            }
        }
        else
        {
            transform.Translate(Vector3.up * 0.5f);
            var h = hit.airborneForce;
            h.y = 0;
            horizontalVelocity = dir * h.x;
            verticalVelocity = hit.airborneForce.y;
            isFreeMoveEnabled = false;
            activeGravity = defaultGravity;
        }
    }

    public void OnStunEnd() { isFreeMoveEnabled = true; friction = 0; }
    public void OnWakeUpEnd() { isFreeMoveEnabled = true; }

    protected void CheckGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.5f))
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

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Ground") && isNearGround && verticalVelocity < 0)
        {
            verticalVelocity = 0f;
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

    public void OnKnockdown()
    {
        horizontalVelocity = Vector3.zero;
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
        foreach (var col in GetComponents<Collider>())
            if (!col.isTrigger) col.enabled = false;
    }

    public void EndGrabbed()
    {
        activeGravity = defaultGravity;
        foreach (var col in GetComponents<Collider>())
            if (!col.isTrigger) col.enabled = true;
    }

    public void MoveToPosition(Vector3 worldPosition)
    {
        rigid.MovePosition(worldPosition);
    }
}