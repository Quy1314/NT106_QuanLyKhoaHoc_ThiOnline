# GEMINI.md - User Rules

> This file defines project-specific rules that the agent must always follow.

## UI/UX Design Rules

### 🎨 Design Consistency (Designer vs Runtime)
- **Rule**: The Visual Studio Designer View MUST always match the Runtime UI.
- **Implementation**: 
    - Do NOT rely solely on code-behind code (e.g., `CustomizeUI()`) for static styling (colors, fonts, sizes).
    - You MUST update the `.Designer.cs` (for WinForms) or XAML (for WPF) to reflect the intended design.
    - Code-behind should only be used for truly dynamic logic (e.g., responsive resizing based on window changes) that cannot be expressed in the Designer.
