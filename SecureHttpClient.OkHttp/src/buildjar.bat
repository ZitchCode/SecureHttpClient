@echo off

SETLOCAL
set Version_Okhttp=5.2.1
set Version_Okio=3.16.2
set Version_KotlinStdlib=2.2.21
set Version_Brotli=0.1.2

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DOWNLOAD JARS -------------------------------------------------------------------------------------------------------------------------------------------
mkdir jars
bitsadmin.exe /transfer "Download okhttp-android %Version_Okhttp%" https://repo1.maven.org/maven2/com/squareup/okhttp3/okhttp-android/%Version_Okhttp%/okhttp-android-%Version_Okhttp%.aar "%~dp0\jars\okhttp-android-%Version_Okhttp%.aar"
bitsadmin.exe /transfer "Download okio %Version_Okio%" https://repo1.maven.org/maven2/com/squareup/okio/okio/%Version_Okio%/okio-%Version_Okio%.jar "%~dp0\jars\okio-%Version_Okio%.jar"
bitsadmin.exe /transfer "Download okio-jvm %Version_Okio%" https://repo1.maven.org/maven2/com/squareup/okio/okio-jvm/%Version_Okio%/okio-jvm-%Version_Okio%.jar "%~dp0\jars\okio-jvm-%Version_Okio%.jar"
bitsadmin.exe /transfer "Download kotlin-stdlib %Version_KotlinStdlib%" https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/%Version_KotlinStdlib%/kotlin-stdlib-%Version_KotlinStdlib%.jar "%~dp0\jars\kotlin-stdlib-%Version_KotlinStdlib%.jar"
bitsadmin.exe /transfer "Download org.brotli.dec %Version_Brotli%" https://repo1.maven.org/maven2/org/brotli/dec/%Version_Brotli%/dec-%Version_Brotli%.jar "%~dp0\jars\org.brotli.dec-%Version_Brotli%.jar"

echo -- EXTRACT JAR FROM AAR ------------------------------------------------------------------------------------------------------------------------------------
jar xf jars\okhttp-android-%Version_Okhttp%.aar classes.jar
move classes.jar jars\okhttp-android-%Version_Okhttp%.jar
del jars\okhttp-android-%Version_Okhttp%.aar

echo -- BUILD JAVA ----------------------------------------------------------------------------------------------------------------------------------------------
javac -Xlint:all -classpath jars/* *.java
mkdir src
mkdir src\securehttpclient-okhttp
move *.class src\securehttpclient-okhttp\
jar cvf securehttpclient-okhttp.jar -C src .
move securehttpclient-okhttp.jar ..\Jars\

echo -- BUILD KT ------------------------------------------------------------------------------------------------------------------------------------------------
call kotlinc JavaNetCookieJar.kt -cp "jars/okhttp-android-%Version_Okhttp%.jar;jars/kotlin-stdlib-%Version_KotlinStdlib%.jar" -d securehttpclient-javanetcookiejar.jar
move securehttpclient-javanetcookiejar.jar ..\Jars\

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
for /d /r . %%d in (jars,src) do @if exist "%%d" rd /s/q "%%d"

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
ENDLOCAL
