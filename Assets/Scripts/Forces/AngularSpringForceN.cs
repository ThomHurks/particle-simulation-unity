using UnityEngine;

public class AngularSpringForceN : Force
	{
	private readonly Particle m_MassPoint;
	private readonly Particle m_ParticleA;
	private readonly Particle m_ParticleB;
	// Cosine of rest angle
	private float m_Angle;
	// Spring constants
	private float m_SpringConstant;
	private float m_DampingConstant;

	private Vector2 m_ForceA;
	private Vector2 m_ForceB;
	private Vector2 m_ForceM;

	public AngularSpringForceN(Particle a_MassPoint, Particle a_ParticleA, Particle a_ParticleB,
		float a_Angle, float a_SpringConstant, float a_DampingConstant)
	{
		m_MassPoint = a_MassPoint;
		m_ParticleA = a_ParticleA;
		m_ParticleB = a_ParticleB;
		m_Angle = a_Angle;
		m_SpringConstant = a_SpringConstant;
		m_DampingConstant = a_DampingConstant;
		m_ForceA = new Vector2 (0, 0);
		m_ForceB = new Vector2 (0, 0);
		m_ForceM = new Vector2 (0, 0);
	}

	public void ApplyForce(ParticleSystem a_ParticleSystem)
	{
		
		Vector2 r1 = m_ParticleA.Position - m_MassPoint.Position;
		Vector2 r3 = m_ParticleB.Position - m_MassPoint.Position;
		Vector2 r1dot = m_ParticleA.Velocity - m_MassPoint.Velocity;
		Vector2 r3dot = m_ParticleB.Velocity - m_MassPoint.Velocity;

		float r1mag = r1.magnitude;
		float r3mag = r3.magnitude;
		float r1r3mag = r1.magnitude * r3.magnitude;
		float r1r3magsq = r1r3mag * r1r3mag;
		float r1dotr3 = Vector2.Dot(r1,r3);
		float r1dotr1d = Vector2.Dot (r1, r1dot);
		float r3dotr3d = Vector2.Dot (r3, r3dot);
		float r1dotr3d = Vector2.Dot (r1, r3dot);
		float r3dotr1d = Vector2.Dot (r3, r1dot);
		float y = r1dotr3 / (r1r3mag);
		if (1f - y*y < .01) {//parallel line, stop force appliance, will get an /0

			m_ForceA = new Vector2(0,0);
			m_ForceB = new Vector2(0,0);
			m_ForceM = new Vector2(0,0);
			return;
		}
		float dCdy = 1f / (Mathf.Sqrt (1 - y * y));
		float ydot = (r1r3mag * (r1dotr3d + r3dotr1d) - r1dotr3 * (r1dotr1d * r3mag / r1mag + r3dotr3d * r1mag / r3mag)) / r1r3magsq;
		float Cdot = dCdy * ydot;
		float C = Mathf.Acos (r1dotr3/r1r3mag)-m_Angle;
		Vector2 dydr1 = (1f / r1r3magsq) * (r1r3mag * r3 - (r3mag / r1mag) * r1);
		Vector2 dydr3 = (1f / r1r3magsq) * (r1r3mag * r1 - (r1mag / r3mag) * r3);
		Vector2 dCdr1 = dCdy * dydr1;
		Vector2 dCdr3 = dCdy * dydr3;
		float x = (m_SpringConstant * C + m_DampingConstant * Cdot);
		Vector2 f1 = x * dCdr1;
		Vector2 f2 = -x * (dCdr1 + dCdr3);
		Vector2 f3 = x * dCdr3;

		m_ParticleA.ForceAccumulator += f1;
		m_MassPoint.ForceAccumulator += f2;
		m_ParticleB.ForceAccumulator += f3;
		m_ForceA = f1;
		m_ForceB = f3;
		m_ForceM = f2;


	}

	public void Draw()
	{
		GL.Begin(GL.LINES);
		GL.Color(new Color(0.3f, 0.3f, 1f));
		GL.Vertex(m_MassPoint.Position);
		GL.Vertex(m_ParticleA.Position);
		GL.End();

		GL.Begin(GL.LINES);
		GL.Color(new Color(0.3f, 0.3f, 1f));
		GL.Vertex(m_MassPoint.Position);
		GL.Vertex(m_ParticleB.Position);
		GL.End();


		GL.Begin(GL.LINES);
		GL.Color(new Color(0.8f, 0.8f, .1f));
		GL.Vertex(m_MassPoint.Position + m_ForceM);
		GL.Vertex(m_MassPoint.Position);
		GL.End();

		GL.Begin(GL.LINES);
		GL.Color(new Color(0.8f, 0.8f, .1f));
		GL.Vertex(m_ParticleA.Position + m_ForceA);
		GL.Vertex(m_ParticleA.Position);
		GL.End();

		GL.Begin(GL.LINES);
		GL.Color(new Color(0.8f, 0.8f, .1f));
		GL.Vertex(m_ParticleB.Position + m_ForceB);
		GL.Vertex(m_ParticleB.Position);
		GL.End();
	}
}


