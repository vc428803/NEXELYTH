# NEXELYTH

NEXELYTH is a prototype VR world system exploring world travel,
persistent player state, and immersive player experiences across
multiple Unity scenes.

## Key Features

- Portal-based teleportation between multiple world scenes
- Memo teleportation with three independent slots
- Persistent XR Origin across additive scene loading
- Unified player location service as the single source of truth
- XR player awakening sequence with head-locked eyelid effects,
  gradual vision recovery, and delayed interaction visibility

## Architecture

- **World Travel Service (`IWorldTravelService`)**  
  Decouples gameplay features from Unity scene management.

- **Player Location Service (`NexelythPlayerLocationService`)**  
  Maintains consistent player state across scenes.

- **Scene Management**  
  Additive loading with persistent XR Origin, ensuring smooth transitions.

- **Memo System**  
  Runtime storage of world positions, enabling return teleportation.

- **Reusable World Locations**  
  ScriptableObject-based fixed positions for portals and landmarks.

- **Player Awakening Sequence**  
  Coordinates an XR-compatible eyelid shader, vision recovery,
  and interaction visibility to create a staged first-person
  transition into the VR world.

## Development Roadmap

- Transition from **Domain Prototype** to **Persistence / DB Schema** stage
- Integrate a **C# / ASP.NET Core REST API backend** for world state synchronization
- Expand Memo system with persistence (database-backed slots)
- Implement transaction-safe player state updates (e.g., using EF Core transactions)
- Continue developing immersive VR player-state transitions and interactions

## Purpose

This repository is provided for **interview and technical showcase** only.  
Commercial use is strictly prohibited.

## License

Copyright (c) 2026 Nexelyth. All rights reserved.  
Unauthorized commercial use is prohibited.
