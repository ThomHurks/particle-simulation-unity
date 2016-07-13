public abstract class LinearSolver
{
    protected double m_SolverEpsilon;
    protected int m_SolverSteps;

    protected LinearSolver(double a_SolverEpsilon, int a_SolverSteps)
    {
        m_SolverEpsilon = a_SolverEpsilon;
        m_SolverSteps = a_SolverSteps;
    }

    public abstract double Solve(ImplicitMatrix A, double[] x, double[] b, out int out_StepsPerformed);

    protected static double vecDot(int n, double[] v1, double[] v2)
    {
        double dot = 0;
        for (int i = 0; i < n; i++)
        {
            dot += v1[i] * v2[i];
        }
        return dot;
    }

    protected static double vecSqrLen(int n, double[] v)
    {
        return vecDot(n, v, v);
    }

    protected static string VectorToString(double[] a)
    {
        if (a != null && a.Length > 0)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder((a.Length * 2) - 1);
            int length = a.Length - 1;
            const string sep = ", ";
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(a[i]);
                stringBuilder.Append(sep);  
            }
            stringBuilder.Append(a[length]);
            return stringBuilder.ToString();
        }
        return string.Empty;
    }

    protected static void ClearVectorWithValue(double[] a, double v)
    {
        for (int i = 0; i < a.Length; ++i)
        {
            a[i] = v;
        }
    }
}


