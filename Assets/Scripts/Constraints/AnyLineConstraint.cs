using UnityEngine;

public class AnyLineConstraint:Constraint
{
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private readonly Particle m_Particle;
    private readonly Vector2 m_A;
    private readonly Vector2 m_B;
    private readonly Vector2 m_Aperp;

    public AnyLineConstraint(Particle a_Particle, Vector2 a_A, ParticleSystem a_System)
    {
        m_Particle = a_Particle;
        m_A = a_A.normalized;
        m_B = a_Particle.Position;
        m_Aperp = new Vector2(a_A.y, -a_A.x);
        // We can probably de-duplicate this code:
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int i = a_System.AddConstraint(this);

        Debug.Log("Creating fixed point V constraint with index " + i);
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }

    public void UpdateJacobians(ParticleSystem a_ParticleSystem)
    {
        //J = I
        m_MatrixBlockJ.data[0] = m_Aperp.x;
        m_MatrixBlockJ.data[1] = m_Aperp.y;
        //Jdot = 0
        m_MatrixBlockJDot.data[0] = 0;
        m_MatrixBlockJDot.data[1] = 0; 
    }

    public double[] GetValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Position - m_B, m_Aperp);
        return v;
    }

    public double[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Velocity, m_Aperp);
        return v;
    }

    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0.5f, 0.5f));
        GL.Vertex(m_B - 100 * m_A);
        GL.Vertex(m_B + 100 * m_A);
        GL.End();
    }
}


