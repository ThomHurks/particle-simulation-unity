using UnityEngine;

public class EllipticalWireConstraint : Constraint
{
    public static bool OLD = false;

    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private Particle m_Particle;
    private readonly Vector2 m_Center1;
    private readonly Vector2 m_Center2;
    private readonly float m_A;
    private readonly float m_B;
    private readonly float m_C;

    private readonly float m_Radius;
    private readonly float m_RadiusSquared;

    public EllipticalWireConstraint(Particle a_Particle, Vector2 a_Center1, Vector2 a_Center2, ParticleSystem a_System)
    {
        int i = a_System.AddConstraint(this);
        Debug.Log("Creating " + (OLD ? "old" : "new") + " elliptical wire constraint with index " + i);
        m_Particle = a_Particle;
        m_Center1 = a_Center1;
        m_Center2 = a_Center2;
        m_Radius = (a_Particle.Position - a_Center1).magnitude + (a_Particle.Position - a_Center2).magnitude;
        m_C = (a_Center1 - a_Center2).magnitude / 2;
        m_A = m_Radius / 2;
        m_B = Mathf.Sqrt(m_A * m_A - m_C * m_C);
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
        Vector2 l1 = m_Particle.Position - m_Center1; 
        Vector2 l2 = m_Particle.Position - m_Center2; 
        Vector2 l1dot = m_Particle.Velocity;
        Vector2 l2dot = m_Particle.Velocity;
        float l1mag = l1.magnitude;
        float l2mag = l2.magnitude;
        Vector2 dCdl1 = l1.normalized;
        Vector2 dCdl2 = l2.normalized;
        Vector2 t11 = l1mag * l1dot;
        Vector2 t21 = l2mag * l2dot;
        Vector2 t12 = (Vector2.Dot(l1, l1dot) / l1mag) * l1;
        Vector2 t22 = (Vector2.Dot(l2, l2dot) / l2mag) * l2;
        Vector2 dCdotdl1 = (t11 - t12);
        Vector2 dCdotdl2 = (t21 - t22);
        dCdotdl1 = (1f / (l1mag * l1mag)) * dCdotdl1;
        dCdotdl2 = (1f / (l2mag * l2mag)) * dCdotdl2;

        m_MatrixBlockJ.data[0] = dCdl1.x + dCdl2.x;
        m_MatrixBlockJ.data[1] = dCdl1.y + dCdl2.y;
        m_MatrixBlockJDot.data[0] = dCdotdl1.x + dCdotdl2.x;
        m_MatrixBlockJDot.data[1] = dCdotdl1.y + dCdotdl2.y; 
    }

    public double[] GetValueNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Center1;
        Vector2 relative2 = m_Particle.Position - m_Center2;
        double[] v = new double[GetConstraintDimension()];
        v[0] = relative1.magnitude + relative2.magnitude - m_Radius;
        return v;
    }

    public double[] GetDerivativeValueNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Center1;
        Vector2 relative2 = m_Particle.Position - m_Center2;
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Velocity, relative1) / relative1.magnitude + Vector2.Dot(m_Particle.Velocity, relative2) / relative2.magnitude;
        return v;
    }






    public void UpdateJacobiansOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Center1;
        Vector2 relative2 = m_Particle.Position - m_Center2;
        m_MatrixBlockJ.data[0] = relative1.x + relative2.x;
        m_MatrixBlockJ.data[1] = relative1.y + relative2.y;
        m_MatrixBlockJDot.data[0] = 2 * m_Particle.Velocity.x;
        m_MatrixBlockJDot.data[1] = 2 * m_Particle.Velocity.y; 
    }

    public double[] GetValueOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Center1;
        Vector2 relative2 = m_Particle.Position - m_Center2;
        double[] v = new double[GetConstraintDimension()];
        v[0] = (relative1.sqrMagnitude + relative2.sqrMagnitude - m_RadiusSquared) / 2f;
        return v;
    }

    public double[] GetDerivativeValueOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Center1;
        Vector2 relative2 = m_Particle.Position - m_Center2;
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Velocity, relative1) + Vector2.Dot(m_Particle.Velocity, relative2);
        return v;
    }


    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        float h = .1f;
        GL.Color(Color.blue);
        GL.Begin(GL.QUADS);
        GL.Vertex(new Vector2(m_Center1.x - h / 2f, m_Center1.y - h / 2f));
        GL.Vertex(new Vector2(m_Center1.x + h / 2f, m_Center1.y - h / 2f));
        GL.Vertex(new Vector2(m_Center1.x + h / 2f, m_Center1.y + h / 2f));
        GL.Vertex(new Vector2(m_Center1.x - h / 2f, m_Center1.y + h / 2f));
        GL.End();

        GL.Color(Color.blue);
        GL.Begin(GL.QUADS);
        GL.Vertex(new Vector2(m_Center2.x - h / 2f, m_Center2.y - h / 2f));
        GL.Vertex(new Vector2(m_Center2.x + h / 2f, m_Center2.y - h / 2f));
        GL.Vertex(new Vector2(m_Center2.x + h / 2f, m_Center2.y + h / 2f));
        GL.Vertex(new Vector2(m_Center2.x - h / 2f, m_Center2.y + h / 2f));
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        Vector2 prev = new Vector2(m_Center1.x + m_Radius, m_Center1.y);
        for (int i = 18; i <= 360; i += 18)
        {
            float degInRad = i * Mathf.PI / 180;
            Vector2 vertex = new Vector2(m_Center1.x + Mathf.Cos(degInRad) * m_Radius, m_Center1.y + Mathf.Sin(degInRad) * m_Radius);
            GL.Vertex(prev);
            GL.Vertex(vertex);
            prev = vertex;
        }
        GL.End();
    }

}
