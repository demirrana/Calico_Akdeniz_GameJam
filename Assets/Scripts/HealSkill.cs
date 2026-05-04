using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/HealSkill")]
public class HealSkill : Skill
{
    int ranSayi;
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: HealSkill Execute");
        context.Caster.PlayAnimation(SkillType.HealSkill, context.Caster.isDefending);
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            Fighter animatorOwner = sender as Fighter;
            if (context.Caster != animatorOwner)    return;
            ranSayi = UnityEngine.Random.Range(0, 5);
            if(ranSayi == 0)
                AudioManager.Instance.PlayOneShotSFX("CanAzalmadiki");
            else if(ranSayi == 1)
                AudioManager.Instance.PlayOneShotSFX("CanBuyusu");

            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            context.Caster.Heal(15); //for now, heal by 15 health
            context.Caster.EnableHeal();
            context.Caster.StartCoroutine(WaitAndFinish());

            IEnumerator WaitAndFinish()
            {
                yield return new WaitForSeconds(1f);
                context.Caster.RaiseOnHealthChanged(this, context.Caster.GetHealth());
                context.Caster.DisableHeal();
                context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
            }
        }
    }
}