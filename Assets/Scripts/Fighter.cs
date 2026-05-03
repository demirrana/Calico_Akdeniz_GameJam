using System;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public event EventHandler<FightManager.FighterArgs> OnSkillChosen; //triggered when a skill is picked by player

    public event EventHandler OnAnimationEnd;
    public event EventHandler<FightManager.FighterArgs> OnSkillCompleted; //triggered when animation is finished and its effect is safely applied
    
    public event EventHandler<int> OnHealthChanged;

    [SerializeField] protected int health = 100;
    private int maxHealth;

    //Animation related variables
    private Animator fighterAnimator;
    protected readonly string AttackTrigger = "Attack";
    protected readonly string IsDefending = "IsDefending";
    protected readonly string HealTrigger = "Heal";

    public bool isDefending = false; //is set to false when other party applies something and decreases the damage taken

    private Skill previousSkill;
    private Skill currentSkill;

    protected virtual void Awake()
    {
        fighterAnimator = GetComponent<Animator>();
        SetInitialSkills();
        maxHealth = health;
    }

    private void SetInitialSkills()
    {
        previousSkill = null;
        currentSkill = null;
    }

    public void PlayAnimation(SkillType skillType, bool isDefending)
    {
        Debug.Log("8: PlayAnimation with skill " + skillType.ToString());
        Debug.Log("Is fighter animator null: " + (fighterAnimator == null));
        switch (skillType)
        {
            case SkillType.AttackSkill:
            Debug.Log("Attack triggered");
                if (this.isDefending) //stop defending when another skill is chosen
                    fighterAnimator.SetBool(IsDefending, false);
                fighterAnimator.SetTrigger(AttackTrigger);
                break;
            case SkillType.DefenseSkill:
                fighterAnimator.SetBool(IsDefending, true);
                break;
            case SkillType.HealSkill:
                if (this.isDefending) //stop defending when another skill is chosen
                    fighterAnimator.SetBool(IsDefending, false);
                fighterAnimator.SetTrigger(HealTrigger);
                break;
            default:
                break;            
        }
    }

    public void EndAnimation() //this is called by animations at the end of each of them
    {
        //Debug.Log("fighter is " + this.name);
        //Debug.Log("9: EndAnimation is called as animation's end event");
        this.OnAnimationEnd?.Invoke(this, EventArgs.Empty); //is triggered for both of them on each time
    }
    
    public virtual void Fight()
    {
        
    }

    public void RaiseOnSkillChosen(object sender, Fighter fighter, Fighter targetFighter, Skill skill)
    {
        Debug.Log("5: RaiseOnSkillChosen. fighter:" + fighter.name + ", target:" + targetFighter.name + ", skill:" + skill.name);
        FightManager.FighterArgs fighterArgs = new(fighter, targetFighter, skill);
        OnSkillChosen?.Invoke(sender, fighterArgs);
    }

    public void RaiseOnSkillCompleted(object sender, Fighter fighter, Fighter targetFighter, Skill skill)
    {
        Debug.Log("11: Raise Complete Skill");
        FightManager.FighterArgs f = new(fighter, targetFighter, skill);
        OnSkillCompleted?.Invoke(sender, f);
    }

    public void RaiseOnHealthChanged(object sender, int newHealth)
    {
        OnHealthChanged?.Invoke(sender, newHealth);
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetHealth()
    {
        return health;
    }

    public void GetDamaged(int damage)
    {
        health -= damage;
    }

    public void Heal(int amount)
    {
        health += amount;
    }
}
