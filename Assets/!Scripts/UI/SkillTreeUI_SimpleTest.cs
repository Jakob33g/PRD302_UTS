using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// SIMPLE TEST SCRIPT - Add this to ANY active GameObject to test if input is working
// This will help us figure out if the problem is with input detection or the SkillTreeUI component
public class SkillTreeUI_SimpleTest : MonoBehaviour
{
    void Update()
    {
        // Check if any key is pressed (very simple test)
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                Debug.Log("[SIMPLE TEST] TAB KEY PRESSED!");
            if (Keyboard.current.mKey.wasPressedThisFrame)
                Debug.Log("[SIMPLE TEST] M KEY PRESSED!");
            if (Keyboard.current.kKey.wasPressedThisFrame)
                Debug.Log("[SIMPLE TEST] K KEY PRESSED!");
        }
        #else
        if (Input.GetKeyDown(KeyCode.Tab))
            Debug.Log("[SIMPLE TEST] TAB KEY PRESSED!");
        if (Input.GetKeyDown(KeyCode.M))
            Debug.Log("[SIMPLE TEST] M KEY PRESSED!");
        if (Input.GetKeyDown(KeyCode.K))
            Debug.Log("[SIMPLE TEST] K KEY PRESSED!");
        #endif
        
        // Also check if SkillTreeUI exists in the scene
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkillTreeUI ui = FindAnyObjectByType<SkillTreeUI>();
            if (ui == null)
            {
                Debug.LogError("[SIMPLE TEST] ❌ SkillTreeUI component NOT FOUND in scene!");
                Debug.LogError("[SIMPLE TEST] You need to add SkillTreeUI component to a GameObject.");
            }
            else
            {
                Debug.Log($"[SIMPLE TEST] ✅ Found SkillTreeUI on GameObject: {ui.gameObject.name}");
                Debug.Log($"[SIMPLE TEST] Component enabled: {ui.enabled}");
                Debug.Log($"[SIMPLE TEST] GameObject active: {ui.gameObject.activeInHierarchy}");
            }
        }
    }
}

