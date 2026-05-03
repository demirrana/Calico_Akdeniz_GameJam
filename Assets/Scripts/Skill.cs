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

public enum SkillType
{
    AttackSkill,
    DefenseSkill,
    HealSkill
}