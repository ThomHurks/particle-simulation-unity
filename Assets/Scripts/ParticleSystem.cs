using System;
using UnityEngine;
using System.Collections.Generic;

public class ParticleSystem
{
    private readonly List<Particle> m_Particles;
    private readonly List<Force> m_Forces;
    private readonly List<Constraint> m_Constraints;
    private BlockSparseMatrix m_J;
    private BlockSparseMatrix m_JDot;
    private float m_Time;
    private float m_ConstraintSpringConstant;
    private float m_ConstraintDampingConstant;
    private float m_SolverEpsilon;
    private int m_SolverSteps;

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

    public ParticleSystem(float a_SolverEpsilon, int a_SolverSteps, float a_ConstraintSpringConstant, float a_ConstraintDampingConstant)
    {
        m_Particles = new List<Particle>();
        m_Forces = new List<Force>();
        m_Constraints = new List<Constraint>();
        m_J = new BlockSparseMatrix();
        m_JDot = new BlockSparseMatrix();
        m_SolverEpsilon = a_SolverEpsilon;
        m_SolverSteps = a_SolverSteps;
        m_ConstraintSpringConstant = a_ConstraintSpringConstant;
        m_ConstraintDampingConstant = a_ConstraintDampingConstant;
    }

    public int GetParticleDimension()
    {
        return 2;
    }

    public void AddParticle(Particle a_Particle)
    {
        m_Particles.Add(a_Particle);
        m_J.SetN(GetParticleDimension() * m_Particles.Count);
        m_JDot.SetN(GetParticleDimension() * m_Particles.Count);
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
            m_Constraints[i].UpdateJacobians(this);
        }
        SolveEquation11(m_ConstraintSpringConstant, m_ConstraintDampingConstant, m_SolverEpsilon, m_SolverSteps);
    }

    private static void ValidateVector(float[] a_Vector)
    {
        for (int i = 0; i < a_Vector.Length; ++i)
        {
            if (float.IsNaN(a_Vector[i]) || float.IsInfinity(a_Vector[i]))
            {
                throw new System.Exception("Vector did not validate: NaN or Inf found.");
            }
        }
    }

    private void SolveEquation11(float a_SpringConstant, float a_DampingConstant, float a_SolverEpsilon, int a_SolverSteps)
    {
        float[] qdot = ParticlesGetVelocities();
        ValidateVector(qdot);
        float[] W = ParticlesInverseMassMatrix();
        ValidateVector(W);
        float[] Q = ParticlesGetForces();
        ValidateVector(Q);
        float[] C = ConstraintsGetValues();
        ValidateVector(C);
        int numConstraints = C.Length;
        float[] CDot = ConstraintsGetDerivativeValues();
        ValidateVector(CDot);
        int n = GetParticleDimension() * m_Particles.Count;
        // JDot times qdot.
        float[] JDotqdot = new float[numConstraints];
        m_JDot.MatrixTimesVector(qdot, JDotqdot);
        ValidateVector(JDotqdot);
        // W times Q.
        float[] WQ = new float[n];
        for (int i = 0; i < n; ++i)
        {
            WQ[i] = W[i] * Q[i];
        }
        ValidateVector(WQ);
        // J times WQ.
        float[] JWQ = new float[numConstraints];
        m_J.MatrixTimesVector(WQ, JWQ);
        ValidateVector(JWQ);
        // Compute the RHS of equation 11.
        float[] RHS = new float[n];
        for (int i = 0; i < numConstraints; ++i)
        {
            RHS[i] = -JDotqdot[i] - JWQ[i] - a_SpringConstant * C[i] - a_DampingConstant * CDot[i];
            if (float.IsNaN(RHS[i]) || float.IsNaN(RHS[i]))
            {
                throw new System.Exception("NaN or Inf in RHS of eq 11");
            }
        }
        // Set up implicit matrix of LHS and solve.
        Eq11LHS LHS = new Eq11LHS(m_J, W);// J W JT = m*m if all goes well
        LinearSolver solver = new LinearSolver();
        float[] lambda = new float[numConstraints];
        int stepsPerformed = 0;
        solver.ConjGrad(numConstraints, LHS, lambda, RHS, a_SolverEpsilon, a_SolverSteps, out stepsPerformed);
        ValidateVector(lambda);
        //Debug.Log("Nr of iterations in conjgrad solver: " + stepsPerformed);
        float[] QHat = new float[n];
        m_J.MatrixTransposeTimesVector(lambda, QHat);
        ValidateVector(QHat);
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            m_Particles[i].ForceAccumulator += new Vector2(QHat[i * 2], QHat[(i * 2) + 1]);
            Vector2 newForce = m_Particles[i].ForceAccumulator;
            if (float.IsNaN(newForce.x) || float.IsNaN(newForce.y) || float.IsInfinity(newForce.x) || float.IsInfinity(newForce.y))
            {
                throw new System.Exception("NaN or Inf in accumulated force after eq 11");
            }
        }
    }

    private float[] ConstraintsGetValues()
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

    private float[] ConstraintsGetDerivativeValues()
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

    private float[] ParticlesGetPositions()
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

    private float[] ParticlesGetVelocities()
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

    private float[] ParticlesInverseMassMatrix()
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

    private float[] ParticlesGetForces()
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
            int index = i * 4;
            a_DST[index + 0] = curParticle.Position.x;
            a_DST[index + 1] = curParticle.Position.y;
            a_DST[index + 2] = curParticle.Velocity.x;
            a_DST[index + 3] = curParticle.Velocity.y;
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
            int index = i * 4;
            curParticle.Position = new Vector2(a_DST[index], a_DST[index + 1]);
            curParticle.Velocity = new Vector2(a_DST[index + 2], a_DST[index + 3]);
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
            int index = i * 4;
            a_DST[index] = curParticle.Velocity.x;
            a_DST[index + 1] = curParticle.Velocity.y;
            a_DST[index + 2] = curParticle.ForceAccumulator.x / curParticle.Mass;
            a_DST[index + 3] = curParticle.ForceAccumulator.y / curParticle.Mass;
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

    public void Draw()
    {
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            m_Particles[i].Draw();
        }
        for (int i = 0; i < m_Forces.Count; ++i)
        {
            m_Forces[i].Draw();
        }
        for (int i = 0; i < m_Constraints.Count; ++i)
        {
            m_Constraints[i].Draw();
        }
    }

}
