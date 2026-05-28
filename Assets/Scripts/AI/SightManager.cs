using System;
using System.Collections.Generic;
using UnityEngine;

public class SightManager : MonoBehaviour
{
    private Character character;
    private CharacterStat stat;
    private CapsuleCollider sightCollider;

    [SerializeField] private bool autoCenter = true;

    public HashSet<Character> visibleCharacters { get; private set; } = new HashSet<Character>();
    public Character FirstEncounter { get; private set; }

    public event Action<Character> onDetected;
    public event Action<Character> onLost;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
        stat = GetComponentInParent<CharacterStat>();
        sightCollider = GetComponent<CapsuleCollider>();
        sightCollider.isTrigger = true;

        stat.onStatChanged += ApplySightRange;
        ApplySightRange();
    }

    private void ApplySightRange()
    {
        float radius = stat.SightRange * 0.5f + 1.5f;
        sightCollider.radius = autoCenter ? radius : stat.SightRange;
        sightCollider.center = autoCenter
            ? new Vector3(0, 0, radius - 3f)
            : Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character == null || character.team == this.character.team) return;

        if (visibleCharacters.Count == 0)
            FirstEncounter = character;

        visibleCharacters.Add(character);
        onDetected?.Invoke(character);
    }

    private void OnTriggerExit(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character == null || character.team == this.character.team) return;

        visibleCharacters.Remove(character);
        if (visibleCharacters.Count == 0)
            FirstEncounter = null;

        onLost?.Invoke(character);
    }
}
