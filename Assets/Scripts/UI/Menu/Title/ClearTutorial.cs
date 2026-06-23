using UnityEngine;

public class ClearTutorial : Interactor
{
    public override bool OnDetected(Character character)
    {
        SaveManager.SaveRequest();
        GameSceneManager.LoadBaseCamp();
        return true;
    }
}
