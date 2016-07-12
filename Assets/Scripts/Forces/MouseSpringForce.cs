using UnityEngine;

public class MouseSpringForce : Force
{
    private readonly Particle m_Particle;
    private float m_RestLength;
    private float m_SpringConstant;
    private float m_DampingConstant;
    private Vector2 m_MousePosition;

    public MouseSpringForce(Particle a_Particle, Vector2 a_MousePosition, float a_RestLength, float a_SpringConstant, float a_DampingConstant)
    {
        m_Particle = a_Particle;
        m_RestLength = a_RestLength;
        m_SpringConstant = a_SpringConstant;
        m_DampingConstant = a_DampingConstant;
        m_MousePosition = a_MousePosition;
    }

    public void UpdateMousePosition(Vector2 a_MousePosition)
    {
        m_MousePosition = a_MousePosition;
    }

    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_MousePosition - m_Particle.Position;
        float magnitude = relative.magnitude;
        Vector2 velocity = m_Particle.Velocity;
        Vector2 direction = relative.normalized;

        if (magnitude > float.Epsilon)
        {
            Vector2 f = (m_SpringConstant * (magnitude - m_RestLength) - m_DampingConstant * (Vector2.Dot(velocity, relative) / magnitude)) * direction;
            m_Particle.ForceAccumulator += f;
            if (float.IsNaN(f.x) || float.IsNaN(f.y) || float.IsInfinity(f.x) || float.IsInfinity(f.y))
            {
                throw new System.Exception("NaN or Inf force in particle attached to mouse");
            }
        }
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0.7f, 0.8f));
        GL.Vertex(m_MousePosition);
        GL.Vertex(m_Particle.Position);
        GL.End();
    }
    
}
