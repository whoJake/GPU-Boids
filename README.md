# GPU-Boids

### 2D Boids simulation ran on the GPU. Lots of settings to mess around with to achieve different results.

Original inspiration from Sebastian Lague in his video at : https://www.youtube.com/watch?v=bqtqltqcQhw 

Also with help of the paper he references at : http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/

Boids are generated on the CPU with random positions and velocities. This data is then sent to the GPU which calculates the next time step of the simulation using a Compute Shader in HLSL. The new information is then sent to another compute shader which draws the boids onto a render texture to be displayed on the screen. A couple of other compute shaders perform post processing on the image such as a simple box blur and a gentle fade to black of trails.

![showcaseGif](https://user-images.githubusercontent.com/37589250/229784378-417a13bd-ab3c-4fa6-a8bb-16fdad7e1cb0.gif)



### Settings available to play with in real-time
![Settings](https://user-images.githubusercontent.com/37589250/229784689-f83f3bc6-f371-4d2c-bf61-89a63e021be6.png)


## Improvements and next steps for this project
I have plans to rewrite this project at some point but implement spatial hashing into it all. I tried to do this at some point during this project but was approaching it wrong. It should massively improve performance and allow me to run simulations with boids in the 10-100 thousands as right now theres a limit of around 6000. I've never implemented spatial hashing so it should be a good learning experience with very practical use cases.
