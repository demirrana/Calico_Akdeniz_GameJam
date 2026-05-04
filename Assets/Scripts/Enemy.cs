using UnityEngine;

public class Enemy : Fighter
{
    public static Enemy Instance { get; private set; }

    [SerializeField] public SpriteRenderer enemySpriteRenderer;

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
            if (Instance.GetHealth() >= 40)
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
        /*
        if (health <= 40)
        {
            return FightManager.Instance.attackSkill; //change to heal later
        }
        else
        {
            if (health > 40)
            {
                return FightManager.Instance.attackSkill;
            }
            else
            {
                return FightManager.Instance.defendSkill;
            }
        }*/
        
        float rand = UnityEngine.Random.value; // 0-1 arası

        if (health <= 30)
        {
            // Kritik can: ağırlıklı heal, biraz atak
            if (rand < 0.55f) return FightManager.Instance.healSkill;
            if (rand < 0.85f) return FightManager.Instance.attackSkill;
            return FightManager.Instance.defendSkill;
        }
        else if (health <= 60)
        {
            // Orta can: dengeli
            if (rand < 0.45f) return FightManager.Instance.attackSkill;
            if (rand < 0.75f) return FightManager.Instance.healSkill;
            return FightManager.Instance.defendSkill;
        }
        else
        {
            // Yüksek can: agresif
            if (rand < 0.65f) return FightManager.Instance.attackSkill;
            if (rand < 0.85f) return FightManager.Instance.defendSkill;
            return FightManager.Instance.healSkill;
        }
        
    }

    public void ChangeSprite(Sprite sprite)
    {
        enemySpriteRenderer.sprite = sprite;
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
