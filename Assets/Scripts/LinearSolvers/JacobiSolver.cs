﻿public class JacobiSolver : LinearSolver
{
    public override double Solve(ImplicitMatrix A, double[] x, double[] b,
                                 double epsilon,    // how low should we go?
                                 int steps, out int stepsPerformed)
    {
        ExplicitMatrix B = A.toExplicitMatrix();
        int n = A.getN();
        int steps2 = 0;
        double[] xnext = new double[n];
        double[] r = new double[n];
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
}


/*
 Choose an initial guess {\displaystyle x^{(0)}}  to the solution
{\displaystyle k=0} 
while convergence not reached do
  for i := 1 step until n do
    {\displaystyle \sigma =0} 
    for j := 1 step until n do
      if j ≠ i then
        {\displaystyle \sigma =\sigma +a_{ij}x_{j}^{(k)}} 
      end if
    end (j-loop)
    {\displaystyle x_{i}^{(k+1)}={{\left({b_{i}-\sigma }\right)} \over {a_{ii}}}} 
  end (i-loop)
  check if convergence is reached
  {\displaystyle k=k+1} 
loop (while convergence condition not reached)
 */