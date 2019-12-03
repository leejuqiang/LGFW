### Neural Network
The class NeuralNetwork is a neural network with a structure similar to Pytorch.  
The network itself does nothing, you need to add layers to it:

Linear layer is the basic perceptron layers.  
Convolution layer is the filters of a convolutional neural network.  
Pooling layer is the pooling layer of a convolutional neural network.  
Sigmoid, rule and soft max layer are the layers of the active functions.  
MSELoss and logloss are the layers for loss function.  

You don't need to specific the input number of each layer except the first layer. The input number for each layer is computed automatically. But for some layers, you need to make sure the parameters for the constructor are correct, so the output of one layer can match the input of the next layer.


[Back to main guild page](https://github.com/leejuqiang/LGFW/blob/master/README.md)
