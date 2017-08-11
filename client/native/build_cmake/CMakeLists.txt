cmake_minimum_required(VERSION 2.8)
project(PlayServices)

set(CMAKE_MODULE_PATH
	${CMAKE_CURRENT_SOURCE_DIR}/../../../Dependencies/cmake-modules
	${CMAKE_MODULE_PATH}
)

set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -D_DEBUG")
set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -DNDEBUG")

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_definitions(-D_UNICODE)
add_definitions(-DUNICODE)

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

include_directories(../../include ../../../../Framework/include)

set(SRC_FILES
	../test/Main.cpp
)

if(WIN32)
	list(APPEND PROJECT_LIBS wininet)
endif()

if(NOT ANDROID)
add_executable(PlayServicesTest ${SRC_FILES})
else()
add_library(PlayServicesTest SHARED ${SRC_FILES})
endif()
target_link_libraries(PlayServicesTest Framework_Http Framework ${PROJECT_LIBS})
