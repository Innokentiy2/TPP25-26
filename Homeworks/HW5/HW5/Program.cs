using System.IO.Compression;

interface IDamageable
{
    void TakeDamage(int damage);
}

abstract class Character: IDamageable
{
    protected string Name;
    protected int Health;
    public Character(string name, int health)
    {
        Name = name;
        Health = health;
    }
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Console.WriteLine($"{Name} получил {damage} урона");
    }
    public abstract void Attack();
    public void Move()
    {
        Console.WriteLine($"{Name} двигается");
    }
}

class Warrior: Character
{
    public Warrior(string name, int health): base(name, health){}
    public override void Attack()
    {
        Console.WriteLine($"{Name} бьет мечом");
    }
}

class Mage: Character
{
    public Mage(string name, int health): base(name, health){}
    public override void Attack()
    {
        Console.WriteLine($"{Name} использует заклинание");
    }
}

class Archer: Character
{
    public Archer(string name, int health): base(name, health){}
    public override void Attack()
    {
        Console.WriteLine($"{Name} выстрелил из лука");
    }
}

class Program
{
    static void Main()
    {
        Character[] characters = new Character[]
        {
            new Warrior("Воин", 100),
            new Mage("Маг", 70),
            new Archer("Лучник", 80),
        };
        foreach (Character ch in characters){
            ch.Attack();
        }
    }
} 