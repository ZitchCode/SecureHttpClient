package securehttpclient.okhttp;

import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;
import java.io.BufferedInputStream;
import java.util.zip.Inflater;
import java.util.zip.InflaterInputStream;
import java.util.zip.GZIPInputStream;
import org.brotli.dec.BrotliInputStream;

import okhttp3.Headers;
import okhttp3.Interceptor;
import okhttp3.Response;
import okhttp3.ResponseBody;
import okio.BufferedSource;
import okio.Okio;

public class DecompressInterceptor implements Interceptor {
    @Override
    public Response intercept(Chain chain) throws IOException {
        Response response = chain.proceed(chain.request());

        if (isCompressed(response)) {
            return decompress(response);
        } else {
            return response;
        }
    }

    private Response decompress(final Response response) throws IOException {

        if (response.body() == null) {
            return response;
        }

        BufferedSource source = response.body().source();
        InputStream inputStream = null;

		switch(response.header("Content-Encoding").toLowerCase()) {
            case "gzip": {
                inputStream = new GZIPInputStream(source.inputStream());
				break;
			}
            case "deflate": {
                boolean hasZlibHeader = false;
                BufferedInputStream bufferedInputStream = new BufferedInputStream(source.inputStream());
                try {
                    source.require(2); // throws EOFException if size < 2
                    byte[] headerBytes = new byte[2];
                    bufferedInputStream.mark(0);
                    bufferedInputStream.read(headerBytes);
                    bufferedInputStream.reset();
                    hasZlibHeader = (headerBytes[0] & 0xFF) == 0x78 && (headerBytes[1] & 0xFF) <= 0xDA;
                } catch (EOFException e) {
                }
                if (hasZlibHeader) {
                    // zlib decompression (rfc 1951)
                    inputStream = new InflaterInputStream(bufferedInputStream, new Inflater());
                } else {
                    // raw deflate decompression (rfc 1950)
                    inputStream = new InflaterInputStream(bufferedInputStream, new Inflater(true));
                }
				break;
			}
            case "br": {
				inputStream = new BrotliInputStream(source.inputStream());
				break;
			}
        }

        byte[] bodyBytes = Okio.buffer(Okio.source(inputStream)).readByteArray();
        ResponseBody responseBody = ResponseBody.create(bodyBytes, response.body().contentType());
        inputStream.close();

        Headers strippedHeaders = response.headers().newBuilder()
                .removeAll("Content-Encoding")
                .removeAll("Content-Length")
                .build();
        return response.newBuilder()
                .headers(strippedHeaders)
                .body(responseBody)
                .message(response.message())
                .build();
    }

    private Boolean isCompressed(Response response) {
        String contentEncoding = response.header("Content-Encoding");
        return contentEncoding != null 
            && (contentEncoding.equalsIgnoreCase("gzip") || contentEncoding.equalsIgnoreCase("deflate") || contentEncoding.equalsIgnoreCase("br"));
    }
}