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
        if (this.coeffs.Length == 0) return "0";
        string res = "";
        for (int i = 0; i < this.coeffs.Length; i++)
        {
            if (this.coeffs[i] == 0) continue;
            if (i == 0) res += this.coeffs[i];
            else if (i == 1)
            {
                res += this.coeffs[i] < 0 ? " - " : " + ";
                res += Math.Abs(this.coeffs[i]).ToString() + "x";
            }
            else
            {
                res += this.coeffs[i] < 0 ? " - " : " + ";
                res += Math.Abs(this.coeffs[i]).ToString() + "x^" + i.ToString();
            }
        }
        return res;
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
