using System.Dynamic;
using System.Security;
using System.Collections.Generic;

public class Game
{
    private readonly GameState _state = new GameState();

    private readonly Dictionary<string, ICommand> _commands = new();
    private void RegisterCommands()
{
    _commands.Add("look", new LookCommand());
    _commands.Add("go", new GoCommand());
    _commands.Add("interact", new InteractCommand());
    _commands.Add("inv", new InventoryCommand());
    _commands.Add("status", new StatusCommand());
    _commands.Add("help", new HelpCommand(_commands));
    _commands.Add("take", new TakeCommand());
    _commands.Add("use", new UseCommand());
}
private void ProcessInput(string input)
{
    string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length == 0)
        return;

    string commandName = parts[0].ToLower();
    string[] args = parts.Skip(1).ToArray();

    if (_commands.TryGetValue(commandName, out var command))
    {
        command.Execute(_state, args);
    }
    else
    {
        Console.WriteLine("Неизвестная команда. Напишите help.");
    }
}
    public void Run()
{
    InitWorld();
    RegisterCommands();

    Console.WriteLine("Игра началась.");
    Console.WriteLine("Вы оказались в заблокированном исследовательском комплексе.");
    Console.WriteLine("Ваша цель — восстановить системы и выбраться наружу.");
    Console.WriteLine("Напишите help, чтобы увидеть список команд.");
    Console.WriteLine();
    _state.CurrentLocation.OnEnter(_state);

    while (!_state.isGameOver)
    {
        Console.Write("> ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            continue;

        ProcessInput(input);
        _state.CurrentLocation.OnTurn(_state);
    }

    if (_state.isVictory)
        Console.WriteLine("Поздравляем! Вы выбрались из комплекса.");
    else
        Console.WriteLine("Игра окончена.");
}

    private void InitWorld()
{
    _state.Player = new Player();

    var hall = new CorridorLocation(
        "hall",
        "Главный коридор",
        "Длинный коридор лаборатории с тусклым аварийным освещением."
    );

    var kitchen = new KitchenLocation(
        "kitchen",
        "Кухня",
        "Небольшая кухня для персонала. Здесь пахнет гарью и старым пластиком."
    );

    var storage = new Storage(
        "storage",
        "Склад",
        "Склад с ящиками, канистрами и грузовым лифтом."
    );

    var darkCorridor = new DarkCorridor(
        "dark_corridor",
        "Тёмный коридор",
        "Очень тёмный коридор. Без фонаря здесь опасно.",
        5
    );

    var bioLab = new BioLabLocation(
        "biolab",
        "Биолаборатория",
        "Стерильное помещение с разбитыми колбами и центральным сканером."
    );

    var medicalRoom = new MedicalRoom(
        "medical_room",
        "Медпункт",
        "Небольшой медицинский блок с шкафами и аптечками."
    );

    var controlRoom = new ControlRoom(
        "control_room",
        "Диспетчерская",
        "Здесь расположены экраны наблюдения и главная консоль управления."
    );

    var energyBlock = new EnergyBlock(
        "energy_block",
        "Генераторная",
        "Шумное помещение с резервным генератором и топливными баками."
    );

    hall.Exits.Add("kitchen", kitchen);
    kitchen.Exits.Add("hall", hall);

    hall.Exits.Add("storage", storage);
    storage.Exits.Add("hall", hall);

    hall.Exits.Add("dark", darkCorridor);
    darkCorridor.Exits.Add("hall", hall);

    hall.Exits.Add("medical", medicalRoom);
    medicalRoom.Exits.Add("hall", hall);

    hall.Exits.Add("energy", energyBlock);
    energyBlock.Exits.Add("hall", hall);

    bioLab.Exits.Add("hall", hall);

    darkCorridor.Exits.Add("control_room", controlRoom);
    controlRoom.Exits.Add("dark", darkCorridor);

    hall.AddItem(new Item(
        "note",
        "Записка",
        "На записке написано: 'Сканер можно восстановить только после подачи питания.'"
    ));

    kitchen.Interactables.Add(new Fridge("fridge", "Холодильник"));
    kitchen.Interactables.Add(new Cabinet("cabinet", "Шкаф"));

    storage.AddItem(new Item("fuel1", "Канистра топлива 1", "Одна из канистр для генератора."));
    storage.AddItem(new Item("fuel2", "Канистра топлива 2", "Одна из канистр для генератора."));
    storage.AddItem(new Item("wrench", "Гаечный ключ", "Подходит для ремонта и запуска генератора."));
    storage.AddItem(new Item("bio_key", "Ключ от биолаборатории", "Ключ-карта доступа в биолабораторию."));
    storage.AddItem(new Item("scanner_chip", "Модуль сканера", "Нужен для перезапуска центрального сканера."));

    storage.Interactables.Add(
        new Trap(
            "storage_trap",
            "Повреждённый контейнер",
            new List<IEffect>
            {
                new LogEffect("Из контейнера резко вылетает металлический фиксатор."),
                new DamageEffect(15)
            }
        )
    );

    storage.Interactables.Add(
        new Elevator(
            "elevator",
            "Грузовой лифт",
            new FlagCondition("ElevatorStarted"),
            "exit"
        )
    );

    darkCorridor.Interactables.Add(
        new Trap(
            "wire_trap",
            "Оголённый провод",
            new List<IEffect>
            {
                new LogEffect("Вы задеваете оголённый провод."),
                new DamageEffect(10)
            }
        )
    );

    hall.Interactables.Add(
        new BioLabDoor(
            "biolab_door",
            "Дверь в биолабораторию",
            "bio_key",
            bioLab
        )
    );

    bioLab.Interactables.Add(
        new Trap(
            "gas_trap",
            "Повреждённая колба",
            new List<IEffect>
            {
                new LogEffect("Колба трескается, и в воздух выбрасывается токсичный газ."),
                new DamageEffect(20)
            }
        )
    );

    var scannerCondition = new AndCondition(
        new HasItemCondition("scanner_chip"),
        new FlagCondition("PowerOn")
    );

    bioLab.Interactables.Add(
        new CentralScanner(
            "scanner",
            "Центральный сканер",
            scannerCondition
        )
    );

    medicalRoom.AddItem(new Item("medkit", "Аптечка", "Позволяет восстановить здоровье."));
    medicalRoom.AddItem(new Item("bandage", "Бинты", "Полезны для перевязки ран."));
    medicalRoom.AddItem(new Item("fuel3", "Канистра топлива 3", "Последняя канистра для генератора."));

    controlRoom.AddItem(new Item("control_note", "Журнал диспетчера", "В журнале сказано, что лифт зависит от центрального сканера."));
    controlRoom.AddItem(new Item("access_card", "Карта диспетчера", "Старая карта с кодами доступа."));

    controlRoom.Interactables.Add(
        new Terminal(
            "main_console",
            "Главная консоль",
            "На экране мигает сообщение: 'Центральный сканер отключён. Питание нестабильно.'",
            null,
            new List<IEffect>
            {
                new LogEffect("Консоль сообщает, что для запуска лифта нужен рабочий сканер.")
            }
        )
    );

    energyBlock.AddItem(new Item("gloves", "Перчатки", "Защищают от опасных поверхностей."));
    energyBlock.AddItem(new Item("fuse", "Предохранитель", "Может пригодиться в энергетическом блоке."));

    var generatorCondition = new AndCondition(
        new HasItemCondition("fuel1"),
        new AndCondition(
            new HasItemCondition("fuel2"),
            new AndCondition(
                new HasItemCondition("fuel3"),
                new HasItemCondition("wrench")
            )
        )
    );

    energyBlock.Interactables.Add(
        new Generator(
            "generator",
            "Резервный генератор",
            generatorCondition
        )
    );

    _state.CurrentLocation = hall;
}
}
public class Player
{
    public int Health {get; set;} = 100;
    public int maxHealth {get; set;} = 100;
}

public abstract class LocationBase
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; protected set; }

    public Dictionary<string, LocationBase> Exits { get; } = new();
    public List<IInteractable> Interactables { get; } = new();
    public List<GameEventBase> Events { get; } = new();

    protected LocationBase(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public bool TryGetExit(string id, out LocationBase location) =>
        Exits.TryGetValue(id, out location);

    public IInteractable? FindInteractable(string id) =>
        Interactables.Find(o =>
            string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase));

    public virtual void OnEnter(GameState state)
{
    Console.WriteLine();
    Console.WriteLine("=================================");
    Console.WriteLine(Name);
    Console.WriteLine(Description);
    Console.WriteLine("=================================");

    if (Exits.Count > 0)
    {
        Console.WriteLine("Доступные проходы:");
        foreach (var exit in Exits)
        {
            Console.WriteLine($"- {exit.Key} -> {exit.Value.Name}");
        }
    }
    else
    {
        Console.WriteLine("Из этой локации нет видимых выходов.");
    }

    if (Interactables.Count > 0)
    {
        Console.WriteLine("Объекты рядом:");
        foreach (var obj in Interactables)
        {
            Console.WriteLine($"- {obj.Name} ({obj.Id})");
        }
    }

    foreach (var ev in Events)
        ev.TryTrigger(state);

    Console.WriteLine("Подсказка: используйте команды look, go <id>, interact <id>, inv, status, help.");
    Console.WriteLine();
}
    public virtual void OnTurn(GameState state){}
}

public class Item
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public Item(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}

public class GameState
{
    public Player Player { get; set; } = new Player();
    public LocationBase CurrentLocation { get; set; } = null!;

    public HashSet<string> Inventory { get; } = new HashSet<string>();
    public HashSet<string> Flags { get; } = new HashSet<string>();

    public bool isGameOver { get; set; }
    public bool isVictory { get; set; }

    public bool HasFlag(string flag)
    {
        return Flags.Contains(flag);
    }

    public void SetFlag(string flag)
    {
        Flags.Add(flag);
    }

    public void RemoveFlag(string flag)
    {
        Flags.Remove(flag);
    }
}

public interface ICommand
{
    string Name {get;}
    string Description {get;}
    void Execute(GameState state, string[] args){}

}

public abstract class CommandBase : ICommand
{
    public string Name{get;}
    public string Description{get;}
    protected CommandBase(string name, string description)
    {
        Name = name;
        Description = description;
    }
    public abstract void Execute(GameState state, string[] args);   
}

public interface IInteractable
{
    public string Id {get;}
    public string Name {get;}
    public void Interact(GameState state);
}

public interface ICondition
{
    bool IsMet(GameState State);
}

public abstract class ConditionBase : ICondition
{
    public abstract bool IsMet(GameState state);
}

public interface IEffect
{
    void Apply(GameState state);
}

abstract class EffectBase : IEffect
{
    public abstract void Apply(GameState state);
}

public abstract class GameEventBase
{
    public string Id { get; }
    public bool IsOneTime { get; }

    private bool wasTriggered;

    protected ICondition condition { get; }
    protected List<IEffect> effects { get; }

    protected GameEventBase(string id, ICondition condition, IEnumerable<IEffect> effects, bool isOneTime = false)
    {
        Id = id;
        this.condition = condition;
        this.effects = new List<IEffect>(effects);
        IsOneTime = isOneTime;
    }

    public void TryTrigger(GameState state)
    {
        if (IsOneTime && wasTriggered)
            return;

        if (!condition.IsMet(state))
            return;

        foreach (var effect in effects)
            effect.Apply(state);

        wasTriggered = true;
    }
}

public class LogEffect : IEffect
{
    private readonly string _message;

    public LogEffect(string message)
    {
        _message = message;
    }

    public void Apply(GameState state)
    {
        Console.WriteLine(_message);
    }
}

public class FlagCondition : ICondition
{
    private string _flag;

    public FlagCondition(string flag)
    {
        _flag = flag;
    }

    public bool IsMet(GameState state)
    {
        return state.HasFlag(_flag);
    }
}

public class AndCondition : ConditionBase
{
    private ICondition _left;
    private ICondition _right;

    public AndCondition(ICondition left, ICondition right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsMet(GameState state)
    {
        return _left.IsMet(state) && _right.IsMet(state);
    }
}

class HasItemCondition : ConditionBase
{
    private readonly string itemId;

    public HasItemCondition(string itemId)
    {
        this.itemId = itemId;
    }

    public override bool IsMet(GameState state)
    {
        return state.Inventory.Contains(itemId);
    }
}

class NotCondition : ConditionBase
{
    private readonly ICondition inner;

    public NotCondition(ICondition inner)
    {
        this.inner = inner;
    }

    public override bool IsMet(GameState state) => !inner.IsMet(state);
}

public class Terminal : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private string message;
    private ICondition? accessCondition;
    private List<IEffect> effects;
    private bool isUsed;

    public Terminal(
        string id,
        string name,
        string message,
        ICondition? accessCondition,
        List<IEffect> effects)
    {
        Id = id;
        Name = name;
        this.message = message;
        this.accessCondition = accessCondition;
        this.effects = effects;
    }

    public void Interact(GameState state)
    {
        Console.WriteLine($"Вы взаимодействуете с объектом: {Name}");

        if (accessCondition != null && !accessCondition.IsMet(state))
        {
            Console.WriteLine("Доступ к терминалу запрещён.");
            return;
        }

        Console.WriteLine(message);

        foreach (var effect in effects)
        {
            effect.Apply(state);
        }

        isUsed = true;
    }

    public bool IsUsed()
    {
        return isUsed;
    }
}

public class DarkCorridor : LocationBase
{
    private readonly int _damagePerTurn;

    public DarkCorridor(string id, string name, string description, int damagePerTurn)
        : base(id, name, description)
    {
        _damagePerTurn = damagePerTurn;
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);

        if (!state.Inventory.Contains("Torch"))
        {
            Console.WriteLine("Здесь очень темно. Без фонаря идти опасно.");
        }
        else
        {
            Console.WriteLine("Фонарь освещает путь вперёд.");
        }
    }

    public override void OnTurn(GameState state)
    {
        if (!state.Inventory.Contains("Torch"))
        {
            state.Player.Health -= _damagePerTurn;

            if (state.Player.Health < 0)
                state.Player.Health = 0;

            Console.WriteLine($"Вы идёте в темноте и получаете {_damagePerTurn} урона.");
            Console.WriteLine($"Здоровье: {state.Player.Health}");

            if (state.Player.Health <= 0)
            {
                Console.WriteLine("Вы погибли в тёмном коридоре.");
                state.isGameOver = true;
            }
        }
    }
}

public class DamageEffect : IEffect
{
    private int _amount;

    public DamageEffect(int amount)
    {
        _amount = amount;
    }

    public void Apply(GameState state)
    {
        state.Player.Health -= _amount;

        if (state.Player.Health < 0)
            state.Player.Health = 0;

        Console.WriteLine($"Вы получили {_amount} урона.");
        Console.WriteLine($"Здоровье: {state.Player.Health}");

        if (state.Player.Health <= 0)
            state.isGameOver = true;
    }
}

public class CorridorLocation : LocationBase
{
    public List<Item> Items { get; } = new List<Item>();

    public CorridorLocation(string id, string name, string description)
        : base(id, name, description)
    {
    }

    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    public Item? FindItem(string itemId)
    {
        return Items.Find(i => i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));
    }

    public bool RemoveItem(string itemId)
    {
        var item = FindItem(itemId);
        if (item == null)
            return false;

        Items.Remove(item);
        return true;
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);

        if (Items.Count > 0)
        {
            Console.WriteLine("В коридоре лежат предметы:");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }
        else
        {
            Console.WriteLine("Здесь нет полезных предметов.");
        }
    }
}

public class KitchenLocation : LocationBase
{
    public KitchenLocation(string id, string name, string description)
        : base(id, name, description)
    {
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);

        Console.WriteLine("На кухне вы видите:");
        foreach (var obj in Interactables)
        {
            Console.WriteLine($"- {obj.Name} ({obj.Id})");
        }
    }
}

public class Fridge : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private bool opened = false;

    public Fridge(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void Interact(GameState state)
    {
        if (!opened)
        {
            Console.WriteLine("Вы открыли холодильник и нашли бутылку воды.");
            state.Inventory.Add("Water");
            opened = true;
        }
        else
        {
            Console.WriteLine("Холодильник уже открыт. Внутри больше ничего нет.");
        }
    }
}

public class Cabinet : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private bool searched = false;

    public Cabinet(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void Interact(GameState state)
    {
        if (!searched)
        {
            Console.WriteLine("Вы открыли шкаф и нашли фонарь.");
            state.Inventory.Add("Torch");
            searched = true;
        }
        else
        {
            Console.WriteLine("Шкаф пуст.");
        }
    }
}

public class Storage : LocationBase
{
    public Storage(string id, string name, string description) : base(id, name, description){}
    public List<Item> Items {get; set;} = new List<Item>();
    public void AddItem(Item item)
    {
        Items.Add(item);
    }
    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);

        if (Items.Count > 0)
        {
            Console.WriteLine("На складе лежат предметы:");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }

        if (Interactables.Count > 0)
        {
            Console.WriteLine("Объекты взаимодействия:");
            foreach (var obj in Interactables)
            {
                Console.WriteLine($"- {obj.Name} ({obj.Id})");
            }
        }
    }
}


public class Elevator : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private ICondition _condition;
    private string _targetLocationId;

    public Elevator(string id, string name, ICondition condition, string targetLocationId)
    {
        Id = id;
        Name = name;
        _condition = condition;
        _targetLocationId = targetLocationId;
    }

    public void Interact(GameState state)
    {
        if (!_condition.IsMet(state))
        {
            Console.WriteLine("Лифт не работает.");
            return;
        }

        Console.WriteLine("Лифт запускается.");
        Console.WriteLine("Вы выбираетесь из комплекса.");
        state.isVictory = true;
        state.isGameOver = true;
    }
}

public class Office : LocationBase
{
    public Office(string id, string name, string description) : base(id, name, description){}
    public List<Item> Items {get; set;} = new List<Item>();
    public void AddItem(Item item)
    {
        Items.Add(item);
    }
    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);

        if (Items.Count > 0)
        {
            Console.WriteLine("В офисе лежат предметы:");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }
    }
}

public class BioLabDoor : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private readonly string _requiredKey;
    private readonly LocationBase _targetLocation;

    public BioLabDoor(string id, string name, string requiredKey, LocationBase targetLocation)
    {
        Id = id;
        Name = name;
        _requiredKey = requiredKey;
        _targetLocation = targetLocation;
    }

    public void Interact(GameState state)
    {
        if (!state.Inventory.Contains(_requiredKey))
        {
            Console.WriteLine("Дверь в биолабораторию закрыта. Нужен ключ.");
            return;
        }

        Console.WriteLine("Вы открываете дверь ключом и входите в биолабораторию.");
        state.CurrentLocation = _targetLocation;
        state.CurrentLocation.OnEnter(state);
    }
}

public class BioLabLocation : LocationBase
{
    public BioLabLocation(string id, string name, string description)
        : base(id, name, description)
    {
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);
        Console.WriteLine("В центре помещения находится главный биологический сканер.");
    }
}

public class CentralScanner : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private readonly ConditionBase _condition;
    private bool _restarted;

    public CentralScanner(string id, string name, ConditionBase condition)
    {
        Id = id;
        Name = name;
        _condition = condition;
        _restarted = false;
    }

    public void Interact(GameState state)
    {
        if (_restarted)
        {
            Console.WriteLine("Сканер уже перезапущен.");
            return;
        }

        if (!_condition.IsMet(state))
        {
            Console.WriteLine("Сканер не отвечает. Нужен специальный модуль для перезапуска.");
            return;
        }

        Console.WriteLine("Вы перезапускаете центральный сканер.");
        Console.WriteLine("Грузовой лифт снова работает.");

        state.SetFlag("ElevatorStarted");
        _restarted = true;
    }
}

public class EnergyBlock : LocationBase
{
    public EnergyBlock(string id, string name, string description) : base(id, name, description){}
    public List<Item> Items {get; set;} = new List<Item>();
    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);
        if (Items.Count > 0)
        {
            Console.WriteLine("В генераторной лежат предметы:");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }
        Console.WriteLine("В комнате стоит генератор");
    }
}

public class Generator : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private readonly ConditionBase _condition;
    private bool _started;
    public Generator(string id, string name, ConditionBase condition)
    {
        Id = id;
        Name = name;
        _condition = condition;
        _started = false;
    }
    public void Interact(GameState state)
    {
        if (_started) {
            Console.WriteLine("Генератор уже запущен");
            return;
        }
        if (!_condition.IsMet(state))
        {
            Console.WriteLine("Не получится. Нужны 3 канистры с тполивом и гаечный ключ");
            return;
        }
        Console.WriteLine("Вы запускаете генератор.");
        state.SetFlag("PowerOn");
        _started = true;
    }
}

public class MedicalRoom : LocationBase
{
    public MedicalRoom(string id, string name, string description) : base(id, name, description){}
    public List<Item> Items {get; set;} = new List<Item>();
    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);
        if (Items.Count > 0)
        {
            Console.WriteLine("В медкабинете лежат предметы:");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }
    }
}

public class ControlRoom : LocationBase
{
    public ControlRoom(string id, string name, string description) : base(id, name, description){}
    public List<Item> Items {get; set;} = new List<Item>();
    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    public override void OnEnter(GameState state)
    {
        base.OnEnter(state);

        Console.WriteLine("В диспетчерской вы видите:");
        foreach (var obj in Interactables)
        {
            Console.WriteLine($"- {obj.Name} ({obj.Id})");
        }
        Console.WriteLine("И предметы:");
        foreach (Item item in Items)
        {
            Console.WriteLine($"- {item.Name}");
        }
    }
}

public class Trap : IInteractable
{
    public string Id { get; }
    public string Name { get; }

    private readonly List<IEffect> _effects;
    private bool _triggered;

    public Trap(string id, string name, List<IEffect> effects)
    {
        Id = id;
        Name = name;
        _effects = effects;
    }

    public void Interact(GameState state)
    {
        if (_triggered)
        {
            Console.WriteLine("Ловушка уже сработала.");
            return;
        }

        Console.WriteLine("Сработала ловушка!");

        foreach (IEffect effect in _effects)
        {
            effect.Apply(state);
        }

        _triggered = true;
    }
}

public class LookCommand : CommandBase
{
    public LookCommand() : base("look", "Показать описание текущей локации")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        state.CurrentLocation.OnEnter(state);
    }
}

public class InventoryCommand : CommandBase
{
    public InventoryCommand() : base("inv", "Показать инвентарь")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        if (state.Inventory.Count == 0)
        {
            Console.WriteLine("Инвентарь пуст.");
            return;
        }

        Console.WriteLine("Инвентарь:");
        foreach (var item in state.Inventory)
        {
            Console.WriteLine("- " + item);
        }
    }
}

public class InteractCommand : CommandBase
{
    public InteractCommand() : base("interact", "Взаимодействовать с объектом")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Укажите id объекта.");
            return;
        }

        var obj = state.CurrentLocation.FindInteractable(args[0]);

        if (obj == null)
        {
            Console.WriteLine("Такого объекта здесь нет.");
            return;
        }

        obj.Interact(state);
    }
}

public class GoCommand : CommandBase
{
    public GoCommand() : base("go", "Перейти в другую локацию")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Куда идти?");
            return;
        }

        if (state.CurrentLocation.TryGetExit(args[0], out var next))
        {
            state.CurrentLocation = next;
            state.CurrentLocation.OnEnter(state);
        }
        else
        {
            Console.WriteLine("Туда пройти нельзя.");
        }
    }
}

public class HelpCommand : CommandBase
{
    private readonly Dictionary<string, ICommand> _commands;

    public HelpCommand(Dictionary<string, ICommand> commands)
        : base("help", "Показать список команд")
    {
        _commands = commands;
    }

    public override void Execute(GameState state, string[] args)
{
    Console.WriteLine("Доступные команды:");
    foreach (var cmd in _commands.Values)
    {
        Console.WriteLine($"{cmd.Name} - {cmd.Description}");
    }

    Console.WriteLine();
    Console.WriteLine("Примеры:");
    Console.WriteLine("go kitchen");
    Console.WriteLine("interact fridge");
    Console.WriteLine("take fuel1");
    Console.WriteLine("use medkit");
}
}

public class StatusCommand : CommandBase
{
    public StatusCommand() : base("status", "Показать состояние игрока")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        Console.WriteLine($"Здоровье: {state.Player.Health}");
        Console.WriteLine($"Текущая локация: {state.CurrentLocation.Name}");
    }
}

public class TakeCommand : CommandBase
{
    public TakeCommand() : base("take", "Поднять предмет")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Укажите id предмета.");
            return;
        }

        if (state.CurrentLocation is CorridorLocation corridor)
        {
            var item = corridor.FindItem(args[0]);
            if (item == null)
            {
                Console.WriteLine("Такого предмета здесь нет.");
                return;
            }

            state.Inventory.Add(item.Id);
            corridor.RemoveItem(item.Id);
            Console.WriteLine($"Вы подобрали: {item.Name}");
            return;
        }

        if (state.CurrentLocation is Storage storage)
        {
            var item = storage.Items.Find(i => i.Id.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                Console.WriteLine("Такого предмета здесь нет.");
                return;
            }

            state.Inventory.Add(item.Id);
            storage.Items.Remove(item);
            Console.WriteLine($"Вы подобрали: {item.Name}");
            return;
        }

        if (state.CurrentLocation is MedicalRoom medical)
        {
            var item = medical.Items.Find(i => i.Id.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                Console.WriteLine("Такого предмета здесь нет.");
                return;
            }

            state.Inventory.Add(item.Id);
            medical.Items.Remove(item);
            Console.WriteLine($"Вы подобрали: {item.Name}");
            return;
        }

        if (state.CurrentLocation is ControlRoom control)
        {
            var item = control.Items.Find(i => i.Id.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                Console.WriteLine("Такого предмета здесь нет.");
                return;
            }

            state.Inventory.Add(item.Id);
            control.Items.Remove(item);
            Console.WriteLine($"Вы подобрали: {item.Name}");
            return;
        }

        Console.WriteLine("В этой локации нельзя подбирать предметы.");
    }
}

public class UseCommand : CommandBase
{
    public UseCommand() : base("use", "Использовать предмет")
    {
    }

    public override void Execute(GameState state, string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Укажите id предмета.");
            return;
        }

        string itemId = args[0].ToLower();

        if (!state.Inventory.Contains(itemId))
        {
            Console.WriteLine("У вас нет этого предмета.");
            return;
        }

        if (itemId == "medkit")
        {
            state.Player.Health += 30;
            if (state.Player.Health > state.Player.maxHealth)
                state.Player.Health = state.Player.maxHealth;

            state.Inventory.Remove("medkit");
            Console.WriteLine("Вы использовали аптечку.");
            Console.WriteLine($"Здоровье: {state.Player.Health}");
            return;
        }

        Console.WriteLine("Этот предмет нельзя использовать напрямую.");
    }
}

public static class Program
{
    public static void Main()
    {
        var game = new Game();
        game.Run();
    }
}