## Test device:
**CPU:** Intel i5-11400H
**GPU:** NVIDIA GeForce RTX 2050
**RAM:** 16GB
**OS:** Windows 11 Home 64bit

|               | Base implementation | Using bounding spheres  | Gain | Calculate Bounding Spheres in C++ side | Gain | Cone frustums | Gain | 32x32 tiles | Gain | Total Gain |
|---------------|---------------------|-------------------------|------|----------------------------------------|------|---------------|------|-------------|------|------------|
|**4k lights**  | 115 fps             | 116 fps                 | 1%   | 147 fps                                | 27%  |  166 fps      | 13%  | 162 fps     | -2%  | 40%        |
|**64k lights** | 3 fps               | 21 fps                  | 700% | 43 fps                                 | 104% |  53 fp        | 23%  | 88 fps      | 66%  | 2 833%     |
