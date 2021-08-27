# UnityProceduralBiped

This was just my personal repo for while I worked on this project,
but I made it public so people could look at it if they wanted. I don't recommend
copying this code, I just added and removed things as I went and am not planning on
cleaning it up.

The initial goal was to have something I could quickly attach to a bipedal
model and get a walking animation that covered all directions, rotations, and 
height differences. I don't like working with animations so I figured this would be
a more fun solution.

The methodology:
https://www.youtube.com/watch?v=tXmRIz1g7s0

How to use:
1. Hook up the IK rig for the legs according to this guy or the official
Unity video:
https://www.youtube.com/watch?v=AChwSWU4AaU

2. Add IK solver scripts to target position of feet

3. Attach LegController script to GameObject.

4. Set the variables. There are a lot of them, but most of them just control
the width and length of the box that determines foot placement.
