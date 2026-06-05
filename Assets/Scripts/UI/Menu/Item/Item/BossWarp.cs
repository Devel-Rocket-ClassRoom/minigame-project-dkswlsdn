using UnityEngine;

[CreateAssetMenu(menuName = "Item/BossWarp")]
public class BossWarp : Item
{
    [SerializeField] private Skill skill;

    public override void OnUse(Character character)
    {
        GameSceneManager.LoadBoss();
    }
}
