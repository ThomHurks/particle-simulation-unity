using UnityEngine;

public class FixedPointConstraint: Constraint
{

    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private readonly Particle m_Particle;
    private Vector2 m_Center;

    public FixedPointConstraint(Particle a_Particle, ParticleSystem a_System)
    {
        m_Particle = a_Particle;
        m_Center = new Vector2(a_Particle.Position.x, a_Particle.Position.y);
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
        m_MatrixBlockJ.data[0] = 1;
        m_MatrixBlockJ.data[1] = 0;
        m_MatrixBlockJ.data[2] = 0;
        m_MatrixBlockJ.data[3] = 1;
        //Jdot = 0
        m_MatrixBlockJDot.data[0] = 0;
        m_MatrixBlockJDot.data[1] = 0; 
        m_MatrixBlockJDot.data[2] = 0;
        m_MatrixBlockJDot.data[3] = 0; 
    }

    public double[] GetValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = m_Particle.Position.x - m_Center.x;
        v[1] = m_Particle.Position.y - m_Center.y;
        return v;
    }

    public double[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = m_Particle.Velocity.x;
        v[1] = m_Particle.Velocity.y;
        return v;
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

    public double getAvgMass()
    {
        return m_Particle.Mass;
    }


}


