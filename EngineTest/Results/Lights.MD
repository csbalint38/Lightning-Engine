## Test device:
**CPU:** Intel i5-11400H
**GPU:** NVIDIA GeForce RTX 2050
**RAM:** 16GB
**OS:** Windows 11 Home 64bit

|                        | **4k lights** | **64k lights** | **Average gain from previous** | **Average total gain** |
|------------------------|---------------|----------------|--------------------------------|------------------------|
| Base implementation    | 115 fps       | 3 fps          | -                              | -                      |
| Using bounding spheres | 116 fps       | 21 fps         | 350%                           | 350%                   |
| Bounding pheres in C++ | 147 fps       | 43 fps         | 66%                            | 680%                   |
| Cone frustums          | 166 fps       | 53 fps         | 18%                            | 860%                   |
| GPU Waves              | 241 fps       | 59 fps         | 28%                            | 990%                   |
| 32x32 tiles            | 242 fps       | 102 fps        | 37%                            | 1.71k%                 |