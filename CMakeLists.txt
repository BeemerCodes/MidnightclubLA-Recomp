# midnight_club_la - ReXGlue Recompiled Project
#
# SDK boilerplate lives in generated/rexglue.cmake.

cmake_minimum_required(VERSION 3.25)
project(midnight_club_la LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(CMAKE_SYSTEM_PROCESSOR MATCHES "x86_64|AMD64")
    add_compile_options(-mssse3 -msse4.1)
endif()

include(generated/rexglue.cmake)


# Sources
set(MIDNIGHTCLUBLA_SOURCES
    src/main.cpp
    src/midnight_club_la.rc
)

if(WIN32)
    add_executable(midnight_club_la WIN32 ${MIDNIGHTCLUBLA_SOURCES})
else()
    add_executable(midnight_club_la ${MIDNIGHTCLUBLA_SOURCES})
endif()

# V1 Optimizations: Enable LTO (Interprocedural Optimization) and Native Architecture
set(CMAKE_INTERPROCEDURAL_OPTIMIZATION ON)
if(MSVC) # Clang-cl on Windows
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /arch:AVX2 /O2")
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} /arch:AVX2 /O2")
else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -march=native -O3")
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -march=native -O3")
endif()

rexglue_setup_target(midnight_club_la)

target_include_directories(midnight_club_la PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/thirdparty/rexglue-sdk/thirdparty/imgui"
)



