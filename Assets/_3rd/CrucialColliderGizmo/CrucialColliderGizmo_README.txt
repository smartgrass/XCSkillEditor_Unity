/*

Crucial Collider Gizmo v1.4.1 by TimeFloat
(http://timefloathome.wordpress.com/)

Thanks for purchasing Crucial Collider Gizmo!

To use Crucial Collider Gizmo, simply attach this script as a component to any GameObject with one or more colliders. Once attached, you'll see the following options appear in the inspector:

-- Enabled: This functions just like the enable/disable checkbox on native Unity components. It's on by default, but if you'd like to temporarily hide all the drawing functions of a specific CCG object, just toggle this checkbox off.

-- Color Preset: You can choose to use any of the preexisting color presets to automatically set your colors. If you change a color after choosing a preset, this will change to "custom". Selecting "custom" again after choosing a different preset will set your colors to the state that they were in the last time you were using "custom".

-- Overall Transparency: This slider will make your collider gizmo, as a whole, more or less opaque. Alpha values from colors for each element (wire, fill, center marker) will still be taken into account.

-- Wire Color: Click inside this box of color to pick a color and alpha value for the wireframe of your collider.

-- Fill Color: Click inside this box of color to pick a color and alpha value for the face fill of your collider.

-- Center Color: Click inside this box of color to pick a color and alpha value for the center marker of your collider. The center marker will be a sphere at the center of the collider, with a radius of your choosing.

-- Draw Fill: Check this box if you would like the face fill to be drawn. For instance, if you uncheck this, but check Draw Wire, you would just see the wireframe of the collider.

-- Draw Wire: Check this box to draw the wireframe of the collider.

-- Draw Center: Check this box to draw a sphere in the center of the collider with a radius of your choosing.

-- Edge Collider Marker Radius: This is the radius of the points along your Edge Collider 2d. It's in Unity units.

-- Center Marker Radius: This is the radius of your spherical center marker. It's in Unity units.

-- 2D Collider Z Depth: This is the z size of the box collider drawn when drawing a BoxCollider2D. This is added as a convenience so you can give your collider gizmo thickness and make it easier to see/click on.

-- Include Child Colliders: If this is checked, then every collider in the entire heirarchy of the gameobject that CCG is attached to will have their colliders drawn as well.

Remember, CCG only works for BoxCollider2D, CircleCollider2D, BoxCollider, and SphereCollider types.

If you have any problems or suggestions, feel free to post on the TimeFloat CCG page, at http://timefloathome.wordpress.com/crucial-collider-gizmo/, or contact me at TimeFloatUnity@gmail.com . Thanks again for purchasing! Be sure to check out other TimeFloat products too!

*/
