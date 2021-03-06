# Shader Outlines
Although Jewel3D uses GLSL as its core shading language, shaders in Jewel3D are written as a single, complete, program outline.
Shader Outlines allow you to easily describe your own custom data bindings without having to fumble with C++ or OpenGL.

```cpp
// Passthrough.shader
Attributes
{
	vec4 a_vert	: 0;
	vec2 a_uv   : 1;
}

Vertex
{
	out vec2 texcoord;

	void main()
	{
		gl_Position = Jwl_MVP * a_vert;
		texcoord = a_uv;
	}
}

Samplers
{
	sampler2D sTex : 0;
}

Fragment
{
	in vec2 texcoord;
	out vec4 outColor;

	void main()
	{
		outColor = texture(sTex, texcoord);
	}
}
```

# Shader Blocks
All key sections of the shader are described in their own blocks.

* ```Attributes```
	* Describes input mesh data and bindings
* ```Vertex```
	* A standard GLSL vertex shader
* ```Geometry```
	* A standard GLSL geometry shader
* ```Fragment```
	* A standard GLSL fragment shader
* ```Uniforms```
	* Describes uniform buffer data and bindings
* ```Samplers```
	* Describes textures samplers and bindings
	
If a ```Vertex``` block is not defined, a passthrough is automatically created containing the same ```Attributes``` and ```Vertex``` code from the example above.
This allows you to easily create post-process shaders. A ```Fragment``` block can also be omitted and will be substituted with the ```Samplers``` and ```Fragment``` code from the example above.
	
# Uniforms
A Uniform block can itself contain multiple Uniform Buffers. A Uniform Buffer is simply a structure of data bound to a specific ID.
The actual values of a Uniform Buffer are managed on the CPU with the ```Jwl::UniformBuffer``` resource.

A Uniform Buffer can be qualified as ```static```, ```template```, or both. If a Uniform Buffer is either ```static``` or ```template```, its variables can be assigned default values inline.

A ```static``` buffer is global to the shader. It is owned by the shader resource itself and is initialized the first time the shader is loaded.
A buffer created this way can be found in the shader's ```buffers``` data member after loading. The buffer will assume any default values specified inline.

When a Uniform Buffer is qualified as ```template``` it can be reflected to C++ code by calling ```Jwl::Shader::CreateBufferFromTemplate(unsigned)``` with its binding ID.
A uniform buffer created this way will assume any default values.

```Jwl::UniformBuffer```'s and ```Texture```'s can be attached to either the shader resource for global control of the shader's behaviour, or they can
be attached to the Material Component for per-object control. When rendering, Jewel3D will bind all Textures and Buffers from the currently rendering Shader and Material.
If both the shader and Material bind a buffer or texture to the same ID, the Material's per-object data is used.

# System Uniforms, Functions, and Variables
A series of uniforms are available for all GLSL shader blocks.
```cpp
mat4 Jwl_View;
mat4 Jwl_Proj;
mat4 Jwl_ViewProj;
mat4 Jwl_InvView;
mat4 Jwl_InvProj;
vec3 Jwl_CameraPosition; // World space.

mat4 Jwl_MVP;
mat4 Jwl_ModelView;
mat4 Jwl_Model;
mat4 Jwl_InvModel;
mat4 Jwl_NormalToWorld;
```

The following functions are available to use in all GLSL shader blocks.
```cpp
// Converts between color spaces.
float sRGB_to_linear(float x);
vec3 sRGB_to_linear(vec3 v);

float linear_to_sRGB(float x);
vec3 linear_to_sRGB(vec3 v);

// Constructs the Tangent/Bitangent/Normal matrix for use with normal-maps.
// You can pass the standard model attributes as arguments (a_normal, a_tangent.xyz, a_tangent.w).
mat3 make_TBN(vec3 normal, vec3 tangent, float handedness);

// Computes the surface contribution from any type of light and its parameters.
// The first parameter should be a Light's Uniform Buffer.
// 'normal' and 'pos' should be world-space vectors describing the surface.
vec3 compute_light(Light light, vec3 normal, vec3 pos);

bool is_point_light(Light light);
bool is_directional_light(Light light);
bool is_spot_light(Light light);
```

The standard math definitions from Jewel3D/Math/Math.h are also defined in all GLSL shader blocks.
```cpp
#define M_PI     3.14159265358979323846
#define M_E      2.71828182845904523536
#define M_LOG2E  1.44269504088896340736
#define M_LOG10E 0.434294481903251827651
#define M_LN2    0.693147180559945309417
#define M_LN10   2.30258509299404568402
```

# Lighting Example
```cpp
// Lambert.shader
Attributes
{
	vec4 a_vert		: 0;
	vec2 a_uv		: 1;
	vec3 a_normal	: 2;
}

Uniforms
{
	// Default light structure.
	// Populated automatically by the Light Component.
	Light : 0
	{
		vec3 Color;
		vec3 Position;
		vec3 Direction;
		float AttenuationLinear;
		float AttenuationQuadratic;
		float Angle;
		uint Type;
	}

	// Static values owned globally by the shader.
	template static Ambient : 1
	{
		vec3 Color = (0.1, 0.1, 0.1);
	}
}

Vertex
{
	out vec2 texcoord;
	out vec3 norm;
	out vec3 pos;

	void main()
	{
		// Save the world-space position for per-pixel lighting.
		pos = (Jwl_Model * a_vert).xyz;
		// Complete the transformation for the vertex.
		gl_Position = Jwl_ViewProj * vec4(pos, 1.0);

		texcoord = a_uv;
		// Rotate the normal into world-space.
		norm = mat3(Jwl_NormalToWorld) * a_normal;
	}
}

Samplers
{
	sampler2D sTex : 0;
}

Fragment
{
	in vec2 texcoord;
	in vec3 norm;
	in vec3 pos;

	out vec3 outColor;

	void main()
	{
		// Re-normalize after rasterization.
		vec3 normal = normalize(norm);

		// The built-in lighting function respects all light parameters.
		vec3 lighting = Ambient.Color;
		lighting += compute_light(Light, normal, pos);

		// Combine lighting with the diffuse texture.
		outColor = texture(sTex, texcoord).rgb * lighting;
	}
}
```

# Variants
A single Jewel3D shader resource can actualy represent different compiled variants of the original shader source. Such shaders are often refered to as "Uber Shaders".
This allows you to switch the behaviour of your shaders dynamically without having to juggle many different shader instances.

Shader variants in Jewel3D are managed by the ```Jwl::ShaderVariantControl variantDefinitions``` member of the Material Component.

```cpp
auto& material = entity.Get<Material>();

// #define Use_Feature_X
material.variantDefinitions.Define("Use_Feature_X");

// #define BlurAmount 3
material.variantDefinitions.Define("BlurAmount", 3);

// Removes #define Use_Feature_X
material.variantDefinitions.Undefine("Use_Feature_X");

// BlendFunc::CutOut also notifies the shader with a system define.
// #define JWL_CUTOUT
material.SetBlendMode(BlendFunc::CutOut);
```

# sRGB Conversions
It is recommended to use sRGB textures and to composite your final scene into a RenderTarget with an sRGB color buffer.
This will preserve the color balance of your original textures and will improve the accuracy of lighting effects.

When reading from an sRGB texture in a shader the texture fetch will automatically convert the color to linear space.
When rendering to a RenderTarget with an sRGB color buffer, a linear->sRGB conversion will automatically be done for you.

However, when rendering directly to the backbuffer, a linear->sRGB conversion does NOT take place and textures will appear too dark.
In this case, you can call linear_to_sRGB() on your shader's output color.

# Notes
* Uniform Buffer values are seamlessly used between all variants of a shader. 
* A particular shader variant is only compiled the first time that it is used.
