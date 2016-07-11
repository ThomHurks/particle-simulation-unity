using UnityEngine;

public class CircularWireConstraint : Constraint
{
    public static bool OLD = false;

    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private Particle m_Particle;
    private readonly Vector2 m_Center;
    private readonly float m_Radius;
    private readonly float m_RadiusSquared;

    public CircularWireConstraint(Particle a_Particle, Vector2 a_Center, ParticleSystem a_System)
    {
        int i = a_System.AddConstraint(this);
        Debug.Log("Creating " + (OLD ? "old" : "new") + " circular wire constraint with index " + i);
        m_Particle = a_Particle;
        m_Center = a_Center;
        m_Radius = (a_Particle.Position - a_Center).magnitude;
        m_RadiusSquared = m_Radius * m_Radius;
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }

    public void UpdateJacobians(ParticleSystem a_ParticleSystem)
    {
        if (OLD)
        {
            UpdateJacobiansOld(a_ParticleSystem);
        }
        else
        {
            UpdateJacobiansNew(a_ParticleSystem);
        }
    }

    public double[] GetValue(ParticleSystem a_ParticleSystem)
    {
        return OLD ? GetValueOld(a_ParticleSystem) : GetValueNew(a_ParticleSystem);
    }

    public double[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        return OLD ? GetDerivativeValueOld(a_ParticleSystem) : GetDerivativeValueNew(a_ParticleSystem);
    }

    public void UpdateJacobiansNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 l = m_Particle.Position - m_Center; 
        Vector2 ldot = m_Particle.Velocity;
        float lmag = l.magnitude;
        Vector2 dCdl = l.normalized;
        Vector2 t1 = lmag * ldot;
        Vector2 t2 = (Vector2.Dot(l, ldot) / lmag) * l;
        Vector2 dCdotdl = (t1 - t2);
        dCdotdl = (1f / (lmag * lmag)) * dCdotdl;

        m_MatrixBlockJ.data[0] = dCdl.x;
        m_MatrixBlockJ.data[1] = dCdl.y;
        m_MatrixBlockJDot.data[0] = dCdotdl.x;
        m_MatrixBlockJDot.data[1] = dCdotdl.y; 
    }

    public double[] GetValueNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        double[] v = new double[GetConstraintDimension()];
        v[0] = relative.magnitude - m_Radius;
        return v;
    }

    public double[] GetDerivativeValueNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Velocity, relative) / relative.magnitude;
        return v;
    }


    public void UpdateJacobiansOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        m_MatrixBlockJ.data[0] = relative.x;
        m_MatrixBlockJ.data[1] = relative.y;
        m_MatrixBlockJDot.data[0] = m_Particle.Velocity.x;
        m_MatrixBlockJDot.data[1] = m_Particle.Velocity.y; 
    }

    public double[] GetValueOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        double[] v = new double[GetConstraintDimension()];
        v[0] = (relative.sqrMagnitude - m_RadiusSquared) / 2f;
        return v;
    }

    public double[] GetDerivativeValueOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Velocity, relative);
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
        Vector2 prev = new Vector2(m_Center.x + m_Radius, m_Center.y);
        for (int i = 12; i <= 360; i += 12)
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
