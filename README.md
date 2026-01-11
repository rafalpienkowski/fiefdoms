# Fiefdoms & Emissaries Pattern

A C# implementation demonstrating **Pat Helland's Fiefdoms and Emissaries** distributed systems pattern, applied to an online chess matchmaking service.

## Overview

This project illustrates how autonomous systems (Fiefdoms) can interact through representatives (Emissaries) without centralized coordination. Each server fiefdom creates its own emissary to represent its interests, and clients negotiate with multiple emissaries to find the best match.

## Architecture

### Key Components

- **Fiefdoms (Servers)**: Independent, autonomous domains that manage their own players and game sessions
- **Emissaries**: Representatives created BY each fiefdom to advocate for their home domain
- **EmissaryRegistry**: Lightweight discovery service (provides no business logic)
- **Clients**: Negotiate with multiple emissaries to find the best fiefdom to join

### Pattern Benefits

✅ **Distributed decision-making** - No central authority making choices
✅ **Autonomy** - Each fiefdom controls its own emissary and policies
✅ **Competition** - Emissaries compete to attract clients based on their fiefdom's state
✅ **Scalability** - New fiefdoms can be added without changing existing architecture
✅ **Resilience** - Failure of one fiefdom doesn't affect others

## How It Works

1. Each **Fiefdom** (Server) creates its own **Emissary** on startup
2. Emissaries register with the **EmissaryRegistry** for discovery
3. **Client** queries the registry to get all available emissaries
4. Client asks each **Emissary** for an offer (can you accept me?)
5. Each **Emissary** evaluates based on its home fiefdom's current state:
   - **Accepts** if fiefdom has good load (2+ players, not full)
   - **Rejects** if fiefdom is full
   - **Rejects but offers fallback** if too few players
6. Client selects the best offer (or least-bad fallback)
7. Winning **Emissary facilitates connection** to its home fiefdom
8. Client connects and plays in the selected fiefdom

## Running the Demo

```bash
cd Fiefdoms
dotnet run
```

The demo demonstrates three scenarios:
- **Scenario 1**: First players negotiate when all fiefdoms are empty
- **Scenario 2**: Emissaries compete as fiefdoms reach different load levels
- **Scenario 3**: Handling full fiefdoms and finding alternatives

## Visual Documentation

The repository includes a sequence diagram (`game-start-sequence.mmd`) that visualizes the complete interaction flow between clients, emissaries, and fiefdoms.

## Related Presentation

This project was created to demonstrate concepts from the presentation:

**[Z pamiętnika modelarza (From a Modeler's Diary)](https://modeler.z36.web.core.windows.net/)**

A presentation exploring Domain-Driven Design through the metaphors of castles (autonomous boundaries), emissaries (diplomatic representatives), and collaboration patterns in distributed systems.

## Further Reading

### Pat Helland's Papers on Distributed Systems
- [Data on the Outside versus Data on the Inside](https://www.cidrdb.org/cidr2005/papers/P12.pdf)
- [Autonomous Computing (short version)](https://pathelland.substack.com/p/autonomous-computing-short-version)
- [Identity by Any Other Name](https://queue.acm.org/detail.cfm?id=3314115)
- [Immutability Changes Everything](https://www.cidrdb.org/cidr2015/Papers/CIDR15_Paper16.pdf)
- [Building on Quicksand](https://arxiv.org/pdf/0909.1788)
- [Life beyond Distributed Transactions: an Apostate's Opinion](https://queue.acm.org/detail.cfm?id=3025012)
- [Memories, Guesses, and Apologies](https://blogs.msdn.microsoft.com/pathelland/2007/05/15/memories-guesses-and-apologies/)

### Domain-Driven Design
- Context Mapping videos by Michael Plöd (DDD Europe 2022 & KanDDDinsky 2019)

### Guiding Philosophy
> "All models are wrong, but some are useful." — George E.P. Box

## License

MIT

## Author

Rafał Pieńkowski (rafal.pienkowski@wp.pl)
