using UnityEngine;

public class CirclesAndSpringsScenario : Scenario
{
	public void CreateScenario(ParticleSystem a_ParticleSystem)
	{
		Particle p1 = new Particle (1f);
		p1.Position = new Vector2 (3, 4);
		Particle p2 = new Particle (1f);
		p2.Position = new Vector2 (-3, 4);
		a_ParticleSystem.AddParticle (p1);
		a_ParticleSystem.AddParticle (p2);
		new CircularWireConstraint (p1, p1.Position + Vector2.right, a_ParticleSystem);
		new CircularWireConstraint (p2, p2.Position + Vector2.left, a_ParticleSystem);
		Particle p3 = new Particle (1);
		p3.Position = new Vector2 (3, 2);
		Particle p4 = new Particle (1);
		p4.Position = new Vector2 (-3, 2);
		Particle p5 = new Particle (1);
		p5.Position = new Vector2 (-4, 0);
		Particle p6 = new Particle (1);
		p6.Position = new Vector2 (0, 0);
		Particle p7 = new Particle (1);
		p7.Position = new Vector2 (4, 0);
		a_ParticleSystem.AddParticle (p3);
		a_ParticleSystem.AddParticle (p4);
		a_ParticleSystem.AddParticle (p5);
		a_ParticleSystem.AddParticle (p6);
		a_ParticleSystem.AddParticle (p7);

		Force f1 = new HooksLawSpring (p1, p3, 1, 1, 1f);
		Force f2 = new HooksLawSpring (p2, p4, 1, 1, 1f);
		Force f3 = new HooksLawSpring (p3, p7, 1, 1, 1f);
		Force f4 = new HooksLawSpring (p3, p6, 1, 1, 1f);
		Force f5 = new HooksLawSpring (p4, p6, 1, 1, 1f);
		Force f6 = new HooksLawSpring (p4, p5, 1, 1, 1f);
		Force f7 = new HooksLawSpring (p7, p6, 3, 1, 1f);
		Force f8 = new HooksLawSpring (p7, p5, 3, 1, 1f);
		Force f9 = new HooksLawSpring (p6, p5, 3, 1, 1f);
		a_ParticleSystem.AddForce (f1);
		a_ParticleSystem.AddForce (f2);
		a_ParticleSystem.AddForce (f3);
		a_ParticleSystem.AddForce (f4);
		a_ParticleSystem.AddForce (f5);
		a_ParticleSystem.AddForce (f6);
		a_ParticleSystem.AddForce (f7	);
		a_ParticleSystem.AddForce (f8	);
		a_ParticleSystem.AddForce (f9	);

		a_ParticleSystem.AddForce (new GravityForce(.1f));
	}
}

