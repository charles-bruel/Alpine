using System;
using UnityEngine;

public struct Catenary {
    public float a, b, c;

    public float Evaluate(float x) {
        return a * (float) Math.Cosh((x - b) / a) + c;
    }

    // https://math.stackexchange.com/questions/3557767/how-to-construct-a-catenary-of-a-specified-length-through-two-specified-points
    public static Catenary FromCoordinates(Vector2 a, Vector2 b, float L) {
        Catenary result = new Catenary();

        float dx = b.x - a.x;
        float x_bar = (a.x + b.x) / 2;
        float dy = b.y - a.y;
        float y_bar = (a.y + b.y) / 2;

        //"If L^2≤dx^2+dy^2, there is no solution --- the points are too far apart."
        if(L * L < dx * dx + dy * dy) {
            throw new ArgumentException();
        }

        float r = Mathf.Sqrt(L * L + dy * dy) / dx;
        float A = (float) SinHAoverA(r);
        
        result.a = dx / (2 * A);
        result.b = x_bar - result.a * (float) Math.Atanh(dy / L);
        result.c = a.y - result.a * (float) Math.Cosh((a.x - result.b) / result.a);

        return result;
    }

    // https://math.stackexchange.com/questions/3557767/how-to-construct-a-catenary-of-a-specified-length-through-two-specified-points
    // Appendix A. Solving r=sinh(A)/A for A.
    private static double SinHAoverA(double r) {
        double A0;
        if(r < 3) {
            A0 = Math.Sqrt(6 * (r - 1));
        } else {
            //A0 = ln(2r) + lnln(2r)
            A0 = Math.Log(2 * r);
            A0 = A0 + Math.Log(A0);
        }

        // Now use 5 iterations of Newton's method
        double An = A0;
        for(int i = 0;i < 5;i ++) {
            double Anp1 = An - (Math.Sinh(An) - r * An) / (Math.Cosh(An) - r);
            An = Anp1;
        }

        return An;
    }
}