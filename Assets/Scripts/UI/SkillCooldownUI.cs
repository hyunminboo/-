using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public enum SkillType { Dash, AutoShoot, BladeDash, DynamicWeaponSkill }
    public SkillType skillType;
    
    public PlayerMovement playerMovement;
    public PlayerShooting playerShooting;
    public PlayerMelee playerMelee;
    public Image cooldownOverlay; 

    [Header("Dynamic Weapon Icons")]
    public Sprite gunIcon;
    public Sprite swordIcon;

    void Start()
    {
        // 씬 시작 시 자동으로 플레이어 스크립트 탐색
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            if (playerMovement == null) playerMovement = player.GetComponent<PlayerMovement>();
            if (playerShooting == null) playerShooting = player.GetComponent<PlayerShooting>();
            if (playerMelee == null)    playerMelee    = player.GetComponent<PlayerMelee>();
        }
    }

    void Update()
    {
        if (cooldownOverlay == null) return;
        
        float percent = 0f;
        Image mainImage = GetComponent<Image>();

        if (skillType == SkillType.Dash && playerMovement != null)
        {
            percent = playerMovement.GetDashCooldownPercent();
        }
        else if (skillType == SkillType.AutoShoot || skillType == SkillType.BladeDash || skillType == SkillType.DynamicWeaponSkill)
        {
            bool hasGun = (playerShooting != null && playerShooting.canShoot);
            bool hasSword = (playerMelee != null && playerMelee.canAttack);
            
            bool hasWeapon = hasGun || hasSword;

            if (mainImage != null) mainImage.enabled = hasWeapon;
            cooldownOverlay.enabled = hasWeapon;

            if (hasWeapon)
            {
                if (hasGun)
                {
                    percent = playerShooting.GetAutoShootCooldownPercent();
                    if (gunIcon != null && mainImage != null && mainImage.sprite != gunIcon)
                    {
                        mainImage.sprite = gunIcon;
                        cooldownOverlay.sprite = gunIcon;
                    }
                }
                else if (hasSword)
                {
                    percent = playerMelee.GetDashSkillCooldownPercent();
                    if (swordIcon != null && mainImage != null && mainImage.sprite != swordIcon)
                    {
                        mainImage.sprite = swordIcon;
                        cooldownOverlay.sprite = swordIcon;
                    }
                }
            }
        }
        
        cooldownOverlay.fillAmount = percent;
    }
}
