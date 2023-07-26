# ContourLineGenerator
Created by Patrick Kavanagh

The ContourDrawer is a tool designed to generate smooth and continuous contour lines from a 2D heightmap image. Each pixel's R channel value in the heightmap represents its height, ranging from 0 to 255.

HOW IT WORKS:

Heightmap Processing:
The tool evaluates the height values of each pixel in the heightmap at a specified interval, constructing a grid of data points.

Marching Squares Algorithm:
Using the grid data, the tool applies the marching squares algorithm, assigning binary values (0 or 1) to each point based on its height compared to the chosen isovalue. These binary values form squares of points by considering their adjacent points in a clockwise direction.

Example:

 A    B 
 O----O 
 |    | 
 |    | 
 O----O
 D    C 


Case Identification:
Analyzing the bitwise OR value of points within each square, the tool identifies one of 15 possible line configurations, such as horizontal or top-left corner. Each case is associated with an appropriate line drawing method.

Example:

 A    B 
 O----O 
 |    | 
 |    | 
 O----1
 D    C 

binary = 0010

case id = 2

Interpolation for Smoother Lines:
To achieve smoother contour lines, the tool applies interpolation, adjusting line angles using weighted height values of surrounding points. This process reduces jaggedness and enhances the visual quality of the contour lines.

Iterative Contouring:
The tool creates multiple sets of contour lines at different height levels by repeating the contouring process for various isovalue levels, decrementing the isovalue by a set contour interval.

Line Connection and Bezier Curves:
Once all isovalue levels are processed, the tool connects points where contour lines meet, creating continuous Line2D objects. A cubic Bezier curve function is applied to each Line2D to reduce the number of points and smooth out the lines, resulting in visually appealing contour representations.

Spot Anomaly Removal:
A function is run on each contour line to calculate the area within it. This is simply the squared distance value from each point to the start point, divided by the total number of points. If the value is below a threshold, that line will be deleted. This is done to remove a few very small lines that appear anomolously and some unnecessary spot points.

Features:

Generates smooth and continuous contour lines from heightmap data.
Real-time manipulation of contour lines, allowing customization of width, colour, and other attributes.
Efficient Bezier curve implementation for line smoothing and optimisation.

ContourDrawer provides a straightforward way to visualize elevation data and other scalar field attributes from heightmap images. It is particularly useful for creating terrain representations or contour maps with professional-looking results.

CONFIGURABLE OPTIONS:

The ContourDrawer class has options to configure a few variables for drawing and calculating contour lines.

ContourInterval:
This is the interval between height values that each line will be drawn (in most real world maps this is 10m of terrain elevation). By default this is set to 4, which means that if the intended height between contour lines on your map is 10m, then the colour difference between 255 and 251 in your heightmap should be 10m.
Each loop that the heightmap processing function runs over will subtract this from isovalue. So if isoValue starts at 255, the next loop will be 251, 247 etc. 

StepSize:
This is the amount of distance between each pixel that is checked for a height value during the initial pass of processing the heightmap. A smaller value results in more intricate detail.

Smallest Allowed Radius:
The smallest area value allowed for contour lines. Any line with a calculated area below this amount will be deleted. Note: area is calculated by adding the squared distance value from each point in the line to the start point, and dividing by the total number of points. There is probably a much better way to calculate area but this way works and is sufficient for the time being.

USAGE:

Simply drag a greyscale heightmap image into the Height Map property on the ContourDrawer node in the Godot editor. 

A simple Player Camera node is also implemented. Click middle-mouse button and move the mouse to drag the camera around and use the mouse wheel to zoom in and out. Alternatively (for Macbook), hold CMD and move mouse to move around and hit = to zoom in and - to zoom out.

ACKNOWLEDGEMENTS:

Nothing in here is really new. The marching squares algorithm is the primary method for generating the contour lines and that was taken from various online tutorials. 

The most valuable one that explained it in simple terms was The Coding Train's video on YouTube:
https://www.youtube.com/watch?v=0ZONMNUKTfU&ab_channel=TheCodingTrain

Other functions, like bezier curves, were taken from the Godot documentation:
https://docs.godotengine.org/en/stable/tutorials/math/beziers_and_curves.html

And the rest was mostly done by getting familiar with the Line2D and Vector2 nodes.

ChatGPT wrote some of the ReadMe, and helped tidy up some code when my brain stopped working from time to time.

CONTRIBUTING & LICENSE:

Feel free to contribute however you like. Feel free to use this however you like. Nothing really new has been done here, just implemented in Godot for use in other projects. Was fun & challenging to create, hope others can get some use out of it.