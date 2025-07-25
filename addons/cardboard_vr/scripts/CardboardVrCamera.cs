using Godot;

/// <summary>
/// CardboardVrCamera 类继承自 Camera3D，用于实现 Cardboard VR 相机功能。
/// 该类支持通过陀螺仪或鼠标控制视角，并且能创建左右眼视图以实现立体视觉效果。
/// </summary>
public partial class CardboardVrCamera : Camera3D
{
    /// <summary>
    /// 相机是否激活，可在编辑器中导出设置，默认值为 true。
    /// </summary>
    [Export] public bool Active { get; set; } = true;
    
    [ExportCategory("Controls")]
    /// <summary>
    /// 是否使用陀螺仪控制视角，可在编辑器中导出设置，默认值为 true。
    /// </summary>
    [Export] public bool UseGyroscope { get; set; } = true;
    /// <summary>
    /// 鼠标控制视角的灵敏度，可在编辑器中导出设置，默认值为 0.003f。
    /// </summary>
    [Export] public float MouseSensitivity { get; set; } = 0.003f;
    /// <summary>
    /// 陀螺仪控制视角的系数，可在编辑器中导出设置，默认值为 0.2f。
    /// </summary>
    [Export] public float GyroscopeFactor { get; set; } = 0.2f;
    /// <summary>
    /// 是否旋转父节点，可在编辑器中导出设置，默认值为 true。
    /// </summary>
    [Export] public bool RotateParent { get; set; } = true;
    /// <summary>
    /// 是否处理鼠标捕获，可在编辑器中导出设置，默认值为 true。
    /// </summary>
    [Export] public bool HandleMouseCapture { get; set; } = true;
    /// <summary>
    /// 用于取消鼠标捕获的输入动作名称，可在编辑器中导出设置，默认值为 "cancel"。
    /// </summary>
    [Export] public string InputCancel { get; set; } = "cancel";

    [ExportCategory("Eyes")]
    /// <summary>
    /// 左右眼的间距，可在编辑器中导出设置，范围为 0.1 到 2.0，默认值为 2.0f。
    /// </summary>
    [Export(PropertyHint.Range, "0.1,2.0")] public float EyesSeparation { get; set; } = 2.0f;
    /// <summary>
    /// 眼睛的高度，可在编辑器中导出设置，范围为 0 到 5.0，默认值为 0.8f。
    /// </summary>
    [Export(PropertyHint.Range, "0,5.0")] public float EyeHeight { get; set; } = 0.8f;
    /// <summary>
    /// 眼睛的会聚角度，可在编辑器中导出设置，范围为 -360 到 360，默认值为 3.0f。
    /// </summary>
    [Export(PropertyHint.Range, "-360,360")] public float EyeConvergencyAngle { get; set; } = 3.0f;

    /// <summary>
    /// 存储 CardboardView 场景的打包资源。
    /// </summary>
    private PackedScene _viewScene;
    /// <summary>
    /// 左眼看板相机节点。
    /// </summary>
    private Camera3D _leftCamera3D;
    /// <summary>
    /// 右眼看板相机节点。
    /// </summary>
    private Camera3D _rightCamera3D;
    /// <summary>
    /// 左眼看板的枢轴节点。
    /// </summary>
    private Node3D _leftEyePivot;
    /// <summary>
    /// 右眼看板的枢轴节点。
    /// </summary>
    private Node3D _rightEyePivot;
    /// <summary>
    /// CardboardView 节点实例，用于显示左右眼视图。
    /// </summary>
    private CardboardView _view;
    /// <summary>
    /// 左眼看板的子视口节点。
    /// </summary>
    private SubViewport _leftEyeSubViewPort;
    /// <summary>
    /// 右眼看板的子视口节点。
    /// </summary>
    private SubViewport _rightEyeSubViewPort;
    /// <summary>
    /// 父节点，类型为 CharacterBody3D。
    /// </summary>
    private CharacterBody3D _parent;

    /// <summary>
    /// 处理输入事件，包括鼠标捕获和鼠标控制视角。
    /// </summary>
    /// <param name="event">输入事件对象。</param>
    public override void _Input(InputEvent @event)
    {
        // 如果相机未激活，则不处理输入事件
        if (!Active)
            return;

        // 处理鼠标捕获逻辑
        if (HandleMouseCapture)
        {
            // 如果是鼠标按键事件，捕获鼠标
            if (@event is InputEventMouseButton)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            // 如果按下取消输入动作，释放鼠标
            else if (Input.IsActionJustPressed(InputCancel))
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
        }

        // 如果不使用陀螺仪，且是鼠标移动事件，并且鼠标处于捕获状态，则处理鼠标控制视角
        if (!UseGyroscope && @event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // 如果需要旋转父节点，则旋转父节点的 Y 轴
            if (RotateParent)
            {
                _parent.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            }
            // 旋转左右眼枢轴节点的 Y 轴
            _leftEyePivot.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            _rightEyePivot.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            // 绕左右眼枢轴节点的局部 X 轴旋转
            _leftEyePivot.RotateObjectLocal(Vector3.Right, -mouseMotion.Relative.Y * MouseSensitivity);
            _rightEyePivot.RotateObjectLocal(Vector3.Right, -mouseMotion.Relative.Y * MouseSensitivity);
            
            // 限制左右眼枢轴节点的 X 轴旋转角度在 -90 到 90 度之间
            var leftRotX = Mathf.Clamp(_leftEyePivot.GlobalRotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            var rightRotX = Mathf.Clamp(_rightEyePivot.GlobalRotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            _leftEyePivot.GlobalRotation = new Vector3(leftRotX, _leftEyePivot.GlobalRotation.Y, _leftEyePivot.GlobalRotation.Z);
            _rightEyePivot.GlobalRotation = new Vector3(rightRotX, _rightEyePivot.GlobalRotation.Y, _rightEyePivot.GlobalRotation.Z);
        }
    }

    /// <summary>
    /// 节点进入场景树时调用，初始化左右眼相机、枢轴节点、子视口和视图。
    /// </summary>
    public override void _Ready()
    {
        // 加载 CardboardView 场景
        _viewScene = GD.Load<PackedScene>("res://addons/cardboard_vr/scenes/CardboardView.tscn");
        // 创建左右眼相机节点
        _leftCamera3D = new Camera3D();
        _rightCamera3D = new Camera3D();
        // 创建左右眼枢轴节点
        _leftEyePivot = new Node3D();
        _rightEyePivot = new Node3D();
        // 创建左右眼子视口节点
        _leftEyeSubViewPort = new SubViewport();
        _rightEyeSubViewPort = new SubViewport();
        
        // 获取父节点
        _parent = GetParent<CharacterBody3D>();
        
        // 将左右眼相机节点添加到对应的枢轴节点下
        _leftEyePivot.AddChild(_leftCamera3D);
        _leftEyeSubViewPort.AddChild(_leftEyePivot);
        _rightEyePivot.AddChild(_rightCamera3D);
        _rightEyeSubViewPort.AddChild(_rightEyePivot);
        
        // 实例化 CardboardView 节点
        _view = _viewScene.Instantiate<CardboardView>();
        // 将视图和子视口节点添加到当前节点下
        AddChild(_view);
        AddChild(_leftEyeSubViewPort);
        AddChild(_rightEyeSubViewPort);
        
        // 设置视图的左右眼子视口
        _view.SetViewPorts(_leftEyeSubViewPort, _rightEyeSubViewPort);
        
        // 设置左右眼相机的位置
        _leftCamera3D.Position = new Vector3(-EyesSeparation, 0, 0);
        _rightCamera3D.Position = new Vector3(EyesSeparation, 0, 0);
        // 设置左右眼枢轴节点的位置
        _leftEyePivot.Position = new Vector3(0, EyeHeight, 0);
        _rightEyePivot.Position = new Vector3(0, EyeHeight, 0);
        
        // 设置左右眼相机的会聚角度
        _leftCamera3D.RotateObjectLocal(Vector3.Up, Mathf.DegToRad(EyeConvergencyAngle));
        _rightCamera3D.RotateObjectLocal(Vector3.Up, -Mathf.DegToRad(EyeConvergencyAngle));
    }

    /// <summary>
    /// 每帧调用，更新左右眼枢轴节点的位置，若使用陀螺仪则处理陀螺仪控制视角。
    /// </summary>
    /// <param name="delta">帧间隔时间。</param>
    public override void _Process(double delta)
    {
        // 如果相机未激活，则不进行处理
        if (!Active)
            return;

        // 更新左右眼枢轴节点的位置
        _leftEyePivot.GlobalPosition = new Vector3(_parent.GlobalPosition.X, _parent.GlobalPosition.Y + EyeHeight, _parent.GlobalPosition.Z);
        _rightEyePivot.GlobalPosition = new Vector3(_parent.GlobalPosition.X, _parent.GlobalPosition.Y + EyeHeight, _parent.GlobalPosition.Z);

        // 如果使用陀螺仪，则处理陀螺仪控制视角
        if (UseGyroscope)
        {
            // 获取陀螺仪数据并乘以系数
            Vector3 gyroscope = Input.GetGyroscope() * GyroscopeFactor;
            // 如果需要旋转父节点，则旋转父节点的 Y 轴
            if (RotateParent)
            {
                _parent.RotateY(gyroscope.Y * GyroscopeFactor);
            }
            // 旋转左右眼枢轴节点的 Y 轴
            _leftEyePivot.RotateY(gyroscope.Y * GyroscopeFactor);
            _rightEyePivot.RotateY(gyroscope.Y * GyroscopeFactor);
            // 绕左右眼枢轴节点的局部 X 轴旋转
            _leftEyePivot.RotateObjectLocal(Vector3.Right, gyroscope.X * GyroscopeFactor);
            _rightEyePivot.RotateObjectLocal(Vector3.Right, gyroscope.X * GyroscopeFactor);
            
            // 限制左右眼枢轴节点的 X 轴旋转角度在 -90 到 90 度之间
            var leftRotX = Mathf.Clamp(_leftEyePivot.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            var rightRotX = Mathf.Clamp(_rightEyePivot.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            _leftEyePivot.Rotation = new Vector3(leftRotX, _leftEyePivot.Rotation.Y, _leftEyePivot.Rotation.Z);
            _rightEyePivot.Rotation = new Vector3(rightRotX, _rightEyePivot.Rotation.Y, _rightEyePivot.Rotation.Z);
        }
    }
}
