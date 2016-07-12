using UnityEngine;

public class EllipticalWireConstraint : Constraint
{

    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private Particle m_Particle;
    private readonly Vector2 m_Focus1;
    private readonly Vector2 m_Focus2;
    private readonly float m_A;
    private readonly float m_B;
    private readonly float m_C;
    private readonly Vector2 m_Midpoint;
    private readonly float m_Tilt;

    private readonly float m_Radius;

    public EllipticalWireConstraint(Particle a_Particle, Vector2 a_Center1, Vector2 a_Center2, ParticleSystem a_System)
    {
        int i = a_System.AddConstraint(this);
        Debug.Log("Creating  elliptical wire constraint with index " + i);
        m_Particle = a_Particle;
        m_Focus1 = a_Center1;
        m_Focus2 = a_Center2;
        m_Midpoint = 0.5f * (a_Center1 + a_Center2);
        m_Radius = (a_Particle.Position - a_Center1).magnitude + (a_Particle.Position - a_Center2).magnitude;
        m_C = (a_Center1 - a_Center2).magnitude / 2;
        m_A = m_Radius / 2;
        m_B = Mathf.Sqrt(m_A * m_A - m_C * m_C);
        //Debug.Log("a = " + m_A + ", b = " + m_B);
        m_Tilt = Mathf.Acos(Vector2.Dot(Vector2.right, (m_Focus1 - m_Focus2).normalized));
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }



    public void UpdateJacobians(ParticleSystem a_ParticleSystem)
    {
        Vector2 l1 = m_Particle.Position - m_Focus1; 
        Vector2 l2 = m_Particle.Position - m_Focus2; 
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

    public double[] GetValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Focus1;
        Vector2 relative2 = m_Particle.Position - m_Focus2;
        double[] v = new double[GetConstraintDimension()];
        v[0] = relative1.magnitude + relative2.magnitude - m_Radius;
        return v;
    }

    public double[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative1 = m_Particle.Position - m_Focus1;
        Vector2 relative2 = m_Particle.Position - m_Focus2;
        double[] v = new double[GetConstraintDimension()];
        v[0] = Vector2.Dot(m_Particle.Velocity, relative1) / relative1.magnitude + Vector2.Dot(m_Particle.Velocity, relative2) / relative2.magnitude;
        return v;
    }



    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        float h = .1f;
        GL.Begin(GL.QUADS);
        GL.Color(Color.blue);
        GL.Vertex(new Vector2(m_Focus1.x - h / 2f, m_Focus1.y - h / 2f));
        GL.Vertex(new Vector2(m_Focus1.x + h / 2f, m_Focus1.y - h / 2f));
        GL.Vertex(new Vector2(m_Focus1.x + h / 2f, m_Focus1.y + h / 2f));
        GL.Vertex(new Vector2(m_Focus1.x - h / 2f, m_Focus1.y + h / 2f));
        GL.End();

        GL.Begin(GL.QUADS);
        GL.Color(Color.blue);
        GL.Vertex(new Vector2(m_Focus2.x - h / 2f, m_Focus2.y - h / 2f));
        GL.Vertex(new Vector2(m_Focus2.x + h / 2f, m_Focus2.y - h / 2f));
        GL.Vertex(new Vector2(m_Focus2.x + h / 2f, m_Focus2.y + h / 2f));
        GL.Vertex(new Vector2(m_Focus2.x - h / 2f, m_Focus2.y + h / 2f));
        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        Vector2 prev = new Vector2(m_Focus1.x + m_Radius, m_Focus1.y);
        for (int i = 12; i <= 360 + 12; i += 12)
        {
            float degInRad = i * Mathf.PI / 180;
            float x = m_A * Mathf.Cos(degInRad);
            float y = m_B * Mathf.Sin(degInRad);
            float xprime = x * Mathf.Cos(m_Tilt) - y * Mathf.Sin(m_Tilt) + m_Midpoint.x;
            float yprime = x * Mathf.Sin(m_Tilt) + y * Mathf.Cos(m_Tilt) + m_Midpoint.y;
            Vector2 vertex = new Vector2(xprime, yprime);
            if (i != 12)
            {
                GL.Vertex(prev);
                GL.Vertex(vertex);
            }
            prev = vertex;
        }
        GL.End();
    }

    public double getAvgMass()
    {
        return m_Particle.Mass;
    }

}
