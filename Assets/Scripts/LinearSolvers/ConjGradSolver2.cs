using UnityEngine;

public sealed class ConjGradSolver2 : LinearSolver
{
    //****************************************************************************80

    private int iter;
    private double rho;
    private double rho_old;
    private int rlbl;

    private int cg_rc(int n, double[] b, double[] x, double[] r, double[] z, 
                      double[] p, double[] q, int job)

    //****************************************************************************80
    //
    //  Purpose:
    //
    //    CG_RC is a reverse communication conjugate gradient routine.
    //
    //  Discussion:
    //
    //    This routine seeks a solution of the linear system A*x=b
    //    where b is a given right hand side vector, A is an n by n
    //    symmetric positive definite matrix, and x is an unknown vector
    //    to be determined.
    //
    //    Under the assumptions that the matrix A is large and sparse,
    //    the conjugate gradient method may provide a solution when
    //    a direct approach would be impractical because of excessive
    //    requirements of storage or even of time.
    //
    //    The conjugate gradient method presented here does not require the 
    //    user to store the matrix A in a particular way.  Instead, it only 
    //    supposes that the user has a way of calculating
    //      y = alpha * A * x + b * y
    //    and of solving the preconditioned linear system
    //      M * x = b
    //    where M is some preconditioning matrix, which might be merely
    //    the identity matrix, or a diagonal matrix containing the
    //    diagonal entries of A.
    //
    //    This routine was extracted from the "templates" package.
    //    There, it was not intended for direct access by a user;
    //    instead, a higher routine called "cg()" was called once by
    //    the user.  The cg() routine then made repeated calls to 
    //    cgrevcom() before returning the result to the user.
    //
    //    The reverse communication feature of cgrevcom() makes it, by itself,
    //    a very powerful function.  It allows the user to handle issues of
    //    storage and implementation that would otherwise have to be
    //    mediated in a fixed way by the function argument list.  Therefore,
    //    this version of cgrecom() has been extracted from the templates
    //    library and documented as a stand-alone procedure.
    //
    //    The user sets the value of JOB to 1 before the first call,
    //    indicating the beginning of the computation, and to the value of
    //    2 thereafter, indicating a continuation call.  
    //    The output value of JOB is set by cgrevcom(), which
    //    will return with an output value of JOB that requests a particular
    //    new action from the user.
    //
    //  Licensing:
    //
    //    This code is distributed under the GNU LGPL license.
    //
    //  Modified:
    //
    //    13 January 2013
    //
    //  Author:
    //
    //    John Burkardt
    //
    //  Reference:
    //
    //    Richard Barrett, Michael Berry, Tony Chan, James Demmel,
    //    June Donato, Jack Dongarra, Victor Eijkhout, Roidan Pozo,
    //    Charles Romine, Henk van der Vorst,
    //    Templates for the Solution of Linear Systems:
    //    Building Blocks for Iterative Methods,
    //    SIAM, 1994,
    //    ISBN: 0898714710,
    //    LC: QA297.8.T45.
    //
    //  Parameters:
    //
    //    Input, int N, the dimension of the matrix.
    //
    //    Input, double B[N], the right hand side vector.
    //
    //    Input/output, double X[N].  On first call, the user 
    //    should store an initial guess for the solution in X.  On return with
    //    JOB = 4, X contains the latest solution estimate.
    //
    //    Input/output, double R[N], Z[N], P[N], Q[N],
    //    information used by the program during the calculation.  The user
    //    does not need to initialize these vectors.  However, specific
    //    return values of JOB may require the user to carry out some computation
    //    using data in some of these vectors.
    //
    //    Input/output, int JOB, communicates the task to be done.
    //    The user needs to set the input value of JOB to 1, before the first call,
    //    and then to 2 for every subsequent call for the given problem.
    //    The output value of JOB indicates the requested user action.  
    //    * JOB = 1, compute Q = A * P;
    //    * JOB = 2: solve M*Z=R, where M is the preconditioning matrix;
    //    * JOB = 3: compute R = R - A * X;
    //    * JOB = 4: check the residual R for convergence.  
    //               If satisfactory, terminate the iteration.
    //               If too many iterations were taken, terminate the iteration.
    //
    {
        double alpha;
        double beta;
        int i;
        int job_next = 0;
        double pdotq;
        //
        //  Initialization.
        //  Ask the user to compute the initial residual.
        //
        if (job == 1)
        {
            for (i = 0; i < n; i++)
            {
                r[i] = b[i];
            }
            job_next = 3;
            rlbl = 2;
        }
        //
        //  Begin first conjugate gradient loop.
        //  Ask the user for a preconditioner solve.
        //
        else if (rlbl == 2)
        {
            iter = 1;

            job_next = 2;
            rlbl = 3;
        }
        //
        //  Compute the direction.
        //  Ask the user to compute ALPHA.
        //  Save A*P to Q.
        //
        else if (rlbl == 3)
        {
            rho = 0.0;
            for (i = 0; i < n; i++)
            {
                rho = rho + r[i] * z[i];
            }

            if (1 < iter)
            {
                beta = rho / rho_old;
                for (i = 0; i < n; i++)
                {
                    z[i] = z[i] + beta * p[i];
                }
            }

            for (i = 0; i < n; i++)
            {
                p[i] = z[i];
            }

            job_next = 1;
            rlbl = 4;
        }
        //
        //  Compute current solution vector.
        //  Ask the user to check the stopping criterion.
        //
        else if (rlbl == 4)
        {
            pdotq = 0.0;
            for (i = 0; i < n; i++)
            {
                pdotq = pdotq + p[i] * q[i];
            }
            alpha = rho / pdotq;
            for (i = 0; i < n; i++)
            {
                x[i] = x[i] + alpha * p[i];
            }
            for (i = 0; i < n; i++)
            {
                r[i] = r[i] - alpha * q[i];
            }
            job_next = 4;
            rlbl = 5;
        }
        //
        //  Begin the next step.
        //  Ask for a preconditioner solve.
        //
        else if (rlbl == 5)
        {
            rho_old = rho;
            iter = iter + 1;

            job_next = 2;
            rlbl = 3;
        }

        return job_next;
    }

    public override double Solve(ImplicitMatrix A, double[] x, double[] b,
                                 double epsilon,    // how low should we go?
                                 int steps, out int stepsPerformed)
    {
        // Reset previous state first.
        iter = 0;
        rho = 0d;
        rho_old = 0d;
        rlbl = 0;

        int n = A.getN();
        double[] r = new double[n];
        double[] z = new double[n];
        double[] p = new double[n];
        double[] q = new double[n];
        double[] temp = new double[n];

        // Inital guess solution for x.
        ClearVectorWithValue(x, 1);

        int job_next = cg_rc(n, b, x, r, z, p, q, 1);
        int iterations = 1;
        while (true)
        {
            switch (job_next)
            {
                case 1:
                    // compute Q = A * P;
                    A.MatrixTimesVector(p, q);
                    break;
                case 2:
                    // solve M*Z=R, where M is the preconditioning matrix;
                    for (int i = 0; i < n; ++i)
                    {
                        z[i] = r[i]; // choose M = identity matrix
                    }
                    break;
                case 3:
                    // compute R = R - A * X;
                    ClearVectorWithValue(temp, 0);
                    A.MatrixTimesVector(x, temp);
                    for (int i = 0; i < n; ++i)
                    {
                        r[i] = r[i] - temp[i];
                    }
                    break;
                case 4:
                    // check the residual R for convergence.  
                    //               If satisfactory, terminate the iteration.
                    //               If too many iterations were taken, terminate the iteration.
                    double rSqrLen = vecSqrLen(n, r);

                    // Converged! Let's get out of here
                    if (rSqrLen <= epsilon || iterations >= steps)
                    {
                        stepsPerformed = iterations;
                        Debug.Log(VectorToString(x));
                        Debug.Log(VectorToString(b));
                        return rSqrLen;
                    }
                    break;
            }
            job_next = cg_rc(n, b, x, r, z, p, q, 2);
            ++iterations;
        }
    }
}
