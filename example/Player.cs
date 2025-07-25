using Godot;

/// <summary>
/// Player 类继承自 CharacterBody3D，用于实现玩家角色的移动和跳跃逻辑。
/// 在物理帧中处理玩家的重力、跳跃和移动操作。
/// </summary>
public partial class Player : CharacterBody3D
{
    // 定义玩家的移动速度，单位为单位/秒
    private const float Speed = 5.0f;
    // 定义玩家跳跃时的垂直速度
    private const float JumpVelocity = 4.5f;

    /// <summary>
    /// 每一个物理帧都会调用此方法，处理玩家的物理相关逻辑，包括重力、跳跃和移动。
    /// </summary>
    /// <param name="delta">上一帧到当前帧的时间间隔，单位为秒。</param>
    public override void _PhysicsProcess(double delta)
    {
        // 添加重力效果。如果玩家不在地面上，则在垂直方向上不断增加重力加速度
        if (!IsOnFloor())
        {
            // 将重力加速度乘以时间间隔，累加到当前速度的垂直分量上
            Velocity += GetGravity() * (float)delta;
        }

        // 处理跳跃逻辑。当玩家按下跳跃键且在地面上时执行跳跃操作
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            // 将当前速度的垂直分量设置为跳跃速度，实现跳跃效果
            Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
        }

        // 获取用户输入的方向，并处理移动和减速逻辑。
        // 建议将 UI 动作替换为自定义的游戏玩法动作，以提高代码的可维护性
        // 获取用户在水平方向上的输入向量，"left"、"right"、"up"、"down" 为在 Godot 输入映射中定义的动作名称
        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        // 将 2D 输入向量转换为 3D 世界空间中的方向向量，并进行归一化处理
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        
        // 如果用户有输入方向，则更新玩家的速度以实现移动
        if (direction != Vector3.Zero)
        {
            // 根据输入方向和速度设置玩家的水平速度，保持垂直速度不变
            Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
        }
        else
        {
            // 如果用户没有输入方向，则让玩家的水平速度逐渐减为 0，实现减速效果
            Velocity = new Vector3(
                // 水平 X 轴速度逐渐趋近于 0
                Mathf.MoveToward(Velocity.X, 0, Speed),
                Velocity.Y,
                // 水平 Z 轴速度逐渐趋近于 0
                Mathf.MoveToward(Velocity.Z, 0, Speed)
            );
        }

        // 调用 MoveAndSlide 方法，根据当前速度移动玩家，并处理碰撞检测
        MoveAndSlide();
    }
}
