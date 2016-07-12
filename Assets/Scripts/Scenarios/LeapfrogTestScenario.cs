using UnityEngine;


public class LeapfrogTestScenario : Scenario
{

    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {

        Particle prev = new Particle(1f);
        prev.Position = new Vector2(0, 0);
        a_ParticleSystem.AddParticle(prev);
        new CircularWireConstraint(prev, Vector2.right, a_ParticleSystem);
        Force f = new GravityForce(1f);
        a_ParticleSystem.AddForce(f);
        f = new ViscousDragForce(.05f);
        a_ParticleSystem.AddForce(f);
    }
        
}




