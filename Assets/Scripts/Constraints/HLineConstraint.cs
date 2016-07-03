using UnityEngine;


public class HLineConstraint :Constraint 
	{

	private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
	private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
	private readonly Particle m_Particle;
	private float m_Y;

	public HLineConstraint(Particle a_Particle, ParticleSystem a_System)
	{
		m_Particle = a_Particle;
		m_Y = a_Particle.Position.y;
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
		m_MatrixBlockJ.data[0] = 0;
		m_MatrixBlockJ.data[1] = 1;
		//Jdot = 0
		m_MatrixBlockJDot.data[0] = 0;
		m_MatrixBlockJDot.data[1] = 0; 
	}

	public float[] GetValue(ParticleSystem a_ParticleSystem)
	{
		float[] v = new float[GetConstraintDimension ()];
		v [0] = m_Particle.Position.y - m_Y;
		return v;
	}

	public float[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
	{
		float[] v = new float[GetConstraintDimension ()];
		v [0] = m_Particle.Velocity.x;
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
		GL.Vertex(new Vector2(-100,m_Y));
		GL.Vertex(new Vector2(100,m_Y));
		GL.End();
	}
	}


