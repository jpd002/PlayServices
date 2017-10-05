#include "basedb/BaseDbClient.h"
#include <cassert>

using namespace BaseDb;

static const char* g_dbPath = "C:\\ProjectsGit\\PlayServices\\basedb\\games.db";

CClient::CClient()
{
	m_db = CSqliteDb(g_dbPath, SQLITE_OPEN_READONLY);
}

Game CClient::GetGame(const char* serial)
{
	int result = SQLITE_OK;
	sqlite3_stmt* statement = nullptr;

	result = sqlite3_prepare_v2(m_db, "SELECT GameID, GameTitle FROM games WHERE serial = ?", -1, &statement, nullptr);
	assert(result == SQLITE_OK);

	result = sqlite3_bind_text(statement, 1, serial, -1, nullptr);
	assert(result == SQLITE_OK);

	result = sqlite3_step(statement);
	if(result != SQLITE_ROW)
	{
		throw std::runtime_error("Failed to get game.");
	}

	Game game;
	game.theGamesDbId = sqlite3_column_int(statement, 0);
	game.title = reinterpret_cast<const char*>(sqlite3_column_text(statement, 1));

	result = sqlite3_finalize(statement);
	assert(result == SQLITE_OK);

	return game;
}
