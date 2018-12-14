using UnityEngine;

public static class hwmMath
{
    public const float IVNERT_PI = 1.0f / Mathf.PI;

    public static Vector2 CircleLerp(Vector2 from, Vector2 to, float angle)
    {
        if (from == to)
        {
            return to;
        }
        else if (angle < 0)
        {
            return from;
        }

        float angleOffset = Vector2.SignedAngle(to, from);
        return Quaternion.Euler(0
                , 0
                , angleOffset > 0
                    ? -Mathf.Min(angleOffset, angle)
                    : Mathf.Min(-angleOffset, angle)
            ) * from;
    }

    /// <summary>
    /// Gets the reciprocal of this vector, avoiding division by zero.
    /// Zero components are set to float.MaxValue.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns>Reciprocal of this vector.</returns>
    public static Vector2 Reciprocal(Vector2 vec)
    {
        return new Vector2(vec.x != 0.0f ? 1.0f / vec.x : float.MaxValue
            , vec.y != 0.0f ? 1.0f / vec.y : float.MaxValue);
    }

    /// <summary>
    /// Transforms a direction by this matrix.
    /// </summary>
    public static Vector2 MatrixMultiplyVector(Matrix4x4 matrix, Vector2 vector)
    {
        Vector2 vector2;
        vector2.x = matrix.m00 * vector.x + matrix.m01 * vector.y;
        vector2.y = matrix.m10 * vector.x + matrix.m11 * vector.y;
        return vector2;
    }

    public static Vector2 QuaternionMultiplyVector(Quaternion rotation, Vector2 vector)
    {
        float num1 = rotation.x * 2f;
        float num2 = rotation.y * 2f;
        float num3 = rotation.z * 2f;
        float num4 = rotation.x * num1;
        float num5 = rotation.y * num2;
        float num6 = rotation.z * num3;
        float num7 = rotation.x * num2;
        float num12 = rotation.w * num3;
        Vector2 vector2;
        vector2.x = (1.0f - (num5 + num6)) * vector.x + (num7 - num12) * vector.y;
        vector2.y = (num7 + num12) * vector.x + (1.0f - (num4 + num6)) * vector.y;
        return vector2;
    }

    public static float Square(float value)
    {
        return value * value;
    }

    public static float Max(float a, float b, float c, float d)
    {
        return Max(Max(a, b), Max(c, d));
    }

    public static float Max(float a, float b)
    {
        return a > b ? a : b;
    }

    public static float Min(float a, float b, float c, float d)
    {
        return Min(Min(a, b), Min(c, d));
    }

    public static float Min(float a, float b)
    {
        return a < b ? a : b;
    }

    public static float Evaluate(float sampleAt, Keyframe[] keyframes)
    {
        hwmDebug.Assert(keyframes.Length > 0, "keyframes.Length > 0");

        int iHigh = 0;
        do
        {
            if (sampleAt < keyframes[iHigh].time)
            {
                break;
            }

        } while (++iHigh < keyframes.Length);

        return iHigh == 0
            ? keyframes[iHigh].value
            : iHigh == keyframes.Length
                ? keyframes[iHigh - 1].value
                : Mathf.Lerp(keyframes[iHigh - 1].value
                    , keyframes[iHigh].value
                    , (sampleAt - keyframes[iHigh - 1].time)
                        / (keyframes[iHigh].time - keyframes[iHigh - 1].time));
    }

    public static float ClampAbs(float value, float maxAbs)
    {
        return Mathf.Sign(value) * Mathf.Min(Mathf.Abs(value), maxAbs);
    }

    /// <summary>
    /// 计算施加推力后，产生的加速度
    /// kinetic energy：E_k = 1 / 2 * m * v^2 <see cref="https://en.wikipedia.org/wiki/Kinetic_energy"/>
    /// </summary>
    /// <param name="power">推力</param>
    /// <param name="currentSpeed">当前速度</param>
    /// <param name="mass">质量</param>
    /// <param name="delta">持续时间</param>
    /// <returns>加速度</returns>
    public static float PowerToAcceleration(float power, float currentSpeed, float mass, float delta)
    {
        hwmDebug.Assert(mass > Mathf.Epsilon, "mass > Mathf.Epsilon");

        // 施加推力后的动能
        float kineticEnergy = 0.5f * mass * currentSpeed * currentSpeed // 当前的动能
            + power * delta;
        kineticEnergy = Mathf.Max(0, kineticEnergy); // HACK 减速时，如果速度过慢，动能可能小于0。暂不支持飞机倒飞，所以动能最小为0
        float newSpeed = Mathf.Sqrt(kineticEnergy * 2.0f / mass); // 用动能算速度
        return (newSpeed - currentSpeed) / delta;
    }

    /// <summary>
    /// 计算物体受到的空气阻力
    /// 
    /// 参考流体力学的阻力方程：F_D = 0.5 * p * v^2 * C_D * A <see cref="https://en.wikipedia.org/wiki/Drag_equation"/>
    /// F_D: 阻力
    /// ρ: 流体密度
    /// v: 物体速度
    /// A: 参考面积
    /// C_D: 阻力系数, 是一个无因次的系数, 像汽车的阻力系数约在0.25到0.4之间
    /// 
    /// 计算的阻力是有方向的, 且方向和速度相反
    /// 降低复杂度, 忽略流体密度和参考面积
    /// 所以这里用公式
    ///     F_D = 0.5 * (-v * |v|) * C_D
    /// </summary>
    /// <param name="velocity">速度(v)</param>
    /// <param name="dragCoefficient">阻力系数(C_D)</param>
    /// <returns>速度相反方向的阻力(F_D)</returns>
    public static Vector3 CalculateDrag(Vector3 velocity, Vector3 dragCoefficient)
    {
        // -v * |v|
        Vector3 dragForce = new Vector3(-velocity.x * Mathf.Abs(velocity.x),
            -velocity.y * Mathf.Abs(velocity.y),
            -velocity.z * Mathf.Abs(velocity.z));

        // (-v * |v|) * C_D
        dragForce.Scale(dragCoefficient);

        // 0.5 * (-v * |v|) * C_D
        return dragForce * 0.5f;
    }

    public static float MoveTowards(float current, float target, ref float velocity, float maxVelocity, float maxAcceleration, float deltaTime)
    {
        float vCurrentToTarget = target - current;
        float dirCurrentToTarget = Mathf.Sign(vCurrentToTarget);
        float fCurrentToTarget = vCurrentToTarget * dirCurrentToTarget;

        // Vt^2 - V0^2 = 2*a*s
        float fCurrentIdealSpeed = Mathf.Sqrt(2.0f * maxAcceleration * fCurrentToTarget);
        float fNewIdealSpeed = Mathf.Max(0, fCurrentIdealSpeed - maxAcceleration * deltaTime);


        float vNewVelocity = Mathf.MoveTowards(velocity, dirCurrentToTarget * fNewIdealSpeed, maxAcceleration * deltaTime);
        vNewVelocity = ClampAbs(vNewVelocity, maxVelocity);

        float a = (vNewVelocity - velocity) / deltaTime;
        float vDelta = velocity * deltaTime + 0.5f * a * deltaTime * deltaTime;
        current += vDelta;
        velocity = vNewVelocity;

        return current;
    }
}