# Adaptive-Brightness-Unity
Adaptive brightness post processing effect. Created with Unity, URP 14.

When entering scenes with different brightnesses, human eyes and cameras automatically adapt to the different brightnesses. That's why when you stand outside in a brighter area and look in, you will see that the interior will be darker. As is shown in the following pictures.
![{_}1PRB2O3`65H{J@U%72T](https://github.com/SuperDragonXu/Adaptive-Brightness-Unity/assets/110776343/b29f6b70-ae31-4feb-9354-61dc0e02cb56)
The room looks dark from outside.

![1BDCA5 X5REZHF%$OC`1_)J](https://github.com/SuperDragonXu/Adaptive-Brightness-Unity/assets/110776343/cee90949-3af8-4174-a013-5d7a54edc2eb)
When the camera enters the room, it looks brighter.
this repo simulates this effect in Unity using compute shader and post processing.

The post processing effect works with a grab pass. To enable it, the adaptive brightness effect, you first have to add the Grab Color pass into the render feature.
