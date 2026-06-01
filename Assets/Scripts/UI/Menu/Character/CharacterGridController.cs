using UnityEngine;

public class CharacterGridController : MonoBehaviour
{
    private void Awake()
    {
        var paneles = GetComponentsInChildren<CharacterGridController>();
    }

    public void RequestOpenStatusMenu(string id)
    {

    }

    public void RequestOpenSubWeaponMenu(string id)
    {

    }

    public void RequestOpenSkillMenu(string id)
    {

    }
}
