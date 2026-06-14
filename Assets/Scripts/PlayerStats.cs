using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Currency")]
    public int skillPoints = 0; // 경험치 모듈로 획득

    [Header("Upgrade Levels")]
    public int hpLevel = 1;
    public int atkLevel = 1;
    public int dashLevel = 1;
    public int autoShootLevel = 1;

    [Header("Upgrade Costs")]
    public int hpCost = 10;
    public int atkCost = 10;
    public int dashCost = 10;
    public int autoShootCost = 10;

    /// <summary>
    /// 경험치 모듈 픽업 시 호출. SP를 추가합니다.
    /// </summary>
    public void AddSkillPoints(int amount)
    {
        skillPoints += amount;
    }

    // Getters for actual values based on levels
    public float GetMaxHP()
    {
        return 100f + (hpLevel - 1) * 20f;
    }

    public float GetAttackPower()
    {
        return 35f + (atkLevel - 1) * 10f;
    }

    public float GetDashCooldown()
    {
        return Mathf.Max(0.2f, 1.0f - (dashLevel - 1) * 0.15f); // Base 1s, min 0.2s
    }

    public float GetAutoShootCooldown()
    {
        return Mathf.Max(5f, 15f - (autoShootLevel - 1) * 2f); // Base 15s, min 5s
    }

    public float GetBladeDashCooldown()
    {
        return Mathf.Max(1f, 3f - (autoShootLevel - 1) * 0.3f); // 검 돌진 스킬은 기본 3초로 쿨타임이 짧음
    }

    // Upgrade methods
    public bool TryUpgradeHP()
    {
        if (skillPoints >= hpCost)
        {
            skillPoints -= hpCost;
            hpLevel++;
            hpCost += 5; // Increase cost for next level
            GetComponent<PlayerHealth>().UpdateMaxHealth(GetMaxHP());
            return true;
        }
        return false;
    }

    public bool TryUpgradeATK()
    {
        if (skillPoints >= atkCost)
        {
            skillPoints -= atkCost;
            atkLevel++;
            atkCost += 5;
            
            PlayerShooting ps = GetComponent<PlayerShooting>();
            if (ps != null) ps.UpdateAttackPower(GetAttackPower());
            
            PlayerMelee pm = GetComponent<PlayerMelee>();
            if (pm != null) pm.UpdateAttackPower(GetAttackPower());
            
            return true;
        }
        return false;
    }

    public bool TryUpgradeDash()
    {
        if (skillPoints >= dashCost)
        {
            skillPoints -= dashCost;
            dashLevel++;
            dashCost += 5;
            GetComponent<PlayerMovement>().UpdateDashCooldown(GetDashCooldown());
            return true;
        }
        return false;
    }

    public bool TryUpgradeAutoShoot()
    {
        if (skillPoints >= autoShootCost)
        {
            skillPoints -= autoShootCost;
            autoShootLevel++;
            autoShootCost += 5;
            PlayerShooting ps = GetComponent<PlayerShooting>();
            if (ps != null) ps.UpdateAutoShootCooldown(GetAutoShootCooldown());

            PlayerMelee pm = GetComponent<PlayerMelee>();
            if (pm != null) pm.UpdateDashSkillCooldown(GetBladeDashCooldown());

            return true;
        }
        return false;
    }
}
