## Test device:
**CPU:** Intel i5-11400H
**GPU:** NVIDIA GeForce RTX 2050
**RAM:** 16GB
**OS:** Windows 11 Home 64bit

|                        | **4k lights** | **64k lights** | **Average gain from previous** | **Average total gain** |
|------------------------|---------------|----------------|--------------------------------|------------------------|
| Base implementation    | 115 fps       | 3 fps          | -                              | -                      |
| Using bounding spheres | 116 fps       | 21 fps         | 350%                           | 350 %                  |
| Bounding pheres in C++ | 147 fps       | 43 fps         | 66%                            | 420 %                  |
| Cone frustums          | 166 fps       | 53 fps         | 18%                            | 440 %                  |
| GPU Waves              | x fps         | x fps          | x%                             | x%                     |
| 32x32 tiles            | x fps         | x fps          | x%                             | x %                    |
