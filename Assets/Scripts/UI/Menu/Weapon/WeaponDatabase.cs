using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Scriptable Objects/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<Weapon> weapons;
}
