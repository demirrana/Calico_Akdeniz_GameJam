using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public abstract class Skill : ScriptableObject
{
    public string skillName;
    
    public abstract void Execute(SkillContext context);
}

public class SkillContext
{
    public Fighter Caster;
    public Fighter Target;
    public Action OnComplete;

    public SkillContext(Fighter caster, Fighter target, Action onComplete)
    {
        Caster = caster;
        Target = target;
        OnComplete = onComplete;
    }
}

[CreateAssetMenu(menuName = "Scriptable Objects/AttackSkill")]
public class AttackSkill : Skill
{
    //firstly plays the animation of that skill, then applies the required health changes and etc.
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: AttackSkill Execute");
        context.Caster.PlayAnimation();
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Target.GetDamaged(10); //for now, each attack decreases health by 10
        }
    }
}

[CreateAssetMenu(menuName = "Scriptable Objects/DefenseSkill")]
public class DefenseSkill : Skill
{
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: DefenseSkill Execute");
        context.Caster.PlayAnimation();
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Caster.isDefending = true;
        }
    }
}

[CreateAssetMenu(menuName = "Scriptable Objects/HealSkill")]
public class HealSkill : Skill
{
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: HealSkill Execute");
        context.Caster.PlayAnimation();
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Caster.Heal(15); //for now, heal by 15 health
        }
    }
}

public enum SkillType
{
    AttackSkill,
    DefenseSkill,
    HealSkill
}