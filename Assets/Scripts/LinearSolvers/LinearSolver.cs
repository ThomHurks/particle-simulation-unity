using UnityEngine;

public interface LinearSolver
{
    double Solve(ImplicitMatrix A, double[] x, double[] b,
                 double epsilon,    // how low should we go?
                 int steps, out int stepsPerformed);
}


