using UnityEngine;

public class AngularSpringForce : Force
{
    private readonly Particle m_MassPoint;
    private readonly Particle m_ParticleA;
    private readonly Particle m_ParticleB;
    // Cosine of rest angle
    private float m_Angle;
    // Spring constants
    private float m_Ks;
    private float m_Kd;

    public AngularSpringForce(Particle a_MassPoint, Particle a_ParticleA, Particle a_ParticleB,
                              float a_Angle, float a_Ks, float a_Kd)
    {
        m_MassPoint = a_MassPoint;
        m_ParticleA = a_ParticleA;
        m_ParticleB = a_ParticleB;
        m_Angle = a_Angle;
        m_Ks = a_Ks;
        m_Kd = a_Kd;
    }

    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        Vector2 S1 = m_ParticleA.Position - m_MassPoint.Position;
        Vector2 S2 = m_ParticleB.Position - m_MassPoint.Position;

        float S1_mag = S1.magnitude;
        float S2_mag = S2.magnitude;

        if (S1_mag * S2_mag != 0f)
        {
            float currentAngle = Mathf.Acos(Vector2.Dot(S1, S2) / (S1_mag * S2_mag));
            float angleDelta = (m_Angle - currentAngle) / 2;

            Vector2 t1 = RotateAroundPoint(m_MassPoint.Position, m_ParticleA.Position, -angleDelta);
            Vector2 t2 = RotateAroundPoint(m_MassPoint.Position, m_ParticleB.Position, angleDelta);

            Vector2 d1 = t1 - m_ParticleA.Position;
            Vector2 d2 = t2 - m_ParticleB.Position;

            float d1_mag = d1.magnitude;
            float d2_mag = d2.magnitude;

            Vector2 I1_Dot = m_ParticleA.Velocity - m_MassPoint.Velocity;
            Vector2 I2_Dot = m_ParticleB.Velocity - m_MassPoint.Velocity;

            Vector2 F1 = -((m_Ks * d1_mag) + m_Kd * (Vector2.Dot(I1_Dot, d1) / d1_mag)) * d1.normalized;
            Vector2 F2 = -((m_Ks * d2_mag) + m_Kd * (Vector2.Dot(I2_Dot, d2) / d2_mag)) * d2.normalized;

            if (float.IsNaN(F1.x) || float.IsInfinity(F1.x) || float.IsNaN(F1.y) || float.IsInfinity(F1.y) ||
                float.IsNaN(F2.x) || float.IsInfinity(F2.x) || float.IsNaN(F2.y) || float.IsInfinity(F2.y))
            {
                throw new System.Exception("NaN or Inf in AngularSpring");
            }
            m_ParticleA.ForceAccumulator += F1;
            m_MassPoint.ForceAccumulator -= F1;

            m_ParticleB.ForceAccumulator += F2;
            m_MassPoint.ForceAccumulator -= F2;
        }
    }


    private Vector2 RotateAroundPoint(Vector2 a_Pivot, Vector2 a_Point, float a_Angle)
    {
        float s = Mathf.Sin(a_Angle);
        float c = Mathf.Cos(a_Angle);
        Vector2 delta = a_Point - a_Pivot;
        float xNew = (delta.x * c) - (delta.y * s);
        float yNew = (delta.x * s) + (delta.y * c);
        return new Vector2(xNew + a_Pivot.x, yNew + a_Pivot.y);
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.3f, 0.3f, 1f));
        GL.Vertex(m_MassPoint.Position);
        GL.Vertex(m_ParticleA.Position);
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(new Color(0.3f, 0.3f, 1f));
        GL.Vertex(m_MassPoint.Position);
        GL.Vertex(m_ParticleB.Position);
        GL.End();
    }
}
