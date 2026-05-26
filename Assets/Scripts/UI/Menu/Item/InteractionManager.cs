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
            interaction();
        }
    }

    private void interaction()
    {
        var hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);
        if (hits.Length == 0) return;

        var instance = hits[0].GetComponent<Interactor>();
        if (instance == null) return;

        instance.OnDetected(character);
    }
}
