
# ROS Component of Mosaic Maker

## Introduction
This section focuses on the ROS-based backend of the Mosaic Maker, dealing with robotic arm control, motion planning, and sensor integration.

## ROS Version
- **ROS Noetic:** All development and testing are conducted on ROS Noetic.

## ROS Architecture
- **Services and Topics:**
  - `listen_move`: Manages robotic arm logic for motion planning based on Unity's environmental data.
  - `cameraScanning`: Handles image and point cloud segmentation, crucial for accurate object reconstruction.
  - `ur_hardware_interface/set_io`: Operates the robotic arm's gripper for precise object manipulation.
- **Additional Components:** Description of any additional ROS nodes, services, or packages used.

## ROS Setup
- **Initial Setup:** Steps to set up the ROS environment, including ROS Noetic installation and workspace configuration.
- **Unity-ROS Integration:** Detailed instructions on integrating ROS with Unity, referencing the Unity Robotics Hub.
- **MoveIt Configuration:** Guide for configuring MoveIt with the specific robotic arm model, including setting up the planning environment and customizing motion planning algorithms.

## Running the ROS Component
- Instructions on initializing the ROS environment, launching necessary nodes and services, and ensuring proper communication with Unity.
