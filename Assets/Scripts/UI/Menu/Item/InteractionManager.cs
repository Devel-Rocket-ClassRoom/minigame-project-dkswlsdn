using UnityEngine;
using UnityEngine.TextCore.Text;

public class InteractionManager : MonoBehaviour
{
    private Character character;
    private CharacterCommander commander;
    [SerializeField] private float interactionRadius;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask interactionLayer;

    private void Awake()
    {
        character = GetComponent<Character>();
        commander = GetComponent<CharacterCommander>();
    }

    private void Update()
    {
        if (commander.GetInput(ConditionInput.Interaction))
        {
            PickupItem();
            interaction();
        }
    }

    private void PickupItem()
    {
        var hits = Physics.OverlapSphere(transform.position, interactionRadius, itemLayer);
        if (hits.Length == 0) return;

        var instance = hits[0].GetComponent<ItemInstance>();
        if (instance == null) return;

        instance.GetItem().OnGet(character);
        Destroy(hits[0].gameObject);
    }

    private void interaction()
    {
        var hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);
        if (hits.Length == 0) return;

        var instance = hits[0].GetComponent<Interactor>();
        if (instance == null) return;

        instance.OnDetected();
    }
}
