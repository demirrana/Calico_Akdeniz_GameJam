using UnityEngine;

public class Player : Fighter
{
    public static Player Instance { get; private set; }

    //trigger condition names of animator of player
    public string moveToLevel2 = "MoveToLevel2";
    public string moveToLevel3 = "MoveToLevel3";
    public string moveToFinish = "MoveToFinish";

    public void AnimateMovingOnMap(string triggerStr)
    {
        fighterAnimator.SetTrigger(triggerStr);
    }

    public void ReachToNextLevel()
    {
        Debug.Log("=== ReachToNextLevel CALLED ===");
        Debug.Log("Is this being called? Level: " + GameManager.Instance.GetCurrentLevel());
        GameManager.Instance.RaiseOnLevelStarted(this);
        Debug.Log("=== RaiseOnLevelStarted returned ===");
    }

    protected override void Awake()
    {
        base.Awake();
        SetSingleton();
    }

    private void SetSingleton()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
}
