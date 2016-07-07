using UnityEngine;

public class UnitCircularWireConstraint : Constraint
{
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private Particle m_Particle;

    public UnitCircularWireConstraint(Particle a_Particle, ParticleSystem a_System)
    {
        int i = a_System.AddConstraint(this);
        m_Particle = a_Particle;
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }

    public void UpdateJacobians(ParticleSystem a_ParticleSystem)
    {
        m_MatrixBlockJ.data[0] = m_Particle.Position.x;
        m_MatrixBlockJ.data[1] = m_Particle.Position.y;
        m_MatrixBlockJDot.data[0] = m_Particle.Velocity.x;
        m_MatrixBlockJDot.data[1] = m_Particle.Velocity.y; 
    }

    public double[] GetValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = (Vector2.Dot(m_Particle.Position, m_Particle.Position) - 1d) / 2d;
        return v;
    }

    public double[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Position, m_Particle.Velocity);
        return v;
    }

    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        Vector2 prev = new Vector2(1, 0);
        for (int i = 18; i <= 360; i += 18)
        {
            float degInRad = i * Mathf.PI / 180;
            Vector2 vertex = new Vector2(Mathf.Cos(degInRad), Mathf.Sin(degInRad));
            GL.Vertex(prev);
            GL.Vertex(vertex);
            prev = vertex;
        }
        GL.End();
    }

}
