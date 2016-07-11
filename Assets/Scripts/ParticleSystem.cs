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
        vectorLength /= 4;
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

    private void SolveEquation11(float a_SpringConstant, float a_DampingConstant, double a_SolverEpsilon, int a_SolverSteps)
    {
        double[] qdot = ParticlesGetVelocities();
        ValidateVector(qdot);
        //double[] M = ParticlesMassMatrix();
        //ValidateVector(M);
        double[] W = ParticlesInverseMassMatrix();
        ValidateVector(W);
        double[] Q = ParticlesGetForces();
        ValidateVector(Q);
        double[] C = ConstraintsGetValues();
        ValidateVector(C);
        //double[] CMass = ConstraintsGetAvgMasses();
        //ValidateVector(CMass);
        int numConstraints = C.Length; // = number of SCALAR! constraints, so fixedpoint contributes 2 to this!
        double[] CDot = ConstraintsGetDerivativeValues();
        ValidateVector(CDot);
        int n = GetParticleDimension() * m_Particles.Count;
        // JDot times qdot.
        double[] JDotqdot = new double[numConstraints];
        m_JDot.MatrixTimesVector(qdot, JDotqdot);
        ValidateVector(JDotqdot);
        // W times Q.
        double[] WQ = new double[n];
        for (int i = 0; i < n; ++i)
        {
            //Debug.Log("Q[ " + i + "] =" + Q[i]);
            WQ[i] = W[i] * Q[i];
            //WQ[i] = Q[i];
        }
        ValidateVector(WQ);
        // J times WQ.
        double[] JWQ = new double[numConstraints];
        m_J.MatrixTimesVector(WQ, JWQ);
        ValidateVector(JWQ);
        // Compute the RHS of equation 11.
        double[] RHS = new double[numConstraints];
        for (int i = 0; i < numConstraints; ++i)
        {
            RHS[i] = -JDotqdot[i] - JWQ[i] - a_SpringConstant * C[i] - a_DampingConstant * CDot[i];
            //Debug.Log("RHS[" + i + "] = " + (-JDotqdot[i]) + " + " + (-JWQ[i]) + " + " + (-a_SpringConstant) + "*" + C[i] + " + " + (-a_DampingConstant) + "*" + CDot[i] + " = " + RHS[i]);
            if (double.IsNaN(RHS[i]) || double.IsNaN(RHS[i]))
            {
                throw new System.Exception("NaN or Inf in RHS of eq 11");
            }
        }
        // Set up implicit matrix of LHS and solve.
        Eq11LHS LHS = new Eq11LHS(m_J, W);// J W JT = m*m if all goes well
        double[] lambda = new double[numConstraints];
        int stepsPerformed = 0;
        m_Solver.Solve(LHS, lambda, RHS, a_SolverEpsilon, a_SolverSteps, out stepsPerformed);
        ValidateVector(lambda);
        //Debug.Log("Nr of iterations in conjgrad solver: " + stepsPerformed);
        double[] QHat = new double[n];
        m_J.MatrixTransposeTimesVector(lambda, QHat);
        ValidateVector(QHat);
        if (QHat.Length != m_Particles.Count * 2)
        {
            throw new Exception("QHat does not match particles!");
        }
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            //Debug.Log(QHat[i * 2] + " // " + QHat[(i * 2) + 1]);
            m_Particles[i].ForceAccumulator += new Vector2((float)QHat[i * 2], (float)QHat[(i * 2) + 1]);
            Vector2 newForce = m_Particles[i].ForceAccumulator;
            if (double.IsNaN(newForce.x) || double.IsNaN(newForce.y) || double.IsInfinity(newForce.x) || double.IsInfinity(newForce.y))
            {
                throw new System.Exception("NaN or Inf in accumulated force after eq 11");
            }
        }
    }

    private double[] ConstraintsGetValues()
    {
        // Gather constraint values into vector C.
        int numConstraints = 0;
        for (int i = 0; i < m_Constraints.Count; i++)
        {
            numConstraints += m_Constraints[i].GetConstraintDimension();
        }
        double[] C = new double[numConstraints];
        int k = 0;
        for (int i = 0; i < m_Constraints.Count; ++i)
        {
            double[] cVal = m_Constraints[i].GetValue(this);
            for (int j = 0; j < m_Constraints[i].GetConstraintDimension(); j++)
            {
                C[k++] = cVal[j];
            }
        }
        return C;
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

    private double[] ConstraintsGetDerivativeValues()
    {
        // Gather constraint values into vector CDot.
        int numConstraints = 0;
        for (int i = 0; i < m_Constraints.Count; i++)
        {
            numConstraints += m_Constraints[i].GetConstraintDimension();
        }
        double[] CDot = new double[numConstraints];
        int k = 0;
        for (int i = 0; i < m_Constraints.Count; ++i)
        {
            double[] cVal = m_Constraints[i].GetDerivativeValue(this);
            for (int j = 0; j < m_Constraints[i].GetConstraintDimension(); j++)
            {
                CDot[k++] = cVal[j];
            }
        }
        return CDot;
    }

    private double[] ParticlesGetPositions()
    {
        // Gather positions into state vector q.
        int numParticles = m_Particles.Count;
        int particlePosDims = numParticles * 2;
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

    private double[] ParticlesGetVelocities()
    {
        // Gather velocities into state vector qdot.
        int numParticles = m_Particles.Count;
        int particlePosDims = numParticles * 2;
        double[] qdot = new double[particlePosDims];
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

    private double[] ParticlesInverseMassMatrix()
    {
        // Construct inverse, W, of diagonal of mass matrix M as a vector.
        int numParticles = m_Particles.Count;
        int particleMassDims = numParticles * 2;
        double[] W = new double[particleMassDims];
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int curIndex = 2 * i;
            double massInverse = 1d / curParticle.Mass;
            W[curIndex] = massInverse;
            W[curIndex + 1] = massInverse;
        }
        return W;
    }

    private double[] ParticlesMassMatrix()
    {
        // Construct  of mass matrix M as a vector.
        int numParticles = m_Particles.Count;
        int particleMassDims = numParticles * 2;
        double[] M = new double[particleMassDims];
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_Particles[i];
            int curIndex = 2 * i;
            double massInverse = curParticle.Mass;
            M[curIndex] = massInverse;
            M[curIndex + 1] = massInverse;
        }
        return M;
    }

    private double[] ParticlesGetForces()
    {
        // Gather forces into global force vector Q.
        int numParticles = m_Particles.Count;
        int particleForceDims = numParticles * 2;
        double[] Q = new double[particleForceDims];
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

    public void ParticlesGetState(double[] a_DST)
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

    public void ParticlesSetState(double[] a_DST)
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
            curParticle.Position = new Vector2((float)a_DST[index], (float)a_DST[index + 1]);
            curParticle.Velocity = new Vector2((float)a_DST[index + 2], (float)a_DST[index + 3]);
        }
    }

    public void ParticleDerivative(double[] a_DST)
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

    public void Clear()
    {
        m_Particles.Clear();
        m_Forces.Clear();
        m_Constraints.Clear();
        m_J.Clear();
        m_JDot.Clear();
        m_Time = 0f;
    }

}
