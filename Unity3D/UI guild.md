### UIPanelManager

A panel is an interface of UI. Such as shop, player, etc. UIPanelManager is the manager for all panels.  
You can create the UIPanelManager by the menu "LGFW -> create global manager", this will create a GameObject named Global. The UIPanelManager is on it. You could only have one UIPanelManager.  
UIPanelManager also handles the escape key on key board or android. Pressing that key calls the onPressBackKey() on the most front panel. You can override onBackKey() in UIPanel to define custom behavior. The default behavior is closing the panel forward.

### UIPanelGroup

A UIPanelGroup is a group of panels. This should be eauql to a Canvas. You should first create a canvas, then attach the UIPanelGroup on it. Every UIPanel under it is controlled by it. You can't nest UIPanelGroup.

### UIPanel
UIPanel is a state machine. You can create as many as UIState to add more states. UIState has 4 functions to override to let you listen to the entering and exiting of a state.  
The UIPanel has 4 special states, openForward, openBackward, closeForward and closeBackward. These 4 states, if you assign values to them, are automatically switched to when you open or close a panel. If you are not familiar with the concepts of forward and backward, you can always use forward to open and close a panel.    
You should attach a CanvasGroup to a UIPanel. Because if you have a CanvasGroup, when you switching a state, the state may plays some animation, the CanvasGroup can block the input for buttons until the animation finished.   

The UIPanelManager has 2 functions pushToStack() and popFromStack(). pushToStack() is used to create a functionality that you open a panel, and the last opened panel is automatically closed. Then you can use popFromStack() to close the current one and open the last one. Here you will need to understand forward and backward. You can think they are just 2 groups. When you push a panel, the old panel is closed backward and the new one is opend forward. When you pop the panel, the top panel of the stack is closed forward and poped, the next one is opend backward. You can use the same state for both forward and backward if you don't need different behaviors for switching to a panel and switching back to a panel.