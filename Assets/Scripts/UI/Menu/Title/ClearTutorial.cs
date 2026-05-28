using UnityEngine;

public class ClearTutorial : Interactor
{
    public override bool OnDetected(Character character)
    {
        SaveManager.instance.CurrentSave.isTutorialCleared = true;
        SaveManager.instance.SaveRequest();
        GameSceneManager.instance.LoadBaseCamp();
        return true;
    }
}
