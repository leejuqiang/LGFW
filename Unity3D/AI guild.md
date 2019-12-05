### Neural Network
The class NeuralNetwork is a neural network with a structure similar to Pytorch.  
The network itself does nothing, you need to add layers to it:

Linear layer is the basic perceptron layers.  
Convolution layer is the filters of a convolutional neural network.  
Max pooling layer and average pooling layer are the pooling layers of a convolutional neural network.  
Sigmoid, rule and soft max layer are the layers of the active functions.  
MSELoss and logloss are the layers for loss function.  

You don't need to specific the input number of each layer except the first layer. The input number for each layer is computed automatically. But for some layers, you need to make sure the parameters for the constructor are correct, so the output of one layer can match the input of the next layer.

There is a precompile macro "NN_USE_FLOAT", define this to specific the neural network using float instead of double for input,  output and parameters.  

To use a custom pooling layer, the subclass of PoolingLayer must override computePool() and bpInToOut(). The first one used to compute the value of a pool. The second used to compute the derivative for one output to one input.

Most of the layers support dropout. Pooling layer doesn't support dropout.


[Back to main guild page](https://github.com/leejuqiang/LGFW/blob/master/README.md)
