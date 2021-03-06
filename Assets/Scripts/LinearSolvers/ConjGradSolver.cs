﻿using UnityEngine;

public sealed class ConjGradSolver : LinearSolver
{
    private const int MAX_STEPS = 10000;
    private const bool logging = false;
    private double[] r;
    private double[] d;
    private double[] t;
    private double[] temp;
    private bool m_Initialized = false;
    private int m_Size;

    public ConjGradSolver(double a_SolverEpsilon, int a_SolverSteps)
        : base(a_SolverEpsilon, a_SolverSteps)
    {
    }

    // Solve Ax = b for a symmetric, positive definite matrix A
    // A is represented implicitly by the function "matVecMult"
    // which performs a matrix vector multiple Av and places result in x
    // "n" is the length of the vectors x and b
    // "epsilon" is the error tolerance
    // "steps", as passed, is the maximum number of steps, or 0 (implying MAX_STEPS)
    // Upon completion, "steps" contains the number of iterations taken
    public override double Solve(ImplicitMatrix A, double[] x, double[] b, out int out_StepsPerformed)
    {
        int n = A.GetN();
        int i, iMax;
        double alpha, beta, rSqrLen, rSqrLenOld, u;

        InitializeOrClear(n);

        if (x.Length != b.Length)
        {
            throw new System.Exception("Sizes do not match");
        }
        else if (x.Length != A.GetN())
        {
            throw new System.Exception("Sizes do not match!" + x.Length + " " + A.GetN());
        }
        else if (x.Length != n)
        {
            throw new System.Exception("Sizes do not match!");
        }

        vecAssign(n, x, b);//x:=b

        vecAssign(n, r, b);//r:=b
        A.MatrixTimesVector(x, temp); // temp := Ab
        vecDiffEqual(n, r, temp);//r := r-temp = b- Ab

        rSqrLen = vecSqrLen(n, r);
        vecAssign(n, d, r);//d := r = b-Ab

        i = 0;
        if (m_SolverSteps > 0 && !float.IsInfinity(m_SolverSteps))
        {
            iMax = m_SolverSteps;
        }
        else
        {
            iMax = MAX_STEPS;
        }

        if (rSqrLen > m_SolverEpsilon)
        {
            if (logging)
            {
                Debug.Log("start - eps = " + m_SolverEpsilon + ", iter = " + m_SolverSteps + " A = ");
                A.printX();
                Debug.Log("b = " + VectorToString(b));
            }
            while (i < iMax)
            {
				
                i++;
                A.MatrixTimesVector(d, t);
                u = vecDot(n, d, t);
                if (double.IsInfinity(u))
                {
                    throw new System.Exception("u = infinity");
                }

                if (System.Math.Abs(u) <= double.Epsilon)
                {
                    //Debug.Log("(SolveConjGrad) d'Ad = 0\n");
                    break;
                }

                // How far should we go?
                alpha = rSqrLen / u;

                // Take a step along direction d
                vecAssign(n, temp, d);//temp = d
                vecTimesScalar(n, temp, alpha);//temp = alpha * temp = alpha* d
                vecAddEqual(n, x, temp);
                ValidateVector(x);

                //Debug.Log (toString (temp));
                //Debug.Log (u);
                //Debug.Log (toString(d));
                //Debug.Log (toString(t));
                // if (i % 64 != 0)
                {
                    vecAssign(n, temp, t);
                    vecTimesScalar(n, temp, alpha);
                    vecDiffEqual(n, r, temp);
                }
                /*
                else
                {
                    
                    // For stability, correct r every 64th iteration
                    vecAssign(n, r, b);
                    A.MatrixTimesVector(x, temp);
                    vecDiffEqual(n, r, temp);
                }
                */
                rSqrLenOld = rSqrLen;
                rSqrLen = vecSqrLen(n, r);

                // Converged! Let's get out of here
                if (rSqrLen <= m_SolverEpsilon)
                {
                    
                    break;
                }

                // Change direction: d = r + beta * d
                beta = rSqrLen / rSqrLenOld;
                vecTimesScalar(n, d, beta);
                vecAddEqual(n, d, r);
            }
            if (logging)
            {
                Debug.Log("Residual sq: " + rSqrLen);
                Debug.Log("Result: x =" + VectorToString(x) + " steps: " + i);
            }
        }
        out_StepsPerformed = i;
        return rSqrLen;
    }

    private static void ValidateVector(double[] a)
    {
        for (int i = 0; i < a.Length; ++i)
        {
            if (double.IsNaN(a[i]) || double.IsInfinity(a[i]))
            {
                throw new System.Exception("Wrong value in solver!");
            }
        }
    }

    private static void vecAddEqual(int n, double[] r, double[] v)
    {
        for (int i = 0; i < n; ++i)
        {
            r[i] = r[i] + v[i];
        }
    }

    private static void vecDiffEqual(int n, double[] r, double[] v)
    {
        for (int i = 0; i < n; ++i)
        {
            r[i] = r[i] - v[i];
        }
    }

    private static void vecAssign(int n, double[] v1, double[] v2)
    {
        for (int i = 0; i < n; ++i)
        {
            v1[i] = v2[i];
        }
    }

    private static void vecTimesScalar(int n, double[] v, double s)
    {
        for (int i = 0; i < n; ++i)
        {
            v[i] *= s;
        }
    }

    private void InitializeOrClear(int a_Size)
    {
        if (m_Initialized == false || m_Size != a_Size)
        {
            r = new double[a_Size];
            d = new double[a_Size];
            t = new double[a_Size];
            temp = new double[a_Size];
        }
        else
        {
            ClearVectorWithValue(r, 0d);
            ClearVectorWithValue(d, 0d);
            ClearVectorWithValue(t, 0d);
            ClearVectorWithValue(temp, 0d);
        }
        m_Size = a_Size;
        m_Initialized = true;
    }
}
