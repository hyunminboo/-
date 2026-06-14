using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("Skill Upgrade Texts")]
    public Text skillPointsText;
    public Text hpUpgradeText;
    public Text atkUpgradeText;
    public Text dashUpgradeText;
    public Text autoShootUpgradeText;

    [Header("Panel References (optional)")]
    public GameObject leftItemPanel; // 아이템 패널 - 비활성화 유지

    void OnEnable()
    {
        if (playerStats == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) playerStats = p.GetComponent<PlayerStats>();
        }

        // 아이템 패널은 항상 숨김 처리
        if (leftItemPanel != null)
            leftItemPanel.SetActive(false);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerStats == null) return;

        if (skillPointsText != null) skillPointsText.text = "Skill Points: " + playerStats.skillPoints;
        
        if (hpUpgradeText != null) 
            hpUpgradeText.text = $"HP Lv.{playerStats.hpLevel} (Cost: {playerStats.hpCost})";
            
        if (atkUpgradeText != null) 
            atkUpgradeText.text = $"ATK Lv.{playerStats.atkLevel} (Cost: {playerStats.atkCost})";
            
        if (dashUpgradeText != null) 
            dashUpgradeText.text = $"Dash Lv.{playerStats.dashLevel} (Cost: {playerStats.dashCost})";
            
        if (autoShootUpgradeText != null) 
            autoShootUpgradeText.text = $"AutoShoot Lv.{playerStats.autoShootLevel} (Cost: {playerStats.autoShootCost})";

        // 새 UI 빌더 동기화
        SkillUpgradeUIBuilder builder = GetComponent<SkillUpgradeUIBuilder>();
        if (builder != null) builder.RefreshAllUI();
    }


    public void OnUpgradeHP()
    {
        if (playerStats != null && playerStats.TryUpgradeHP()) UpdateUI();
    }

    public void OnUpgradeATK()
    {
        if (playerStats != null && playerStats.TryUpgradeATK()) UpdateUI();
    }

    public void OnUpgradeDash()
    {
        if (playerStats != null && playerStats.TryUpgradeDash()) UpdateUI();
    }

    public void OnUpgradeAutoShoot()
    {
        if (playerStats != null && playerStats.TryUpgradeAutoShoot()) UpdateUI();
    }
}
