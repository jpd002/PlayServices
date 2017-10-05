#include <boost/filesystem.hpp>
#include "PathUtils.h"
#include "StdStreamUtils.h"
#include "coverdb/CoverDbClient.h"
#include "http/HttpClientFactory.h"

using namespace CoverDb;

boost::filesystem::path GetCoverDirectoryPath()
{
	//TODO
	throw std::exception();
	//return CAppConfig::GetBasePath() / boost::filesystem::path("covers/");
}

void CClient::SetGameCover(const char* serial, const char* url)
{
	auto requestResult = 
		[&] ()
		{
			auto client = Framework::Http::CreateHttpClient();
			client->SetUrl(url);
			return client->SendRequest();
		}();

	if(requestResult.statusCode != Framework::Http::HTTP_STATUS_CODE::OK)
	{
		return;
	}

	auto coverDirectoryPath = GetCoverDirectoryPath();
	Framework::PathUtils::EnsurePathExists(coverDirectoryPath);
	auto coverPath = coverDirectoryPath / serial;

	auto outputStream = Framework::CreateOutputStdStream(coverPath.string());
	auto& inputStream = requestResult.data;
	while(!inputStream.IsEOF())
	{
		uint8 buffer[0x10000];
		auto readCount = inputStream.Read(buffer, sizeof(buffer));
		outputStream.Write(buffer, readCount);
	}
}
