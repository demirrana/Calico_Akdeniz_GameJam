using UnityEngine;

public class Player : Fighter
{
    public static Player Instance { get; private set; }

    //trigger condition names of animator of player
    public string moveToLevel2 = "MoveToLevel2";
    public string moveToLevel3 = "MoveToLevel3";
    public string moveToFinish = "MoveToFinish";

    [SerializeField] private GameObject hand1;
    [SerializeField] private GameObject hand2;

    public void AnimateMovingOnMap(string triggerStr)
    {
        fighterAnimator.SetTrigger(triggerStr);
    }

    public void ReachToNextLevel()
    {
        GameManager.Instance.RaiseOnLevelStarted(this);
    }

    public void SetHandVisible()
    {
        hand1.SetActive(true);
        hand2.SetActive(true);
    }

    public void SetHandInvisible()
    {
        hand1.SetActive(false);
        hand2.SetActive(false);
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
