using UnityEngine;

public class ElipticalEngineScenario : Scenario
{

    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        const float flatness = .975f;
        Vector2 dir = new Vector2(2, -8);
        Vector2 dir2 = new Vector2(2, 8);

        Particle p1 = new Particle(1);
        p1.Position = new Vector2(-6, 4);
        a_ParticleSystem.AddParticle(p1);
        new EllipticalWireConstraint(p1, p1.Position + (1 - flatness) * dir, p1.Position + flatness * dir, a_ParticleSystem);

        Particle p2 = new Particle(1);
        p2.Position = new Vector2(-3, -4);
        a_ParticleSystem.AddParticle(p2);
        new EllipticalWireConstraint(p2, p2.Position + flatness * dir2, p2.Position + (1 - flatness) * dir2, a_ParticleSystem);


        Particle p3 = new Particle(1);
        p3.Position = new Vector2(0, 4);
        a_ParticleSystem.AddParticle(p3);
        new EllipticalWireConstraint(p3, p3.Position + (1 - flatness) * dir, p3.Position + flatness * dir, a_ParticleSystem);

        Particle p4 = new Particle(1);
        p4.Position = new Vector2(3, -4);
        a_ParticleSystem.AddParticle(p4);
        new EllipticalWireConstraint(p4, p4.Position + flatness * dir2, p4.Position + (1 - flatness) * dir2, a_ParticleSystem);

        a_ParticleSystem.AddForce(new HooksLawSpring(p1, p3, (p1.Position - p3.Position).magnitude, 1, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p2, p4, (p2.Position - p4.Position).magnitude, 1, 1));
        //a_ParticleSystem.AddForce(new HooksLawSpring(p3, p4, (p3.Position - p4.Position).magnitude, 1, 1));

        Particle p5 = new Particle(1);
        p5.Position = new Vector2(6, 0);
        a_ParticleSystem.AddParticle(p5);
        new HLineConstraint(p5, a_ParticleSystem);
        Particle p6 = new Particle(1);
        p6.Position = new Vector2(6, 0);
        a_ParticleSystem.AddParticle(p6);
        new HLineConstraint(p6, a_ParticleSystem);
        a_ParticleSystem.AddForce(new HooksLawSpring(p1, p5, 1, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p2, p6, 1, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p3, p5, 1, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p4, p6, 1, 1));


    }
    
}

