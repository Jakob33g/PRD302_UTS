# How to Add Skills to Your Skill Tree - Step by Step Guide

## Quick Overview
Skills are **ScriptableObject** assets that you create in Unity, then drag into the **SkillTree** component's "All Skills" list.

---

## Step 1: Create Skill Assets

### Method 1: Right-Click Menu (Easiest)

1. **In Project window** (bottom panel), navigate to `Assets/Data/` folder
   - If the folder doesn't exist, create it: Right-click in Project → Create → Folder → Name it "Data"

2. **Right-click** in the `Data` folder (or anywhere in Project window)

3. Select **Create** → **Game** → **Skill**
   - This creates a new skill asset

4. **Name it** something like "Skill_Health1" or "Skill_Speed1"

5. ✅ Repeat for each skill you want!

---

## Step 2: Configure Each Skill

After creating a skill asset, **click on it** in Project window. In the **Inspector**, you'll see:

### Basic Info:
- **Skill Name**: What the skill is called (e.g., "Health Boost")
- **Description**: What it does (e.g., "Increases your maximum health by 50")
- **Icon**: (Optional) Drag a sprite/image here for the skill icon

### What This Skill Does:
- **Skill Type**: Choose what stat it affects:
  - `Health` - Increases max health
  - `MoveSpeed` - Makes you move faster
  - `Defense` - Reduces damage taken
  - `XPBonus` - Get more XP from kills
  - `TowerDamage` - Towers do more damage
  - `TowerRange` - Towers shoot farther
  - `TowerFireRate` - Towers shoot faster

### How Much It Boosts Stats:
- **Flat Bonus**: Fixed number (e.g., `50` = +50 health)
- **Percent Bonus**: Percentage (e.g., `0.25` = +25%, `0.5` = +50%)

**Example:**
- Flat Bonus: `50`, Percent Bonus: `0` = +50 health
- Flat Bonus: `0`, Percent Bonus: `0.25` = +25% health
- Flat Bonus: `50`, Percent Bonus: `0.1` = +50 health AND +10% health

### What You Need to Unlock:
- **Required Level**: Player must be this level or higher (e.g., `1` = available from start)
- **Prerequisites**: Drag other skill assets here that must be unlocked first

### How Much It Costs:
- **Skill Point Cost**: How many skill points needed (usually `1`)

### How Many Times You Can Upgrade:
- **Max Rank**: How many times you can buy this skill
  - `1` = Can only buy once
  - `3` = Can upgrade 3 times (each time costs skill points)

---

## Step 3: Add Skills to SkillTree Component

1. **In Hierarchy**, select the **Player** GameObject

2. **In Inspector**, find the **SkillTree** component

3. Find **"All Skills"** section (it's an array/list)

4. **Click the + button** to add slots (or set "Size" to how many skills you have)

5. **Drag each skill asset** from Project window into the slots

6. ✅ Done! Skills will now appear in the skill tree UI!

---

## Example Skills to Create

Here are some good starter skills:

### 1. Health Boost (Tier 1)
- **Name**: "Health Boost"
- **Description**: "Increases your maximum health by 50"
- **Skill Type**: `Health`
- **Flat Bonus**: `50`
- **Percent Bonus**: `0`
- **Required Level**: `1`
- **Cost**: `1` skill point
- **Max Rank**: `3` (can upgrade 3 times)

### 2. Speed Boost (Tier 1)
- **Name**: "Speed Boost"
- **Description**: "Increases your movement speed by 20%"
- **Skill Type**: `MoveSpeed`
- **Flat Bonus**: `0`
- **Percent Bonus**: `0.2`
- **Required Level**: `1`
- **Cost**: `1` skill point
- **Max Rank**: `3`

### 3. Defense (Tier 1)
- **Name**: "Tough Skin"
- **Description**: "Reduces damage taken by 10%"
- **Skill Type**: `Defense`
- **Flat Bonus**: `0`
- **Percent Bonus**: `0.1`
- **Required Level**: `1`
- **Cost**: `1` skill point
- **Max Rank**: `3`

### 4. XP Bonus (Tier 1)
- **Name**: "Fast Learner"
- **Description**: "Gain 25% more experience from kills"
- **Skill Type**: `XPBonus`
- **Flat Bonus**: `0`
- **Percent Bonus**: `0.25`
- **Required Level**: `2`
- **Cost**: `1` skill point
- **Max Rank**: `2`

### 5. Tower Damage (Tier 2 - Requires Health Boost)
- **Name**: "Tower Mastery"
- **Description**: "Towers deal 15% more damage"
- **Skill Type**: `TowerDamage`
- **Flat Bonus**: `0`
- **Percent Bonus**: `0.15`
- **Required Level**: `3`
- **Prerequisites**: Drag "Health Boost" skill here
- **Cost**: `2` skill points
- **Max Rank**: `2`

### 6. Tower Range (Tier 2)
- **Name**: "Long Range"
- **Description**: "Towers shoot 20% farther"
- **Skill Type**: `TowerRange`
- **Flat Bonus**: `0`
- **Percent Bonus**: `0.2`
- **Required Level**: `3`
- **Cost**: `2` skill points
- **Max Rank**: `2`

---

## Visual Guide

```
Unity Editor:

[Project Window]                    [Inspector - Skill Asset]
Assets/                            Skill_Health1 (SkillSO)
├─ Data/                           ├─ Basic Info:
│  ├─ Skill_Health1.asset  ←      │  ├─ Skill Name: "Health Boost"
│  ├─ Skill_Speed1.asset          │  ├─ Description: "Increases health..."
│  └─ Skill_Defense1.asset        │  └─ Icon: [Drag sprite here]
│                                  │
[Hierarchy]                        ├─ What This Skill Does:
├─ Player                          │  └─ Skill Type: Health ▼
│  └─ SkillTree Component         │
│     └─ All Skills:              ├─ How Much It Boosts:
│        [0] Skill_Health1  ←     │  ├─ Flat Bonus: 50
│        [1] Skill_Speed1   ←     │  └─ Percent Bonus: 0
│        [2] Skill_Defense1 ←     │
                                  ├─ What You Need:
                                  │  ├─ Required Level: 1
                                  │  └─ Prerequisites: [empty]
                                  │
                                  ├─ How Much It Costs:
                                  │  └─ Skill Point Cost: 1
                                  │
                                  └─ How Many Times:
                                     └─ Max Rank: 3
```

---

## Tips

1. **Start Simple**: Create 3-5 basic skills first, test them, then add more
2. **Use Prerequisites**: Make some skills require others (creates a skill tree path)
3. **Balance Costs**: Early skills should cost 1 point, later ones can cost 2-3
4. **Test Often**: Press '2' to gain XP, level up, then test unlocking skills
5. **Icons**: You can add icons later - skills work without them

---

## Troubleshooting

### Skills don't appear in UI?
- ✅ Check: Are skills added to SkillTree's "All Skills" array?
- ✅ Check: Is SkillTree component on the Player GameObject?
- ✅ Check: Are there any errors in Console?

### Can't unlock a skill?
- ✅ Check: Do you have enough skill points? (Press '2' to gain XP)
- ✅ Check: Are you high enough level? (Check Required Level)
- ✅ Check: Are prerequisites unlocked? (Check Prerequisites list)

### Skill doesn't do anything?
- ✅ Check: Is the skill type correct?
- ✅ Check: Are Flat Bonus or Percent Bonus set? (At least one should be > 0)
- ✅ Check: Is the skill actually unlocked? (Should show "Unlocked" in UI)

---

## Next Steps

1. Create 3-5 skills using the examples above
2. Add them to SkillTree component
3. Press Play and test!
4. Press '2' to gain XP and level up
5. Press Tab/M/K to open skill tree
6. Click skills to unlock them!

