@echo off

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DOWNLOAD JARS -------------------------------------------------------------------------------------------------------------------------------------------
mkdir jars
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/com/squareup/okhttp3/okhttp/4.9.1/okhttp-4.9.1.jar "%~dp0\jars\okhttp.jar"
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/com/squareup/okio/okio/2.10.0/okio-2.10.0.jar "%~dp0\jars\okio.jar"
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/1.4.32/kotlin-stdlib-1.4.32.jar "%~dp0\jars\kotlin-stdlib.jar"

echo -- BUILD JAVA ----------------------------------------------------------------------------------------------------------------------------------------------
javac -classpath jars/* DecompressInterceptor.java

echo -- BUILD JAR -----------------------------------------------------------------------------------------------------------------------------------------------
mkdir src
mkdir src\securehttpclient
move DecompressInterceptor.class src\securehttpclient\
jar cvf okhttp-decompressinterceptor.jar -C src .
move okhttp-decompressinterceptor.jar ..\Jars\

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
