# NEXELYTH — VR World Travel System Prototype

## Overview
NEXELYTH is a prototype VR World Travel System designed to explore seamless scene transitions and player state management in immersive environments.

Key features:
- Portal-based teleportation between multiple world scenes
- Memo teleportation with three independent slots
- Persistent XR Origin across additive scene loading
- Unified player location service as the single source of truth

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

## Development Roadmap
- Transition from **Domain Prototype** to **Persistence / DB Schema** stage
- Integrate **Spring Boot REST API backend** for world state synchronization
- Expand Memo system with persistence (database-backed slots)
- Implement transaction-safe player state updates
- Future integration with VR Unity world state and external services

## Purpose
This repository is provided for **interview and technical showcase** only.  
Commercial use is strictly prohibited.

## License
Copyright (c) 2026 Vic / Nexelyth  
All rights reserved.  
Unauthorized commercial use is prohibited.
