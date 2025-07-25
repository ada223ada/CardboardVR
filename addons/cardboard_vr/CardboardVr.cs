#if TOOLS
using Godot;

// 标记该类为工具类，这意味着该类仅在 Godot 编辑器环境中运行
[Tool]
/// <summary>
/// 自定义的编辑器插件类，用于在 Godot 编辑器中添加和移除自定义类型。
/// </summary>
public partial class CardboardVr : EditorPlugin
{
    /// <summary>
    /// 当该插件节点进入场景树时调用此方法，用于添加自定义类型。
    /// </summary>
    public override void _EnterTree()
    {
        // 向 Godot 编辑器添加一个自定义类型
        // 参数 1: 自定义类型的名称，这里为 "CardboardVRCamera3D"
        // 参数 2: 自定义类型继承的基类，这里继承自 "Camera3D"
        // 参数 3: 自定义类型关联的 C# 脚本，通过资源路径加载
        // 参数 4: 自定义类型在编辑器中显示的图标，通过资源路径加载 SVG 纹理
        AddCustomType("CardboardVRCamera3D", "Camera3D",
            GD.Load<CSharpScript>("res://addons/cardboard_vr/scripts/CardboardVrCamera.cs"),
            GD.Load<Texture2D>("res://addons/cardboard_vr/icons/CardboardVRCamera3D.svg"));
    }

    /// <summary>
    /// 当该插件节点离开场景树时调用此方法，用于移除之前添加的自定义类型。
    /// </summary>
    public override void _ExitTree()
    {
        // 从 Godot 编辑器中移除指定名称的自定义类型
        RemoveCustomType("CardboardVRCamera3D");
    }
}

#endif
