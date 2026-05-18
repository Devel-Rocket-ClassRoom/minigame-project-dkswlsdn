using UnityEngine;

public interface IDamageable
{
    void TakeDamage(AttackInfo hit, Vector3 origin);
    void TakeAbsoluteDamage(float damage, out float damageTaken); // 상태이상 데미지, 기믹데미지, 퍼센트데미지 등등

    void Dead();
}