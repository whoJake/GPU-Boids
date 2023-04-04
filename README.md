# GPU-Boids

### 2D Boids simulation ran on the GPU. Lots of settings to mess around with to achieve different results.

Original inspiration from Sebastian Lague in his video at : https://www.youtube.com/watch?v=bqtqltqcQhw 

Also with help of the paper he references at : http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/

Boids are generated on the CPU with random positions and velocities. This data is then sent to the GPU which calculates the next time step of the simulation using a Compute Shader in HLSL. The new information is then sent to another compute shader which draws the boids onto a render texture to be displayed on the screen. A couple of other compute shaders perform post processing on the image such as a simple box blur and a gentle fade to black of trails.

![showcaseGif](https://user-images.githubusercontent.com/37589250/229784378-417a13bd-ab3c-4fa6-a8bb-16fdad7e1cb0.gif)



### Settings available to play with in real-time
![Settings](https://user-images.githubusercontent.com/37589250/229784689-f83f3bc6-f371-4d2c-bf61-89a63e021be6.png)
