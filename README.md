# Combat-Bees
 
My own submission into the Assembly vs DOTS race based on the video series from Lingon Studios. You can watch the series here: https://www.youtube.com/watch?v=82XkA2r8HNQ&list=PLpeTAN1zvmSP7H_8SFxDEVlGE8N1qGICd

Unfortunately I was unable to achieve a direct comparison of my own implementation and the assembly version. The application ran on my machine but only displayed a white screen.
If someone is able to do this comparison, please let me know! I would love to know the results and add it here. 

Hardware:
11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz   2.30 GHz
32.0 GB DDR4
NVIDIA GeForce RTX 3060 Laptop GPU

Notes:
All jobs which relate to the simulation are based directly off Lingon Studio’s implementation. Exceptions to this are jobs related to dead bees or bee rendering.
All timings taken within the simulation are averages from a set number of previous frames. The global min and max of the applications are seen to the right of the average. See BeeGizmos.cs for implementation details. 
Dead bees are held separately from the alive to ensure there is no way to “fake” performance by having a large number of bees in the dead state.

Important Numbers:
Simulation Time – The CPU time in which all DOTS code ran in the application (per frame).
Render Time – The CPU time in which Unity spent preparing rendering and rendering (per frame).
Total Time – The sum of both of these times, not the total time to run one frame.

Results:

50,000 Bees:
FPS: 527.
SimTime: 0.222 ms.
RenderTime: 0.041 ms.

500,000 Bees:
FPS: 197.
SimTime: 2.980 ms.
RenderTime: 0.803 ms.
TotalRenderTime: 3.551 ms.

1000000 Bees:
FPS: 94.
SimTime: 8.452 ms.
RenderTime: 1.627 ms.
TotalRenderTime: 10.139 ms.

Results: Although it is unreasonable for DOTS to outperform any hand crafted assembly solution, it can, with the correct designs, get close. Even so, it is important to know that although DOTS may function well, we must still act within the confines of the Unity engine. And so, when it came to rendering, we saw huge differences in the Lingon Studios renderer versus the Unitys.

To my DOTS developers: Part of the reason I took up this challenge was to show a common trap eager DOTS developers fall into. That is of course forcing designs on the entities package. The entities package is an important part of the DOTS ecosystem and is important for providing quick and easy scaling of game worlds. But this also means that it is not always the best tool for the job – especially when dealing with simple structured data. So remember to question how you interact, and what you really need from your data.
