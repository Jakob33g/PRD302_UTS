# Skill Tree Setup Instructions - For Beginners

## Step 1: Check if SkillTreeUI Component Exists

### Quick Test:
1. **In Unity**, press **SPACEBAR** while playing the game
2. **Check the Console** (bottom panel) - it should tell you if SkillTreeUI exists

If you see "❌ SkillTreeUI component NOT FOUND", follow Step 2.

---

## Step 2: Add SkillTreeUI Component to a GameObject

### Option A: Add to HUD_Canvas (Recommended)

1. **In Unity Hierarchy** (left panel), find **HUD_Canvas**
   - If you don't see it, look for any **Canvas** GameObject
   
2. **Click on HUD_Canvas** to select it

3. **In Inspector** (right panel), click "**Add Component**" button at the bottom

4. **Type "SkillTreeUI"** in the search box

5. **Click on SkillTreeUI** to add it

6. ✅ Done! The component is now added

---

### Option B: Create a New GameObject (If HUD_Canvas doesn't exist)

1. **Right-click in Hierarchy** panel
2. Select **Create Empty**
3. **Name it "SkillTreeManager"**
4. **In Inspector**, click "**Add Component**"
5. **Type "SkillTreeUI"** and add it

---

## Step 3: Assign References in Inspector

After adding the SkillTreeUI component, you'll see many empty fields in the Inspector. Here's what to do:

### Automatic Finding (Easiest):
Most things are found automatically! But you need to assign the **SkillTreePanel**:

1. **In Hierarchy**, find **HUD_Canvas** → **SkillTreePanel**
   - If it doesn't exist, see Step 4
   
2. **Drag SkillTreePanel** from Hierarchy into the **"Skill Tree Panel"** field in Inspector

3. ✅ That's it! The rest finds itself automatically.

---

## Step 4: Create SkillTreePanel (If it doesn't exist)

If there's no "SkillTreePanel" in your scene:

1. **In Hierarchy**, find **HUD_Canvas** (or any Canvas)

2. **Right-click** on **HUD_Canvas** → **UI** → **Panel**
   - This creates a new Panel

3. **Rename it** to "**SkillTreePanel**" (right-click → Rename, or press F2)

4. **Make sure it's a child of HUD_Canvas**

5. ✅ Done! Now go back to Step 3 to assign it.

---

## Step 5: Test It!

1. **Press Play** button (top center)

2. **Press SPACEBAR** - Check Console:
   - Should say "✅ Found SkillTreeUI" or "❌ NOT FOUND"
   
3. **Press Tab, M, or K** - Should see logs in Console:
   - "[SkillTreeUI] Tab KEY PRESSED!"
   - "[SkillTreeUI] Panel OPENED" or "CLOSED"

---

## Step 6: Troubleshooting

### No logs appear at all?
- ✅ Check: Is the GameObject with SkillTreeUI **ACTIVE**? (checkbox at top of Inspector)
- ✅ Check: Is the SkillTreeUI component **ENABLED**? (checkbox next to component name)
- ✅ Check: Are there **any error messages** in Console (red text)?

### "SkillTreePanel is NULL" error?
- ✅ Make sure you dragged the panel into the Inspector field (Step 3)
- ✅ Or make sure the panel is named exactly "SkillTreePanel" (Step 4)

### Panel opens but you can't see it?
- ✅ Check if **HUD_Canvas** is active (checkbox in Inspector)
- ✅ Check if **Canvas** component is enabled on HUD_Canvas
- ✅ Try making the panel bigger: Select SkillTreePanel → Inspector → Rect Transform → Set Width/Height to 800x600

---

## Visual Guide (Where to Click)

```
Unity Editor Layout:

[Hierarchy Panel]          [Inspector Panel]
├─ HUD_Canvas              [SkillTreeUI Component]
│  ├─ SkillTreePanel  ←    ├─ Links:
│  │  └─ ...              │  ├─ Skill Tree: (auto)
│  └─ ...                  │  └─ Player XP: (auto)
│                          │
│                          ├─ UI References:
│                          │  ├─ Skill Tree Panel: [Drag here] ←
│                          │  └─ ...
│                          │
│                          Component Enabled: ☑ (must be checked!)
│                          GameObject Active: ☑ (must be checked!)
```

---

## Quick Checklist

- [ ] SkillTreeUI component added to a GameObject
- [ ] GameObject with SkillTreeUI is ACTIVE (checkbox checked)
- [ ] SkillTreeUI component is ENABLED (checkbox checked)
- [ ] SkillTreePanel exists in Hierarchy
- [ ] SkillTreePanel assigned in Inspector
- [ ] No red errors in Console
- [ ] Press Play and test with Tab/M/K keys

---

## Still Not Working?

1. Add the **SkillTreeUI_SimpleTest** script to any active GameObject
2. This will test if input detection works at all
3. Press Tab/M/K and check Console for "[SIMPLE TEST]" messages
4. If those appear but SkillTreeUI doesn't, the component isn't set up correctly

