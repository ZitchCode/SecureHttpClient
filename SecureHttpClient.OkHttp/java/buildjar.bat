@echo off

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DOWNLOAD JARS -------------------------------------------------------------------------------------------------------------------------------------------
mkdir jars
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/com/squareup/okhttp3/okhttp/4.11.0/okhttp-4.11.0.jar "%~dp0\jars\okhttp.jar"
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/com/squareup/okio/okio/3.3.0/okio-3.3.0.jar "%~dp0\jars\okio.jar"
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/com/squareup/okio/okio-jvm/3.3.0/okio-jvm-3.3.0.jar "%~dp0\jars\okio-jvm.jar"
bitsadmin.exe /transfer "Download okhttp jar" https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/1.8.20/kotlin-stdlib-1.8.20.jar "%~dp0\jars\kotlin-stdlib.jar"

echo -- BUILD JAVA ----------------------------------------------------------------------------------------------------------------------------------------------
javac -classpath jars/* *.java

echo -- BUILD JAR -----------------------------------------------------------------------------------------------------------------------------------------------
mkdir src
mkdir src\securehttpclient-okhttp
move *.class src\securehttpclient-okhttp\
jar cvf securehttpclient-okhttp.jar -C src .
move securehttpclient-okhttp.jar ..\Jars\

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
