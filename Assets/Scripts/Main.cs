using UnityEngine;

public sealed class Main : MonoBehaviour
{
	private ParticleSystem m_ParticleSystem;
	private Solver m_Solver;

	void Awake()
	{
		m_ParticleSystem = new ParticleSystem();
		m_Solver = new EulerSolver();
		Particle particle1 = new Particle(0.1f);
		m_ParticleSystem.AddParticle(particle1);
		Particle particle2 = new Particle(0.1f);
		m_ParticleSystem.AddParticle(particle2);
		Force springForce1 = new HooksLawSpring(particle1, particle2, 5f, 0.1f, 0.1f);
		m_ParticleSystem.AddForce(springForce1);
		new CircularWireConstraint(particle1, particle1.Position, 5f, m_ParticleSystem);
	}

	void Update()
	{
		m_Solver.Step(m_ParticleSystem, Time.deltaTime);
	}
}
