using UnityEngine;

[RequireComponent(typeof(CharacterStat))]
public class CharacterVisualStateHandler : MonoBehaviour
{
    [Header("Outline Colors")]
    [SerializeField] private Color fullArmorColor = Color.white;
    [SerializeField] private Color longArmorColor = Color.green;
    [SerializeField] private Color shortArmorColor = Color.blue;
    [SerializeField] private float outlineWidth = 0.03f;

    [Header("Immune - Transparency")]
    [SerializeField] private float immuneAlpha = 0.4f;

    private CharacterStat stat;
    private Renderer[] renderers;
    private MaterialPropertyBlock block;

    private ArmorType prevArmor;
    private bool prevImmune;

    private void Awake()
    {
        stat = GetComponent<CharacterStat>();
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        ArmorType currentArmor = stat.Armor;
        bool currentImmune = stat.IsImmune;

        if (currentArmor == prevArmor && currentImmune == prevImmune) return;

        prevArmor = currentArmor;
        prevImmune = currentImmune;

        ApplyBlock(currentArmor, currentImmune);
    }

    private void ApplyBlock(ArmorType armor, bool immune)
    {
        Color outlineColor = GetOutlineColor(armor);
        float width = outlineColor == Color.clear ? 0f : outlineWidth;
        float alpha = immune ? immuneAlpha : 1f;

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(block);
            block.SetColor("_OutlineColor", outlineColor);
            block.SetFloat("_OutlineWidth", width);
            block.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
            r.SetPropertyBlock(block);
        }
    }

    private Color GetOutlineColor(ArmorType armor)
    {
        if ((armor & ArmorType.Full) != 0) return fullArmorColor;
        if ((armor & ArmorType.Long) != 0) return longArmorColor;
        if ((armor & ArmorType.Short) != 0) return shortArmorColor;
        return Color.clear;
    }
}