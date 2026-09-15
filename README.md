# Kinesthetic_Teleoperation
full-immersion robotic teleoperation system that maps human locomotion directly to remote hardware. By combining a VR treadmill for omnidirectional walking with real-time video feedback, the system allows a user to physically walk through a remote space while simultaneously controlling a physical robot's movement and camera orientation


Steps to Follow
Install hello-robot-stretch-body:
```
pip install hello-robot-stretch-body
```

Clone the Repository:
```
git clone https://github.com/hello-robot/stretch_body.git
```

Navigate to the Demo Directory:
```cd stretch_body/tools```

Run the Demos: The tools directory contains various demo scripts. For example, to run the basic movement demo, you can execute:
```python demo_move.py```

Connecting the Device
You need to plug in the device to the computer. The device typically includes a USB cable to connect to your computer. Ensure that the USB cable is properly connected to both the device and the computer.

Example Script
Here is an example script from the demo_move.py file, which you can use to move the robot:
```
import time
from hello_robot_stretch_body import StretchBody

body = StretchBody('robot_name')  # Replace 'robot_name' with your actual robot name

try:
    body.calibrate('arm')
    body.calibrate('wheels')

    body.arm.move_to_joint_positions([0, 0, 0, 0, 0, 0], 0.5)
    body.wheels.set_speed(1.0)

    time.sleep(5)

finally:
    # Stop the wheels and reset joint angles
    body.wheels.set_speed(0)
    body.arm.move_to_joint_positions([0, 0, 0, 0, 0, 0], 0.5)
    body.close()
```
Running the Script
Ensure your robot is connected and powered on.
Run the script from the terminal:
```python demo_move.py```

Troubleshooting
If you encounter any issues, you can refer to the following resources:
https://docs-arch.hello-robot.com/0.3/
https://github.com/hello-robot/stretch_body
https://github.com/hello-robot/stretch_ai


#Get Started Writing Scripts

https://docs-arch.hello-robot.com/0.3/getting_started/writing_code/
