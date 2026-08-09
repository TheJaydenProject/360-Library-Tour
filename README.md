# 360 Library Tour

A 360° interactive virtual tour of three lesser-known corners of the NP Library, built in Unity. Navigate the space like a Google Maps-style panorama, talk to the NPC guarding each corner, interact with objects to uncover clues, and piece them together to solve a riddle rooted in real NP Library history. Solve all three to collect every medal and walk away with a genuine taste of the library's heritage.

All footage was recorded on-site at the [Lien Ying Chow Library - Singapore](https://www.np.edu.sg/library) (Ngee Ann Polytechnic Library) using a 360° GoPro camera.

## Features

- **360° panorama navigation** — click hotspots to zoom and transition between rooms.
- **Three interactive corners**, one per team member, each with its own dialogue and riddle mini-game:
  - **Newspaper Reading Corner** (Jayden) — click hotspots to hear info, ask the NPC questions via video, then answer a riddle.
  - **Comic Area** (Wei Cheng) — hotspot exploration, video Q&A, and a riddle to close it out.
  - **DVD & Blu-Ray Corner** (Keagan) — hotspots with narrated hints and DVD content leading into the riddle.
- **Persistent HUD** — start screen, map, a running objective indicator, background music toggle, and a medal tracker that fills in as each corner's riddle is solved.

## Requirements

- Unity **6000.4.5f1** (Unity 6)

## Getting Started

1. Clone or download this repository.
2. Open the project in Unity 6000.4.5f1.
3. Open `Assets/Scenes/Merger.unity` — the combined scene with all three rooms.
4. Press Play.

> **Note:** The screen may stay black for the first ~10 seconds after pressing Play, this is normal, the 360 video just needs a moment to load. Click around and give it time. If it's still black after 10+ seconds, check **Edit > Project Settings > Player > Other Settings** and make sure the graphics API is set to **DirectX11**, not DirectX12.

## Project Structure

| Scene | Description |
|---|---|
| `Merger.unity` | Combined scene with all three rooms — the one to run for the full experience. |
| `Jayden.unity`, `WeiCheng.unity`, `k.unity`, `Library*.unity` | Earlier checkpoint scenes kept for reference, not the current build. |