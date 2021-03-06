﻿using UnityEngine;


public class VLineConstraint :Constraint
{
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJ;
    private BlockSparseMatrix.MatrixBlock m_MatrixBlockJDot;
    private readonly Particle m_Particle;
    private readonly double m_X;

    public VLineConstraint(Particle a_Particle, ParticleSystem a_System)
    {
        m_Particle = a_Particle;
        m_X = a_Particle.Position.x;
        // We can probably de-duplicate this code:
        int j = a_System.GetParticleIndex(a_Particle) * a_System.GetParticleDimension();
        int i = a_System.AddConstraint(this);

        Debug.Log("Creating VLine constraint with index " + i);
        int iLength = GetConstraintDimension();
        int jLength = a_System.GetParticleDimension();
        m_MatrixBlockJ = a_System.MatrixJ.CreateMatrixBlock(i, j, iLength, jLength);
        m_MatrixBlockJDot = a_System.MatrixJDot.CreateMatrixBlock(i, j, iLength, jLength);
    }

    public void UpdateJacobians(ParticleSystem a_ParticleSystem)
    {
        m_MatrixBlockJ.data[0] = 1;
        m_MatrixBlockJ.data[1] = 0;
        //Jdot = 0
        m_MatrixBlockJDot.data[0] = 0;
        m_MatrixBlockJDot.data[1] = 0; 
    }

    public double[] GetValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = m_Particle.Position.x - m_X;
        return v;
    }

    public double[] GetDerivativeValue(ParticleSystem a_ParticleSystem)
    {
        double[] v = new double[GetConstraintDimension()];
        v[0] = m_Particle.Velocity.x;
        return v;
    }

    public int GetConstraintDimension()
    {
        return 1;
    }

    public void Draw()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 0.5f, 0.5f));
        GL.Vertex(new Vector2((float)m_X, -100));
        GL.Vertex(new Vector2((float)m_X, 100));
        GL.End();
    }

    public double getAvgMass()
    {
        return m_Particle.Mass;
    }

}


