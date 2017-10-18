#pragma once

#include <string>
#include <boost/filesystem.hpp>
#include "Types.h"
#include "Singleton.h"
#include "../SqliteDb.h"

namespace LocalDb
{
	struct Game
	{
		std::string gameId;
		time_t lastPlayedTime = 0;
	};

	class CClient : public CSingleton<CClient>
	{
	public:
		CClient();
		virtual ~CClient() = default;

		Game GetGame(const boost::filesystem::path&);
		void RegisterGame(const boost::filesystem::path&);

		void SetGameId(const boost::filesystem::path&, const char*);
		void SetLastPlayedTime(const boost::filesystem::path&, time_t);

	private:
		void CheckDatabaseVersion();

		CSqliteDb m_db;
	};
};
