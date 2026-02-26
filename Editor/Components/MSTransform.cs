using Editor.Common.Enums;
using Editor.DLLs;
using Editor.Utilities;
using System.Diagnostics;
using System.Numerics;

namespace Editor.Components;

sealed class MSTransform : MSComponent<Transform>
{
    private static bool _isLocalRotation = true;
    private static bool _isUniformScale = true;

    private float? _posX;
    private float? _posY;
    private float? _posZ;
    private float? _rotX;
    private float? _rotY;
    private float? _rotZ;
    private float? _scaleX;
    private float? _scaleY;
    private float? _scaleZ;
    private Vector3 _previousLocalPos;
    private Vector3 _localPos;
    private Vector3 _previousRotOffset;
    private Vector3 _rotOffset;

    public float? PosX
    {
        get => _posX;
        set => SetPropertyValue(ref _posX, value, nameof(PosX));
    }

    public float? PosY
    {
        get => _posY;
        set => SetPropertyValue(ref _posY, value, nameof(PosY));
    }

    public float? PosZ
    {
        get => _posZ;
        set => SetPropertyValue(ref _posZ, value, nameof(PosZ));
    }

    public float? RotX
    {
        get => _rotX;
        set => SetPropertyValue(ref _rotX, value, nameof(RotX));
    }

    public float? RotY
    {
        get => _rotY;
        set => SetPropertyValue(ref _rotY, value, nameof(RotY));
    }

    public float? RotZ
    {
        get => _rotZ;
        set => SetPropertyValue(ref _rotZ, value, nameof(RotZ));
    }

    public float? ScaleX
    {
        get => _scaleX;
        set => SetPropertyValue(ref _scaleX, value, nameof(ScaleX));
    }

    public float? ScaleY
    {
        get => _scaleY;
        set => SetPropertyValue(ref _scaleY, value, nameof(ScaleY));
    }

    public float? ScaleZ
    {
        get => _scaleZ;
        set => SetPropertyValue(ref _scaleZ, value, nameof(ScaleZ));
    }

    public bool IsLocalRotation
    {
        get => _isLocalRotation;
        set
        {
            if (_isLocalRotation != value)
            {
                _isLocalRotation = value;

                OnPropertyChanged(nameof(IsLocalRotation));
            }

        }
    }

    public bool IsUniformScale
    {
        get => _isUniformScale;
        set
        {
            if (_isUniformScale != value)
            {
                _isUniformScale = value;

                OnPropertyChanged(nameof(IsUniformScale));
            }
        }
    }

    public float LocalPosX
    {
        get => _localPos.X;
        set
        {
            if (!_localPos.X.IsEqual(value))
            {
                _localPos.X = value;

                OnPropertyChanged(nameof(LocalPosX));
            }
        }
    }

    public float LocalPosY
    {
        get => _localPos.Y;
        set
        {
            if (!_localPos.Y.IsEqual(value))
            {
                _localPos.Y = value;

                OnPropertyChanged(nameof(LocalPosY));
            }
        }
    }

    public float LocalPosZ
    {
        get => _localPos.Z;
        set
        {
            if (!_localPos.Z.IsEqual(value))
            {
                _localPos.Z = value;

                OnPropertyChanged(nameof(LocalPosZ));
            }
        }
    }

    public float RotOffsetX
    {
        get => _rotOffset.X;
        set
        {
            if (!_rotOffset.X.IsEqual(value))
            {
                _rotOffset.X = value;

                OnPropertyChanged(nameof(RotOffsetX));
            }
        }
    }

    public float RotOffsetY
    {
        get => _rotOffset.Y;
        set
        {
            if (!_rotOffset.Y.IsEqual(value))
            {
                _rotOffset.Y = value;

                OnPropertyChanged(nameof(RotOffsetY));
            }
        }
    }

    public float RotOffsetZ
    {
        get => _rotOffset.Z;
        set
        {
            if (!_rotOffset.Z.IsEqual(value))
            {
                _rotOffset.Z = value;

                OnPropertyChanged(nameof(RotOffsetZ));
            }
        }
    }

    public MSTransform(MSEntityBase msEntity) : base(msEntity)
    {
        Refresh();
    }

    protected override bool UpdateComponents(string propertyName)
    {
        var count = SelectedComponents.Count;
        var componentIds = SelectedComponents.Select(c => c.ParentEntity.EntityId).ToArray();

        Debug.Assert(count == componentIds.Length);

        float[] x = new float[count], y = new float[count], z = new float[count];
        var index = 0;

        switch (propertyName)
        {
            case nameof(PosX):
            case nameof(PosY):
            case nameof(PosZ):
                SelectedComponents.ForEach(c =>
                {
                    var pos = new Vector3(_posX ?? c.Position.X, _posY ?? c.Position.Y, _posZ ?? c.Position.Z);

                    x[index] = pos.X;
                    y[index] = pos.Y;
                    z[index] = pos.Z;

                    ++index;

                    c.Position = pos;
                });

                EngineAPI.SetPosition(componentIds, x, y, z, count, (int)Space.ABSOLUTE);

                return true;

            case nameof(RotX):
            case nameof(RotY):
            case nameof(RotZ):
                SelectedComponents.ForEach(c =>
                {
                    var rot = new Vector3(_rotX ?? c.Rotation.X, _rotY ?? c.Rotation.Y, _rotZ ?? c.Rotation.Z);

                    x[index] = rot.X;
                    y[index] = rot.Y;
                    z[index] = rot.Z;

                    ++index;

                    c.Rotation = rot;
                });

                EngineAPI.SetRotation(componentIds, x, y, z, count, (int)Space.ABSOLUTE);

                return true;

            case nameof(ScaleX):
            case nameof(ScaleY):
            case nameof(ScaleZ):
                SelectedComponents.ForEach(c =>
                {
                    var scale = new Vector3(_scaleX ?? c.Scale.X, _scaleY ?? c.Scale.Y, _scaleZ ?? c.Scale.Z);

                    x[index] = scale.X;
                    y[index] = scale.Y;
                    z[index] = scale.Z;

                    ++index;

                    c.Scale = scale;
                });

                EngineAPI.SetScale(componentIds, x, y, z, count, (int)Space.LOCAL);

                return true;

            case nameof(LocalPosX):
            case nameof(LocalPosY):
            case nameof(LocalPosZ):
                {
                    var dx = !_previousLocalPos.X.IsEqual(_localPos.X) ? _localPos.X - _previousLocalPos.X : 0f;
                    var dy = !_previousLocalPos.Y.IsEqual(_localPos.Y) ? _localPos.Y - _previousLocalPos.Y : 0f;
                    var dz = !_previousLocalPos.Z.IsEqual(_localPos.Z) ? _localPos.Z - _previousLocalPos.Z : 0f;

                    _previousLocalPos = _localPos;

                    Array.Fill(x, dx);
                    Array.Fill(y, dy);
                    Array.Fill(z, dz);
                    EngineAPI.SetPosition(componentIds, x, y, z, count, (int)Space.LOCAL);
                }

                return true;

            case nameof(RotOffsetX):
            case nameof(RotOffsetY):
            case nameof(RotOffsetZ):
                {
                    var dx = !_previousRotOffset.X.IsEqual(_rotOffset.X) ? _rotOffset.X - _previousRotOffset.X : 0f;
                    var dy = !_previousRotOffset.Y.IsEqual(_rotOffset.Y) ? _rotOffset.Y - _previousRotOffset.Y : 0f;
                    var dz = !_previousRotOffset.Z.IsEqual(_rotOffset.Z) ? _rotOffset.Z - _previousRotOffset.Z : 0f;

                    _previousRotOffset = _rotOffset;

                    Array.Fill(x, dx);
                    Array.Fill(y, dy);
                    Array.Fill(z, dz);

                    EngineAPI.SetRotation(componentIds, x, y, z, count, _isLocalRotation ? (int)Space.LOCAL : (int)Space.WORLD);
                }

                return true;
        }

        return false;
    }

    protected override bool UpdateMSComponent()
    {
        ResetLocalFrame();

        if (SelectedComponents.Count == 1)
        {
            var c = SelectedComponents[0];

            PosX = c.Position.X;
            PosY = c.Position.Y;
            PosZ = c.Position.Z;
            RotX = c.Rotation.X;
            RotY = c.Rotation.Y;
            RotZ = c.Rotation.Z;
            ScaleX = c.Scale.X;
            ScaleY = c.Scale.Y;
            ScaleZ = c.Scale.Z;

            return true;
        }

        PosX = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Position.X));
        PosY = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Position.Y));
        PosZ = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Position.Z));

        RotX = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Rotation.X));
        RotY = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Rotation.Y));
        RotZ = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Rotation.Z));

        ScaleX = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Scale.X));
        ScaleY = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Scale.Y));
        ScaleZ = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Scale.Z));

        return true;
    }

    private void SetPropertyValue(ref float? field, float? value, string propertyName)
    {
        if (value.HasValue) value = float.Round(value.Value, 3);

        if (!field.IsEqual(value))
        {
            field = value;

            OnPropertyChanged(propertyName);
        }
    }

    private void ResetLocalFrame()
    {
        _previousLocalPos = Vector3.Zero;
        LocalPosX = LocalPosY = LocalPosZ = 0;
        _previousRotOffset = Vector3.Zero;
        RotOffsetX = RotOffsetY = RotOffsetZ = 0;
    }
}
