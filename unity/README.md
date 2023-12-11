
# Unity Component of Mosaic Maker

## Overview
This Unity project is an integral part of Mosaic Maker, focusing on two main modules: Nesting and ROS. The Nesting module is at the heart of the mosaic creation process, managing color bins, object placement, and SVG representations. The ROS module facilitates communication with the Robot Operating System (ROS), ensuring seamless integration between Unity and the robotic control systems.

## Nesting Module

### Key Components

#### ColourBinsManager
- **Functionality:** Manages color bins for mosaic creation.
- **Features:**
  - Instantiates and manages bin objects.
  - Handles setting up 3D clones and managing placed objects.

#### MosaicNesting
- **Purpose:** Central to the mosaic creation logic.
- **Key Features:**
  - `SaveDataApp` class for saving nesting progress.
  - Functions for checking object status and clearing all placed objects.

#### BinPlacer
- **Role:** Handles object placement within bins.
- **Strategy:**
  - Implements a bottom-left placement algorithm.
  - Manages 3D positioning of scanned objects.

### Technical Insights
- The module employs advanced algorithms for efficient and aesthetically pleasing mosaic layouts.
- Innovative approaches are used for color management and object placement, ensuring optimal use of space and color harmony.

## ROS Module

### Overview
The ROS module is designed to bridge Unity and the Robot Operating System. It enables real-time data exchange and control, playing a crucial role in the robotic aspect of Mosaic Maker.

### Key Functionalities
- **Data Exchange:** Facilitates seamless communication between Unity and ROS, ensuring accurate and timely control commands.
- **Real-Time Updates:** Handles updates and feedback from the robotic systems, integrating them into the Unity environment.

## Integration Points
- **Nesting and ROS Interaction:** The modules work in tandem, with the Nesting module providing layout data that informs the robotic actions controlled via the ROS module.
- **Data Protocols:** Utilizes specific data formats and communication protocols to maintain consistency and reliability in data exchange.

## Future Enhancements
- Continuous improvement in the algorithms for nesting and color management.
- Further refinement in Unity-ROS communication for enhanced performance and capabilities.
