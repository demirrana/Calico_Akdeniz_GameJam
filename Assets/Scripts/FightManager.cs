using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
    public static FightManager Instance { get; private set; }

    //EventArgs to get info through events such as fighter, target and the skill that is used
    public class FighterArgs : EventArgs
    {
        public Fighter Fighter;
        public Fighter TargetFighter;
        public Skill Skill;

        public FighterArgs(Fighter fighter, Fighter targetFighter, Skill skill)
        {
            Fighter = fighter;
            TargetFighter = targetFighter;
            Skill = skill;
        }
    }

    public event EventHandler<Turn> OnTurnChanged;

    public event EventHandler OnSkillStarted;
    public event EventHandler OnSkillParried;
    public event EventHandler OnSkillFinished;

    public enum Turn
    {
        Player,
        Enemy    
    }

    [SerializeField] List<Button> skillButtons; //from left to right: attack, defense, heal
    [SerializeField] List<Skill> SOskills;

    public Skill attackSkill;
    public Skill defendSkill;
    public Skill healSkill;

    public Dictionary<Skill, Button> skillMapping = new();

    private Turn previousTurn;
    private Turn currentTurn; //this is changed through other methods

    public Turn GetCurrentTurn()
    {
        return currentTurn;
    }

    public void Reset()
    {
        previousTurn = Turn.Player;
        previousTurn = Turn.Enemy;
        Player.Instance.Reset();
        Enemy.Instance.Reset();
    }

    private void Awake()
    {
        SetSingleton();
        SetSkillButtons();
        previousTurn = Turn.Player; //start from the player to play
        currentTurn = Turn.Player;
    }

    private void SetSingleton()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    //Sets dictionary of skill and button mappings to access buttons from that skill info
    private void SetSkillButtons()
    {
        skillMapping[SOskills[0]] = skillButtons[0];
        skillMapping[SOskills[1]] = skillButtons[1];
        skillMapping[SOskills[2]] = skillButtons[2];
    }

    private void Start()
    {
        OnTurnChanged += TurnChanged;
        Player.Instance.OnSkillCompleted += CompletePlayerSkill;
        Enemy.Instance.OnSkillCompleted += CompleteEnemySkill;
        StartFight();
    }

    private void TurnChanged(object sender, Turn turn)
    {
        Debug.Log("3: TurnChanged");
        if (turn == Turn.Player)
        {
            Debug.Log("player's turn");
            DetectAnySkillButtonClicked(Player.Instance);
        }
        else
        {
            Debug.Log("enemy's turn");
            DetectAnySkillButtonClicked(Enemy.Instance);
            //Enemy.Instance.Fight();
        }
    }

    //call ending animations to manage turns one after another
    private void CompletePlayerSkill(object sender, FighterArgs fighterArgs)
    {
        Debug.Log("12: Player's skill is completed and turn is changed.");
        CompleteFighterSkill(fighterArgs);
        currentTurn = Turn.Enemy;
    }

    private void CompleteEnemySkill(object sender, FighterArgs fighterArgs)
    {
        CompleteFighterSkill(fighterArgs);
        currentTurn = Turn.Player;
    }

    //set isDefending when that fighter's turn is over
    private void CompleteFighterSkill(FighterArgs fighterArgs)
    {
        if (fighterArgs.Skill == defendSkill)
            fighterArgs.Fighter.isDefending = true;
        else
            fighterArgs.Fighter.isDefending = false;
    }

    private void StartFight()
    {
        Debug.Log("Start Fight");
        previousTurn = Turn.Enemy;
        currentTurn = Turn.Player;
    }

    //triggers event OnSkillChosen by detecting if any button is clicked by the player
    private void DetectAnySkillButtonClicked(Fighter fighter)
    {
        Debug.Log("4: DetectAnySkillButtonClicked");
        Fighter targetFighter = fighter == Player.Instance ? Enemy.Instance : Player.Instance;

        if (fighter == Player.Instance)
            foreach (Button skillButton in skillButtons)
            {
                skillButton.onClick.RemoveAllListeners();

                skillButton.onClick.AddListener(() => {
                    fighter.OnSkillChosen += ChooseFighterSkill;
                    //invoke OnSkillChosen
                    fighter.RaiseOnSkillChosen(this, fighter, targetFighter, skillMapping.FirstOrDefault(x => x.Value == skillButton).Key);
                    ClearAllButtonListeners();
                });
            }
        else //do Enemy's Fight method in here
        {
            Skill chosenSkill = Enemy.Instance.DecideOnAndGetSkill();
            fighter.OnSkillChosen += ChooseFighterSkill;
            fighter.RaiseOnSkillChosen(this, fighter, targetFighter, chosenSkill);
        }
    }

    private void ChooseFighterSkill(object sender, FighterArgs fighterArgs) //this is called when player has chosen a skill
    {
        Debug.Log("6: ChoosePlayerSkill");
        fighterArgs.Fighter.OnSkillChosen -= ChooseFighterSkill;

        if (currentTurn == Turn.Enemy && fighterArgs.Skill == attackSkill) //parry chance
        {
            StartCoroutine(ParryManager.Instance.TryParry(GameManager.Instance.GetCurrentLevel(), () => //!!!!!!!!!!!!!!!!!!!!!!!!! level instead of 1
            {
                fighterArgs.Skill.Execute(new SkillContext(fighterArgs.Fighter, fighterArgs.TargetFighter, SkillFinished));
            }));
        }
        else
        {
            fighterArgs.Skill.Execute(new SkillContext(fighterArgs.Fighter, fighterArgs.TargetFighter, SkillFinished));   
        }
    }

    private void SkillFinished()
    {
        Debug.Log("1: Skill has finished");
    }

    private void ClearAllButtonListeners()
    {
        foreach (Button button in skillButtons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void Update()
    {
        DetectLevelFinished();
        DetectTurnChange();
    }

    private void DetectLevelFinished()
    {
        if (Enemy.Instance.GetHealth() <= 0) //player got past this level
        {
            GameManager.Instance.RaiseOnLevelCompleted(this);
            //successful window
        }
        else if (Player.Instance.GetHealth() <= 0) //player has to go through the same level
        {
            //fail window
        }
    }

    //Triggers event OnTurnChanged when isPlayersTurn bool is changed
    private void DetectTurnChange()
    {
        if (previousTurn != currentTurn)
        {
            Debug.Log("2: DetectTurnChange turn changed");
            OnTurnChanged?.Invoke(this, currentTurn);
            previousTurn = currentTurn;
        }
    }
}