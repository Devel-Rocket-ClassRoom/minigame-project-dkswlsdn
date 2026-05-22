using System;
using System.Collections.Generic;
using UnityEngine;

public class SightManager : MonoBehaviour
{
    private Character self;
    private CharacterStat stat;
    private CapsuleCollider sightCollider;

    [SerializeField] private bool autoCenter = true;

    public List<Character> visibleCharacters { get; private set; } = new List<Character>();

    public event Action<Character> onDetected;
    public event Action<Character> onLost;

    private void Awake()
    {
        self = GetComponentInParent<Character>();
        stat = GetComponentInParent<CharacterStat>();
        sightCollider = GetComponent<CapsuleCollider>();
        sightCollider.isTrigger = true;

        stat.onStatChanged += ApplySightRange;
        ApplySightRange();
    }

    private void ApplySightRange()
    {
        float range = stat.SightRange;
        sightCollider.radius = range;
        sightCollider.center = autoCenter
            ? new Vector3(0, 0, range - 3f)
            : Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character == null || character.team == self.team) return;

        visibleCharacters.Add(character);
        onDetected?.Invoke(character);
    }

    private void OnTriggerExit(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character == null || character.team == self.team) return;

        visibleCharacters.Remove(character);
        onLost?.Invoke(character);
    }
}
