using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private Attack[] hitboxes;
    public static AttackManager instance;
    private List<Attack> attackList = new List<Attack>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Attack RequestAttack(Character character, AttackMethod method, Vector3 targetPoint, bool canSpawn = true)
    {
        var instance = Instantiate(hitboxes[(int)method.type]);
        instance.Activate(character, method, targetPoint, canSpawn);
        attackList.Add(instance);
        return instance;
    }

    private void Update()
    {
        for (int i = attackList.Count - 1; i >= 0; i--)
        {
            if (attackList[i] == null)
            {
                attackList.RemoveAt(i);
            }
        }
    }

    public void DestroyAttack(Attack attack)
    {
        attackList.Remove(attack);
        Destroy(attack.gameObject);
    }
}
