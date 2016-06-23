using UnityEngine;
using System.Collections;

public class HooksLawSpring : Force
{
	private Particle m_ParticleA;
	private Particle m_ParticleB;
	private float m_RestLength;
	private float m_SpringConstant;
	private float m_DampingConstant;

	public HooksLawSpring(Particle a_ParticleA, Particle a_ParticleB, float a_RestLength,
						  float a_SpringConstant, float a_DampingConstant)
	{
		m_ParticleA = a_ParticleA;
		m_ParticleB = a_ParticleB;
		m_RestLength = a_RestLength;
		m_SpringConstant = a_SpringConstant;
		m_DampingConstant = a_DampingConstant;
	}

	public void ApplyForce(ParticleSystem a_ParticleSystem)
	{
		Vector2 l = m_ParticleA.Position - m_ParticleB.Position;
		Vector2 i = m_ParticleA.Velocity - m_ParticleB.Velocity;
		float lMagnitude = l.magnitude;
		Vector2 f_a = -(m_SpringConstant * (lMagnitude - m_RestLength) + m_DampingConstant * (Vector2.Dot(i, l) / lMagnitude)) * l.normalized;
		m_ParticleA.ForceAccumulator += f_a;
		m_ParticleB.ForceAccumulator -= f_a;
	}
}
