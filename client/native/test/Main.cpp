#include "string_format.h"
#include "http/HttpClientFactory.h"
#include "basedb/BaseDbClient.h"
#include "thegamesdb/TheGamesDbClient.h"

void PrintGameInfo(const char* gameId)
{
	uint32 theGamesDbId = 0;

	try
	{
		auto legacyGame = BaseDb::CClient::GetInstance().GetGame(gameId);
		theGamesDbId = legacyGame.theGamesDbId;
		if(theGamesDbId == 0)
		{
			//If no ID found in database, then, try a fuzzy lookup using the game name specified in the
			//legacy database
			auto gamesList = TheGamesDb::CClient::GetInstance().GetGamesList("sony playstation 2", legacyGame.title);
			if(gamesList.empty())
			{
				//Nothing found, we're screwed
			}
			else
			{
				//This is the one (might be wrong due to fuzzy search)
				auto gamesListItem = gamesList[0];
				theGamesDbId = gamesListItem.id;
			}
		}
	}
	catch(const std::exception& exception)
	{
		printf("Failed to obtain game id: '%s'.\r\n", exception.what());
		return;
	}

	try
	{
		if(theGamesDbId != 0)
		{
			//We've got an ID, so, we can fetch information for that one
			auto theGamesDbGame = TheGamesDb::CClient::GetInstance().GetGame(theGamesDbId);
			auto imageUrl = string_format("%s/%s", theGamesDbGame.baseImgUrl.c_str(), theGamesDbGame.boxArtUrl.c_str());
			//CoverDb::CClient::GetInstance().SetGameCover(gameId, imageUrl.c_str());
			printf("Game '%s' title: '%s', coverUrl: '%s'.\r\n", 
				gameId, theGamesDbGame.title.c_str(), theGamesDbGame.boxArtUrl.c_str());
		}
		else
		{
			printf("Could not find info for game id '%s'.\r\n", gameId);
		}
	}
	catch(const std::exception& exception)
	{
		printf("Failed to obtain game info: '%s'.\r\n", exception.what());
		return;
	}
}

void DoHttpTest()
{
	//GET test
	if(false)
	{
		auto httpClient = Framework::Http::CreateHttpClient();
		httpClient->SetUrl("http://www.google.ca");
		httpClient->SetVerb(Framework::Http::HTTP_VERB::GET);
		auto result = httpClient->SendRequest();
		printf("Status code: %d\r\n", result.statusCode);
	}
	
	//POST test
	if(false)
	{
		auto httpClient = Framework::Http::CreateHttpClient();
		httpClient->SetUrl("http://ps.purei.org/api.php?endpoint=compatibility");
		httpClient->SetRequestBody("{ \"gameId\": \"SLUS_213.09\", \"rating\": 5, \"deviceInfo\": {} }");
		httpClient->SetVerb(Framework::Http::HTTP_VERB::POST);
		auto result = httpClient->SendRequest();
		printf("Status code: %d\r\n", result.statusCode);
	}
}

int main(int argc, const char** argv)
{
	static const char* gameIds[] =
	{
		"SCUS_971.02",   //Gran Turismo 3
		"SLPM_625.53",   //SEGA AGES Phantasy Star 1
		"SLPM_625.32",   //Ys 3: Wanderers from Ys
		"SLPM_660.31",   //Phantasy Star Universe
		"SLPS_250.33",   //Klonoa 2
	};

	for(const auto& gameId : gameIds)
	{
		PrintGameInfo(gameId);
	}

	//Typical usage
	//- (Mobile Platforms) Scan games available on filesystem
	//- For every game, launch game info (from base db)
	//- For every game, get cover
	//  - Cover is obtained from cover cache (can this differ from platform to platform, 
	//    do we want to rely on browser cache? Check SDWebImage, check Android equivalent)

	return 0;
}

#ifdef ANDROID

#include <jni.h>
#include "android/JavaVM.h"
#include "http/java_net_URL.h"
#include "http/java_net_HttpURLConnection.h"
#include "http/java_io_InputStream.h"
#include "http/java_io_OutputStream.h"
#include "http/AndroidHttpClient.h"

extern "C" JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void* aReserved)
{
	Framework::CJavaVM::SetJavaVM(vm);
	java::net::URL_ClassInfo::GetInstance().PrepareClassInfo();
	java::net::HttpURLConnection_ClassInfo::GetInstance().PrepareClassInfo();
	java::io::InputStream_ClassInfo::GetInstance().PrepareClassInfo();
	java::io::OutputStream_ClassInfo::GetInstance().PrepareClassInfo();
	return JNI_VERSION_1_6;
}

extern "C" JNIEXPORT void JNICALL Java_com_virtualapplications_playservices_NativeInterop_start(JNIEnv* env, jobject obj)
{
	main(0, nullptr);
}

#endif
