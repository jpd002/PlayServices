cmake_minimum_required(VERSION 2.8)
project(PlayServices)

set(CMAKE_MODULE_PATH
	${CMAKE_CURRENT_SOURCE_DIR}/../../../../Dependencies/cmake-modules
	${CMAKE_MODULE_PATH}
)

set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -D_DEBUG")
set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -DNDEBUG")

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_definitions(-D_UNICODE)
add_definitions(-DUNICODE)

find_package(SQLite3)
if(NOT SQLITE3_FOUND)
	MESSAGE("-- Using Provided sqlite3 source")
	if(NOT TARGET sqlite3static)
		add_subdirectory(
			${CMAKE_CURRENT_SOURCE_DIR}/../../../../Dependencies/build_cmake/sqlite-3.20.1
			${CMAKE_CURRENT_BINARY_DIR}/sqlite-3.20.1
		)
	endif()
	include_directories(${CMAKE_CURRENT_SOURCE_DIR}/../../../../Dependencies/sqlite-3.20.1)
	list(APPEND PROJECT_LIBS sqlite3static)
else()
	include_directories(${SQLITE3_INCLUDE_DIRS})
	list(APPEND PROJECT_LIBS ${SQLITE3_LIBRARIES})
endif()

if(NOT TARGET Boost::boost)
	add_subdirectory(
		${CMAKE_CURRENT_SOURCE_DIR}/../../../../Dependencies/boost-cmake
		${CMAKE_CURRENT_BINARY_DIR}/boost-cmake
	)
endif()
list(APPEND PROJECT_LIBS Boost::boost)
list(APPEND PROJECT_LIBS Boost::system)
list(APPEND PROJECT_LIBS Boost::filesystem)

if(NOT TARGET Framework)
	add_subdirectory(
		${CMAKE_CURRENT_SOURCE_DIR}/../../../../Framework/build_cmake/Framework
		${CMAKE_CURRENT_BINARY_DIR}/Framework
	)
endif()

if(NOT TARGET Framework_Http)
	add_subdirectory(
		${CMAKE_CURRENT_SOURCE_DIR}/../../../../Framework/build_cmake/FrameworkHttp
		${CMAKE_CURRENT_BINARY_DIR}/FrameworkHttp
	)
endif()

include_directories(../include ../../../../Framework/include)

add_library(PlayServices
	../src/basedb/BaseDbClient.cpp
	../include/basedb/BaseDbClient.h
	../src/coverdb/CoverDbClient.cpp
	../include/coverdb/CoverDbClient.h
	../src/thegamesdb/TheGamesDbClient.cpp
	../include/thegamesdb/TheGamesDbClient.h
	../src/localdb/LocalDbClient.cpp
	../include/localdb/LocalDbClient.h
)
target_link_libraries(PlayServices Boost::boost)

set(TEST_SRC_FILES
	../test/Main.cpp
)

if(WIN32)
	list(APPEND PROJECT_LIBS wininet)
endif()

if(NOT ANDROID)
add_executable(PlayServicesTest ${TEST_SRC_FILES})
else()
add_library(PlayServicesTest SHARED ${TEST_SRC_FILES})
endif()
target_link_libraries(PlayServicesTest PlayServices Framework_Http Framework ${PROJECT_LIBS})
