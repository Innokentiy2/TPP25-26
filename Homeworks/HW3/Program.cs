using System;

class Polynomial
{
    private int degree;
    private double[] coeffs;

    public Polynomial()
    {
        degree = 0;
        coeffs = new double[1] { 0.0 };
    }

    public Polynomial(double[] new_coeffs)
    {
        degree = new_coeffs.Length - 1;
        coeffs = (double[])new_coeffs.Clone();
    }

    public int Degree
    {
        get { return degree; }
    }

    public double[] Coeffs
    {
        get { return (double[])coeffs.Clone(); }
    }

    public override string ToString()
    {
        double c = coeffs[0];
        double b = coeffs[1];
        double a = coeffs[2];
        string s = "";
        if (c > 0)
        {
            s += Convert.ToString(c);
        }
        else if (c < 0)
        {
            s += "-";
            s += Convert.ToString(c * (-1));
        }

        if (s != "")
        {
            if (b > 0)
            {
                s += " + ";
                s += Convert.ToString(b);
                s += "x";
            }
            else if (b < 0)
            {
                s += " - ";
                s += s += Convert.ToString(b * (-1));
                s += "x";
            }
        }
        else
        {
            if (b > 0)
            {
                s += Convert.ToString(b);
                s += "x";
            }
            else if (b < 0)
            {
                s += "-";
                s += Convert.ToString(b * (-1));
                s += "x";
            }
        }

        if (s != "")
        {
            if (a > 0)
            {
                s += " + ";
                s += Convert.ToString(a);
                s += "x^2";
            }
            else if (a < 0)
            {
                s += " - ";
                s += Convert.ToString(a * (-1));
                s += "x^2";
            }
            
        }
        else
        {
            if (a > 0)
            {
                s += Convert.ToString(a);
                s += "x^2";
            }
            else if (a < 0)
            {
                s += "-";
                s += s += Convert.ToString(a * (-1));
                s += "x^2";
            }
        }

        /*
        *Метод должен возвращать строковое представление многочлена.
        * 
        * Например, если коэффициенты: { 1.0, 0.0, 2.0 },
        * то многочлен имеет вид:
        *     P(x) = 1 + 2x^2
        * 
        * Правила форматирования:
        *  - Пропускать члены, у которых коэффициент равен 0.
        *  - Если коэффициент положительный и это не первый член — добавлять " + ".
        *  - Если отрицательный — добавлять " - " и брать модуль коэффициента.
        *  - Для x^1 писать просто "x", для x^0 — только число.
        * 
        * Пример вывода:
        *     "1 + 2x^2"
        */
        return s;
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs = { 1.0, 0.0, 2.0 };
        Polynomial p = new Polynomial(coeffs); // 1 + 2x^2

        Console.WriteLine(p);
    }
}