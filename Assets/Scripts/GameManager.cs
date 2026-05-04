using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnLevelCompleted; //shows cutscene between
    public event EventHandler OnLevelStarted; //switches to the level and fight

    public enum BackgroundType
    {
        Cutscene,
        Playscene
    }

    [SerializeField] SpriteRenderer backGround;

    [SerializeField] private GameObject canvasObject;

    [SerializeField] private List<Sprite> allBackgrounds;
    [SerializeField] private List<BackgroundType> backgroundTypes;
    private int backgroundIndex = 0;

    [SerializeField] private List<Sprite> enemySprites;

    //iki obje tutmak: biri background listesi diğeri de bunların sırasına göre arkaya mı öne mi gideceği (türü)

    private int currentLevel = 1;

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void RaiseOnLevelCompleted(object sender)
    {
        OnLevelCompleted?.Invoke(sender, EventArgs.Empty);
    }

    public void RaiseOnLevelStarted(object sender) //end of animation will call this
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
        LoadImage(allBackgrounds[backgroundIndex]);
        SendBackgroundToBack();
    }

    private void CompleteLevel(object sender, EventArgs e)
    {
        //BringBackgroundToFront();
        canvasObject.SetActive(false);
        Enemy.Instance.gameObject.SetActive(false);
        FightManager.Instance.Reset();
        
        backgroundIndex += 1;

        //win screen

        currentLevel += 1;

        LoadImage(allBackgrounds[backgroundIndex]);

        //animations call RaiseOnLevelStarted from Player instance and the last animation doesn't
        switch (currentLevel)
        {
            case 2:
                Debug.Log("Animate moving on map to level 2");
                Player.Instance.AnimateMovingOnMap(Player.Instance.moveToLevel2);
                break;
            case 3:
                Player.Instance.AnimateMovingOnMap(Player.Instance.moveToLevel3);
                break;
            case 4:
                Player.Instance.AnimateMovingOnMap(Player.Instance.moveToFinish);
                break;
        }
    }

    private void StartLevel(object sender, EventArgs e) //call when cutscene is completed
    {
        backgroundIndex++;
        LoadImage(allBackgrounds[backgroundIndex]);
        SendBackgroundToBack();
        ChangeEnemy(enemySprites[currentLevel - 1]);
        Enemy.Instance.gameObject.SetActive(true);
        canvasObject.SetActive(true);
    }

    private void LoadImage(Sprite imgSprite)
    {
        backGround.sprite = imgSprite;
    }

    private void ChangeEnemy(Sprite newEnemySprite)
    {
        Enemy.Instance.ChangeSprite(newEnemySprite);
    }

    private void BringBackgroundToFront()
    {
        backGround.sortingOrder = 100;
    }

    private void SendBackgroundToBack()
    {
        backGround.sortingOrder = -1;
    }
}