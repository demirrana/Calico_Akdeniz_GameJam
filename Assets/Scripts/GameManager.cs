using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnLevelCompleted; //shows cutscene between
    public event EventHandler OnLevelStarted; //switches to the level and fight

    [SerializeField] SpriteRenderer backGround;

    private int currentLevel = 1;

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void RaiseOnLevelCompleted(object sender)
    {
        OnLevelCompleted?.Invoke(sender, EventArgs.Empty);
    }

    public void RaiseOnLevelStarted(object sender)
    {
        OnLevelStarted?.Invoke(sender, EventArgs.Empty);
    }

    private void Awake()
    {
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

    private void Start()
    {
        OnLevelCompleted += CompleteLevel;
        OnLevelStarted += StartLevel;
    }

    private void CompleteLevel(object sender, EventArgs e)
    {
        FightManager.Instance.Reset();

        //LoadImage

        if (currentLevel == 3) //finish game by showing last cutscene
        {
            
        }

        //load cutscene

        currentLevel += 1;
    }

    private void StartLevel(object sender, EventArgs e) //call when cutscene is completed
    {
        
    }

    private void LoadImage(Sprite imgSprite)
    {
        backGround.sprite = imgSprite;
    }
}