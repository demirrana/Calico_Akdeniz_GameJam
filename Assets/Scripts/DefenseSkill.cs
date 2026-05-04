using System;
using UnityEngine;

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