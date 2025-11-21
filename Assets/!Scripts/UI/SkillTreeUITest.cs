using UnityEngine;

/// <summary>
/// Test script to check if SkillTreeUI is working
/// Add this to any GameObject in your scene to test if the skill tree opens
/// </summary>
public class SkillTreeUITest : MonoBehaviour
{
    void Update()
    {
        // Look for SkillTreeUI component when you press M, K, or Tab
        SkillTreeUI ui = FindAnyObjectByType<SkillTreeUI>();
        
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (ui == null)
            {
                Debug.LogError("[SkillTreeUITest] SkillTreeUI component not found in scene!");
                Debug.LogError("[SkillTreeUITest] Make sure you have added SkillTreeUI component to a GameObject.");
                return;
            }
            
            if (!ui.enabled)
            {
                Debug.LogError("[SkillTreeUITest] SkillTreeUI component is disabled!");
                return;
            }
            
            if (!ui.gameObject.activeInHierarchy)
            {
                Debug.LogError($"[SkillTreeUITest] GameObject '{ui.gameObject.name}' with SkillTreeUI is inactive!");
                return;
            }
            
            Debug.Log($"[SkillTreeUITest] Found SkillTreeUI on GameObject: {ui.gameObject.name}");
            Debug.Log($"[SkillTreeUITest] Trying to open skill tree...");
            
            // Try to open the skill tree
            ui.ToggleSkillTree();
        }
    }
}

