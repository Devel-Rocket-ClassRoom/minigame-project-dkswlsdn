using UnityEngine;
using UnityEngine.TextCore.Text;

public class InteractionManager : MonoBehaviour
{
    protected Character character;
    private CharacterCommander commander;
    [SerializeField] protected float interactionRadius;
    [SerializeField] protected LayerMask itemLayer;
    [SerializeField] protected LayerMask interactionLayer;

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

    protected virtual void interaction()
    {
        var hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);
        if (hits.Length == 0) return;

        var instance = hits[0].GetComponent<Interactor>();
        if (instance == null) return;

        instance.OnDetected(character);
    }
}
