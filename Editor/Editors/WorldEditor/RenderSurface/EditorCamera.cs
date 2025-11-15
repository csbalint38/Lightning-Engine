using Editor.Utilities;
using MahApps.Metro.Controls;
using System.Numerics;
using System.Windows.Input;

namespace Editor.Editors.WorldEditor.RenderSurface;

internal class EditorCamera
{
    private static readonly float _minNearZ = .001f; // 1mm
    private static readonly float _minDiffNearZFarZ = .001f;
    private static readonly float _minFov = .01f;
    private static readonly float _maxFov = 100f;

    private int _surfaceId = 1;
    private bool _updatePosition;
    private bool _updateRotation;
    private float _acceleration = 0f;
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _desiredPosition;
    private Vector3 _desiredRotation;

    public float OrbitRadius { get => field; private set; }
    public Vector3 Target { get => field; private set; }

    public float Speed
    {
        get => field;
        set
        {
            value = Math.Clamp(value, 1f, 10f);

            if (!field.IsEqual(value)) field = value;
        }
    } = 5f;

    public float FoV
    {
        get => field;
        set
        {
            value = Math.Clamp(value, _minFov, _maxFov);

            if (!field.IsEqual(value)) field = value;
        }
    } = 45f;

    public float NearZ
    {
        get => field;
        set
        {
            value = Math.Clamp(value, _minNearZ, _minDiffNearZFarZ - _minDiffNearZFarZ);

            if (!field.IsEqual(value)) field = value;
        }
    } = .1f;

    public float FarZ
    {
        get => field;
        set
        {
            value = Math.Max(value, NearZ + _minDiffNearZFarZ);

            if (!field.IsEqual(value)) field = value;
        }
    } = 100f;

    public EditorCamera()
    {
        _position = _desiredPosition = new(0, 1, 10);
        _rotation = _desiredRotation = new(0, -MathUtilities.Pi, 0);
        OrbitRadius = 3f;
        Speed = 5f;
        Target = new(0, 1, _position.Z - OrbitRadius);
    }

    public void GoTo(Vector3 desiredPosition)
    {
        Target = desiredPosition;

        Orbit(0, 0, 0, true);
    }

    public void Orbit(double dx, double dy, int dz) => Orbit(dx, dy, dz, false);

    public void ChangePosition(Vector3 direction, float dt)
    {
        if (direction.LengthSquared() <= MathUtilities.Epsilon) return;

        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_desiredRotation.Y, _desiredRotation.X, 0);
        var dtScale = dt * 60;

        if (_acceleration < 1f) _acceleration += .02f * dtScale;

        var v = Vector3.Transform(
            direction * Speed * dtScale * (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? .1f : .01f),
            rotationMatrix
        ) * _acceleration;

        _desiredPosition += v;
        Target += v;
        _updatePosition = true;
    }

    public void ChangeDirection(double dx, double dy)
    {
        var theta = _desiredRotation.X + (float)dy * .005f;

        theta = Math.Clamp(theta, .0001f - MathUtilities.HalfPi, MathUtilities.HalfPi - .0001f);

        var phi = _desiredRotation.Y - (float)dx * .005f;
        var v = Vector3.TransformNormal(new(0, 0, 1), Matrix4x4.CreateFromYawPitchRoll(phi, theta, 0));

        v = Vector3.Normalize(v);
        v *= OrbitRadius;

        Target = _desiredPosition + v;
        _desiredRotation.X = theta;
        _desiredRotation.Y = phi;
        _updatePosition = true;
    }

    public void Update(float dt)
    {
        if ((_updatePosition || _updateRotation) && Id.IsValid(_surfaceId)) Seek(dt);
    }

    public void SetSurfaceId(int surfaceId)
    {
        _surfaceId = surfaceId;
    }

    private void Orbit(double dx, double dy, int dz, bool slide)
    {
        var theta = _desiredRotation.Y + (float)dy * .005f;

        theta = Math.Clamp(theta, .0001f - MathUtilities.HalfPi, MathUtilities.HalfPi - 0.0001f);

        var phi = _desiredRotation.Y - (float)dx * .005f;

        OrbitRadius *= 1f - (.1f * dz);

        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(phi, theta, 0);
        var v = Vector3.TransformNormal(new(0, 0, 1), rotationMatrix);

        v = Vector3.Normalize(v);
        v *= OrbitRadius;

        _desiredPosition = Target - v;
        _desiredRotation.X = theta;
        _desiredRotation.Y = phi;

        if(!slide)
        {
            _position = _desiredPosition;
            _rotation = _desiredRotation;
        }
        else
        {
            _updatePosition = true;
            _updateRotation = true;
        }
    }

    private void Seek(float dt)
    {
        var dtScale = .2f * dt * 60;

        if (_updatePosition)
        {
            var p = _desiredPosition - _position;

            _updatePosition = p.LengthSquared() > 1e-8f;

            if (_updatePosition)
            {
                _position += p * dtScale;
            }
            else
            {
                _position = _desiredPosition;
                _acceleration = 0f;
            }
        }

        if (_updateRotation)
        {
            var o = _desiredRotation - _rotation;

            _updateRotation = o.LengthSquared() > 1e-8f;
            _rotation = _updateRotation ? _rotation + o * dtScale : _desiredRotation;
        }
    }
}
