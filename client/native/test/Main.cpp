#include "string_format.h"
#include "http/HttpClientFactory.h"

int main(int argc, const char** argv)
{
	auto httpClient = Framework::Http::CreateHttpClient();
	//httpClient->SetUrl("http://localhost/playservices/server/api.php?endpoint=compatibility");
	httpClient->SetUrl("http://www.google.ca");
	httpClient->SetVerb(Framework::Http::HTTP_VERB::GET);
	//httpClient->SetRequestBody("{ \"gameId\": \"SLUS_213.09\", \"rating\": 5, \"deviceInfo\": {} }");
	auto result = httpClient->SendRequest();
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
