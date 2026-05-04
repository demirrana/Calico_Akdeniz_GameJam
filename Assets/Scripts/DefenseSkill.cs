using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/DefenseSkill")]
public class DefenseSkill : Skill
{
    int randomSAyi;
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: DefenseSkill Execute");
        context.Caster.PlayAnimation(SkillType.DefenseSkill, context.Caster.isDefending);
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            Fighter animatorOwner = sender as Fighter;
            if (context.Caster != animatorOwner)    return;
            randomSAyi = UnityEngine.Random.Range(0, 5);
            if(randomSAyi == 0)
                AudioManager.Instance.PlayOneShotSFX("BuyuKalkani");
            else if(randomSAyi == 1)
                AudioManager.Instance.PlayOneShotSFX("Kalkan");

            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Caster.isDefending = true;
            context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
        }
    }
}