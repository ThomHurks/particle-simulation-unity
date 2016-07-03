using UnityEngine;

public class TestScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        Particle particle1 = new Particle(1000f);
        particle1.Position = new Vector2(-2f, 0f);
        a_ParticleSystem.AddParticle(particle1);
        Particle particle2 = new Particle(1f);
        particle2.Position = new Vector2(0f, 5f);
        a_ParticleSystem.AddParticle(particle2);
        Particle particle3 = new Particle(1f);
        particle3.Position = new Vector2(4f, 4f);
        a_ParticleSystem.AddParticle(particle3);

		Force angleforce = new AngularSpringForceN (particle2, particle1, particle3, Mathf.PI, 1f, 0.1f);
		a_ParticleSystem.AddForce(angleforce);
		Force spring1 = new HooksLawSpring (particle1, particle2, 2, 1, .1f);
		Force spring2 = new HooksLawSpring (particle3, particle2, 2, 1, .1f);
		a_ParticleSystem.AddForce(spring1);
		a_ParticleSystem.AddForce(spring2);
        Force gravityForce = new GravityForce(.001f);
        a_ParticleSystem.AddForce(gravityForce);


		Particle p4 = new Particle (1);
		p4.Position = new Vector2 (-2, -2);
		a_ParticleSystem.AddParticle (p4);
		Constraint c = new FixedPointConstraint (p4, a_ParticleSystem);
		//a_ParticleSystem.AddConstraint (c);

		Particle p5 = new Particle (1);
		p5.Position = new Vector2 (2, 2);
		a_ParticleSystem.AddParticle (p5);
		Constraint c2 = new CircularWireConstraint (p5,p5.Position+Vector2.right,a_ParticleSystem);


		Particle p6 = new Particle (1);
		p6.Position = new Vector2 (2, -3);
		a_ParticleSystem.AddParticle (p6);
		Particle p7 = new Particle (1);
		p7.Position = new Vector2 (1, -3);
		a_ParticleSystem.AddParticle (p7);
		Constraint c3 = new RodConstraint (p6,p7,a_ParticleSystem);

		Particle p8 = new Particle (1);
		p8.Position = new Vector2 (-3, -1);
		a_ParticleSystem.AddParticle (p8);
		Constraint c4 = new FixedPointConstraint (p8,a_ParticleSystem);


		Particle p9 = new Particle (1);
		p9.Position = new Vector2 (-3, -2);
		a_ParticleSystem.AddParticle (p9);
		Constraint c5 = new HLineConstraint (p9,a_ParticleSystem);

		Particle p10 = new Particle (1);
		p10.Position = new Vector2 (5, -2);
		a_ParticleSystem.AddParticle (p10);
		Constraint c6 = new VLineConstraint (p10,a_ParticleSystem);


		Force dragForce = new ViscousDragForce(2f);
		a_ParticleSystem.AddForce(dragForce);




    }
}
