using UnityEngine;

public class ClearTutorial : Interactor
{
    public override bool OnDetected(Character character)
    {
        SaveManager.CurrentSave.isTutorialCleared = true;
        SaveManager.SaveRequest();
        GameSceneManager.LoadBaseCamp();
        return true;
    }
}
