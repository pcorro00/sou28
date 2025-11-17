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

    private List<TraitSlotUI> slotPool = new List<TraitSlotUI>();
    private bool isExpanded = false;
    private bool initialized = false;

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
        }

        InitializeSlotPool();
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

    private void InitializeSlotPool()
    {
        Debug.Log($"InitializeSlotPool 호출! initialized={initialized}, slotPool.Count={slotPool.Count}");

        if (initialized)
        {
            Debug.Log("이미 초기화됨, 리턴");
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            CreateSlot();
        }

        initialized = true;
        Debug.Log($"슬롯풀 생성 완료: {slotPool.Count}개");
    }

    private void CreateSlot()
    {
        if (traitSlotPrefab == null || traitsContainer == null)
        {
            Debug.LogError("Prefab or Container is null!");
            return;
        }

        GameObject slotObj = Instantiate(traitSlotPrefab, traitsContainer);
        TraitSlotUI slot = slotObj.GetComponent<TraitSlotUI>();

        if (slot != null)
        {
            slotPool.Add(slot);
            slotObj.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        Debug.Log("=== RefreshUI 호출 ===");

        if (TraitManager.Instance == null)
        {
            Debug.LogError("TraitManager.Instance is NULL!");
            return;
        }

        // 모든 슬롯 숨김
        foreach (var slot in slotPool)
        {
            slot.gameObject.SetActive(false);
        }

        // 시너지 데이터 수집
        List<TraitDisplayData> traits = CollectTraitData();
        Debug.Log($"수집된 시너지 개수: {traits.Count}");

        // 각 시너지 출력
        foreach (var t in traits)
        {
            Debug.Log($"시너지: {t.name}, 카운트: {t.count}, 아이콘: {(t.icon != null ? "있음" : "없음")}");
        }

        // 카운트 0인 것 제거
        traits = traits.Where(t => t.count > 0).ToList();
        Debug.Log($"카운트 > 0인 시너지: {traits.Count}개");

        // 정렬
        traits = traits.OrderByDescending(t => t.count).ToList();

        // 슬롯 업데이트
        int visibleCount = isExpanded ? traits.Count : Mathf.Min(traits.Count, maxVisibleSlots);
        Debug.Log($"표시할 슬롯 개수: {visibleCount}, 슬롯풀 크기: {slotPool.Count}");

        for (int i = 0; i < visibleCount && i < slotPool.Count; i++)
        {
            if (i < traits.Count)
            {
                Debug.Log($"슬롯 {i} 설정: {traits[i].name}");
                slotPool[i].Setup(traits[i].name, traits[i].info, traits[i].icon);
                slotPool[i].gameObject.SetActive(true);
            }
        }

        // 확장 버튼
        if (expandButton != null)
        {
            expandButton.gameObject.SetActive(traits.Count > maxVisibleSlots);
        }
    }

    private List<TraitDisplayData> CollectTraitData()
    {
        List<TraitDisplayData> traits = new List<TraitDisplayData>();

        // 직업 시너지
        AddIfExists(traits, "전사", TraitManager.Instance.GetClassTrait(UnitClass.Warrior), warriorIcon);
        AddIfExists(traits, "마법사", TraitManager.Instance.GetClassTrait(UnitClass.Mage), mageIcon);
        AddIfExists(traits, "궁수", TraitManager.Instance.GetClassTrait(UnitClass.Archer), archerIcon);

        // 종족 시너지
        AddIfExists(traits, "인간", TraitManager.Instance.GetRaceTrait(UnitRace.Human), humanIcon);
        AddIfExists(traits, "엘프", TraitManager.Instance.GetRaceTrait(UnitRace.Elf), elfIcon);
        AddIfExists(traits, "드워프", TraitManager.Instance.GetRaceTrait(UnitRace.Dwarf), dwarfIcon);

        return traits;
    }

    private void AddIfExists(List<TraitDisplayData> list, string name, TraitInfo info, Sprite icon)
    {
        if (info != null)
        {
            list.Add(new TraitDisplayData
            {
                name = name,
                info = info,
                icon = icon,
                count = info.currentCount
            });
        }
    }

    private void ToggleExpand()
    {
        isExpanded = !isExpanded;
        RefreshUI();
    }

    private class TraitDisplayData
    {
        public string name;
        public TraitInfo info;
        public Sprite icon;
        public int count;
    }
}