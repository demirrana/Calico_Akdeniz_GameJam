using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AttackSkill")]
public class AttackSkill : Skill
{
    private int normalDamage = 20;
    private int defensedDamage = 10;

    int bearDamage = 45;
    int defensedBearDamage = 30;

    //firstly plays the animation of that skill, then applies the required health changes and etc.
    public override void Execute(SkillContext context)
    {
        Debug.Log("7: AttackSkill Execute");
        context.Caster.PlayAnimation(SkillType.AttackSkill, context.Caster.isDefending);
        context.Caster.OnAnimationEnd += OnAnimationComplete;

        void OnAnimationComplete(object sender, EventArgs e)
        {
            Fighter animatorOwner = sender as Fighter;
            if (context.Caster != animatorOwner) return;

            Debug.Log("10: Target gets damaged");
            int RantomkiSAyi = UnityEngine.Random.Range(0, 5);
            if (RantomkiSAyi == 0)
                AudioManager.Instance.PlayOneShotSFX("Tekme");
            else if (RantomkiSAyi == 1)
                AudioManager.Instance.PlayOneShotSFX("Yumruk");

            context.Caster.OnAnimationEnd -= OnAnimationComplete;
            Debug.Log("Health before: " + context.Target.GetHealth());
            if (ParryManager.Instance.IsParryActive) //parry occurred (whether successful or not)
            {
                if (ParryManager.Instance.IsParrySuccessful()) //enemy gets damaged
                {
                    Debug.Log("Parry is successful!");
                    AudioManager.Instance.PlayOneShotSFX("Parry");
                    context.Caster.GetDamaged(normalDamage);
                    context.Caster.RaiseOnHealthChanged(sender, context.Caster.GetHealth()); //show it in UI
                }
                else //failed parry (may vary from the "else" below)
                {
                    Debug.Log("Parry is NOT successful");
                    if (context.Target.isDefending)
                        context.Target.GetDamaged(defensedDamage);
                    else
                        context.Target.GetDamaged(normalDamage);

                    context.Target.RaiseOnHealthChanged(sender, context.Target.GetHealth()); //show it in UI
                }
                ParryManager.Instance.IsParryActive = false;
            }
            else
            {
                Debug.Log("NO PARRY");
                if (context.Target.isDefending)
                    context.Target.GetDamaged(defensedDamage);
                else
                    context.Target.GetDamaged(normalDamage);

                context.Target.RaiseOnHealthChanged(sender, context.Target.GetHealth()); //show it in UI
            }

            Debug.Log("Health after: " + context.Target.GetHealth());
            context.Caster.RaiseOnSkillCompleted(this, context.Caster, context.Target, this);
        }
    }
}