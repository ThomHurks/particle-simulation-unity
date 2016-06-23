﻿using System;
using UnityEngine;
using System.Collections.Generic;

public class ParticleSystem
{
	private List<Particle> m_Particles;
	private List<Force> m_Forces;
	private List<Constraint> m_Constraints;
	private BlockSparseMatrix m_J;
	private BlockSparseMatrix m_JDot;
	private float m_Time;
	private float m_ConstraintKS = 0.5f;
	private float m_ConstraintKD = 0.5f;

	public int Count
	{
		get { return m_Particles.Count; }
	}

	public float Time
	{
		get { return m_Time; }
		set { m_Time = value; }
	}

	public List<Particle> Particles
	{
		get { return m_Particles; }
	}

	public BlockSparseMatrix MatrixJ
	{
		get { return m_J; }
	}

	public BlockSparseMatrix MatrixJDot
	{
		get { return m_JDot; }
	}

	public ParticleSystem()
	{
		m_Particles = new List<Particle>();
		m_Forces = new List<Force>();
		m_Constraints = new List<Constraint>();
		m_J = new BlockSparseMatrix();
		m_JDot = new BlockSparseMatrix();
	}

	public int GetParticleDimension()
	{
		return 2;
	}

	public void AddParticle(Particle a_Particle)
	{
		m_Particles.Add(a_Particle);
	}

	public void AddForce(Force a_Force)
	{
		m_Forces.Add(a_Force);
	}

	public int AddConstraint(Constraint a_Constraint)
	{
		int numConstraints = m_Constraints.Count;
		int j = 0;
		for (int i = 0; i < numConstraints; ++i)
		{
			j += m_Constraints[i].GetConstraintDimension();
		}
		m_Constraints.Add(a_Constraint);
		return j;
	}

	public int GetParticleIndex(Particle a_Particle)
	{
		int numParticles = m_Particles.Count;
		for (int i = 0; i < numParticles; ++i)
		{
			if (m_Particles[i] == a_Particle)
			{
				return i;
			}
		}
		throw new System.Exception("Particle not found in particle list!");
	}

	public void ComputeForces()
	{
		int numForces = m_Forces.Count;
		for (int i = 0; i < numForces; ++i)
		{
			m_Forces[i].ApplyForce(this);
		}
	}

	private void ComputeConstraints()
	{
		int numConstraints = m_Constraints.Count;
		for (int i = 0; i < numConstraints; ++i)
		{
			m_Constraints [i].UpdateJacobians(this);
		}
		SolveEquation11(m_ConstraintKS, m_ConstraintKD);
	}

	private void SolveEquation11(float a_Ks, float a_Kd)
	{
		float[] qdot = ParticlesGetVelocities();
		float[] W = ParticlesInverseMassMatrix();
		float[] Q = ParticlesGetForces();
		float[] C = ConstraintsGetValues();
		float[] CDot = ConstraintsGetDerivativeValues();
		int n = GetParticleDimension() * m_Particles.Count;
		// JDot times qdot.
		float[] JDotqdot = new float[n];
		m_JDot.MatrixTimesVector(qdot, JDotqdot);
		// W times Q.
		float[] WQ = new float[n];
		for (int i = 0; i < n; ++i)
		{
			WQ[i] = W[i] * Q[i];
		}
		// J times WQ.
		float[] JWQ = new float[n];
		m_J.MatrixTimesVector(WQ, JWQ);
		// Compute the RHS of equation 11.
		float[] RHS = new float[n];
		for (int i = 0; i < n; ++i)
		{
			RHS[i] = -JDotqdot[i] - JWQ[i] - a_Ks * C[i] - a_Kd * CDot[i];
		}
		// Set up implicit matrix of LHS and solve.
		Eq11LHS LHS = new Eq11LHS(m_J, W);
		LinearSolver solver = new LinearSolver();
		float[] lambda = new float[n];
		int stepsPerformed = 0;
		solver.ConjGrad(n, LHS, lambda, RHS, 0.01f, -1, out stepsPerformed);
		Debug.Log(stepsPerformed);
		float[] QHat = new float[n];
		m_J.MatrixTransposeTimesVector(lambda, QHat);
		for (int i = 0; i < m_Particles.Count; ++i)
		{
			m_Particles[i].ForceAccumulator += new Vector2(lambda[i * 2], lambda[(i * 2) + 1]);
		}
	}

	float[] ConstraintsGetValues()
	{
		// Gather constraint values into vector C.
		int numConstraints = m_Constraints.Count;
		float[] C = new float[numConstraints];
		for (int i = 0; i < numConstraints; ++i)
		{
			C[i] = m_Constraints[i].GetValue(this);
		}
		return C;
	}

	float[] ConstraintsGetDerivativeValues()
	{
		// Gather constraint values into vector CDot.
		int numConstraints = m_Constraints.Count;
		float[] CDot = new float[numConstraints];
		for (int i = 0; i < numConstraints; ++i)
		{
			CDot[i] = m_Constraints[i].GetDerivativeValue(this);
		}
		return CDot;
	}

	float[] ParticlesGetPositions()
	{
		// Gather positions into state vector q.
		int numParticles = m_Particles.Count;
		int particlePosDims = numParticles * 2;
		float[] q = new float[particlePosDims];
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			int curIndex = 2 * i;
			q[curIndex] = curParticle.Position.x;
			q[curIndex + 1] = curParticle.Position.y;
		}
		return q;
	}

	float[] ParticlesGetVelocities()
	{
		// Gather velocities into state vector qdot.
		int numParticles = m_Particles.Count;
		int particlePosDims = numParticles * 2;
		float[] qdot = new float[particlePosDims];
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			int curIndex = 2 * i;
			qdot[curIndex] = curParticle.Velocity.x;
			qdot[curIndex + 1] = curParticle.Velocity.y;
		}
		return qdot;
	}

	float[] ParticlesInverseMassMatrix()
	{
		// Construct inverse, W, of diagonal of mass matrix M as a vector.
		int numParticles = m_Particles.Count;
		int particleMassDims = numParticles * 2;
		float[] W = new float[particleMassDims];
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			int curIndex = 2 * i;
			float massInverse = 1 / curParticle.Mass;
			W[curIndex] = massInverse;
			W[curIndex + 1] = massInverse;
		}
		return W;
	}

	float[] ParticlesGetForces()
	{
		// Gather forces into global force vector Q.
		int numParticles = m_Particles.Count;
		int particleForceDims = numParticles * 2;
		float[] Q = new float[particleForceDims];
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			int curIndex = 2 * i;
			Q[curIndex] = curParticle.ForceAccumulator.x;
			Q[curIndex + 1] = curParticle.ForceAccumulator.y;
		}
		return Q;
	}

	public int ParticleDimensions
	{
		get { return 4 * m_Particles.Count; }
	}

	public void ParticlesGetState(float[] a_DST)
	{
		int numParticles = m_Particles.Count;
		int requiredLength = 4 * numParticles;
		if (a_DST.Length != requiredLength)
		{
			throw new Exception("Input DST has wrong length!");
		}
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			a_DST[0] = curParticle.Position.x;
			a_DST[1] = curParticle.Position.y;
			a_DST[2] = curParticle.Velocity.x;
			a_DST[3] = curParticle.Velocity.y;
		}
	}

	public void ParticlesSetState(float[] a_DST)
	{
		int numParticles = m_Particles.Count;
		int requiredLength = 4 * numParticles;
		if (a_DST.Length != requiredLength)
		{
			throw new Exception("Input DST has wrong length!");
		}
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			curParticle.Position = new Vector2(a_DST[0], a_DST[1]);
			curParticle.Velocity = new Vector2(a_DST[2], a_DST[3]);
		}
	}

	public void ParticleDerivative(float[] a_DST)
	{
		int numParticles = m_Particles.Count;
		int requiredLength = 4 * numParticles;
		if (a_DST.Length != requiredLength)
		{
			throw new Exception("Input DST has wrong length!");
		}
		ClearForces();
		ComputeForces();
		ComputeConstraints();
		Particle curParticle = null;
		for (int i = 0; i < numParticles; ++i)
		{
			curParticle = m_Particles[i];
			a_DST[0] = curParticle.Velocity.x;
			a_DST[1] = curParticle.Velocity.y;
			a_DST[2] = curParticle.ForceAccumulator.x / curParticle.Mass;
			a_DST[3] = curParticle.ForceAccumulator.y / curParticle.Mass;
		}
	}

	private void ClearForces()
	{
		int numParticles = m_Particles.Count;
		for (int i = 0; i < numParticles; ++i)
		{
			m_Particles[i].ForceAccumulator = Vector2.zero;
		}
	}
}
