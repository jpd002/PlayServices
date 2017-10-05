#pragma once

#include "Singleton.h"

namespace CoverDb
{
	class CClient : public CSingleton<CClient>
	{
	public:
		virtual ~CClient() = default;

		void SetGameCover(const char*, const char*);
	};
}
