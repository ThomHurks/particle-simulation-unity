using UnityEngine;
using System.Collections;

public class CircularWireConstraint : Constraint
{
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private Particle m_Particle;
    private Vector2 m_Center;
    private float m_Radius;
    private float m_RadiusSquared;

    public CircularWireConstraint(Particle a_Particle, Vector2 a_Center, float a_Radius, ParticleSystem a_System)
    {
        m_Particle = a_Particle;
        m_Center = a_Center;
        m_Radius = a_Radius;
        m_RadiusSquared = a_Radius * a_Radius;
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
        m_MatrixBlockJDot.data[1] = m_Particle.Velocity.y; // TODO: VERIFY THIS
    }

    public float GetValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        return (relative.sqrMagnitude - m_RadiusSquared) / 2;
    }

    public float GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        return Vector2.Dot(m_Particle.Velocity, relative);
    }

    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        Vector2 prev = new Vector2(m_Center.x + m_Radius, m_Center.y);
        for (int i = 18; i <= 360; i += 18)
        {
            float degInRad = i * Mathf.PI / 180;
            Vector2 vertex = new Vector2(m_Center.x + Mathf.Cos(degInRad) * m_Radius, m_Center.y + Mathf.Sin(degInRad) * m_Radius);
            GL.Vertex(prev);
            GL.Vertex(vertex);
            prev = vertex;
        }
        GL.End();
    }

}
