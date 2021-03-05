Thanks for purchasing this asset.

If you have any problems\questions regarding this asset please contact me by e-mail: stanislavdol@gmail.com

Troubleshooting:

Problem - I drag&dropped exhaust prefab to the scene but it's not working.
Solution - All particles systems use distance based emission (which means they emit when moved). 
If you want to change this for a certain prefab just open the "emission tab" of corresponding Particle System and you change Distance to Time

P - All particles look flat
S - Do you use lighting in your scenes? As particles are 3d objects they need lighting to work correctly

P - Particles look weird, not the way I want them to. They are overlit\insufficiently lit 
S - There are 2 solutions - shader modifications or custom directional light that affects only these particles.
	I would suggest you to contact me with a few screenshot and I'm sure that I will be able to help.