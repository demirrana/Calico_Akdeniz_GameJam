using System;
using UnityEngine;

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
            context.Caster.EnableHeal();
            context.Caster.RaiseOnHealthChanged(this, context.Caster.GetHealth());
            context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
            context.Caster.DisableHeal();
        }
    }
}