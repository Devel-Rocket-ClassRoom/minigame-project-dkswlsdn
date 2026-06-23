using System;
using UnityEngine;

/// <summary>
/// 게임 내 데이터베이스 SO(무기 / 서브웨폰 / 아이템)를 총괄 관리하는 싱글톤.
/// 인스펙터에 SO를 연결하고, 정적 접근자와 조회 헬퍼를 제공한다.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private SubWeaponDatabase subWeaponDatabase;
    [SerializeField] private ItemDatabase itemDatabase;

    // 데이터베이스 직접 접근
    public static WeaponDatabase WeaponDB    => Instance.weaponDatabase;
    public static SubWeaponDatabase SubWeaponDB => Instance.subWeaponDatabase;
    public static ItemDatabase ItemDB        => Instance.itemDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 조회 헬퍼 ─────────────────────────────────────────────
    public static Weapon FindWeapon(string weaponName)
    {
        var db = WeaponDB;
        if (db == null || db.weapons == null) return null;
        return db.weapons.Find(w =>
            w != null && string.Equals(w.weaponName, weaponName, StringComparison.OrdinalIgnoreCase));
    }

    public static Skill FindSubWeapon(string skillId)
    {
        return SubWeaponDB != null ? SubWeaponDB.Find(skillId) : null;
    }

    public static Item FindItem(string itemName)
    {
        var db = ItemDB;
        if (db == null || db.items == null) return null;
        return db.items.Find(i =>
            i != null && string.Equals(i.itemName, itemName, StringComparison.OrdinalIgnoreCase));
    }
}
