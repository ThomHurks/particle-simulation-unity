using System;
using UnityEngine;
using System.Collections;

public class EulerSolver : Solver
{
    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions;
        float[] temp1 = new float[particleDimensions];
        float[] temp2 = new float[particleDimensions];
        a_ParticleSystem.ParticleDerivative(temp1);
        ScaleVector(temp1, a_DeltaTime);
        a_ParticleSystem.ParticlesGetState(temp2);
        AddVectors(temp1, temp2, temp2);
        a_ParticleSystem.ParticlesSetState(temp2);
        a_ParticleSystem.Time += a_DeltaTime;
    }

}
