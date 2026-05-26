using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Interactor : MonoBehaviour
{
    public abstract bool OnDetected(Character character);
}
