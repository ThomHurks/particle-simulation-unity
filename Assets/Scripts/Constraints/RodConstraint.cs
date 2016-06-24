using UnityEngine;

public class RodConstraint : Constraint
{
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ_A;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ_B;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot_A;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot_B;
    private readonly Particle m_ParticleA;
	private readonly Particle m_ParticleB;
	private readonly float m_DistanceSquared;
	private readonly float m_Distance;

    public RodConstraint(Particle a_ParticleA, Particle a_ParticleB, float a_Distance, ParticleSystem a_System)
    {
		int i = a_System.AddConstraint(this);
		m_DistanceSquared = a_Distance * a_Distance;
		m_Distance = a_Distance;
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
        Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position; //l
        Vector2 deltaVelocity = m_ParticleA.Velocity - m_ParticleB.Velocity;//ldot

        m_MatrixBlockJ_A.data[0] = deltaPosition.x;
        m_MatrixBlockJ_A.data[1] = deltaPosition.y;
        m_MatrixBlockJDot_A.data[0] = deltaVelocity.x;
        m_MatrixBlockJDot_A.data[1] = deltaVelocity.y;

        m_MatrixBlockJ_B.data[0] = -deltaPosition.x;
        m_MatrixBlockJ_B.data[1] = -deltaPosition.y;
        m_MatrixBlockJDot_B.data[0] = -deltaVelocity.x;
        m_MatrixBlockJDot_B.data[1] = -deltaVelocity.y;
    }

    public float GetValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
		return deltaPosition.magnitude - m_Distance;
    }

    public float GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
        Vector2 deltaVelocity = m_ParticleA.Velocity - m_ParticleB.Velocity;
        return Vector2.Dot(deltaVelocity, deltaPosition)/deltaPosition.magnitude;
    }

	/*public void UpdateJacobians(ParticleSystem a_ParticleSystem)
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

	public float GetValue(ParticleSystem a_ParticleSystem)
	{
		Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
		return (deltaPosition.sqrMagnitude - m_DistanceSquared) / 2f;
	}

	public float GetDerivativeValue(ParticleSystem a_ParticleSystem)
	{
		Vector2 deltaPosition = m_ParticleA.Position - m_ParticleB.Position;
		Vector2 deltaVelocity = m_ParticleA.Velocity - m_ParticleB.Velocity;
		return Vector2.Dot(deltaVelocity, deltaPosition);
	}*/

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
