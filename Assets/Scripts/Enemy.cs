using UnityEngine;

public class Enemy : Fighter
{
    public static Enemy Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        SetSingleton();
    }

    public override void Fight()
    {
        Skill chosenSkill;

        if (health <= 40)
        {
            chosenSkill = FightManager.Instance.attackSkill; //change to heal later
            RaiseOnSkillChosen(this, this, Player.Instance, chosenSkill);
        }
        else
        {
            if (Player.Instance.GetHealth() <= 40)
            {
                chosenSkill = FightManager.Instance.attackSkill;
                RaiseOnSkillChosen(this, this, Player.Instance, chosenSkill);
            }
            else
            {
                chosenSkill = FightManager.Instance.defendSkill;
                RaiseOnSkillChosen(this, this, Player.Instance, chosenSkill);
            }
        }
        //decide on what skill to use and call RaiseOnSkillChosen. the rest should be same
    }

    public Skill DecideOnAndGetSkill()
    {
        if (health <= 40)
        {
            return FightManager.Instance.attackSkill; //change to heal later
        }
        else
        {
            if (Player.Instance.GetHealth() <= 80)
            {
                return FightManager.Instance.attackSkill;
            }
            else
            {
                return FightManager.Instance.defendSkill;
            }
        }
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
