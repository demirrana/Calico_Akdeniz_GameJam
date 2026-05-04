using System;
using System.Collections;
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
        Debug.Log("Complete level " + currentLevel);
        canvasObject.SetActive(false);
        Enemy.Instance.gameObject.SetActive(false);
        
        // Reset() KALDIRILDI — sadece disable et
        FightManager.Instance.enabled = false;
        
        backgroundIndex += 1;
        currentLevel += 1;

        Player.Instance.SetHandInvisible();
        LoadImage(allBackgrounds[backgroundIndex]);

        switch (currentLevel)
        {
            case 2: 
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
        Debug.Log("=== StartLevel BEGIN ===");
        Debug.Log("currentLevel: " + currentLevel);
        Debug.Log("backgroundIndex: " + backgroundIndex);
        Debug.Log("enemySprites count: " + enemySprites.Count);
        Debug.Log("allBackgrounds count: " + allBackgrounds.Count);

        FightManager.Instance.enabled = true;
        FightManager.Instance.Reset();
        backgroundIndex++;

        if (backgroundIndex == allBackgrounds.Count - 1) //show the last scene and finish the game
        {
            LoadImage(allBackgrounds[backgroundIndex]);
            Player.Instance.transform.gameObject.SetActive(false);
            StartCoroutine(WaitForEnter());
            return;
        }

        LoadImage(allBackgrounds[backgroundIndex]);
        Debug.Log("BACKGROUND NAME: " + allBackgrounds[backgroundIndex].name);
        SendBackgroundToBack();
        ChangeEnemy(enemySprites[currentLevel - 1]);

        Player.Instance.SetHandVisible();
        
        Debug.Log("Before SetActive - Enemy active: " + Enemy.Instance.gameObject.activeSelf);
        Debug.Log("Before SetActive - Canvas active: " + canvasObject.activeSelf);
        Enemy.Instance.gameObject.SetActive(true);
        canvasObject.SetActive(true);
        Debug.Log("After SetActive - Enemy active: " + Enemy.Instance.gameObject.activeSelf);
        Debug.Log("After SetActive - Canvas active: " + canvasObject.activeSelf);
        Debug.Log("=== StartLevel END ===");
    }

    private IEnumerator WaitForEnter()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
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
        backGround.sortingOrder = -5;
    }
}