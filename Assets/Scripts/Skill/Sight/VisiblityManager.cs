using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public static VisibilityManager instance;

    private static readonly int SightPositions = Shader.PropertyToID("_SightPositions");
    private static readonly int SightCount = Shader.PropertyToID("_SightCount");

    private List<Character> friendlyUnits = new List<Character>();
    private Vector4[] sightData = new Vector4[8];

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        int count = 0;
        foreach (var unit in friendlyUnits)
        {
            if (unit == null) continue;
            if (count >= 8) break;

            float range = unit.Stat.SightRange;
            Vector3 pos = unit.transform.position + unit.transform.forward * (range - 5);

            sightData[count] = new Vector4(pos.x, pos.y, pos.z, range);
            count++;
        }

        Shader.SetGlobalVectorArray(SightPositions, sightData);
        Shader.SetGlobalInt(SightCount, count);
    }

    public void Register(Character character)
    {
        if (!friendlyUnits.Contains(character))
            friendlyUnits.Add(character);
    }

    public void Unregister(Character character)
    {
        friendlyUnits.Remove(character);
    }
}