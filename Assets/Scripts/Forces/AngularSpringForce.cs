using UnityEngine;

public class AngularSpringForce : Force
{
    private Particle m_MassPoint;
    private Particle m_ParticleA;
    private Particle m_ParticleB;
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
        /*Vec2f S1 = m_p1->m_Position - m_MassPoint->m_Position;
        Vec2f S2 = m_p2->m_Position - m_MassPoint->m_Position;

        float S1_mag = magnitude(S1);
        float S2_mag = magnitude(S2);

        double currentAngle = acos(Dot(S1, S2) / (S1_mag * S2_mag));
        double angleDelta = (m_angle - currentAngle) / 2;

        Vec2f t1 = RotateAroundPoint(m_MassPoint->m_Position, m_p1->m_Position, -angleDelta);
        Vec2f t2 = RotateAroundPoint(m_MassPoint->m_Position, m_p2->m_Position, angleDelta);

        Vec2f d1 = t1 - m_p1->m_Position;
        Vec2f d2 = t2 - m_p2->m_Position;

        float d1_mag = magnitude(d1);
        float d2_mag = magnitude(d2);

        Vec2f I1_Dot = m_p1->m_Velocity - m_MassPoint->m_Velocity;
        Vec2f I2_Dot = m_p2->m_Velocity - m_MassPoint->m_Velocity;

        Vec2f F1 = -((m_ks * d1_mag) + m_kd * (Dot(I1_Dot, d1) / d1_mag)) * normalized(d1);
        Vec2f F2 = -((m_ks * d2_mag) + m_kd * (Dot(I2_Dot, d2) / d2_mag)) * normalized(d2);

        assert(!isnan(F1) && isfinite(F1));
        m_p1->m_AccumulatedForce += F1;
        m_MassPoint->m_AccumulatedForce -= F1;

        assert(!isnan(F2) && isfinite(F2));
        m_p2->m_AccumulatedForce += F2;
        m_MassPoint->m_AccumulatedForce -= F2;*/
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
