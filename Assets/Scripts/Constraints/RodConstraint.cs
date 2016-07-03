using UnityEngine;

public class RodConstraint : Constraint
{

	private static readonly bool OLD= false;

    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ_A;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ_B;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot_A;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot_B;
    private readonly Particle m_ParticleA;
    private readonly Particle m_ParticleB;
    private readonly float m_DistanceSquared;
    private readonly float m_Distance;

    public RodConstraint(Particle a_ParticleA, Particle a_ParticleB, ParticleSystem a_System)
    {
        int i = a_System.AddConstraint(this);
		Debug.Log("Creating " + (OLD?"old":"new") + " rod constraint with index " + i);
		m_Distance = (a_ParticleA.Position-a_ParticleB.Position).magnitude;
		m_DistanceSquared = m_Distance * m_Distance;
        m_ParticleA = a_ParticleA;
        m_ParticleB = a_ParticleB;
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        // We can probably de-duplicate this code:
        int j_A = a_System.GetParticleIndex(a_ParticleA) * a_System.GetParticleDimension();
        int j_B = a_System.GetParticleIndex(a_ParticleB) * a_System.GetParticleDimension();
        
        m_MatrixBlockJ_A = a_System.MatrixJ.CreateMatrixBlock(i, j_A, iLength, jLength);
        m_MatrixBlockJ_B = a_System.MatrixJ.CreateMatrixBlock(i, j_B, iLength, jLength);
        m_MatrixBlockJDot_A = a_System.MatrixJDot.CreateMatrixBlock(i, j_A, iLength, jLength);
        m_MatrixBlockJDot_B = a_System.MatrixJDot.CreateMatrixBlock(i, j_B, iLength, jLength);
    }

	public void UpdateJacobians(ParticleSystem a_ParticleSystem)
	{
		if (OLD) {
			UpdateJacobiansOld (a_ParticleSystem);
		} else {
			UpdateJacobiansNew (a_ParticleSystem);
		}
	}

	public float[] GetValue(ParticleSystem a_ParticleSystem)
	{
		return OLD ? GetValueOld (a_ParticleSystem) : GetValueNew (a_ParticleSystem);
	}

	public float[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
	{
		return OLD ? GetDerivativeValueOld (a_ParticleSystem) : GetDerivativeValueNew (a_ParticleSystem);
	}

    public void UpdateJacobiansNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 l    = m_ParticleA.Position - m_ParticleB.Position; 
        Vector2 ldot = m_ParticleA.Velocity - m_ParticleB.Velocity;
        float lmag = l.magnitude;
        if (Mathf.Abs(lmag) < float.Epsilon)
        {
            lmag = float.Epsilon;
        }
        Vector2 dCdl = l.normalized;
        Vector2 t1 = lmag * ldot;
        Vector2 t2 = (Vector2.Dot(l, ldot) / lmag) * l;
        Vector2 dCdotdl = (t1 - t2);
        dCdotdl = (1f / (lmag * lmag)) * dCdotdl;


        m_MatrixBlockJ_A.data[0] = dCdl.x;
        m_MatrixBlockJ_A.data[1] = dCdl.y;
        m_MatrixBlockJDot_A.data[0] = dCdotdl.x;
        m_MatrixBlockJDot_A.data[1] = dCdotdl.y;

        m_MatrixBlockJ_B.data[0] = -dCdl.x;
        m_MatrixBlockJ_B.data[1] = -dCdl.y;
        m_MatrixBlockJDot_B.data[0] = -dCdotdl.x;
        m_MatrixBlockJDot_B.data[1] = -dCdotdl.y;
    }

    public float[] GetValueNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 l = m_ParticleA.Position - m_ParticleB.Position;
		float[] v = new float[GetConstraintDimension ()];
		v [0] = l.magnitude - m_Distance;
		return v;
    }

    public float[] GetDerivativeValueNew(ParticleSystem a_ParticleSystem)
    {
        Vector2 l = m_ParticleA.Position - m_ParticleB.Position;
        Vector2 ldot = m_ParticleA.Velocity - m_ParticleB.Velocity;
        float mag = l.magnitude;
        if (Mathf.Abs(mag) < float.Epsilon)
        {
            mag = float.Epsilon;
        }
		float[] v = new float[GetConstraintDimension ()];
		v [0] = Vector2.Dot(l, ldot) / mag;
		return v;
    }




    public void UpdateJacobiansOld(ParticleSystem a_ParticleSystem)
	{
		Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
		Vector2 deltaVelocity = m_ParticleA.Velocity - m_ParticleB.Velocity;

		m_MatrixBlockJ_A.data[0] = deltaPosition.x;
		m_MatrixBlockJ_A.data[1] = deltaPosition.y;
		m_MatrixBlockJDot_A.data[0] = deltaVelocity.x;
		m_MatrixBlockJDot_A.data[1] = deltaVelocity.y;

		m_MatrixBlockJ_B.data[0] = -deltaPosition.x;
		m_MatrixBlockJ_B.data[1] = -deltaPosition.y;
		m_MatrixBlockJDot_B.data[0] = -deltaVelocity.x;
		m_MatrixBlockJDot_B.data[1] = -deltaVelocity.y;
	}

	public float[] GetValueOld(ParticleSystem a_ParticleSystem)
	{
		Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
		float[] v = new float[GetConstraintDimension ()];
		v [0] = (deltaPosition.sqrMagnitude - m_DistanceSquared) / 2f;
		return v;
	}

	public float[] GetDerivativeValueOld(ParticleSystem a_ParticleSystem)
	{
		Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
		Vector2 deltaVelocity = m_ParticleA.Velocity - m_ParticleB.Velocity;
		float[] v = new float[GetConstraintDimension ()];
		v [0] = Vector2.Dot(deltaVelocity, deltaPosition);
		return v;
	}

    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.8f, 0.7f, 0.6f));
        GL.Vertex(m_ParticleA.Position);
        GL.Color(new Color(0.8f, 0.7f, 0.6f));
        GL.Vertex(m_ParticleB.Position);
        GL.End();
    }
    
}
