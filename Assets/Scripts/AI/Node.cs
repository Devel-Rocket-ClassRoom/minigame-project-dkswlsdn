using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> adjacentNodes;

    public Node GetNearest(Vector3 position)
    {
        Node nearest = this;
        float minSqrDist = (transform.position - position).sqrMagnitude;

        foreach (var node in adjacentNodes)
        {
            float sqrDist = (node.transform.position - position).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                nearest = node;
            }
        }

        return nearest;
    }
}