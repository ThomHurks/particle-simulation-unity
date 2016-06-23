using UnityEngine;

public class MouseSpringForce : Force
{
    private readonly Particle m_Particle;
    private float m_RestLength;
    private float m_Ks;
    private float m_Kd;

    public MouseSpringForce(Particle a_Particle, float a_RestLength, float a_Ks, float a_Kd)
    {
        m_Particle = a_Particle;
        m_RestLength = a_RestLength;
        m_Ks = a_Ks;
        m_Kd = a_Kd;
    }

    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(mousePos3D.x, mousePos3D.y);


        Vector2 relative = mousePos - m_Particle.Position;
        float magnitude = relative.magnitude;
        Vector2 velocity = m_Particle.Velocity;
        Vector2 direction = relative.normalized;

        if (magnitude != 0f)
        {
            Vector2 f = (m_Ks * (magnitude - m_RestLength) + m_Kd * (Vector2.Dot(velocity, relative) / magnitude)) * direction;
            m_Particle.ForceAccumulator += f;
            if (float.IsNaN(f.x) || float.IsNaN(f.y) || float.IsInfinity(f.x) || float.IsInfinity(f.y))
            {
                throw new System.Exception("NaN or Inf force in particle attached to mouse");
            }
        }
    }

    public void Draw()
    {
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(mousePos3D.x, mousePos3D.y);

        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0.7f, 0.8f));
        GL.Vertex(mousePos);
        GL.Vertex(m_Particle.Position);
        GL.End();
    }
    
}
