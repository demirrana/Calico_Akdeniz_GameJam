using System;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public event EventHandler<FightManager.FighterArgs> OnSkillChosen; //triggered when a skill is picked by player

    public event EventHandler OnAnimationEnd;
    
    [SerializeField] private int health = 100;

    public bool isDefending = false; //is set to false when other party applies something and decreases the damage taken

    public void PlayAnimation()
    {
        
    }
    
    public void Fight()
    {
        
    }

    public void RaiseOnSkillChosen(object sender, Fighter fighter, Fighter targetFighter, Skill skill)
    {
        Debug.Log("5: RaiseOnSkillChosen. fighter:" + fighter.name + ", target:" + targetFighter.name + ", skill:" + skill.name);
        FightManager.FighterArgs fighterArgs = new(fighter, targetFighter, skill);
        OnSkillChosen?.Invoke(sender, fighterArgs);
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
