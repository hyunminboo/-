using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum PlayerCharacterType
{
    Female,
    Male
}

public class CharacterSelector : MonoBehaviour
{
    [Header("Character Buttons")]
    public Button btnFemale;
    public Button btnMale;

    public PlayerCharacterType currentSelected = PlayerCharacterType.Female;

    public UnityAction<PlayerCharacterType> OnCharacterSelected;

    private void Awake()
    {
        if (btnFemale != null) btnFemale.onClick.AddListener(() => SelectCharacter(PlayerCharacterType.Female));
        if (btnMale != null) btnMale.onClick.AddListener(() => SelectCharacter(PlayerCharacterType.Male));
    }

    private void Start()
    {
        // Default selection
        SelectCharacter(PlayerCharacterType.Female);
    }

    public void SelectCharacter(PlayerCharacterType charType)
    {
        currentSelected = charType;
        
        UpdateButtonVisual(btnFemale, charType == PlayerCharacterType.Female);
        UpdateButtonVisual(btnMale, charType == PlayerCharacterType.Male);

        OnCharacterSelected?.Invoke(charType);
    }

    private void UpdateButtonVisual(Button btn, bool isSelected)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            if (isSelected)
            {
                // 선택 시 약간 밝은 사이언 계열 유리 느낌
                img.color = new Color(0.2f, 0.5f, 0.5f, 0.9f);
            }
            else
            {
                // 선택 안 된 기본 어두운 유리 느낌
                img.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            }
        }
    }
}
