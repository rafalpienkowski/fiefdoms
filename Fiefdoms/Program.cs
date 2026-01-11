var demo = new ChessServiceDemo();
ChessServiceDemo.Run();

record ServerInfo(string Address, int CurrentLoad, int MaxLoad);
record Player(Guid Id, string Name, string Color);
record Game(Guid Id, string ServerAddress, Player Player1, Player Player2);
record GameSession(Game Game, List<string> Moves);

class Server(string address, int maxLoad)
{
    private readonly Queue<Player> _waitingPlayers = new();
    private readonly List<GameSession> _activeSessions = [];

    public void Start()
    {
        Console.WriteLine($"[Server {address}] Server started (Max load: {maxLoad})");
    }

    public ServerInfo GetServerInfo()
    {
        var currentLoad = _waitingPlayers.Count + (_activeSessions.Count * 2);
        return new ServerInfo(address, currentLoad, maxLoad);
    }

    public GameSession? ConnectPlayer(Player player)
    {
        var currentLoad = _waitingPlayers.Count + (_activeSessions.Count * 2);

        if (currentLoad >= maxLoad)
        {
            Console.WriteLine($"[Server {address}] Server full, cannot accept {player.Name}");
            return null;
        }

        Console.WriteLine($"[Server {address}] {player.Name} connected");
        _waitingPlayers.Enqueue(player);

        return TryMatchPlayers();
    }

    private GameSession? TryMatchPlayers()
    {
        if (_waitingPlayers.Count >= 2)
        {
            var player1 = _waitingPlayers.Dequeue();
            var player2 = _waitingPlayers.Dequeue();

            var game = new Game(
                Guid.NewGuid(),
                address,
                player1 with { Color = "White" },
                player2 with { Color = "Black" }
            );

            var session = new GameSession(game, []);
            _activeSessions.Add(session);

            Console.WriteLine($"[Server {address}] Game started: {player1.Name} (White) vs {player2.Name} (Black)");

            return session;
        }

        return null;
    }

    public void MakeMove(Guid gameId, string playerName, string move)
    {
        var session = _activeSessions.FirstOrDefault(s => s.Game.Id == gameId);
        if (session != null)
        {
            session.Moves.Add(move);
            Console.WriteLine($"[Server {address}] {playerName} made move: {move}");
        }
    }
}

class Client(Player player, List<Server> servers)
{
    private GameSession? _currentGame;
    private Server? _connectedServer;
    private const int MinDesirableLoad = 2;

    public void ConnectAndPlay()
    {
        Console.WriteLine($"\n[Client] {player.Name} is looking for a server...");

        var selectedServer = FindSuitableServer();

        if (selectedServer != null)
        {
            _connectedServer = selectedServer;
            PlayGame(selectedServer);
        }
        else
        {
            Console.WriteLine($"[Client] {player.Name} could not find any server");
        }
    }

    private Server? FindSuitableServer()
    {
        Server? leastBadServer = null;
        int? leastBadScore = null;

        foreach (var server in servers)
        {
            var info = server.GetServerInfo();
            Console.WriteLine($"[Client] {player.Name} checking {info.Address}... (Load: {info.CurrentLoad}/{info.MaxLoad})");

            if (info.CurrentLoad >= info.MaxLoad)
            {
                Console.WriteLine($"[Client] {player.Name} rejected {info.Address}: Server is full");
                continue;
            }

            if (info.CurrentLoad < MinDesirableLoad)
            {
                Console.WriteLine($"[Client] {player.Name} rejected {info.Address}: Too few players (hard to find opponent)");

                var score = Math.Abs(info.CurrentLoad - MinDesirableLoad);
                if (leastBadScore == null || score < leastBadScore)
                {
                    leastBadServer = server;
                    leastBadScore = score;
                }
                continue;
            }

            Console.WriteLine($"[Client] {player.Name} selected {info.Address} (Load: {info.CurrentLoad}/{info.MaxLoad})");
            return server;
        }

        if (leastBadServer != null)
        {
            var info = leastBadServer.GetServerInfo();
            Console.WriteLine($"[Client] {player.Name} couldn't find ideal server, connecting to least-bad option: {info.Address}");
            return leastBadServer;
        }

        return null;
    }

    private void PlayGame(Server server)
    {
        _currentGame = server.ConnectPlayer(player);

        if (_currentGame != null)
        {
            var opponent = GetOpponent();
            if (opponent != null)
            {
                Console.WriteLine($"[Client] {player.Name} found opponent: {opponent.Name}");
            }
        }
        else
        {
            Console.WriteLine($"[Client] {player.Name} waiting for opponent...");
        }
    }

    public void MakeMove(string move)
    {
        if (_currentGame != null && _connectedServer != null)
        {
            _connectedServer.MakeMove(_currentGame.Game.Id, player.Name, move);
        }
    }

    private Player? GetOpponent()
    {
        if (_currentGame == null) return null;

        return _currentGame.Game.Player1.Id == player.Id
            ? _currentGame.Game.Player2
            : _currentGame.Game.Player1;
    }
}

class ChessServiceDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Chess Online Service Demo ===\n");

        // Create servers with different capacities
        var server1 = new Server("Server-EU-1", 2);
        var server2 = new Server("Server-US-1", 6);
        var server3 = new Server("Server-AS-1", 4);

        server1.Start();
        server2.Start();
        server3.Start();
        Console.WriteLine();

        var servers = new List<Server> { server1, server2, server3 };

        Console.WriteLine("--- Scenario 1: First players join (servers empty) ---");
        var alice = new Client(new Player(Guid.NewGuid(), "Alice", ""), servers);
        alice.ConnectAndPlay();

        var bob = new Client(new Player(Guid.NewGuid(), "Bob", ""), servers);
        bob.ConnectAndPlay();
        Console.WriteLine();

        Console.WriteLine("--- Scenario 2: More players join (some servers now have good load) ---");
        var charlie = new Client(new Player(Guid.NewGuid(), "Charlie", ""), servers);
        charlie.ConnectAndPlay();

        var diana = new Client(new Player(Guid.NewGuid(), "Diana", ""), servers);
        diana.ConnectAndPlay();
        Console.WriteLine();

        Console.WriteLine("--- Scenario 3: Even more players (Server-EU-1 becomes full) ---");
        var eve = new Client(new Player(Guid.NewGuid(), "Eve", ""), servers);
        eve.ConnectAndPlay();

        var frank = new Client(new Player(Guid.NewGuid(), "Frank", ""), servers);
        frank.ConnectAndPlay();
        Console.WriteLine();

        Console.WriteLine("--- Players making moves ---");
        alice.MakeMove("e4");
        bob.MakeMove("e5");
        alice.MakeMove("Nf3");
        bob.MakeMove("Nc6");

        charlie.MakeMove("d4");
        diana.MakeMove("d5");
        charlie.MakeMove("c4");
        diana.MakeMove("e6");
        Console.WriteLine();

        Console.WriteLine("=== Demo Complete ===");
    }
}

