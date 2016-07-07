using UnityEngine;

public class AngularSpringForce : Force
{
    public static bool INIT_AD_HOC = true;

    private readonly Particle m_MassPoint;
    private readonly Particle m_ParticleA;
    private readonly Particle m_ParticleB;
    // Cosine of rest angle
    private float m_Angle;
    // Spring constants
    private float m_SpringConstant;
    private float m_DampingConstant;
    private readonly bool m_AdHoc;

    private Vector2 m_ForceA;
    private Vector2 m_ForceB;
    private Vector2 m_ForceM;

    public AngularSpringForce(Particle a_MassPoint, Particle a_ParticleA, Particle a_ParticleB,
                              float a_Angle, float a_SpringConstant, float a_DampingConstant)
    {
        m_MassPoint = a_MassPoint;
        m_ParticleA = a_ParticleA;
        m_ParticleB = a_ParticleB;
        m_Angle = a_Angle;
        m_SpringConstant = a_SpringConstant;
        m_DampingConstant = a_DampingConstant;
        m_ForceA = new Vector2(0, 0);
        m_ForceB = new Vector2(0, 0);
        m_ForceM = new Vector2(0, 0);
        m_AdHoc = INIT_AD_HOC;
    }


    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        if (m_AdHoc)
        {
            ApplyForceAdHoc(a_ParticleSystem);
        }
        else
        {
            ApplyForceAnalitic(a_ParticleSystem);
        }
    }

    public void ApplyForceAdHoc(ParticleSystem a_ParticleSystem)
    {
        Vector2 S1 = m_ParticleA.Position - m_MassPoint.Position;
        Vector2 S2 = m_ParticleB.Position - m_MassPoint.Position;

        float S1_mag = S1.magnitude;
        float S2_mag = S2.magnitude;

        if (S1_mag * S2_mag > float.Epsilon)
        {
            float currentAngle = Mathf.Acos(Vector2.Dot(S1, S2) / (S1_mag * S2_mag));
            float angleDelta = (m_Angle - currentAngle) / 2;

            Vector2 t1 = RotateAroundPoint(m_MassPoint.Position, m_ParticleA.Position, -angleDelta);
            Vector2 t2 = RotateAroundPoint(m_MassPoint.Position, m_ParticleB.Position, angleDelta);

            Vector2 d1 = m_ParticleA.Position - t1;
            Vector2 d2 = m_ParticleB.Position - t2;

            float d1_mag = d1.magnitude;
            float d2_mag = d2.magnitude;

            if (d1_mag > float.Epsilon && d2_mag > float.Epsilon)
            {
                // Vector2 I1_Dot = m_ParticleA.Velocity - m_MassPoint.Velocity;
                // Vector2 I2_Dot = m_ParticleB.Velocity - m_MassPoint.Velocity;
                Vector2 I1_Dot = m_ParticleA.Velocity;
                Vector2 I2_Dot = m_ParticleB.Velocity;

                Vector2 F1 = -((m_SpringConstant * d1_mag) + m_DampingConstant * (Vector2.Dot(I1_Dot, d1) / d1_mag)) * d1.normalized;
                Vector2 F2 = -((m_SpringConstant * d2_mag) + m_DampingConstant * (Vector2.Dot(I2_Dot, d2) / d2_mag)) * d2.normalized;

                if (float.IsNaN(F1.x) || float.IsInfinity(F1.x) || float.IsNaN(F1.y) || float.IsInfinity(F1.y) ||
                    float.IsNaN(F2.x) || float.IsInfinity(F2.x) || float.IsNaN(F2.y) || float.IsInfinity(F2.y))
                {
                    throw new System.Exception("NaN or Inf in AngularSpring");
                }
                m_ParticleA.ForceAccumulator += F1;
                m_MassPoint.ForceAccumulator -= F1;

                m_ParticleB.ForceAccumulator += F2;
                m_MassPoint.ForceAccumulator -= F2;

                m_ForceA = F1;
                m_ForceB = F2;
                m_ForceM = -1 * (F1 + F2);
            }
        }
    }

    public void ApplyForceAnalitic(ParticleSystem a_ParticleSystem)
    {

        Vector2 r1 = m_ParticleA.Position - m_MassPoint.Position;
        Vector2 r3 = m_ParticleB.Position - m_MassPoint.Position;
        Vector2 r1dot = m_ParticleA.Velocity - m_MassPoint.Velocity;
        Vector2 r3dot = m_ParticleB.Velocity - m_MassPoint.Velocity;

        float r1mag = r1.magnitude;
        float r3mag = r3.magnitude;
        float r1r3mag = r1.magnitude * r3.magnitude;
        float r1r3magsq = r1r3mag * r1r3mag;
        float r1dotr3 = Vector2.Dot(r1, r3);
        float r1dotr1d = Vector2.Dot(r1, r1dot);
        float r3dotr3d = Vector2.Dot(r3, r3dot);
        float r1dotr3d = Vector2.Dot(r1, r3dot);
        float r3dotr1d = Vector2.Dot(r3, r1dot);
        float y = r1dotr3 / (r1r3mag);
        if (1f - y * y < .01)
        {//parallel line, stop force appliance, will get an /0

            m_ForceA = new Vector2(0, 0);
            m_ForceB = new Vector2(0, 0);
            m_ForceM = new Vector2(0, 0);
            return;
        }
        float dCdy = 1f / (Mathf.Sqrt(1.005f - y * y));
        float ydot = (r1r3mag * (r1dotr3d + r3dotr1d) - r1dotr3 * (r1dotr1d * r3mag / r1mag + r3dotr3d * r1mag / r3mag)) / r1r3magsq;
        float Cdot = dCdy * ydot;
        float C = Mathf.Acos(r1dotr3 / r1r3mag) - m_Angle;
        Vector2 dydr1 = (1f / r1r3magsq) * (r1r3mag * r3 - (r3mag / r1mag) * r1);
        Vector2 dydr3 = (1f / r1r3magsq) * (r1r3mag * r1 - (r1mag / r3mag) * r3);
        Vector2 dCdr1 = dCdy * dydr1;
        Vector2 dCdr3 = dCdy * dydr3;
        float x = (m_SpringConstant * C + m_DampingConstant * Cdot);
        Vector2 f1 = x * dCdr1;
        Vector2 f2 = -x * (dCdr1 + dCdr3);
        Vector2 f3 = x * dCdr3;

        m_ParticleA.ForceAccumulator += f1;
        m_MassPoint.ForceAccumulator += f2;
        m_ParticleB.ForceAccumulator += f3;
        m_ForceA = f1;
        m_ForceB = f3;
        m_ForceM = f2;


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


        GL.Begin(GL.LINES);
        GL.Color(new Color(0.8f, 0.8f, .1f));
        GL.Vertex(m_MassPoint.Position + m_ForceM);
        GL.Vertex(m_MassPoint.Position);
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(new Color(0.8f, 0.8f, .1f));
        GL.Vertex(m_ParticleA.Position + m_ForceA);
        GL.Vertex(m_ParticleA.Position);
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(new Color(0.8f, 0.8f, .1f));
        GL.Vertex(m_ParticleB.Position + m_ForceB);
        GL.Vertex(m_ParticleB.Position);
        GL.End();
    }
}
