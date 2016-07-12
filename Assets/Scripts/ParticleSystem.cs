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
    private LinearSolver m_Solver;
    private double m_SolverEpsilon;
    private int m_SolverSteps;
    // Variables for equation 11:
    private double[] qdot;
    private double[] W;
    private double[] Q;
    private double[] C;
    private double[] CDot;
    private double[] JDotqdot;
    private double[] WQ;
    private double[] JWQ;
    private double[] RHS;
    private double[] lambda;
    private double[] QHat;
    private Eq11LHS LHS;

    public int Count
    {
        get { return m_Particles.Count; }
    }

    public float Time
    {
        get { return m_Time; }
        set { m_Time = value; }
    }

    public void SetSolver(LinearSolver a_Solver)
    {
        if (a_Solver != null)
        {
            m_Solver = a_Solver;
        }
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

    public ParticleSystem(LinearSolver a_Solver, double a_SolverEpsilon, int a_SolverSteps, float a_ConstraintSpringConstant, float a_ConstraintDampingConstant)
    {
        m_Solver = a_Solver;
        if (m_Solver == null)
        {
            throw new Exception("Please provide a valid solver.");
        }
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

    public void RemoveForce(Force a_Force)
    {
        m_Forces.Remove(a_Force);
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
        throw new Exception("Particle not found in particle list!");
    }

    public void ReverseParticleVelocities()
    {
        int vectorLength = ParticleDimensions;
        double[] temp = new double[vectorLength];
        ParticlesGetState(temp);
        vectorLength = vectorLength >> 2;
        for (int i = 0; i < vectorLength; ++i)
        {
            int index = (i << 2) + 2;
            temp[index] *= -1;
            temp[index + 1] *= -1;
        }
        ParticlesSetState(temp);
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

    private static void ValidateVector(double[] a_Vector)
    {
        for (int i = 0; i < a_Vector.Length; ++i)
        {
            if (double.IsNaN(a_Vector[i]) || double.IsInfinity(a_Vector[i]))
            {
                throw new Exception("Vector did not validate: NaN or Inf found.");
            }
        }
    }

    public void Initialize()
    {
        int particleSize = m_Particles.Count * GetParticleDimension();

        qdot = new double[particleSize];
        W = new double[particleSize];
        Q = new double[particleSize];
        WQ = new double[particleSize];
        QHat = new double[particleSize];

        int numConstraints = 0;
        for (int i = 0; i < m_Constraints.Count; i++)
        {
            numConstraints += m_Constraints[i].GetConstraintDimension();
        }

        C = new double[numConstraints];
        CDot = new double[numConstraints];
        JDotqdot = new double[numConstraints];
        JWQ = new double[numConstraints];
        RHS = new double[numConstraints];
        lambda = new double[numConstraints];

        LHS = new Eq11LHS(m_J, W);
    }

    private void SolveEquation11(float a_SpringConstant, float a_DampingConstant, double a_SolverEpsilon, int a_SolverSteps)
    {
        ParticlesGetVelocities(qdot);
        ValidateVector(qdot);
        //double[] M = ParticlesMassMatrix();
        //ValidateVector(M);
        ParticlesInverseMassMatrix(W);
        ValidateVector(W);
        ParticlesGetForces(Q);
        ValidateVector(Q);
        ConstraintsGetValues(C);
        ValidateVector(C);
        //double[] CMass = ConstraintsGetAvgMasses();
        //ValidateVector(CMass);
        ConstraintsGetDerivativeValues(CDot);
        ValidateVector(CDot);
        // JDot times qdot.
        for (int i = 0; i < JDotqdot.Length; ++i)
        {
            JDotqdot[i] = 0;
        }
        m_JDot.MatrixTimesVector(qdot, JDotqdot);
        ValidateVector(JDotqdot);
        // W times Q.
        for (int i = 0; i < WQ.Length; ++i)
        {
            WQ[i] = W[i] * Q[i];
        }
        ValidateVector(WQ);
        // J times WQ.
        for (int i = 0; i < JWQ.Length; ++i)
        {
            JWQ[i] = 0;
        }
        m_J.MatrixTimesVector(WQ, JWQ);
        ValidateVector(JWQ);
        // Compute the RHS of equation 11.
        for (int i = 0; i < RHS.Length; ++i)
        {
            RHS[i] = -JDotqdot[i] - JWQ[i] - a_SpringConstant * C[i] - a_DampingConstant * CDot[i];
            if (double.IsNaN(RHS[i]) || double.IsNaN(RHS[i]))
            {
                throw new System.Exception("NaN or Inf in RHS of eq 11");
            }
        }
        // Set up implicit matrix of LHS and solve.
        for (int i = 0; i < lambda.Length; ++i)
        {
            lambda[i] = 0;
        }
        int stepsPerformed = 0;
        m_Solver.Solve(LHS, lambda, RHS, a_SolverEpsilon, a_SolverSteps, out stepsPerformed);
        ValidateVector(lambda);
        //Debug.Log("Nr of iterations in conjgrad solver: " + stepsPerformed);
        for (int i = 0; i < QHat.Length; ++i)
        {
            QHat[i] = 0;
        }
        m_J.MatrixTransposeTimesVector(lambda, QHat);
        ValidateVector(QHat);
        if (QHat.Length != m_Particles.Count << 1)
        {
            throw new Exception("QHat does not match particles!");
        }
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            m_Particles[i].ForceAccumulator += new Vector2((float)QHat[i * 2], (float)QHat[(i * 2) + 1]);
            Vector2 newForce = m_Particles[i].ForceAccumulator;
            if (double.IsNaN(newForce.x) || double.IsNaN(newForce.y) || double.IsInfinity(newForce.x) || double.IsInfinity(newForce.y))
            {
                throw new Exception("NaN or Inf in accumulated force after eq 11");
            }
        }
    }

    private void ConstraintsGetValues(double[] a_Vector)
    {
        int k = 0;
        for (int i = 0; i < m_Constraints.Count; ++i)
        {
            double[] cVal = m_Constraints[i].GetValue(this);
            for (int j = 0; j < m_Constraints[i].GetConstraintDimension(); j++)
            {
                a_Vector[k++] = cVal[j];
            }
        }
    }

    private double[] ConstraintsGetAvgMasses()
    {
        // Gather constraint values into vector CMass.
        int numConstraints = 0;
        for (int i = 0; i < m_Constraints.Count; i++)
        {
            numConstraints += m_Constraints[i].GetConstraintDimension();
        }
        double[] C = new double[numConstraints];
        int k = 0;
        for (int i = 0; i < m_Constraints.Count; ++i)
        {
            for (int j = 0; j < m_Constraints[i].GetConstraintDimension(); j++)
            {
                C[k++] = m_Constraints[i].getAvgMass();
            }
        }
        return C;
    }

    private void ConstraintsGetDerivativeValues(double[] a_Vector)
    {
        int k = 0;
        for (int i = 0; i < m_Constraints.Count; ++i)
        {
            double[] cVal = m_Constraints[i].GetDerivativeValue(this);
            for (int j = 0; j < m_Constraints[i].GetConstraintDimension(); j++)
            {
                a_Vector[k++] = cVal[j];
            }
        }
    }

    private double[] ParticlesGetPositions()
    {
        // Gather positions into state vector q.
        int numParticles = m_Particles.Count;
        int particlePosDims = numParticles << 1;
        double[] q = new double[particlePosDims];
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

    private void ParticlesGetVelocities(double[] a_Vector)
    {
        // Gather velocities into state vector qdot.
        int numParticles = m_Particles.Count;
        if (a_Vector.Length != numParticles * 2)
        {
            throw new Exception("Input vector has incorrect size");
        }
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int curIndex = i << 1;
            a_Vector[curIndex] = curParticle.Velocity.x;
            a_Vector[curIndex + 1] = curParticle.Velocity.y;
        }
    }

    private void ParticlesInverseMassMatrix(double[] a_Vector)
    {
        // Construct inverse, W, of diagonal of mass matrix M as a vector.
        int numParticles = m_Particles.Count;
        if (a_Vector.Length != numParticles * 2)
        {
            throw new Exception("Input vector has incorrect size");
        }
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int curIndex = i << 1;
            double massInverse = 1d / curParticle.Mass;
            a_Vector[curIndex] = massInverse;
            a_Vector[curIndex + 1] = massInverse;
        }
    }

    private void ParticlesGetForces(double[] a_Vector)
    {
        // Gather forces into global force vector Q.
        int numParticles = m_Particles.Count;
        if (a_Vector.Length != numParticles * 2)
        {
            throw new Exception("Input vector has incorrect size");
        }
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int curIndex = i << 1;
            a_Vector[curIndex] = curParticle.ForceAccumulator.x;
            a_Vector[curIndex + 1] = curParticle.ForceAccumulator.y;
        }
    }

    public int ParticleDimensions
    {
        get { return 4 * m_Particles.Count; }
    }

    public void ParticlesGetState(double[] a_DST)
    {
        int numParticles = m_Particles.Count;
        int requiredLength = numParticles << 2;
        if (a_DST.Length != requiredLength)
        {
            throw new Exception("Input DST has wrong length!");
        }
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int index = i << 2;
            a_DST[index + 0] = curParticle.Position.x;
            a_DST[index + 1] = curParticle.Position.y;
            a_DST[index + 2] = curParticle.Velocity.x;
            a_DST[index + 3] = curParticle.Velocity.y;
        }
    }

    public void ParticlesSetState(double[] a_DST)
    {
        int numParticles = m_Particles.Count;
        int requiredLength = numParticles << 2;
        if (a_DST.Length != requiredLength)
        {
            throw new Exception("Input DST has wrong length!");
        }
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int index = i << 2;
            curParticle.Position = new Vector2((float)a_DST[index], (float)a_DST[index + 1]);
            curParticle.Velocity = new Vector2((float)a_DST[index + 2], (float)a_DST[index + 3]);
        }
    }

    public void ParticleDerivative(double[] a_DST)
    {
        int numParticles = m_Particles.Count;
        int requiredLength = numParticles << 2;
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
            int index = i << 2;
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

    public void Clear()
    {
        m_Particles.Clear();
        m_Forces.Clear();
        m_Constraints.Clear();
        m_J.Clear();
        m_JDot.Clear();
        qdot = null;
        W = null;
        Q = null;
        WQ = null;
        QHat = null;
        C = null;
        CDot = null;
        JDotqdot = null;
        JWQ = null;
        RHS = null;
        lambda = null;
        m_Time = 0f;
    }

}
