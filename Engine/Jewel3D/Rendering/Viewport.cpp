// Copyright (c) 2017 Emilian Cioca
#include "Jewel3D/Precompiled.h"
#include "Rendering.h"
#include "Viewport.h"

namespace Jwl
{
	Viewport::Viewport(unsigned _x, unsigned _y, unsigned _width, unsigned _height)
		: x(_x)
		, y(_y)
		, width(_width)
		, height(_height)
	{
	}

	void Viewport::bind() const
	{
		SetViewport(x, y, width, height);
	}

	float Viewport::GetAspectRatio() const
	{
		return static_cast<float>(width) / static_cast<float>(height);
	}
}
