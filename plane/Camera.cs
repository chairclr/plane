using System.Numerics;
using Silk.NET.Windowing;

namespace plane;

public class Camera
{
    private Vector3 _translation = Vector3.Zero;

    private Quaternion _rotation = Quaternion.Identity;

    private Vector3 _forward = Vector3.UnitZ;

    private Vector3 _left = -Vector3.UnitX;

    private Vector3 _up = Vector3.UnitY;

    private Matrix4x4 _viewMatrix;

    private bool ViewNeedsUpdate = false;

    private readonly IWindow Window;

    public Matrix4x4 ProjectionMatrix { get; private set; }

    public Camera(IWindow window, float fovDegrees, float nearZ, float farZ)
    {
        Window = window;
        UpdateProjectionMatrix(fovDegrees, nearZ, farZ);
        UpdateViewMatrix();
    }

    public Vector3 Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            ViewNeedsUpdate = true;
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            ViewNeedsUpdate = true;
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

    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (ViewNeedsUpdate)
            {
                UpdateViewMatrix();
                ViewNeedsUpdate = false;
            }

            return _viewMatrix;
        }

        set
        {
            _viewMatrix = value;

            Matrix4x4.Decompose(_viewMatrix, out _, out _rotation, out _translation);
        }
    }

    public void UpdateProjectionMatrix(float fovDegrees, float nearZ, float farZ)
    {
        float fovRadians = fovDegrees * (MathF.PI / 180f);
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, (float)Window.Size.X / (float)Window.Size.Y, nearZ, farZ);
    }

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

    private void UpdateViewMatrix()
    {
        Matrix4x4 rotationMatrix = GetRotationMatrix();

        _forward = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

        _left = Vector3.Transform(-Vector3.UnitX, rotationMatrix);

        _up = Vector3.Transform(Vector3.UnitY, rotationMatrix);

        _viewMatrix = Matrix4x4.CreateLookAt(Translation, Translation + Forward, Up);
    }
}
