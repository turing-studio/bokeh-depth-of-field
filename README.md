# Convolutional Bokeh Depth of Field

Depth of field effect implementation in Unity Engine using URP and custom shader, inspired by Frostbite engine. Image kernel used for this effect is based on linearly separable convolution in complex domain. This implementation offers high quality bokeh without any significant performance decrease.

Detailed description of separable convolutional DoF, with formulas, is available on [this link](http://yehar.com/blog/?p=1495).


| Original | Bokeh | Gaussian |
| -------- | -------- | -------- |
|![picture-1-original](https://user-images.githubusercontent.com/114662379/220890286-1af8a1b3-a282-44a7-92b5-cd3fc4942f33.png) | ![picture-1-bokeh](https://user-images.githubusercontent.com/114662379/220890259-49ced646-28a0-4f4f-bd2e-8e7f646bd817.png)| ![picture-1-gauss](https://user-images.githubusercontent.com/114662379/220890284-f3b8f245-eede-4e44-9f92-1e64c15eeace.png) |
| ![picture-2-original](https://user-images.githubusercontent.com/114662379/220890295-efbe32dd-490d-4c85-a6a9-1d571302b6b0.png) | ![picture-2-bokeh](https://user-images.githubusercontent.com/114662379/220890291-0fca5111-b0d6-4de8-bde9-b589b940104f.png) | ![picture-2-gauss](https://user-images.githubusercontent.com/114662379/220890294-5250dd5a-4c93-4ce1-8329-9412128dab4f.png) |
| ![picture-3-original](https://user-images.githubusercontent.com/114662379/220890305-65f6ef8b-5fa6-4e85-98f9-0b44c63af482.png) | ![picture-3-bokeh](https://user-images.githubusercontent.com/114662379/220890300-61a6f90e-c95f-4197-8439-89aae90b838b.png) | ![picture-3-gauss](https://user-images.githubusercontent.com/114662379/220890303-55d5a178-7079-4df5-82c1-6e799dc61d5b.png) |

