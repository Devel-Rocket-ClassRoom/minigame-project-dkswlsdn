using UnityEngine;

public class NPCCamera : CharacterCamera
{
    [SerializeField] private float rotationSpeed = 360f;

    public void RotateTo(Transform target)
    {
        if (!canRotateCharacter) return;

        var dir = target.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;

        var targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    public void RotateBy(float eulerY)
    {
        if (!canRotateCharacter) return;

        var targetRot = transform.rotation * Quaternion.Euler(0, eulerY, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    public override void OnUseSkill(SkillAction action)
    {
        base.OnUseSkill(action);
        rotationSpeed = action.movementMethod.lookSpeedLimit;
    }

    public override void ReturnOrigin()
    {
        base.ReturnOrigin();
        rotationSpeed = 360f;
    }

    public override void OnStun()
    {
        base.OnStun();
        rotationSpeed = 0;
    }
}
