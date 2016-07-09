using UnityEngine;

public class DualCircleScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        Particle p1 = new Particle(1f);
        p1.Position = new Vector2(0, 3);
        a_ParticleSystem.AddParticle(p1);
        new CircularWireConstraint(p1, p1.Position + new Vector2(1, -1), a_ParticleSystem);
        new CircularWireConstraint(p1, p1.Position + new Vector2(-1, -1), a_ParticleSystem);

        Particle p2 = new Particle(1);
        a_ParticleSystem.AddParticle(p2);
        p2.Position = new Vector2(3, 3);
        new RodConstraint(p1, p2, a_ParticleSystem);

        Particle p3 = new Particle(1);
        Particle p4 = new Particle(1);
        Particle p5 = new Particle(1);
        p3.Position = new Vector2(3, 1);
        p4.Position = new Vector2(2, 0);
        p5.Position = new Vector2(4, 0);
        a_ParticleSystem.AddParticle(p3);
        a_ParticleSystem.AddParticle(p4);
        a_ParticleSystem.AddParticle(p5);
        a_ParticleSystem.AddForce(new HooksLawSpring(p3, p4, Mathf.Sqrt(2), 2, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p3, p5, Mathf.Sqrt(2), 2, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p5, p4, 2, 2, 1));

        a_ParticleSystem.AddForce(new HooksLawSpring(p2, p3, 2, 5, 1));

        a_ParticleSystem.AddForce(new GravityForce(.2f));

        Particle p6 = new Particle(1);
        p6.Position = new Vector2(-3, 4);
        a_ParticleSystem.AddParticle(p6);
        new HLineConstraint(p6, a_ParticleSystem);
        new VLineConstraint(p6, a_ParticleSystem);
    }
}

