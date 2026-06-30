using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Stack/Repeat")]
public class RepeatStack : SpecialStackData
{
    [SerializeField] private AttackInfoEntry[] method;

    public override void OnGained(Character character, int gained, Character grantor)
    {
        character.StartCoroutine(Repeat(character, grantor));
    }

    private IEnumerator Repeat(Character character, Character grantor)
    {
        if (method.Length < 0) { yield break; }

        foreach (AttackInfoEntry entry in method)
        {
            yield return new WaitForSeconds(entry.preDelay);
            character.Stat.TakeDamage(grantor, entry.info, new AttackId(grantor));
        }
    }
}

[Serializable]
public struct AttackInfoEntry
{
    public float preDelay;
    public AttackInfo info;
}
