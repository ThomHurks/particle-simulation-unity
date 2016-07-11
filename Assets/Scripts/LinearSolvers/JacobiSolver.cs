public class JacobiSolver : LinearSolver
{
    private double[] xnext;
    private double[] r;
    private bool m_Initialized = false;
    private int m_Size;

    public override double Solve(ImplicitMatrix A, double[] x, double[] b,
                                 double epsilon,    // how low should we go?
                                 int steps, out int stepsPerformed)
    {
        ExplicitMatrix B = A.toExplicitMatrix();
        int n = A.getN();
        int steps2 = 0;

        InitializeOrClear(n);

        for (; steps2 < steps; steps2++)
        {
            for (int i = 0; i < n; i++)
            {
                double sigma = 0;
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        sigma = sigma + B.getValue(i, j) * x[j];
                    }
                }

                xnext[i] = (b[i] - sigma) / B.getValue(i, i);
            }
            double d = 0;
            A.MatrixTimesVector(xnext, r);
            for (int i = 0; i < n; i++)
            {
                x[i] = xnext[i];
                d += (r[i] - b[i]) * (r[i] - b[i]);
            }
            if (d < epsilon)
            {
                break;
            }
        }

        stepsPerformed = steps2;
        return 0;
    }

    private void InitializeOrClear(int a_Size)
    {
        if (m_Initialized == false || m_Size != a_Size)
        {
            r = new double[a_Size];
            xnext = new double[a_Size];
        }
        else
        {
            ClearVectorWithValue(r, 0d);
            ClearVectorWithValue(xnext, 0d);
        }
        m_Size = a_Size;
        m_Initialized = true;
    }
}