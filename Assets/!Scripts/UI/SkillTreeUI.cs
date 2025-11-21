using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Links")]
    public SkillTree skillTree;
    public PlayerXP playerXP;

    [Header("UI References - Drag These from Your Scene")]
    public GameObject skillTreePanel;
    public Transform skillContainer;  // The container where skill buttons will appear
    public GameObject skillButtonPrefab;  // The button prefab that shows each skill
    public Button closeButton;
    public TextMeshProUGUI skillPointsText;

    [Header("Colors for Different Skill States")]
    public Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);  // Green for unlocked skills
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f);    // Gray for locked skills
    public Color canUnlockColor = new Color(1f, 0.8f, 0.2f);   // Yellow for skills you can unlock
    public Color maxedColor = new Color(0.8f, 0.2f, 0.8f);     // Purple for maxed skills

    [Header("Tooltip (Optional - Leave Empty if You Don't Want Tooltips)")]
    public GameObject tooltipPrefab;  // Prefab for showing skill info when you hover
    public Canvas tooltipCanvas;      // Which canvas to show tooltips on

    // Keeps track of all the skill buttons
    private Dictionary<SkillSO, SkillButtonUI> skillButtons = new Dictionary<SkillSO, SkillButtonUI>();
    private bool isOpen = false;
    
    // These are just for debugging - checking if the script is working
    private int updateCounter = 0;
    private float lastUpdateTime = 0f;

    void Awake()
    {
        Debug.Log($"[SkillTreeUI] Awake() called on GameObject: {gameObject.name}");
        Debug.Log($"[SkillTreeUI] Component enabled: {enabled}, GameObject active: {gameObject.activeInHierarchy}");
        
        if (!skillTree)
            skillTree = FindAnyObjectByType<SkillTree>();
        if (!playerXP)
            playerXP = FindAnyObjectByType<PlayerXP>();

        // Debug missing references
        if (!skillTree)
            Debug.LogWarning("[SkillTreeUI] SkillTree not found! Make sure Player has SkillTree component.");
        else
            Debug.Log($"[SkillTreeUI] SkillTree found: {skillTree.gameObject.name}");
            
        if (!playerXP)
            Debug.LogWarning("[SkillTreeUI] PlayerXP not found! Make sure Player has PlayerXP component.");
        else
            Debug.Log($"[SkillTreeUI] PlayerXP found: {playerXP.gameObject.name}");
            
        if (!skillTreePanel)
            Debug.LogWarning("[SkillTreeUI] SkillTreePanel not assigned! Please assign the panel GameObject in Inspector.");

        if (skillTreePanel)
        {
            skillTreePanel.SetActive(false);
            Debug.Log("[SkillTreeUI] SkillTreePanel initialized and disabled");
        }
        else
        {
            Debug.LogError("[SkillTreeUI] SkillTreePanel is NULL! The skill tree will not work. Please assign it in the Inspector.");
        }

        if (closeButton)
            closeButton.onClick.AddListener(CloseSkillTree);
        else
            Debug.LogWarning("[SkillTreeUI] CloseButton not assigned (optional)");
    }

    void Start()
    {
        Debug.Log("[SkillTreeUI] Start() called");
        
        // Try to find the panel automatically if you didn't assign it
        if (skillTreePanel == null)
        {
            GameObject found = GameObject.Find("SkillTreePanel");
            if (found != null)
            {
                skillTreePanel = found;
                Debug.Log("[SkillTreeUI] Auto-found SkillTreePanel in scene!");
            }
            else
            {
                Debug.LogError("[SkillTreeUI] CRITICAL: skillTreePanel is still null! The skill tree UI will not work.");
                Debug.LogError("[SkillTreeUI] Fix: Either create a GameObject named 'SkillTreePanel' or assign it manually in Inspector.");
                return; // Don't continue if panel doesn't exist
            }
        }

        // Make sure panel is disabled at start
        if (skillTreePanel != null)
        {
            skillTreePanel.SetActive(false);
        }

        BuildSkillUI();
        RefreshAll();
        
        Debug.Log($"[SkillTreeUI] Initialization complete. Panel: {(skillTreePanel != null ? skillTreePanel.name : "NULL")}");
    }

    void Update()
    {
        // This is just for debugging - it prints messages to check if the script is working
        updateCounter++;
        if (updateCounter % 60 == 0)
        {
            lastUpdateTime = Time.time;
            Debug.Log($"[SkillTreeUI] Update() is running! Frame {updateCounter}, Time: {Time.time:F2}");
        }
        
        // Check if any key is pressed (for debugging)
        if (Input.anyKeyDown)
        {
            Debug.Log($"[SkillTreeUI] Key detected! Last pressed key...");
        }
        
        // Open or close skill tree when you press M, K, or Tab
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("[SkillTreeUI] M KEY PRESSED!");
            ToggleSkillTree();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[SkillTreeUI] K KEY PRESSED!");
            ToggleSkillTree();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("[SkillTreeUI] TAB KEY PRESSED!");
            ToggleSkillTree();
        }
    }
    
    // Test button - right-click this component in Inspector and click "Test Toggle Skill Tree"
    [ContextMenu("Test Toggle Skill Tree")]
    public void TestToggle()
    {
        Debug.Log("[SkillTreeUI] TestToggle() called from Inspector!");
        ToggleSkillTree();
    }

    void OnEnable()
    {
        // Listen for when skills change so we can update the UI
        if (skillTree)
        {
            skillTree.onSkillsChanged += RefreshAll;
            skillTree.onSkillUnlocked += OnSkillUnlocked;
        }
        if (playerXP)
        {
            playerXP.onXPChanged += OnXPChanged;
        }
    }

    void OnDisable()
    {
        // Stop listening when this component is disabled
        if (skillTree)
        {
            skillTree.onSkillsChanged -= RefreshAll;
            skillTree.onSkillUnlocked -= OnSkillUnlocked;
        }
        if (playerXP)
        {
            playerXP.onXPChanged -= OnXPChanged;
        }
    }

    public void ToggleSkillTree()
    {
        if (skillTreePanel == null)
        {
            Debug.LogWarning("[SkillTreeUI] Cannot toggle - skillTreePanel is not assigned! Please assign it in the Inspector.");
            return;
        }

        isOpen = !isOpen;
        skillTreePanel.SetActive(isOpen);
        
        Debug.Log($"[SkillTreeUI] Skill tree panel {(isOpen ? "OPENED" : "CLOSED")}");
        
        if (isOpen)
        {
            RefreshAll();
        }
    }

    void CloseSkillTree()
    {
        if (skillTreePanel)
        {
            isOpen = false;
            skillTreePanel.SetActive(false);
        }
    }

    void BuildSkillUI()
    {
        // Check if we have everything we need
        if (skillTree == null || skillContainer == null || skillButtonPrefab == null)
        {
            Debug.LogWarning("SkillTreeUI: Missing references. Assign SkillTree, SkillContainer, and SkillButtonPrefab.");
            return;
        }

        // Remove old buttons if they exist
        foreach (Transform child in skillContainer)
            Destroy(child.gameObject);
        skillButtons.Clear();

        // Make a button for each skill in the skill tree
        if (skillTree.allSkills != null)
        {
            foreach (var skill in skillTree.allSkills)
            {
                if (skill == null) continue;

                var go = Instantiate(skillButtonPrefab, skillContainer);
                var btn = go.GetComponent<SkillButtonUI>();
                if (btn == null)
                    btn = go.AddComponent<SkillButtonUI>();

                btn.Init(skill, this);
                skillButtons[skill] = btn;
            }
        }
    }

    void RefreshAll()
    {
        // Update everything on screen
        RefreshSkillPoints();
        RefreshSkillButtons();
    }

    void RefreshSkillPoints()
    {
        // Update the text showing how many skill points you have
        if (skillPointsText && playerXP != null)
        {
            skillPointsText.text = $"Skill Points: {playerXP.unspentSkillPoints}";
        }
    }

    void OnXPChanged(int current, int toNext, int level)
    {
        // Update skill points display when XP changes
        RefreshSkillPoints();
        if (isOpen)
            RefreshSkillButtons();  // Also update which skills you can unlock
    }

    void RefreshSkillButtons()
    {
        // Update all skill buttons to show current state
        foreach (var kvp in skillButtons)
        {
            var skill = kvp.Key;
            var button = kvp.Value;
            if (button != null)
                button.Refresh(skillTree, playerXP);
        }
    }

    void OnSkillUnlocked(SkillSO skill, int rank)
    {
        RefreshAll();
    }


    public void OnSkillButtonClicked(SkillSO skill)
    {
        if (skillTree == null || skill == null) return;

        // Try to unlock the skill when clicked
        if (skillTree.TryUnlockSkill(skill))
        {
            // It worked! The UI will update automatically
        }
        else
        {
            // It didn't work - figure out why and tell the player
            string reason = "";
            if (playerXP == null || playerXP.unspentSkillPoints < skill.skillPointCost)
                reason = "Not enough skill points!";
            else if (playerXP.level < skill.requiredLevel)
                reason = $"Requires level {skill.requiredLevel}!";
            else if (skillTree.GetSkillRank(skill) >= skill.maxRank)
                reason = "Skill already maxed!";
            else
                reason = "Prerequisites not met!";

            Debug.Log($"[SkillTree] Cannot unlock {skill.skillName}: {reason}");
        }
    }
}

// This handles each individual skill button on the screen
public class SkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SkillSO skill;
    private SkillTreeUI parent;
    private Button button;
    private Image iconImage;
    private Image backgroundImage;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI rankText;
    private TextMeshProUGUI costText;
    private GameObject tooltip;
    private RectTransform tooltipRect;

    public void Init(SkillSO s, SkillTreeUI p)
    {
        skill = s;
        parent = p;

        // Find the button and UI parts (icon, text, etc.)
        button = GetComponent<Button>();
        if (button == null) button = gameObject.AddComponent<Button>();

        iconImage = transform.Find("Icon")?.GetComponent<Image>();
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null) backgroundImage = gameObject.AddComponent<Image>();

        nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        rankText = transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        costText = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();

        if (button)
            button.onClick.AddListener(() => parent?.OnSkillButtonClicked(skill));

        Refresh(parent?.skillTree, parent?.playerXP);
    }

    public void Refresh(SkillTree skillTree, PlayerXP playerXP)
    {
        if (skill == null) return;

        // Update icon
        if (iconImage && skill.icon)
            iconImage.sprite = skill.icon;

        // Update name
        if (nameText)
            nameText.text = skill.skillName;

        // Update rank
        int rank = skillTree != null ? skillTree.GetSkillRank(skill) : 0;
        if (rankText)
        {
            if (skill.maxRank > 1)
                rankText.text = $"Rank {rank}/{skill.maxRank}";
            else
                rankText.text = rank > 0 ? "Unlocked" : "Locked";
        }

        // Update cost
        if (costText)
            costText.text = $"{skill.skillPointCost} SP";

        // Change the button color based on skill state
        if (backgroundImage)
        {
            if (rank >= skill.maxRank)
                backgroundImage.color = parent.maxedColor;  // Purple if fully upgraded
            else if (skillTree != null && skillTree.CanUnlockSkill(skill))
                backgroundImage.color = parent.canUnlockColor;  // Yellow if you can unlock it
            else if (rank > 0)
                backgroundImage.color = parent.unlockedColor;  // Green if already unlocked
            else
                backgroundImage.color = parent.lockedColor;  // Gray if locked
        }

        // Make button clickable only if you can unlock the skill
        if (button)
        {
            bool canUnlock = skillTree != null && skillTree.CanUnlockSkill(skill);
            button.interactable = canUnlock;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skill == null || parent == null) return;
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    void ShowTooltip()
    {
        // Don't show tooltip if skill has no description
        if (skill == null || string.IsNullOrEmpty(skill.description)) return;

        // Create the tooltip box if it doesn't exist yet
        if (tooltip == null && parent.tooltipPrefab != null)
        {
            Canvas canvas = parent.tooltipCanvas != null ? parent.tooltipCanvas : GetComponentInParent<Canvas>();
            if (canvas == null) return;

            tooltip = Instantiate(parent.tooltipPrefab, canvas.transform);
            tooltipRect = tooltip.GetComponent<RectTransform>();
            if (tooltipRect == null) tooltipRect = tooltip.AddComponent<RectTransform>();
        }

        if (tooltip == null) return;

        // Fill the tooltip with skill information
        var tooltipText = tooltip.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltipText != null)
        {
            string tooltipContent = $"{skill.skillName}\n\n{skill.description}\n\n";
            
            // Show what bonuses this skill gives
            if (skill.flatBonus != 0)
                tooltipContent += $"Flat Bonus: +{skill.flatBonus}\n";
            if (skill.percentBonus != 0)
                tooltipContent += $"Percent Bonus: +{skill.percentBonus * 100:F0}%\n";
            
            tooltipContent += $"\nRequired Level: {skill.requiredLevel}\n";
            tooltipContent += $"Cost: {skill.skillPointCost} SP";
            
            // Show what skills you need to unlock first
            if (skill.prerequisites != null && skill.prerequisites.Length > 0)
            {
                tooltipContent += "\n\nPrerequisites:";
                foreach (var prereq in skill.prerequisites)
                {
                    if (prereq != null)
                        tooltipContent += $"\n- {prereq.skillName}";
                }
            }

            tooltipText.text = tooltipContent;
        }

        tooltip.SetActive(true);
        UpdateTooltipPosition();
    }

    void UpdateTooltipPosition()
    {
        if (tooltip == null || tooltipRect == null) return;

        // Make the tooltip follow your mouse cursor
        Vector2 mousePos = Input.mousePosition;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            tooltipRect.position = mousePos + new Vector2(10, -10);
        }
    }

    void HideTooltip()
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }

    void Update()
    {
        // Keep the tooltip following your mouse while it's visible
        if (tooltip != null && tooltip.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }

    void OnDestroy()
    {
        // Clean up tooltip when button is destroyed
        if (tooltip != null)
            Destroy(tooltip);
    }
}

