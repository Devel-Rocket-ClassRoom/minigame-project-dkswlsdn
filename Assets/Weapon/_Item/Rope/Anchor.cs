using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    [SerializeField] private GameObject ropePrefab;
    [SerializeField] private int        length     = 20;
    [SerializeField] private Vector3    exitOffset = new Vector3(0f, 1.5f, 0f);

    private float   TopY         => transform.position.y;
    private float   BottomY      => transform.position.y - length;
    private Vector3 ExitPosition => transform.position + exitOffset;

    private readonly List<Character> climbers = new List<Character>();

    private void OnEnable()
    {
        for (int i = 0; i < length; i++)
        {
            var go   = Instantiate(ropePrefab,
                           transform.position + Vector3.down * i,
                           Quaternion.identity);
            go.GetComponent<Rope>().owner = this;
        }
    }

    public void AddClimber(Character character)
    {
        if (!climbers.Contains(character))
            climbers.Add(character);
    }

    private void Update()
    {
        for (int i = climbers.Count - 1; i >= 0; i--)
        {
            var c = climbers[i];

            if (c == null || c.State.State != CharacterState.Climb)
            {
                climbers.RemoveAt(i);
                continue;
            }

            float y = c.transform.position.y;

            if (y <= BottomY)
            {
                c.State.ChangeState(CharacterState.Idle);
                climbers.RemoveAt(i);
            }
            else if (y >= TopY)
            {
                c.Movement.MoveTo(ExitPosition);
                c.State.ChangeState(CharacterState.Idle);
                climbers.RemoveAt(i);
            }
        }
    }
}
