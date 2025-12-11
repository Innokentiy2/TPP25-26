using System.Reflection.Metadata.Ecma335;

class Student
{
    public string Name{get; set;}
    public int Age{get; set;}
    public string Group{get; set;}

    public Student(string name, int age, string group)
    {
        Name = name;
        Age = age;
        Group = group;
    }

    public void study()
    {
        Console.WriteLine($"Студент по имени {Name}, которому {Age} лет, учится в группе {Group}");
    }
}

class Magistr : Student
{
    public Magistr(string name, int age, string group) : base(name, age, group){}

    public void DefendDiplom(){Console.WriteLine($"Магистр {Name} защищает диплом");}
}

class Bakalavr : Student
{
    public Bakalavr(string name, int age, string group) : base(name, age, group){}

    public void TakeExams(){Console.WriteLine($"Бакалавр {Name} сдает экзамены");}
}

class Programm
{
    static void Main()
    {
        Magistr mag = new Magistr("Александр", 19, "K-04");
        mag.study();
        mag.DefendDiplom();

        Bakalavr bak = new Bakalavr("Иван", 23, "Б-11");
        bak.TakeExams();
    }
}