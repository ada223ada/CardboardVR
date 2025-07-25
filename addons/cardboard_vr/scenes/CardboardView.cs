using Godot;

/// <summary>
/// CardboardView 类继承自 CanvasLayer，用于管理 Cardboard VR 视图的界面显示。
/// 该类主要负责初始化视图节点、设置左右眼视图的纹理。
/// </summary>
public partial class CardboardView : CanvasLayer
{
    // 存储左眼看板的控制节点引用
    private Control _leftEyeControl;
    // 存储右眼看板的控制节点引用
    private Control _rightEyeControl;
    // 存储背景颜色矩形节点引用
    private ColorRect _background;
    // 存储左眼看板的纹理矩形节点引用
    private TextureRect _leftEye;
    // 存储右眼看板的纹理矩形节点引用
    private TextureRect _rightEye;

    // 存储左眼看板的位置
    private Vector2 _leftEyePosition;
    // 存储右眼看板的位置
    private Vector2 _rightEyePosition;
    // 存储当前视图的尺寸
    private float _currentSize;
    // 存储当前视图的中心偏移量
    private float _currentCenterOffset;

    /// <summary>
    /// 当节点进入场景树时调用此方法，用于初始化视图节点。
    /// </summary>
    public override void _Ready()
    {
        // 从场景树中获取左眼看板的控制节点
        _leftEyeControl = GetNode<Control>("HorizontalDivider/LeftEyeControl");
        // 从场景树中获取右眼看板的控制节点
        _rightEyeControl = GetNode<Control>("HorizontalDivider/RightEyeControl");
        // 从场景树中获取背景颜色矩形节点
        _background = GetNode<ColorRect>("Background");
        // 从场景树中获取左眼看板的纹理矩形节点
        _leftEye = GetNode<TextureRect>("HorizontalDivider/LeftEyeControl/LeftEye");
        // 从场景树中获取右眼看板的纹理矩形节点
        _rightEye = GetNode<TextureRect>("HorizontalDivider/RightEyeControl/RightEye");
    }

    /// <summary>
    /// 设置左右眼视图的纹理。
    /// </summary>
    /// <param name="leftEye">左眼看板的子视口节点。</param>
    /// <param name="rightEye">右眼看板的子视口节点。</param>
    public void SetViewPorts(SubViewport leftEye, SubViewport rightEye)
    {
        // 将左眼看板子视口的纹理设置到左眼看板的纹理矩形节点上
        _leftEye.Texture = leftEye.GetTexture();
        // 将右眼看板子视口的纹理设置到右眼看板的纹理矩形节点上
        _rightEye.Texture = rightEye.GetTexture();
    }
}
