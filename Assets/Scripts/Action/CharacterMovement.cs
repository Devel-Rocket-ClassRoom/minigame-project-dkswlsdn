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
    public float moveSpeed = 10f;
    public readonly float defaultGravity = 20f;
    private float activeGravity = 20f;
    private float friction = 0;

    // 현재 물리 정보
    protected Vector3 inputDirection;
    private Vector3 localDirection;
    private Vector3 staticLocalDirection;
    private Vector3 horizontalVelocity;
    [SerializeField] private float verticalVelocity;
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
            if (isFreeMoveEnabled && skillEndTime < Time.time) FreeMove();
            else if (isFreeMoveEnabled && skillEndTime > Time.time) SkillFreeMove();
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
        if (!state.CanNotMove)
            rigid.linearVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        else
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, verticalVelocity, rigid.linearVelocity.z);
    }

    protected void FreeMove()
    {
        if (inputDirection.z > 0.01f)
        {
            horizontalSpeed = moveSpeed;
        }
        else if (inputDirection.z < -0.01f)
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

        if (!method.startToMove && isOnGround)
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

        if (method.startToMove)
        {
            float distance = method.distance;
            float targetY = float.MaxValue;
            float actionTime = 0f;

            verticalVelocity = 0f;

            switch (method.calcType)
            {
                case DistanceCalculateType.Fixed:
                    aim.GetLookAtDistance(action.targetting, distance, out targetY);
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
                    distance = aim.GetLookAtDistance(action.targetting, distance, out targetY);
                    break;
                case DistanceCalculateType.Mixed:
                    if (inputDirection.z < -0.01f)
                    {
                        distance = method.backwardDistance;
                    }
                    else
                    {
                        distance = aim.GetLookAtDistance(action.targetting, distance, out targetY);
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

    public void StunMove(AttackInfo hit)
    {
        var dir = Vector3.zero;
        rigid.linearVelocity = dir;

        switch (hit.forceDirectionType)
        {
            case ForceDirectionType.Fixed:
                dir = hit.origin.forward;
                break;
            case ForceDirectionType.Spread:
                dir = transform.position - hit.origin.position;
                dir.y = 0;
                dir.Normalize();
                break;
            case ForceDirectionType.Random:
                break;
            default:
                dir = -transform.forward;
                break;
        }

        if (state.State != CharacterState.Airborne && state.State != CharacterState.Knockdown)
        {
            switch (hit.reaction)
            {
                case HitReactionType.HitStun:
                    rigid.AddForce(dir * hit.stunForce, ForceMode.Impulse);
                    isFreeMoveEnabled = false;
                    activeGravity = defaultGravity;
                    friction = 6f;
                    break;
                case HitReactionType.Airborne:
                    transform.Translate(Vector3.up * 1f);
                    rigid.AddForce(dir * hit.airborneForce.x, ForceMode.Impulse);
                    verticalVelocity = hit.airborneForce.y;
                    isFreeMoveEnabled = false;
                    activeGravity = defaultGravity;
                    break;
            }
        }
        else
        {
            transform.Translate(Vector3.up * 0.5f);
            rigid.AddForce(dir * hit.airborneForce.x, ForceMode.Impulse);
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


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground") && isNearGround && verticalVelocity < 0)
        {
            verticalVelocity = 0f;
            isOnGround = true;
        }
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
}

public class MovementRequest
{
    public MovementMethod method;
    public Vector3 point;
}