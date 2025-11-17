using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TraitPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform traitsContainer;
    [SerializeField] private Button expandButton;
    [SerializeField] private TextMeshProUGUI expandButtonText;

    [Header("Prefab")]
    [SerializeField] private GameObject traitSlotPrefab;

    [Header("Icons")]
    [SerializeField] private Sprite warriorIcon;
    [SerializeField] private Sprite mageIcon;
    [SerializeField] private Sprite archerIcon;
    [SerializeField] private Sprite humanIcon;
    [SerializeField] private Sprite elfIcon;
    [SerializeField] private Sprite dwarfIcon;

    [Header("Settings")]
    [SerializeField] private int maxVisibleSlots = 8;

    private List<TraitSlotUI> allSlots = new List<TraitSlotUI>();
    private bool isExpanded = false;

    public static TraitPanelUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (TraitManager.Instance != null)
        {
            TraitManager.Instance.OnTraitsChanged += RefreshUI;
        }

        if (expandButton != null)
        {
            expandButton.onClick.AddListener(ToggleExpand);
            expandButton.gameObject.SetActive(false);
        }

        CreateInitialSlots();
        RefreshUI();

        Debug.Log("Trait Panel UI initialized");
    }

    private void OnDestroy()
    {
        if (TraitManager.Instance != null)
        {
            TraitManager.Instance.OnTraitsChanged -= RefreshUI;
        }
    }

    private void CreateInitialSlots()
    {
        // 모든 시너지 슬롯 생성 (숨김 상태)
        CreateSlot("Warrior", warriorIcon);
        CreateSlot("Mage", mageIcon);
        CreateSlot("Archer", archerIcon);
        CreateSlot("Human", humanIcon);
        CreateSlot("Elf", elfIcon);
        CreateSlot("Dwarf", dwarfIcon);
    }

    private void CreateSlot(string traitName, Sprite icon)
    {
        if (traitSlotPrefab == null || traitsContainer == null) return;

        GameObject slotObj = Instantiate(traitSlotPrefab, traitsContainer);
        TraitSlotUI slot = slotObj.GetComponent<TraitSlotUI>();

        if (slot != null)
        {
            allSlots.Add(slot);
            slotObj.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        if (TraitManager.Instance == null) return;

        // 데이터 수집
        List<TraitData> traits = new List<TraitData>();

        // 직업 시너지
        AddTraitData(traits, "Warrior", UnitClass.Warrior, warriorIcon);
        AddTraitData(traits, "Mage", UnitClass.Mage, mageIcon);
        AddTraitData(traits, "Archer", UnitClass.Archer, archerIcon);

        // 종족 시너지
        AddTraitData(traits, "Human", UnitRace.Human, humanIcon);
        AddTraitData(traits, "Elf", UnitRace.Elf, elfIcon);
        AddTraitData(traits, "Dwarf", UnitRace.Dwarf, dwarfIcon);

        // 카운트가 0인 것 제거
        traits = traits.Where(t => t.info.currentCount > 0).ToList();

        // 정렬: currentCount 내림차순
        traits = traits.OrderByDescending(t => t.info.currentCount).ToList();

        // 슬롯 업데이트
        UpdateSlots(traits);

        // 확장 버튼 표시 여부
        UpdateExpandButton(traits.Count);
    }

    private void AddTraitData(List<TraitData> list, string name, UnitClass unitClass, Sprite icon)
    {
        TraitInfo info = TraitManager.Instance.GetClassTrait(unitClass);
        if (info != null)
        {
            list.Add(new TraitData { name = name, info = info, icon = icon });
        }
    }

    private void AddTraitData(List<TraitData> list, string name, UnitRace unitRace, Sprite icon)
    {
        TraitInfo info = TraitManager.Instance.GetRaceTrait(unitRace);
        if (info != null)
        {
            list.Add(new TraitData { name = name, info = info, icon = icon });
        }
    }

    private void UpdateSlots(List<TraitData> traits)
    {
        // 모든 슬롯 숨김
        foreach (var slot in allSlots)
        {
            slot.gameObject.SetActive(false);
        }

        // 필요한 만큼만 표시
        int visibleCount = isExpanded ? traits.Count : Mathf.Min(traits.Count, maxVisibleSlots);

        for (int i = 0; i < visibleCount && i < allSlots.Count; i++)
        {
            if (i < traits.Count)
            {
                allSlots[i].Setup(traits[i].name, traits[i].info, traits[i].icon);
                allSlots[i].gameObject.SetActive(true);
            }
        }
    }

    private void UpdateExpandButton(int totalCount)
    {
        if (expandButton == null) return;

        if (totalCount > maxVisibleSlots)
        {
            expandButton.gameObject.SetActive(true);

            if (expandButtonText != null)
            {
                expandButtonText.text = isExpanded ? "-" : "+";
            }
        }
        else
        {
            expandButton.gameObject.SetActive(false);
        }
    }

    private void ToggleExpand()
    {
        isExpanded = !isExpanded;
        RefreshUI();
    }

    private class TraitData
    {
        public string name;
        public TraitInfo info;
        public Sprite icon;
    }
}