using UnityEngine;
using System.Collections;

public class HooksLawSpring : Force
{
    private readonly Particle m_ParticleA;
    private readonly Particle m_ParticleB;
    private float m_RestLength;
    private float m_SpringConstant;
    private float m_DampingConstant;

    public HooksLawSpring(Particle a_ParticleA, Particle a_ParticleB, float a_RestLength,
                          float a_SpringConstant, float a_DampingConstant)
    {
        m_ParticleA = a_ParticleA;
        m_ParticleB = a_ParticleB;
        m_RestLength = a_RestLength;
        m_SpringConstant = a_SpringConstant;
        m_DampingConstant = a_DampingConstant;
    }

    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        Vector2 l = m_ParticleA.Position - m_ParticleB.Position;
        Vector2 i = m_ParticleA.Velocity - m_ParticleB.Velocity;
        float lMagnitude = l.magnitude;
        if (lMagnitude > float.Epsilon)
        {
            Vector2 f_a = -(m_SpringConstant * (lMagnitude - m_RestLength) + m_DampingConstant * (Vector2.Dot(i, l) / lMagnitude)) * l.normalized;
            if (float.IsNaN(f_a.x) || float.IsNaN(f_a.y) || float.IsInfinity(f_a.x) || float.IsInfinity(f_a.y))
            {
                throw new System.Exception("Computed force was NaN or Inf");
            }
            m_ParticleA.ForceAccumulator += f_a;
            m_ParticleB.ForceAccumulator -= f_a;
        }
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(Color.green);
        GL.Vertex(m_ParticleA.Position);
        GL.Vertex(m_ParticleB.Position);
        GL.End();
    }
}
