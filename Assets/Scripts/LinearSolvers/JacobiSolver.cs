using System;

public class JacobiSolver : LinearSolver
{
    public double Solve(ImplicitMatrix A, double[] x, double[] b,
                        double epsilon,    // how low should we go?
                        int steps, out int stepsPerformed)
    {
        stepsPerformed = 0;
        return 0;
    }
}


