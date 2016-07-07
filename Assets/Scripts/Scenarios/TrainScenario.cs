using UnityEngine;
using System.Collections;

public class TrainScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        Particle p1 = new Particle(1f);
        p1.Position = new Vector2(-3, 0);
        a_ParticleSystem.AddParticle(p1);
        CircularWireConstraint c1 = new CircularWireConstraint(p1, new Vector2(-2, 0), a_ParticleSystem);

        Particle p3 = new Particle(1f);
        p3.Position = p1.Position + new Vector2(0.5f, 0f);
        a_ParticleSystem.AddParticle(p3);
        HooksLawSpring s1 = new HooksLawSpring(p1, p3, 0.5f, 0.1f, 0.1f);
        a_ParticleSystem.AddForce(s1);

        Particle p2 = new Particle(1f);
        p2.Position = new Vector2(2, 0);
        a_ParticleSystem.AddParticle(p2);
        CircularWireConstraint c2 = new CircularWireConstraint(p2, new Vector2(3, 0), a_ParticleSystem);

        Particle p4 = new Particle(1f);
        p4.Position = p2.Position + new Vector2(0.5f, 0f);
        a_ParticleSystem.AddParticle(p4);
        HooksLawSpring s2 = new HooksLawSpring(p2, p4, 0.5f, 0.1f, 0.1f);
        a_ParticleSystem.AddForce(s2);

        RodConstraint c3 = new RodConstraint(p3, p4, a_ParticleSystem);
    }
}
