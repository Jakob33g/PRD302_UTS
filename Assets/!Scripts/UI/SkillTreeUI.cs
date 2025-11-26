using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        
        // Ensure component and GameObject are always enabled
        this.enabled = true;
        this.gameObject.SetActive(true);
        
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
            // IMPORTANT: If SkillTreeUI is on the same GameObject as skillTreePanel,
            // we can't disable the panel or the component will be disabled too!
            // Instead, disable all child UI elements but keep the panel active
            if (skillTreePanel == this.gameObject)
            {
                Debug.LogWarning("[SkillTreeUI] WARNING: SkillTreeUI is on the same GameObject as skillTreePanel!");
                Debug.LogWarning("[SkillTreeUI] This will cause problems. Consider moving SkillTreeUI to a different GameObject (like HUD_Canvas).");
                // Don't disable the panel - disable its children instead
                foreach (Transform child in skillTreePanel.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            else
            {
                skillTreePanel.SetActive(false);
            }
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
            // Try multiple ways to find the panel
            skillTreePanel = GameObject.Find("SkillTreePanel");
            
            // If not found by name, try to find it in HUD_Canvas
            if (skillTreePanel == null)
            {
                GameObject hudCanvas = GameObject.Find("HUD_Canvas");
                if (hudCanvas != null)
                {
                    Transform found = hudCanvas.transform.Find("SkillTreePanel");
                    if (found != null)
                    {
                        skillTreePanel = found.gameObject;
                        Debug.Log("[SkillTreeUI] Found SkillTreePanel inside HUD_Canvas!");
                    }
                }
            }
            
            if (skillTreePanel != null)
            {
                Debug.Log($"[SkillTreeUI] Auto-found SkillTreePanel: {skillTreePanel.name}");
            }
            else
            {
                Debug.LogError("[SkillTreeUI] CRITICAL: skillTreePanel is still null! The skill tree UI will not work.");
                Debug.LogError("[SkillTreeUI] Fix: Either create a GameObject named 'SkillTreePanel' or assign it manually in Inspector.");
                return; // Don't continue if panel doesn't exist
            }
        }

        // Make sure panel is disabled at start (but not if it's the same GameObject as this component)
        if (skillTreePanel != null)
        {
            if (skillTreePanel == this.gameObject)
            {
                // If SkillTreeUI is on the panel itself, disable children instead
                // Also disable the Canvas component or Image component to hide the panel visually
                Canvas panelCanvas = skillTreePanel.GetComponent<Canvas>();
                if (panelCanvas != null)
                {
                    panelCanvas.enabled = false;
                    Debug.Log("[SkillTreeUI] Disabled Canvas component on SkillTreePanel");
                }
                
                UnityEngine.UI.Image panelImage = skillTreePanel.GetComponent<UnityEngine.UI.Image>();
                if (panelImage != null)
                {
                    panelImage.enabled = false;
                }
                
                // Disable all children
                foreach (Transform child in skillTreePanel.transform)
                {
                    child.gameObject.SetActive(false);
                }
                Debug.Log("[SkillTreeUI] Disabled all children of SkillTreePanel (component is on panel itself)");
            }
            else
            {
                skillTreePanel.SetActive(false);
            }
            
            // Also make sure its Canvas parent is active
            Canvas canvas = skillTreePanel.GetComponentInParent<Canvas>();
            if (canvas != null && !canvas.gameObject.activeSelf)
            {
                Debug.LogWarning($"[SkillTreeUI] Canvas '{canvas.gameObject.name}' is disabled at start. Enabling it...");
                canvas.gameObject.SetActive(true);
            }
        }

        BuildSkillUI();
        RefreshAll();
        
        // Ensure component is always enabled
        this.enabled = true;
        this.gameObject.SetActive(true);
        
        Debug.Log($"[SkillTreeUI] Start() complete - Component enabled: {this.enabled}, GameObject active: {this.gameObject.activeInHierarchy}");
        Debug.Log($"[SkillTreeUI] Ready to receive Tab/M/K key presses!");
        Debug.Log($"[SkillTreeUI] Initialization complete. Panel: {(skillTreePanel != null ? skillTreePanel.name : "NULL")}");
    }

    void Update()
    {
        // Check if component is enabled and GameObject is active
        if (!this.enabled || !this.gameObject.activeInHierarchy)
        {
            if (updateCounter % 300 == 0) // Log every 5 seconds if disabled
            {
                Debug.LogWarning($"[SkillTreeUI] Component disabled or GameObject inactive! Enabled: {this.enabled}, Active: {this.gameObject.activeInHierarchy}");
            }
            updateCounter++;
            return;
        }

        // This is just for debugging - it prints messages to check if the script is working
        updateCounter++;
        if (updateCounter % 60 == 0)
        {
            lastUpdateTime = Time.time;
            Debug.Log($"[SkillTreeUI] Update() is running! Frame {updateCounter}, Time: {Time.time:F2}");
            
            // Also check panel status every 60 frames
            if (skillTreePanel != null)
            {
                bool panelActive = skillTreePanel.activeInHierarchy;
                bool componentOnPanel = (skillTreePanel == this.gameObject);
                Debug.Log($"[SkillTreeUI] Panel status - Active: {panelActive}, ActiveSelf: {skillTreePanel.activeSelf}, ComponentOnPanel: {componentOnPanel}");
            }
        }
        
        // Check if keys are pressed to open skill tree
        // Always allow skill tree to open, regardless of time of day or other conditions
        // Check Tab key first and more frequently
        bool tabKey = IsTabKeyDown();
        bool mKey = IsMKeyDown();
        bool kKey = IsKKeyDown();
        
        if (tabKey || mKey || kKey)
        {
            string keyPressed = tabKey ? "Tab" : (mKey ? "M" : "K");
            Debug.Log($"[SkillTreeUI] ===== {keyPressed} KEY PRESSED! =====");
            Debug.Log($"[SkillTreeUI] Before toggle - Panel: {(skillTreePanel != null ? skillTreePanel.name : "NULL")}, IsOpen: {isOpen}");
            Debug.Log($"[SkillTreeUI] Component enabled: {this.enabled}, GameObject active: {this.gameObject.activeInHierarchy}");
            
            ToggleSkillTree();
            
            Debug.Log($"[SkillTreeUI] After toggle - Panel active: {(skillTreePanel != null ? skillTreePanel.activeInHierarchy.ToString() : "NULL")}, IsOpen: {isOpen}");
            Debug.Log($"[SkillTreeUI] ========================================");
        }
    }
    
    // Check if M key is pressed (works with both old and new input systems)
    // Always works, even during night or when UI is focused
    bool IsMKeyDown()
    {
        #if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.M);
        #endif
    }
    
    // Check if K key is pressed (works with both old and new input systems)
    // Always works, even during night or when UI is focused
    bool IsKKeyDown()
    {
        #if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.K);
        #endif
    }
    
    // Check if Tab key is pressed (works with both old and new input systems)
    bool IsTabKeyDown()
    {
        // Always check old input system first (most reliable)
        bool oldInput = Input.GetKeyDown(KeyCode.Tab);
        
        #if ENABLE_INPUT_SYSTEM
        bool newInput = false;
        if (Keyboard.current != null)
        {
            newInput = Keyboard.current.tabKey.wasPressedThisFrame;
        }
        // Use either input system - whichever works
        bool result = oldInput || newInput;
        if (result)
        {
            Debug.Log($"[SkillTreeUI] Tab detected! OldInput: {oldInput}, NewInput: {newInput}, Keyboard.current: {(Keyboard.current != null ? "exists" : "null")}");
        }
        return result;
        #else
        // Old input system only
        if (oldInput)
        {
            Debug.Log("[SkillTreeUI] Tab detected via old input system!");
        }
        return oldInput;
        #endif
    }
    
    // Test button - right-click this component in Inspector and click "Test Toggle Skill Tree"
    [ContextMenu("Test Toggle Skill Tree")]
    public void TestToggle()
    {
        Debug.Log("[SkillTreeUI] TestToggle() called from Inspector!");
        ToggleSkillTree();
    }
    
    // Public method to open skill tree (can be called from anywhere)
    public void OpenSkillTree()
    {
        if (!isOpen)
        {
            Debug.Log("[SkillTreeUI] OpenSkillTree() called!");
            ToggleSkillTree();
        }
    }
    
    // Public method to close skill tree (can be called from anywhere)
    public void CloseSkillTree()
    {
        if (isOpen)
        {
            Debug.Log("[SkillTreeUI] CloseSkillTree() called!");
            ToggleSkillTree();
        }
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
        // Try to find the panel if it's still null
        if (skillTreePanel == null)
        {
            Debug.LogWarning("[SkillTreeUI] Panel is null, trying to find it...");
            skillTreePanel = GameObject.Find("SkillTreePanel");
            if (skillTreePanel == null)
            {
                Debug.LogError("[SkillTreeUI] Cannot toggle - skillTreePanel is not found! Please assign it in the Inspector or create a GameObject named 'SkillTreePanel'.");
                return;
            }
            Debug.Log($"[SkillTreeUI] Found panel: {skillTreePanel.name}");
        }

        // Make sure the panel's parent Canvas is enabled
        Canvas parentCanvas = skillTreePanel.GetComponentInParent<Canvas>();
        if (parentCanvas != null && !parentCanvas.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[SkillTreeUI] Canvas '{parentCanvas.gameObject.name}' is disabled! Enabling it...");
            parentCanvas.gameObject.SetActive(true);
        }

        // Toggle the panel
        isOpen = !isOpen;
        
        // Special handling if SkillTreeUI is on the same GameObject as the panel
        if (skillTreePanel == this.gameObject)
        {
            // Enable/disable Canvas and Image components to show/hide the panel
            Canvas panelCanvas = skillTreePanel.GetComponent<Canvas>();
            if (panelCanvas != null)
            {
                panelCanvas.enabled = isOpen;
            }
            
            UnityEngine.UI.Image panelImage = skillTreePanel.GetComponent<UnityEngine.UI.Image>();
            if (panelImage != null)
            {
                panelImage.enabled = isOpen;
            }
            
            // Enable/disable children
            foreach (Transform child in skillTreePanel.transform)
            {
                child.gameObject.SetActive(isOpen);
            }
            Debug.Log($"[SkillTreeUI] Toggled panel visibility (component is on panel itself) - isOpen: {isOpen}");
        }
        else
        {
            skillTreePanel.SetActive(isOpen);
        }
        
        // Force refresh the layout in case it's needed
        if (isOpen)
        {
            // Make sure all parent objects are active
            Transform parent = skillTreePanel.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[SkillTreeUI] Parent '{parent.name}' was disabled! Enabling it...");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }
            
            RefreshAll();
        }
        
        Debug.Log($"[SkillTreeUI] Skill tree panel {(isOpen ? "OPENED" : "CLOSED")} - Panel active: {skillTreePanel.activeInHierarchy}, Canvas active: {(parentCanvas != null ? parentCanvas.gameObject.activeInHierarchy.ToString() : "N/A")}");
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

