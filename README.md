# GolfVRSimulator
GolfVRSimulator core systems, AI bots, and custom VR putting mechanics
# GolfVRSimulator

A feature-rich VR multiplayer-style mini-golf simulator built with Unity 3D and C#, inspired by classic golf mechanics and fully adapted for the Meta Quest ecosystem.

### 🚀 Core Mechanics & Systems (WIP)
* **VR Club Swinging & Physics:** Realistic golf club impact physics and accurate velocity-based ball launching.
* **Smart Teleportation & Auto-Follow:** Instant teleport to the ball's location upon hit, with an automatic follow system when the ball comes to a complete rest.
* **360° Orientation Control (Thumbstick Direction):** Seamless orientation adjustment using the thumbstick after teleportation (similar to MiniGolfVR mechanics) to correct player alignment.
* **Offline AI Bot Opponents:** Custom bot navigation logic allowing AI players to find their path, calculate trajectories, and reach the hole independently.
* **Hole Attraction Physics:** Magnetic/suction physics ensuring balls near the cup are naturally pulled into the hole.
* **Turn-Based Game Manager:** Automated sequence management that guides the camera/player first to the hole, then back to the ball for the next stroke.

### 🛠️ Tech Stack
* **Game Engine:** Unity 3D
* **Language:** C#
* **VR SDK:** XR Interaction Toolkit / OpenXR
