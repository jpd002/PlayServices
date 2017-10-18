#include <cassert>
#include "string_format.h"
#include "localdb/LocalDbClient.h"
#include "SqliteStatement.h"
#include "Utf8.h"

using namespace LocalDb;

#define DATABASE_VERSION 1

static const char* g_dbPath = "local.db";

static const char* g_gamesTableCreateStatement = 
"CREATE TABLE IF NOT EXISTS games"
"("
"    path TEXT PRIMARY KEY,"
"    gameId VARCHAR(10) DEFAULT '',"
"    lastPlayedTime INTEGER DEFAULT 0"
")";

template <typename StringType>
static std::string GetPathStringInternal(const StringType& str);

template <>
std::string GetPathStringInternal(const std::string& str)
{
	return str;
}

template <>
std::string GetPathStringInternal(const std::wstring& str)
{
	return Framework::Utf8::ConvertTo(str);
}

static std::string GetPathString(const boost::filesystem::path& path)
{
	return GetPathStringInternal(path.native());
}

CClient::CClient()
{
	CheckDatabaseVersion();

	m_db = CSqliteDb(g_dbPath, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE);

	{
		auto query = string_format("PRAGMA user_version = %d", DATABASE_VERSION);
		CSqliteStatement statement(m_db, query.c_str());
		statement.StepNoResult();
	}

	{
		CSqliteStatement statement(m_db, g_gamesTableCreateStatement);
		statement.StepNoResult();
	}
}

Game CClient::GetGame(const boost::filesystem::path& path)
{
	Game game;

	CSqliteStatement statement(m_db, "SELECT gameId, lastPlayedTime FROM games WHERE path = ?");
	statement.BindText(1, GetPathString(path).c_str());
	statement.StepWithResult();

	game.gameId = reinterpret_cast<const char*>(sqlite3_column_text(statement, 0));
	game.lastPlayedTime = sqlite3_column_int(statement, 1);

	return game;
}

void CClient::RegisterGame(const boost::filesystem::path& path)
{
	CSqliteStatement statement(m_db, "INSERT OR IGNORE INTO games (path) VALUES (?)");
	statement.BindText(1, GetPathString(path).c_str());
	statement.StepNoResult();
}

void CClient::SetGameId(const boost::filesystem::path& path, const char* gameId)
{
	CSqliteStatement statement(m_db, "UPDATE games SET gameId = ? WHERE path = ?");
	statement.BindText(1, gameId, true);
	statement.BindText(2, GetPathString(path).c_str());
	statement.StepNoResult();
}

void CClient::SetLastPlayedTime(const boost::filesystem::path& path, time_t lastPlayedTime)
{
	CSqliteStatement statement(m_db, "UPDATE games SET lastPlayedTime = ? WHERE path = ?");
	statement.BindInteger(1, lastPlayedTime);
	statement.BindText(2, GetPathString(path).c_str());
	statement.StepNoResult();
}

void CClient::CheckDatabaseVersion()
{
	bool dbExistsAndMatchesVersion = 
		[] ()
		{
			try
			{
				auto db = CSqliteDb(g_dbPath, SQLITE_OPEN_READONLY);

				CSqliteStatement statement(db, "PRAGMA user_version");
				statement.StepWithResult();
				int version = sqlite3_column_int(statement, 0);
				return (version == DATABASE_VERSION);
			}
			catch(...)
			{
				return false;
			}
		} ();

	if(!dbExistsAndMatchesVersion)
	{
		boost::filesystem::remove(g_dbPath);
	}
}
