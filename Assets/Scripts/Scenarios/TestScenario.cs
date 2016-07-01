using UnityEngine;

public class TestScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        Particle particle1 = new Particle(1000f);
        particle1.Position = new Vector2(-2f, 0f);
        a_ParticleSystem.AddParticle(particle1);
        Particle particle2 = new Particle(1f);
        particle2.Position = new Vector2(0f, 0f);
        a_ParticleSystem.AddParticle(particle2);
        Particle particle3 = new Particle(1f);
        particle3.Position = new Vector2(4f, 4f);
        a_ParticleSystem.AddParticle(particle3);

		Force angleforce = new AngularSpringForce (particle2, particle1, particle3, 2f, 1f, 0.1f);
		a_ParticleSystem.AddForce(angleforce);
		Force spring1 = new HooksLawSpring (particle1, particle2, 2, 1, .1f);
		Force spring2 = new HooksLawSpring (particle3, particle2, 2, 1, .1f);
		a_ParticleSystem.AddForce(spring1);
		a_ParticleSystem.AddForce(spring2);
        Force gravityForce = new GravityForce(1f);
        //a_ParticleSystem.AddForce(gravityForce);

        Force dragForce = new ViscousDragForce(2f);
        a_ParticleSystem.AddForce(dragForce);

      
    }
}
