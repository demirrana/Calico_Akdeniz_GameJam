using System;
using System.Xml.Linq;
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
        context.Caster.PlayAnimation(SkillType.AttackSkill, context.Caster.isDefending);
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            Fighter animatorOwner = sender as Fighter;
            if (context.Caster != animatorOwner)    return;

            Debug.Log("10: Target gets damaged");
            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            Debug.Log("Health before: " + context.Target.GetHealth());
            if (context.Target.isDefending)
                context.Target.GetDamaged(5);
            else
                context.Target.GetDamaged(10);
            Debug.Log("Health after: " + context.Target.GetHealth());
            context.Target.RaiseOnHealthChanged(sender, context.Target.GetHealth());
            context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
        }
    }
}

[CreateAssetMenu(menuName = "Scriptable Objects/DefenseSkill")]
public class DefenseSkill : Skill
{
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: DefenseSkill Execute");
        context.Caster.PlayAnimation(SkillType.DefenseSkill, context.Caster.isDefending);
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            Fighter animatorOwner = sender as Fighter;
            if (context.Caster != animatorOwner)    return;

            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Caster.isDefending = true;
            context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
        }
    }
}

[CreateAssetMenu(menuName = "Scriptable Objects/HealSkill")]
public class HealSkill : Skill
{
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: HealSkill Execute");
        context.Caster.PlayAnimation(SkillType.HealSkill, context.Caster.isDefending);
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            Fighter animatorOwner = sender as Fighter;
            if (context.Caster != animatorOwner)    return;

            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Caster.Heal(15); //for now, heal by 15 health
            context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
        }
    }
}

public enum SkillType
{
    AttackSkill,
    DefenseSkill,
    HealSkill
}