using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터에서 수평면 360도로 레이를 쏴 벽에 막힌 영역을 제외한
/// '보이는 영역' 메시(시야 폴리곤)를 매 프레임 생성한다.
/// 생성된 메시를 가산 머티리얼로 칠하거나 마스크로 사용해 시야를 표현한다.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class VisibilityMesh : MonoBehaviour
{
    [Header("레이 설정")]
    [SerializeField] private int rayCount = 360;        // 레이 개수(많을수록 매끄럽고 무거움)
    [SerializeField] private LayerMask wallMask;        // 시야를 막는 레이어
    [SerializeField] private float planeHeight = 0.1f;  // 메시를 그릴 높이(바닥에서 살짝 띄움)

    [Header("사거리")]
    [SerializeField] private bool useStatRange = true;  // SightRange 스탯 사용 여부
    [SerializeField] private float fallbackRange = 10f; // 스탯 없을 때 기본 사거리

    private CharacterStat stat;
    private Transform origin;       // 레이 출발 기준(캐릭터)
    private Mesh mesh;

    private readonly List<Vector3> verts = new();
    private readonly List<int> tris = new();

    private void Awake()
    {
        stat   = GetComponentInParent<CharacterStat>();
        origin = stat != null ? stat.transform : transform.parent;

        mesh = new Mesh { name = "VisibilityMesh" };
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void LateUpdate()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        float range = (useStatRange && stat != null) ? stat.SightRange : fallbackRange;
        Vector3 center = origin.position;
        center.y = planeHeight;

        verts.Clear();
        tris.Clear();

        // 메시는 이 오브젝트 로컬 공간 기준 → 중심을 원점으로
        verts.Add(Vector3.zero); // 0번: 중심

        float step = 360f / rayCount;
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = i * step * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            Vector3 hitPoint;
            if (Physics.Raycast(center, dir, out var hit, range, wallMask))
                hitPoint = hit.point;
            else
                hitPoint = center + dir * range;

            hitPoint.y = planeHeight;
            verts.Add(transform.InverseTransformPoint(hitPoint));

            if (i > 0)
            {
                tris.Add(0);
                tris.Add(i);
                tris.Add(i + 1);
            }
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
    }
}
