using UnityEngine;


public class SpeedTestScenario : Scenario
{

    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {

        Particle prev = new Particle(1f);
        prev.Position = new Vector2(0, 1);
        a_ParticleSystem.AddParticle(prev);
        prev.Velocity = Vector2.right * 40;
        new EllipticalWireConstraint(prev, Vector2.left * Mathf.Sqrt(3), Vector2.right * Mathf.Sqrt(3), a_ParticleSystem); // 2x4 ellipse
        //a_ParticleSystem.AddForce(new ViscousDragForce(0.01f));
       
    }
        
}




