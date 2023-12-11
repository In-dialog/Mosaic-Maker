# Unity Component of Mosaic Maker

## Overview
The Unity component of Mosaic Maker represents the forefront of combining real-time mosaic creation with advanced robotics. It features two primary modules: Nesting for intricate mosaic layout planning and ROS for precise robotic interactions.

![UI Unity](https://github.com/In-dialog/Mosaic-Maker/blob/main/images/UI.png)  
*The Unity Interface of Mosaic Maker, showcasing the integration of Nesting and ROS modules.*

## Nesting Module

### Overview
The Nesting module, the heart of the mosaic creation process, orchestrates the layout and design of mosaics with precision and artistry.

#### ColourBinsManager
- **Functionality:** This component categorizes and manages color bins, essential for sorting and arranging mosaic tiles according to color themes.
- **Operations:** It's responsible for instantiating bin objects and their 3D representations, enhancing visual interaction and management.

![ColourBins Visualization](https://github.com/In-dialog/Mosaic-Maker/blob/main/images/Color_Bin_Mosaic.png)  
*Visualization of Color Bins managed by ColourBinsManager, highlighting the color organization process.*

#### MosaicNesting
- **Purpose:** Acts as the command center for the nesting logic, integrating various components for a cohesive mosaic layout.
- **Capabilities:**
  - Accommodates a diverse range of object shapes, including both regular and irregular forms.
  - Specifically optimized for single-hole bin configurations, demonstrating versatility in handling different mosaic designs.
- **Nesting Strategy:**
  - The initial object placement follows a bottom-left algorithm, efficiently utilizing space within the bin.
  - Employs a sophisticated Non-Fitting Polygon (NFP) algorithm for subsequent placements, meticulously selecting the best placement based on project-specific requirements.
  - This dynamic, on-the-go nesting approach allows for real-time adjustments, vital for bespoke mosaic creations.

![Mosaic Nesting Regular Shapes](https://github.com/In-dialog/Mosaic-Maker/blob/main/images/Mosaic_Nesting.png)  
*Nesting of Regular Shapes, demonstrating the efficiency of the bottom-left placement strategy.*

![Mosaic Nesting Irregular Shapes](https://github.com/In-dialog/Mosaic-Maker/blob/main/images/Mosaic_Nesting_Irregular.png)  
*Nesting of Irregular Shapes, showcasing the adaptability of the NFP algorithm.*

#### BinPlacer
- **Purpose:** Specialized in strategically placing objects within bins, adapting to the unique contours of each piece.
- **Strategy:**
  - Utilizes bottom-left placement logic for the initial object, ensuring the most efficient use of space.
  - Adapts to the irregular shapes of objects, ensuring each piece fits perfectly within the layout.

### Technical Insights
- The Nesting module incorporates cutting-edge algorithms for both regular and irregular object placement, ensuring each piece is positioned to achieve the best possible aesthetic and functional result. The use of NFP algorithms represents a significant advancement in mosaic technology, allowing for more complex and visually striking designs.

## ROS Module

### Overview
The ROS module acts as a bridge between digital planning and physical execution, translating the intricate designs from Unity into precise robotic actions.

#### RobotManager
- **Functionality:** Serves as the central hub for ROS communications, converting Unity's detailed plans into robotic instructions.
- **Integration with RobotTaskManager:** This subsystem decomposes complex tasks into executable actions, guiding the robotic arm with precision.

#### RobotTaskManager
- **Role:** Manages and sequences each robotic task, ensuring fluid and accurate execution of the mosaic placement.

![ROS Connection](https://github.com/In-dialog/Mosaic-Maker/blob/main/images/ROS.png)  
*The ROS connection interface, highlighting the communication between Unity and the robotic system.*

### User Interaction and Control
- The Unity interface offers a user-friendly control panel, where users can upload images, segment them based on color profiles, and dynamically control the mosaic creation process, including pause and resume functionalities.

## Data Exchange and Integration
- Unity sends detailed positional and orientation data to ROS, dictating the robotic arm's trajectory.
- Feedback from ROS includes joint trajectory data, which Unity uses to refine and adjust the robotic movements for flawless execution.

## Use Case Scenario: Robotic Mosaic Creation
- Mosaic Maker was employed in a scenario where it generated a mosaic using a robot. The system scanned objects on a table using an Intel RealSense camera, reconstructed them in Unity, and then directed the robot to pick and place each piece according to the Nesting module's layout.

## Future Enhancements
- Ongoing developments aim to enhance the parallel processing capabilities of the nesting algorithm, significantly boosting speed and efficiency.
- Continuous refinement of Unity-ROS communication is planned, expanding the system's capabilities and performance.