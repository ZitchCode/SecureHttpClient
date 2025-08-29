@echo off

SETLOCAL
set Version_Okhttp=5.1.0
set Version_Okio=3.16.0
set Version_KotlinStdlib=2.2.10
set Version_Brotli=0.1.2

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DOWNLOAD JARS -------------------------------------------------------------------------------------------------------------------------------------------
mkdir jars
bitsadmin.exe /transfer "Download okhttp-jvm %Version_Okhttp%" https://repo1.maven.org/maven2/com/squareup/okhttp3/okhttp-jvm/%Version_Okhttp%/okhttp-jvm-%Version_Okhttp%.jar "%~dp0\jars\okhttp-jvm-%Version_Okhttp%.jar"
bitsadmin.exe /transfer "Download okio %Version_Okio%" https://repo1.maven.org/maven2/com/squareup/okio/okio/%Version_Okio%/okio-%Version_Okio%.jar "%~dp0\jars\okio-%Version_Okio%.jar"
bitsadmin.exe /transfer "Download okio-jvm %Version_Okio%" https://repo1.maven.org/maven2/com/squareup/okio/okio-jvm/%Version_Okio%/okio-jvm-%Version_Okio%.jar "%~dp0\jars\okio-jvm-%Version_Okio%.jar"
bitsadmin.exe /transfer "Download kotlin-stdlib %Version_KotlinStdlib%" https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/%Version_KotlinStdlib%/kotlin-stdlib-%Version_KotlinStdlib%.jar "%~dp0\jars\kotlin-stdlib-%Version_KotlinStdlib%.jar"
bitsadmin.exe /transfer "Download org.brotli.dec %Version_Brotli%" https://repo1.maven.org/maven2/org/brotli/dec/%Version_Brotli%/dec-%Version_Brotli%.jar "%~dp0\jars\org.brotli.dec-%Version_Brotli%.jar"

echo -- COPY IMPORTS --------------------------------------------------------------------------------------------------------------------------------------------
copy "%~dp0\jars\org.brotli.dec-%Version_Brotli%.jar" ..\import\org.brotli.dec-%Version_Brotli%.jar

echo -- BUILD JAVA ----------------------------------------------------------------------------------------------------------------------------------------------
javac -Xlint:all -classpath jars/* *.java

echo -- BUILD JAR -----------------------------------------------------------------------------------------------------------------------------------------------
mkdir src
mkdir src\securehttpclient-okhttp
move *.class src\securehttpclient-okhttp\
jar cvf securehttpclient-okhttp.jar -C src .
move securehttpclient-okhttp.jar ..\Jars\

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
ENDLOCAL
