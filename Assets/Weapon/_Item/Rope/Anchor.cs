using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;

public class Anchor : MonoBehaviour
{
    [SerializeField] private GameObject ropePrefab;
    [SerializeField] private int length = 20;
    [SerializeField] private Vector3 exitOffset = new Vector3(0f, -1f, 0f);
    [SerializeField] private LayerMask layer;
    [SerializeField] private ItemInstance RopeItem;

    private float TopY => transform.position.y;
    private float BottomY => transform.position.y - length - 0.5f; 
    private Vector3 ExitPosition => transform.position + exitOffset;

    private readonly List<Character> climbers = new List<Character>();
    private readonly HashSet<Character> visitors = new HashSet<Character>();
    private NavMeshLink navMeshLink;

    public bool HasVisited(Character character) => visitors.Contains(character);

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        Destroy(gameObject);
    }

    private IEnumerator RemoveVisitorDelayed(Character character, float delay)
    {
        yield return new WaitForSeconds(delay);
        visitors.Remove(character);
    }

    private void OnEnable()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, length + 1, layer))
        {
            length = Mathf.FloorToInt(transform.position.y - hit.point.y) - 1;
        }

        if (length <= 1)
        {
            var rope = Instantiate(RopeItem, transform);
            rope.transform.position = transform.position;
            rope.transform.rotation = Quaternion.identity;
            Destroy(gameObject);
            return;
        }

        for (int i = 1; i < length + 1; i++)
        {
            var go = Instantiate(ropePrefab, transform);
            go.transform.position = transform.position + Vector3.down * i;
            go.transform.rotation = Quaternion.identity;
            go.GetComponent<Rope>().owner = this;
        }

        CreateNavMeshLink();
    }


    private void CreateNavMeshLink()
    {
        navMeshLink = gameObject.AddComponent<NavMeshLink>();
        navMeshLink.startPoint = Vector3.zero;
        navMeshLink.endPoint = Vector3.down * length;
        navMeshLink.bidirectional = true;
        navMeshLink.activated = true;
    }

    private void OnDisable()
    {
        if (navMeshLink != null)
        {
            Destroy(navMeshLink);
            navMeshLink = null;
        }
    }

    public void AddClimber(Character character)
    {
        if (!climbers.Contains(character))
        {
            climbers.Add(character);
            visitors.Add(character);
        }
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
                visitors.Add(c);
                c.State.ChangeState(CharacterState.Idle);
                climbers.RemoveAt(i);
                StartCoroutine(RemoveVisitorDelayed(c, 2f));
            }
            else if (y >= TopY)
            {
                visitors.Add(c);
                c.Movement.MoveTo(ExitPosition);
                c.State.ChangeState(CharacterState.Idle);
                climbers.RemoveAt(i);
                StartCoroutine(RemoveVisitorDelayed(c, 2f));
            }
        }
    }
}
