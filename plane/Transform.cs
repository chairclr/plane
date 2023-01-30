using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace plane;

public class Transform
{
    private Vector3 _translation = Vector3.Zero;

    private Vector3 _scale = Vector3.Zero;

    private Quaternion _rotation = Quaternion.Identity;

    private Matrix4x4 _worldMatrix = Matrix4x4.Identity;

    private Vector3 _forward = Vector3.UnitZ;

    private Vector3 _left = -Vector3.UnitX;

    private Vector3 _up = Vector3.UnitY;

    private bool WorldMatrixNeedsUpdate = false; 

    public Vector3 Translation
    {
        get => _translation;
        set 
        {
            _translation = value;
            WorldMatrixNeedsUpdate = true;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            WorldMatrixNeedsUpdate = true;
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            WorldMatrixNeedsUpdate = true;
        }
    }

    public Vector3 EulerRotation
    {
        set => Rotation = Quaternion.CreateFromYawPitchRoll(value.X, value.Y, value.Z);
    }

    public Vector3 Forward => _forward;
    public Vector3 Backward => -_forward;

    public Vector3 Left => _left;
    public Vector3 Right => -_left;

    public Vector3 Up => _up;
    public Vector3 Down => -_up;

    public Matrix4x4 WorldMatrix
    {
        get
        {
            if (WorldMatrixNeedsUpdate)
            {
                UpdateWorldMatrix();
                WorldMatrixNeedsUpdate = false;
            }

            return _worldMatrix;
        }

        set
        {
            _worldMatrix = value;

            Matrix4x4.Decompose(_worldMatrix, out _scale, out _rotation, out _translation);
        }
    }

    public Transform()
    {

    }

    public Transform(Matrix4x4 worldMatrix)
    {
        WorldMatrix = worldMatrix;
    }

    public Transform(Vector3 translation, Vector3 scale)
    {
        Translation = translation;

        Scale = scale;

        UpdateWorldMatrix();
    }

    public Transform(Vector3 translation, Vector3 scale, Quaternion rotation)
    {
        _translation = translation;

        _scale = scale;

        _rotation = rotation;

        UpdateWorldMatrix();
    }

    public Transform(Vector3 translation, Vector3 scale, Vector3 eulerRotation)
    {
        _translation = translation;

        _scale = scale;

        _rotation = Quaternion.CreateFromYawPitchRoll(eulerRotation.X, eulerRotation.Y, eulerRotation.Z);

        UpdateWorldMatrix();
    }

    public Matrix4x4 GetScaleMatrix() => Matrix4x4.CreateScale(Scale);

    public Matrix4x4 GetRotationMatrix() => Matrix4x4.CreateFromQuaternion(Rotation);

    public Matrix4x4 GetTranslationMatrix() => Matrix4x4.CreateTranslation(Translation);

    public void LookAt(Vector3 point)
    {
        if (point == Translation)
            return;

        point = Translation - point;

        float pitch = 0.0f;
        if (point.Y != 0.0f)
        {
            float distance = MathF.Sqrt(point.X * point.X + point.Z * point.Z);
            pitch = MathF.Atan(point.Y / distance);
        }

        float yaw = 0.0f;
        if (point.X != 0.0f)
        {
            yaw = MathF.Atan(point.X / point.Z);
        }
        if (point.Z > 0)
            yaw += MathF.PI;

        EulerRotation = new Vector3(yaw, pitch, 0.0f);
    }

    public void SmoothLookAt(Vector3 point, float amount)
    {
        if (point == Translation)
            return;

        point = Translation - point;

        float pitch = 0.0f;
        if (point.Y != 0.0f)
        {
            float distance = MathF.Sqrt(point.X * point.X + point.Z * point.Z);
            pitch = MathF.Atan(point.Y / distance);
        }

        float yaw = 0.0f;
        if (point.X != 0.0f)
        {
            yaw = MathF.Atan(point.X / point.Z);
        }

        if (point.Z > 0)
            yaw += MathF.PI;

        Rotation = Quaternion.Slerp(Rotation, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0.0f), amount);
    }

    private void UpdateWorldMatrix()
    {
        Matrix4x4 rotationMatrix = GetRotationMatrix();

        _forward = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

        _left = Vector3.Transform(-Vector3.UnitX, rotationMatrix);

        _up = Vector3.Transform(Vector3.UnitY, rotationMatrix);

        _worldMatrix = GetScaleMatrix() * rotationMatrix * GetTranslationMatrix();
    }
}