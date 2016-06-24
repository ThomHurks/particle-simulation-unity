using UnityEngine;

public class FixedPointConstraint : Constraint
{
	private readonly static bool OLD = true;

    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private readonly Particle m_Particle;
    private Vector2 m_Center;

    public FixedPointConstraint(Particle a_Particle, ParticleSystem a_System)
    {
        m_Particle = a_Particle;
		m_Center = new Vector2 (a_Particle.Position.x, a_Particle.Position.y);
        // We can probably de-duplicate this code:
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int i = a_System.AddConstraint(this);

		Debug.Log("Creating fixed point constraint with index " + i);
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }

	public void UpdateJacobians(ParticleSystem a_ParticleSystem)
	{
		if (OLD) {
			UpdateJacobiansOld (a_ParticleSystem);
		} else {
			UpdateJacobiansNew (a_ParticleSystem);
		}
	}

	public float GetValue(ParticleSystem a_ParticleSystem)
	{
		return OLD ? GetValueOld (a_ParticleSystem) : GetValueNew (a_ParticleSystem);
	}

	public float GetDerivativeValue(ParticleSystem a_ParticleSystem)
	{
		return OLD ? GetDerivativeValueOld (a_ParticleSystem) : GetDerivativeValueNew (a_ParticleSystem);
	}



    public void UpdateJacobiansOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
        m_MatrixBlockJ.data[0] = relative.x;
        m_MatrixBlockJ.data[1] = relative.y;
        m_MatrixBlockJDot.data[0] = m_Particle.Velocity.x;
        m_MatrixBlockJDot.data[1] = m_Particle.Velocity.y;
    }

    public float GetValueOld(ParticleSystem a_ParticleSystem)
    {
        Vector2 relative = m_Particle.Position - m_Center;
		return relative.sqrMagnitude;
    }

	public float GetDerivativeValueOld(ParticleSystem a_ParticleSystem)
	{
		Vector2 relative = m_Particle.Position - m_Center;
		return Vector2.Dot(m_Particle.Velocity, relative);
	}

    

	public void UpdateJacobiansNew(ParticleSystem a_ParticleSystem)
	{
		Vector2 l = m_Particle.Position - m_Center; 
		Vector2 ldot = m_Particle.Velocity;
		float lmag = l.magnitude;
		if (Mathf.Abs(lmag) < float.Epsilon)
		{
			lmag = float.Epsilon;
		}
		Vector2 dCdl = l.normalized;
		Vector2 t1 = lmag * ldot ;
		Vector2 t2 = (Vector2.Dot (l, ldot) / lmag) * l;
		Vector2 dCdotdl = (t1 - t2);
		dCdotdl = (1f/(lmag*lmag)) * dCdotdl;

		m_MatrixBlockJ.data[0] = dCdl.x;
		m_MatrixBlockJ.data[1] = dCdl.y;
		m_MatrixBlockJDot.data[0] = dCdotdl.x;
		m_MatrixBlockJDot.data[1] = dCdotdl.y; 
	}

	public float GetValueNew(ParticleSystem a_ParticleSystem)
	{
		Vector2 relative = m_Particle.Position - m_Center;
		return relative.magnitude;
	}

	public float GetDerivativeValueNew(ParticleSystem a_ParticleSystem)
	{
		Vector2 relative = m_Particle.Position - m_Center;
		return Vector2.Dot(m_Particle.Velocity, relative)/relative.magnitude;
	}



    public int GetConstraintDimension()
    {
        return 1;
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
