using UnityEngine;

public class FixedPointConstraint : Constraint
{
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private readonly Particle m_Particle;
    private Vector2 m_Center;

    public FixedPointConstraint(Particle a_Particle, Vector2 a_Center, ParticleSystem a_System)
    {
        m_Particle = a_Particle;
        m_Center = a_Center;
        // We can probably de-duplicate this code:
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int i = a_System.AddConstraint(this);
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }

    public void UpdateJacobians(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        m_MatrixBlockJ.data[0] = relative.x;
        m_MatrixBlockJ.data[1] = relative.y;
        m_MatrixBlockJDot.data[0] = m_Particle.Velocity.x;
        m_MatrixBlockJDot.data[1] = m_Particle.Velocity.y;
    }

    public float GetValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        return relative.sqrMagnitude / 2;
    }

    public float GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        return Vector2.Dot(m_Particle.Velocity, relative);
    }

    public int GetConstraintDimension()
    {
        return 2;
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0.5f, 0.5f));
        Vector2 prev = new Vector2(m_Center.x + 0.025f, m_Center.y);
        for (int i = 45; i <= 360; i += 45)
        {
            float degInRad = i * Mathf.PI / 180;
            GL.Vertex(prev);
            GL.Vertex(new Vector2(m_Center.x + Mathf.Cos(degInRad) * 0.025f, m_Center.y + Mathf.Sin(degInRad) * 0.025f));
        }
        GL.End();
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0.7f, 0.8f));
        GL.Vertex(m_Center);
        GL.Vertex(m_Particle.Position);
        GL.End();
    }
}
