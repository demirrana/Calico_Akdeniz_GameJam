using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
    public static FightManager Instance { get; private set; }

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

    public Dictionary<Skill, Button> skillMapping = new();

    private Turn previousTurn;
    private Turn currentTurn; //this is changed through other methods

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
            Enemy.Instance.Fight();
        }
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
        foreach (Button skillButton in skillButtons)
        {
            skillButton.onClick.RemoveAllListeners();

            skillButton.onClick.AddListener(() => {
                fighter.OnSkillChosen += ChoosePlayerSkill;
                //invoke OnSkillChosen
                fighter.RaiseOnSkillChosen(this, fighter, targetFighter, skillMapping.FirstOrDefault(x => x.Value == skillButton).Key);
                ClearAllButtonListeners();
            });
        }
    }

    private void ChoosePlayerSkill(object sender, FighterArgs fighterArgs) //this is called when player has chosen a skill
    {
        Debug.Log("6: ChoosePlayerSkill");
        Player.Instance.OnSkillChosen -= ChoosePlayerSkill;

        fighterArgs.Skill.Execute(new SkillContext(fighterArgs.Fighter, fighterArgs.TargetFighter, SkillFinished));
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
        DetectTurnChange();

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