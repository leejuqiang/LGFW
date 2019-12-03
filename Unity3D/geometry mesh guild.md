### Geometry Mesh 
Geometry meshes are simple geometry shape which is flexible to reshape and combine with the sprite in atlas to easily apply textures for them.  
Currently there are some built-in shapes there. They all require an UIAtlas to apply textures.

### UIAtlas
UIAtlas is similar to Unity's SpriteAtlas. It contains multiple images in one large texture. You can create a UIAtlas by the menu "LGFW -> Asset -> UIAtlas" under a folder you specificd. If you click the asset, there is a "build" button in the inspector panel. When you building an atlas, the UIAtlas searchs the textures within the same folder, if you check include subfolders, also searchs the subfolders as well. Then it packs the textures into one large texture. Although this large texture is also in the same folder with the UIAtlas, this texture won't be packed by the UIAtlas.


[Back to main guild page](https://github.com/leejuqiang/LGFW/blob/master/README.md)
