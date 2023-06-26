# Combat-Bees
 
My own submission into the Assembly vs DOTS race based on the video series from Lingon Studios. You can watch the series [here](https://www.youtube.com/watch?v=82XkA2r8HNQ&list=PLpeTAN1zvmSP7H_8SFxDEVlGE8N1qGICd)
and you can watch my video on this repostitory [here](https://www.youtube.com/watch?v=A28aPGRhU6A).

Fortunatly, with the help of Lingon themself, I was able to run some direct comparison with the assembly versions!

## Results:

**Hardware:**
- 11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz
- NVIDIA GeForce RTX 3060 Laptop GPU
- 32.0 GB DDR4

**Important Numbers:**
- Simulation Time – The CPU time in which all DOTS code ran in the application (per frame).
- Render Time – The CPU time in which Unity spent preparing rendering and rendering (per frame).
- Total Time – The sum of both of these times, not the total time to run one frame.

### | 50,000 Bees | FPS: 527 | SimTime: 0.222 ms | RenderTime: 0.041 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/FifythousandBees.PNG?raw=true" width="800" />

### | 500,000 Bees | FPS: 197 | SimTime: 2.980 ms | RenderTime: 0.803 ms | TotalRenderTime: 3.551 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/HalfMillionBees.PNG?raw=true" width="800" />

### | 1000000 Bees | FPS: 94 | SimTime: 8.452 ms | RenderTime: 1.627 ms | TotalRenderTime: 10.139 ms |
<img src="https://github.com/OfficialFoneE/Combat-Bees/blob/main/Screenshots/MillionBees.PNG?raw=true" width="800" />

### Notes:
- All jobs which relate to the simulation are based directly off Lingon Studio’s C# implementations. Exceptions to this is dead bees and bee rendering. Both could be sped up to better utilization Unity/DOTS.
- All timings taken within the simulation are averages from a set number of the previous frames. The global min and max of the timings are seen to the right of the average in square brackets.
- Dead bees are held separately from the alive to ensure there is no way to “fake” performance by having a large number of bees in the dead state.

## Conclusion
Although it is unreasonable for DOTS to outperform any hand crafted assembly solution, it can, with the correct designs, get close. Even so, it is important to know that although DOTS may function well, we must still act within the confines of the Unity engine. And so, when it came to rendering, we saw huge differences in the Lingon Studios renderer versus the Unitys.

**To my DOTS developers:** Part of the reason I took up this challenge was to show a common trap eager DOTS developers fall into -- designing exclusibly with entities. The entities package is an important part of the DOTS ecosystem and serves to provid quick and easy scaling for game worlds. However, this also means that it is not always the best tool for the job – especially when dealing with the simple structured of data that we had here.
