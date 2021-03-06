﻿using UnityEngine;

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

        Force angleforce = new AngularSpringForce(particle2, particle1, particle3, Mathf.PI / 2, 1f, 0.1f);
        a_ParticleSystem.AddForce(angleforce);
        Force spring1 = new HooksLawSpring(particle1, particle2, 2, 1, .1f);
        Force spring2 = new HooksLawSpring(particle3, particle2, 2, 1, .1f);
        a_ParticleSystem.AddForce(spring1);
        a_ParticleSystem.AddForce(spring2);
        Force gravityForce = new GravityForce(.001f);
        a_ParticleSystem.AddForce(gravityForce);

        Particle p6 = new Particle(.5f);
        p6.Position = new Vector2(0, -4);
        a_ParticleSystem.AddParticle(p6);
        Particle p7 = new Particle(1);
        p7.Position = new Vector2(0, -5f);
        a_ParticleSystem.AddParticle(p7);
        new RodConstraint(p6, p7, a_ParticleSystem);


        Particle p5 = new Particle(500f);
        p5.Position = new Vector2(2, 2);
        a_ParticleSystem.AddParticle(p5);
        new CircularWireConstraint(p5, p5.Position + Vector2.right, a_ParticleSystem);

        Particle p8 = new Particle(0.5f);
        p8.Position = new Vector2(-3, -1);
        a_ParticleSystem.AddParticle(p8);
        new FixedPointConstraint(p8, a_ParticleSystem);


        Particle p9 = new Particle(.5f);
        p9.Position = new Vector2(-3, -2);
        a_ParticleSystem.AddParticle(p9);
        new HLineConstraint(p9, a_ParticleSystem);

        Particle p10 = new Particle(.5f);
        p10.Position = new Vector2(5, -2);
        a_ParticleSystem.AddParticle(p10);
        new VLineConstraint(p10, a_ParticleSystem);


        Particle q1 = new Particle(.5f);
        q1.Position = new Vector2(-3, 3);
        a_ParticleSystem.AddParticle(q1);
        new EllipticalWireConstraint(q1, q1.Position + new Vector2(1f, -.3f), q1.Position + new Vector2(-1f, -1f), a_ParticleSystem);

        Force dragForce = new ViscousDragForce(.2f);
        a_ParticleSystem.AddForce(dragForce);




    }
}
