# Combat-Bees
 
My own submission into the Assembly vs DOTS race based on the video series from Lingon Studios. You can watch the series [here](https://www.youtube.com/watch?v=82XkA2r8HNQ&list=PLpeTAN1zvmSP7H_8SFxDEVlGE8N1qGICd)
and you can watch my video on this repostitory [here](https://www.youtube.com/watch?v=A28aPGRhU6A).

Fortunatly, with the help of Lingon themself, I was able to run some direct comparison with the assembly versions!

**Hardware:**
- 11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz
- NVIDIA GeForce RTX 3060 Laptop GPU
- 32.0 GB DDR4

**Important Numbers:**
- Simulation Time – The CPU time in which all DOTS code ran in the application (per frame).
- Render Time – The CPU time in which Unity spent preparing rendering and rendering (per frame).
- Calculated Frame Time – The miliseconds it would take to render one frame at the average FPS.

## DOTS Results

### Notes:
- All jobs which relate to the simulation are based directly off Lingon Studio’s C# implementations. Exceptions to this is dead bees and bee rendering. Both could be sped up to better utilization Unity/DOTS.
- All timings taken within the simulation are averages from a set number of the previous frames. The global min and max of the timings are seen to the right of the average in square brackets.
- Dead bees are held separately from the alive to ensure there is no way to “fake” performance by having a large number of bees in the dead state.

### | 50,000 Bees | FPS: 527 | SimTime: 0.222 ms | RenderTime: 0.041 ms | Calculated Frame Time 1.898 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/DOTS-FiftyThousandBees.PNG?raw=true" width="800" />

### | 500,000 Bees | FPS: 197 | SimTime: 2.980 ms | RenderTime: 0.803 ms | Calculated Frame Time 5.076 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/DOTS-HalfMillionBees.PNG?raw=true" width="800" />

### | 1000000 Bees | FPS: 94 | SimTime: 8.452 ms | RenderTime: 1.627 ms | Calculated FrameTime 10.638 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/DOTS-MillionBees.PNG?raw=true" width="800" />

## Assembly Results:

### Notes: 
- The Lingon demo only had a generalized FPS counter meaning it did not track mins and maxs. So in order to get the most accurate and fair measurment, the program would be run for at least 30 seconds and only taken once the number was stable. Even so, its likely that all numbers tracked are plus or minus a certain range which I have tried to approximate here.

### | 50,000 Bees | FPS: 1291 +- 80 | Calculated Frame Time 0.774 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/ASM-MT-FiftyThousandBees.PNG?raw=true" width="800" />

### | 500,000 Bees | FPS: 239 +- 7 | Calculated Frame Time: 4.184 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/ASM-MT-HalfMillionBees.PNG?raw=true" width="800" />

### | 1000000 Bees | FPS: 92 +- 3 | Calculated Frame Time: 10.8670 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/ASM-MT-MillionBees.PNG?raw=true" width="800" />
