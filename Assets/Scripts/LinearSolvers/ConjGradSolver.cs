using UnityEngine;

public sealed class ConjGradSolver : LinearSolver
{
    private const int MAX_STEPS = 10000;
    private const bool log = false;

    // Solve Ax = b for a symmetric, positive definite matrix A
    // A is represented implicitly by the function "matVecMult"
    // which performs a matrix vector multiple Av and places result in x
    // "n" is the length of the vectors x and b
    // "epsilon" is the error tolerance
    // "steps", as passed, is the maximum number of steps, or 0 (implying MAX_STEPS)
    // Upon completion, "steps" contains the number of iterations taken
    public override double Solve(ImplicitMatrix A, double[] x, double[] b,
                                 double epsilon,    // how low should we go?
                                 int steps, out int stepsPerformed)
    {
        int n = A.getN();
        int i, iMax;
        double alpha, beta, rSqrLen, rSqrLenOld, u;

        double[] r = new double[n];//n is actually m in our case: the number of constraints
        double[] d = new double[n];
        double[] t = new double[n];
        double[] temp = new double[n];

        if (x.Length != b.Length)
        {
            throw new System.Exception("Sizes do not match");
        }
        else if (x.Length != A.getN())
        {
            throw new System.Exception("Sizes do not match!" + x.Length + " " + A.getN());
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
        if (steps > 0 && !float.IsInfinity(steps))
        {
            iMax = steps;
        }
        else
        {
            iMax = MAX_STEPS;
        }

        if (rSqrLen > epsilon)
        {
            if (log)
            {
                Debug.Log("start - eps = " + epsilon + ", iter = " + steps + " A = ");
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
                if (rSqrLen <= epsilon)
                {
                    
                    break;
                }

                // Change direction: d = r + beta * d
                beta = rSqrLen / rSqrLenOld;
                vecTimesScalar(n, d, beta);
                vecAddEqual(n, d, r);
            }
            if (log)
            {
                Debug.Log("Residual sq: " + rSqrLen);
                Debug.Log("Result: x =" + VectorToString(x) + " steps: " + i);
            }
        }
        stepsPerformed = i;
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
}
