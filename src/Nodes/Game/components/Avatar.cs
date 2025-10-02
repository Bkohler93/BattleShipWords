using Godot;
using System;
using BattleshipWithWords.Utilities;

public partial class Avatar : AspectRatioContainer
{
    [Export] private TextureRect _glowTexture;
    [Export] private TextureRect _avatarTexture;

    private ShaderMaterial _shaderMaterial;
    private Tween _activeTween;
    private bool _isPulsing;
    private float _minRadius = 0.406f;
    private float _maxRadius = 0.506f;
    private float _activeMinRadius = 0.459f;

    public override void _Ready()
    {
        base._Ready();

        _shaderMaterial = _glowTexture.Material as ShaderMaterial;
    }

    public void StartTurn()
    {
        Logger.Print($"avatar width={_avatarTexture.Size.X}, height={_avatarTexture.Size.Y}");
        Logger.Print("starting turn on avatar");
        TweenRadius(_maxRadius, 0.4f);
        _isPulsing = false;
    }

    public void EndTurn()
    {
        Logger.Print("ending turn on avatar");
        TweenRadius(_minRadius, 0.4f);
        _isPulsing = false;
    }

    public void OnServerTick()
    {
        if (_isPulsing) return; 

        _isPulsing = true;
        _activeTween?.Kill();

        _activeTween = CreateTween();

        _activeTween.TweenProperty(_shaderMaterial, "shader_parameter/radius", _activeMinRadius, 0.35f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _activeTween.TweenProperty(_shaderMaterial, "shader_parameter/radius", _maxRadius, 0.35f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut)
            .Finished += () => { _isPulsing = false; };
    }

    private void TweenRadius(float target, float duration)
    {
        _activeTween?.Kill();

        _activeTween = CreateTween();
        _activeTween.TweenProperty(_shaderMaterial, "shader_parameter/radius", target, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _activeTween.Finished += () => { Logger.Print($"radius is now {_shaderMaterial.GetShaderParameter("radius")}"); };
    }
}
